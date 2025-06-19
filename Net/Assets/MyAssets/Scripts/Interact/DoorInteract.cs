using MUSOAR;
using Mirror;
using UnityEngine;

using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class DoorInteract : NetworkBehaviour,IInteractable
{
    [SerializeField] string needItem;

    [SerializeField] private AudioClip doorFix;
    [SerializeField] private AudioClip doorOpenClose;
    [SerializeField] private AudioClip doorLock;

    [SerializeField] private AudioSource doorSource;

    [SerializeField] GameObject door;

    [SerializeField] [SyncVar] private bool hisUnocked;

    [SerializeField] private float openAngle = 90f;

    [SerializeField] private float rotationSpeed = 250f;

    [SyncVar] private bool hisOpen = false;

    private bool canTrigger=true;


    private Quaternion targetRotation; 
    private Quaternion startRotation;

    void Start()
    {
        startRotation = door.transform.rotation;
        targetRotation = startRotation * Quaternion.Euler(0, 0, openAngle);
    }
    public void Interact()
    {
        Debug.Log("DoorInteract");
        if (hisUnocked && canTrigger)
        {
            ToggleDoor();
        }
        else if(Inventory.Instance.TryGetItem(needItem) && !hisUnocked)
        {
            Inventory.Instance.CmdRemoveItem(needItem, false);
            hisUnocked = true;
            CmdPlayDoorFix();
            Debug.Log("DoorOpen");
        }
        else if (!hisUnocked)
        {
            CmdPlayDoorLock();
        }
    }

    public void ToggleDoor()
    {
        if (hisOpen)
        {
            CmdPlayDoorOpenClose();
            StartCoroutine(RotateDoor(startRotation, rotationSpeed));
        }
        else
        {
            CmdPlayDoorOpenClose();
            StartCoroutine(RotateDoor(targetRotation, rotationSpeed));
        }

        hisOpen = !hisOpen;
    }

    IEnumerator RotateDoor(Quaternion target, float speed)
    {
        canTrigger = false;
        while (Quaternion.Angle(door.transform.rotation, target) > 0.1f)
        {
            Quaternion newRotation = Quaternion.RotateTowards(
                door.transform.rotation,
                target,
                speed * Time.deltaTime
            );

            door.transform.rotation = newRotation;

            yield return null;
        }
        canTrigger = true;
        door.transform.rotation = target;
    }

    [ClientRpc]
    private void RpcPlayDoorFix()
    {
        AudioManager.PlaySound(doorSource,doorFix);
    }

    [Command(requiresAuthority =false)]
    private void CmdPlayDoorFix()
    {
        RpcPlayDoorFix();
    }

    [ClientRpc]
    private void RpcPlayDoorOpenClose()
    {
        AudioManager.PlaySound(doorSource, doorOpenClose);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayDoorOpenClose()
    {
        RpcPlayDoorOpenClose();
    }

    [ClientRpc]
    private void RpcPlayDoorLock()
    {
        AudioManager.PlaySound(doorSource, doorLock);
    }

    [Command(requiresAuthority = false)]
    private void CmdPlayDoorLock()
    {
        RpcPlayDoorLock();
    }



}
