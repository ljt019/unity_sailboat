using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;  // The target to follow (e.g., player model)
    public Vector3 positionOffset = new Vector3(0f, 5f, -10f);  // Default position offset
    public Vector3 rotationOffset = Vector3.zero;  // Rotation offset in degrees
    public float smoothSpeed = 0.125f;  // How smoothly the camera follows the target

    private void Start()
    {
        // Check if target is assigned
        if (target == null)
        {
            Debug.LogError("CameraFollow: No target assigned in the inspector!");
        }
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        // Calculate the desired position
        Vector3 desiredPosition = target.position + target.TransformDirection(positionOffset);

        // Smoothly move the camera towards the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        // Update the camera's position
        transform.position = smoothedPosition;

        // Calculate the desired rotation
        Quaternion targetRotation = target.rotation * Quaternion.Euler(rotationOffset);

        // Smoothly rotate the camera
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed);
    }

    // Method to set the target programmatically if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}