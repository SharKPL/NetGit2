using MUSOAR;
using UnityEngine;
using Mirror;

public class StickInteract : NetworkBehaviour, IInteractable
{
    [SerializeField] private string needItem;
    [SerializeField] private Collider stickCol;

    [SerializeField] private MeshRenderer stickRenderer;

    [SerializeField] private Material stickNoFixMat;
    [SerializeField] private Material stickFixMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stickRenderer.material = stickNoFixMat;
        stickCol.isTrigger = true;
    }

    public void Interact()
    {
        if (Inventory.Instance.TryGetItem(needItem))
        {
            Inventory.Instance.CmdRemoveItem(needItem, false);
            stickRenderer.material = stickFixMat;
            stickCol.isTrigger = false;
        }
    }

}
