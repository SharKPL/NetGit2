using MUSOAR;
using UnityEngine;

public class ChildInteract : MonoBehaviour,IInteractable
{
    private bool hisFix=false;

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
