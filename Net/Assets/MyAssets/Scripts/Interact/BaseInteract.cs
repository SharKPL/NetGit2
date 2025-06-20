using Mirror;
using MUSOAR;
using UnityEngine;

public class BaseInteract : NetworkBehaviour,IInteractable
{
    [SerializeField] string needItem;

    [SerializeField] ChildInteract chldInter;

    [SerializeField] AudioSource genSource;
    [SerializeField] AudioClip genFixSound;

    public void Interact()
    {
        if (Inventory.Instance.TryGetItem(needItem) && !chldInter.HisFix)
        {
            Inventory.Instance.CmdRemoveItem(needItem, false);
            chldInter.Fix();
            Debug.Log("FixChild");
            CmdFixGen();
            gameObject.layer = 0;
        }
        Debug.Log("NotFixChild");
    }

    [ClientRpc]
    private void RpcFixGen()
    {
        AudioManager.PlaySound(genSource, genFixSound);
    }

    [Command(requiresAuthority = false)]
    private void CmdFixGen()
    {
        RpcFixGen();
    }
}
