using MUSOAR;
using UnityEngine;
using Mirror;

public class ChildInteract : NetworkBehaviour,IInteractable
{
    [SyncVar]private bool hisFix=false;

    public bool HisFix { get { return hisFix; } }

    public void Fix()
    {
        hisFix = true;
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
