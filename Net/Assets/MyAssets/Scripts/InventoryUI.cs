using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private int baseLineCount=5;
    [SerializeField] private InventoryLineUI linePref;

    [SerializeField] private GameObject content;

    private CustomPool<InventoryLineUI> linesPool;

    private System.Action<InputAction.CallbackContext> openInventoryDelegate;

    private void Start()
    {
        openInventoryDelegate = ctx => OpenCloseInventory();
        InputManager.Instance.GetInventoryAction().performed += openInventoryDelegate;

        linesPool = new CustomPool<InventoryLineUI>(linePref, baseLineCount, content.transform);
        for (int i = 0; i < baseLineCount; i++)
        {
            linesPool.GetByIndex(i).SetPoolLink(linesPool);
        }

        GlobalEventManager.TakeItemEvent.AddListener(AddItem);

        GlobalEventManager.UpdateInventoryUI.AddListener(UpdateUIInventory);

        gameObject.SetActive(false);
    }

    private void AddItem(string itemName)
    {
        InventoryLineUI line = GetLineFromPool(itemName);
        Debug.Log(line.gameObject.activeInHierarchy);
        line.gameObject.SetActive(true);
        line.SetInvLineData(itemName,Inventory.Instance.GetItemCount(itemName));
    }

    private void OpenCloseInventory()
    {
        if (GameManager.Instance.GameInPause) return;
        Debug.Log("OpenChat");
        gameObject.SetActive(!gameObject.activeSelf);
        GlobalEventManager.TurnPlayerControl?.Invoke(gameObject.activeSelf);
    }


    private InventoryLineUI GetLineFromPool(string name)
    {
        for (int i = 0; i < linesPool.Count(); i++)
        {
            var line = linesPool.GetByIndex(i);
            if (line.ItemName == name && line.gameObject.activeSelf)
                return line;
        }
        for (int i = 0; i < linesPool.Count(); i++)
        {
            var line = linesPool.GetByIndex(i);
            if (!line.gameObject.activeSelf)
                return line;
        }
        return linesPool.Get();
    }

    public void UpdateUIInventory()
    {
        for (int i = 0; i < linesPool.Count(); i++)
        {
            var line = linesPool.GetByIndex(i).GetComponent<InventoryLineUI>();
            line.SetInvLineData("", 0);
            line.RealeseSelf();

        }
        foreach (var item in Inventory.Instance.ItemList)
        {
            Debug.Log($"Item: {item.Key}");
            AddItem(item.Key);
        }
    }
}
