using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;

public class PlayerCameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform target; 
    [SerializeField] private Camera playerCamera;

    [Header("Orbit Settings")]
    [SerializeField] private float distance = 5f; 
    [SerializeField] private float sensitivity = 2f; 
    [SerializeField] private float smoothTime = 0.1f; 

    [Header("Vertical Limits")]
    [SerializeField] private float minVerticalAngle = -30f; 
    [SerializeField] private float maxVerticalAngle = 60f;  

    [Header("Position Offset")]
    [SerializeField] private Vector3 cameraOffset = Vector3.zero; 

    private System.Action<InputAction.CallbackContext> lookDelegate;


    public void SetCameraOffset(Vector3 offset)
    {
        cameraOffset = offset;
    }


    public Vector3 GetCameraOffset()
    {
        return cameraOffset;
    }

    private float horizontalAngle = 0f;
    private float verticalAngle = 0f;
    private float currentHorizontalAngle;
    private float currentVerticalAngle;
    private float horizontalVelocity;
    private float verticalVelocity;


    public override void OnStartClient()
    {
        base.OnStartClient();
        playerCamera.gameObject.SetActive(false);

    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        
        if (playerCamera == null)
            return;

        
        if (target == null)
            target = transform;

        
        Vector3 angles = transform.eulerAngles;
        horizontalAngle = angles.y;
        verticalAngle = angles.x;

        currentHorizontalAngle = horizontalAngle;
        currentVerticalAngle = verticalAngle;

        playerCamera.gameObject.SetActive(true);
        playerCamera.enabled = true;

        lookDelegate = ctx => Look(ctx.ReadValue<Vector2>());
        InputManager.Instance.GetLookAction().performed += lookDelegate;
    }

    private void Look(Vector2 lookInput)
    {
        
        horizontalAngle += lookInput.x * sensitivity;
        verticalAngle -= lookInput.y * sensitivity; 

        
        verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer || target == null || playerCamera == null)
            return;

        
        currentHorizontalAngle = Mathf.SmoothDampAngle(currentHorizontalAngle, horizontalAngle, ref horizontalVelocity, smoothTime);
        currentVerticalAngle = Mathf.SmoothDampAngle(currentVerticalAngle, verticalAngle, ref verticalVelocity, smoothTime);

        
        Quaternion rotation = Quaternion.Euler(currentVerticalAngle, currentHorizontalAngle, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * distance) + cameraOffset;

        
        playerCamera.transform.position = position;
        playerCamera.transform.LookAt(target.position + cameraOffset);
    }

    
    public void SetDistance(float newDistance)
    {
        distance = Mathf.Max(1f, newDistance);
    }

   
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
