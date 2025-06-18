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

    private void FixedUpdate()
    {
        GlobalEventManager.showInteract?.Invoke(Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitInfo, maxDistance, interactMask));
    }

    private void Interact()
    {
        Vector3 direction = transform.forward;

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hitInfo, maxDistance, interactMask))
        {
            Debug.Log(hitInfo.collider.name);
            Debug.DrawRay(cam.transform.position, direction * maxDistance, Color.green, 2f);
            if (hitInfo.collider.gameObject.GetComponentInParent<IInteractable>()!=null)
            {
                hitInfo.collider.gameObject.GetComponentInParent<IInteractable>().Interact();
                return;
            }
            var item = hitInfo.collider.gameObject.GetComponent<Item>();
            if (item == null)
            {
                Debug.Log("NoItem");
                return;
            }
            Inventory.Instance.CmdAddItem(item);

        }
        else
        {
            Debug.DrawLine(cam.transform.position, hitInfo.point, Color.red, 2f);
        }

        
    }
}
