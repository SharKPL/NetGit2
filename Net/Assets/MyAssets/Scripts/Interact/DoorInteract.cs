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

    [SerializeField] private bool hisUnocked;

    [SerializeField] private float openAngle = 90f;

    [SerializeField] private float rotationSpeed = 250f;

    [SyncVar] private bool hisOpen;

    [SyncVar] private Quaternion curRot = Quaternion.identity;

    [SyncVar]private bool canTrigger=true;


    private Quaternion targetRotation; 
    private Quaternion startRotation;


    public override void OnStartClient()
    {
        Debug.Log($"curRot:{curRot},null:{curRot == null}");
        base.OnStartClient();
        if (curRot != null && curRot!= Quaternion.identity)
        {
            door.transform.rotation = curRot;
            startRotation = curRot;
            targetRotation = startRotation * Quaternion.Euler(0, 0, openAngle);
        }
        else
        {
            hisOpen = false;
            curRot =door.transform.rotation;
            startRotation = door.transform.rotation;
            targetRotation = startRotation * Quaternion.Euler(0, 0, openAngle);
        }

    }
    public void Interact()
    {
        Debug.Log("DoorInteract");
        if (hisUnocked && canTrigger)
        {
            CmdToggleDoor();
            CmdPlayDoorOpenClose();
        }
        else if(Inventory.Instance.TryGetItem(needItem) && !hisUnocked)
        {
            Inventory.Instance.CmdRemoveItem(needItem, false);
            CmdOpenDoor();
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
            StartCoroutine(RotateDoor(startRotation, rotationSpeed));
        }
        else
        {
            StartCoroutine(RotateDoor(targetRotation, rotationSpeed));
        }

        hisOpen = !hisOpen;
    }

    [ClientRpc]
    private void RpcOpenDoor()
    {
        hisUnocked = true;
    }

    [Command(requiresAuthority = false)]
    private void CmdOpenDoor()
    {
        RpcOpenDoor();
    }

    [ClientRpc]
    private void RpcToggleDoor()
    {
        ToggleDoor();
    }

    [Command(requiresAuthority = false)]
    private void CmdToggleDoor()
    {
        RpcToggleDoor();
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
        curRot = door.transform.rotation;
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
