using MUSOAR;
using UnityEngine;

public class BaseInteract : MonoBehaviour,IInteractable
{
    [SerializeField] string needItem;

    [SerializeField] ChildInteract chldInter;

    public void Interact()
    {
        if (Inventory.Instance.TryGetItem(needItem) && !chldInter.HisFix)
        {
            Inventory.Instance.CmdRemoveItem(needItem, false);
            chldInter.Fix();
            Debug.Log("FixChild");
        }
        Debug.Log("NotFixChild");
    }
}
