using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class InventoryLineUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemCount;
    [SerializeField] private Button dropBtn;

    public string ItemName { get { return itemName.text; } }

    private CustomPool<InventoryLineUI> poolLink;

    private void Start()
    {
        dropBtn.onClick.AddListener(DropItem);
    }

    public void SetPoolLink(CustomPool<InventoryLineUI> Link)
    {
        poolLink = Link;
    }
    public void SetInvLineData(string name, int count)
    {
        if (itemName != null && itemCount!=null && dropBtn != null)
        {
            itemName.text = name;
            itemCount.text = count.ToString();
        }
        else
        {
            Debug.Log("ItemLineNoRef");
        }
    }

    private void DropItem() 
    {
        Inventory.Instance.CmdRemoveItem(itemName.text);
        StartCoroutine(DropRoutine());
    }

    private IEnumerator DropRoutine()
    {
        dropBtn.interactable = false;
        yield return new WaitForSeconds(0.05f);
        if (Inventory.Instance.GetItemCount(itemName.text) > 0)
        {
            SetInvLineData(itemName.text, Inventory.Instance.GetItemCount(itemName.text));
            
        }
        else
        {
            itemName.text = "";
            itemCount.text = "";
            poolLink.Release(this);
        }
        dropBtn.interactable = true;
    }

}
