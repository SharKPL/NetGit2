using MUSOAR;
using UnityEngine;
using Mirror;

public class ChildInteract : NetworkBehaviour,IInteractable
{
    private bool hisFix=false;
    [SerializeField] private GameObject coll;

    public bool HisFix { get { return hisFix; } }

    public override void OnStartClient()
    {
        base.OnStartClient();
        coll.SetActive(false);
    }
    public void Fix()
    {
        hisFix = true;
        coll.SetActive(true);
    }

    [ClientRpc]
    public void RpcFix()
    {
        Debug.LogError("RpcChildFix");
        Fix();
    }

    [Command(requiresAuthority = false)]
    public void CmdFix()
    {
        Debug.LogError("CmdChildFix");
        RpcFix();
    }

    public void Interact()
    {
        if (hisFix)
        {
            Debug.Log("ChildFix");
        }
        else
        {
            Debug.Log("NotFix");
        }
    }

   
}
