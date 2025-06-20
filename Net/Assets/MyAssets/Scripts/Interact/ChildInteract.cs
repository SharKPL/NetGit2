using MUSOAR;
using UnityEngine;
using Mirror;

public class ChildInteract : NetworkBehaviour,IInteractable
{
    [SyncVar]private bool hisFix=false;
    [SerializeField] private Collider coll;

    public bool HisFix { get { return hisFix; } }

    private void Start()
    {
        coll.gameObject.SetActive(false);
    }
    public void Fix()
    {
        hisFix = true;
        coll.gameObject.SetActive(true);
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
