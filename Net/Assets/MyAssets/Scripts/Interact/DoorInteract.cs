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

    [SyncVar] private Quaternion curRot = Quaternion.identity;

    private bool canTrigger=true;


    private Quaternion targetRotation; 
    private Quaternion startRotation;


    public override void OnStartClient()
    {
        Debug.Log($"curRot:{curRot},null:{curRot == null}");
        base.OnStartClient();
        if (curRot != null && curRot!= Quaternion.identity)
        {
            door.transform.rotation = curRot;
        }
        else
        {
            curRot=door.transform.rotation;
        }
        startRotation = door.transform.rotation;
        targetRotation = startRotation * Quaternion.Euler(0, 0, openAngle);
    }
    public void Interact()
    {
        Debug.Log("DoorInteract");
        if (hisUnocked && canTrigger)
        {
            CmdToggleDoor();
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
