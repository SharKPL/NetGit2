using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using MUSOAR;

public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private Camera cam;
    [Header("Interact")]
    [SerializeField] float radius = 2f;
    [SerializeField] float maxDistance = 5f;
    [SerializeField] LayerMask interactMask;

    private System.Action<InputAction.CallbackContext> interactDelegate;

    private void Start()
    {
        interactDelegate = ctx => Interact();

        InputManager.Instance.GetInteractAction().performed += interactDelegate;

    }

    private void Interact()
    {
        Debug.Log("Interact");
        Vector3 direction = transform.forward;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitInfo, maxDistance, interactMask))
        {
            if (hitInfo.collider.gameObject.GetComponent<IInteractable>()!=null)
            {
                hitInfo.collider.gameObject.GetComponent<IInteractable>().Interact();
                return;
            }
            var item = hitInfo.collider.gameObject.GetComponent<Item>();
            if (item == null)
            {
                Debug.Log("NoItem");
                return;
            }
            Inventory.Instance.CmdAddItem(item);
            Debug.DrawLine(cam.transform.position, hitInfo.point, Color.red, 2f);
        }
        else
        {
            Debug.DrawRay(cam.transform.position, direction * maxDistance, Color.green, 2f);
        }

        //if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, maxDistance, interactMask))
        //{
        //    var item = hit.collider.gameObject.GetComponent<Item>();
        //    if (item == null)
        //    {
        //        Debug.Log("NoItem");
        //        return;
        //    }
        //    Inventory.Instance.CmdAddItem(item);

        //    Debug.DrawLine(transform.position, hit.point, Color.red, 2f);
        //}
        //else
        //{
        //    Debug.DrawRay(transform.position, direction * maxDistance, Color.green, 2f);
        //}
    }
}
