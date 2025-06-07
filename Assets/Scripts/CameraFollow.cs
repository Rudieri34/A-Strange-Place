using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("The target object for the camera to follow (e.g., the player).")]
    public Transform target;

    [Header("Position Follow Settings")]
    [Tooltip("The fixed distance and height from the target.")]
    public Vector3 positionOffset = new Vector3(0f, 2f, -5f);

    [Tooltip("How quickly the camera follows the target's position. Higher values are faster.")]
    public float followSpeed = 5f;

    [Header("Rotation Follow Settings")]
    [Tooltip("How quickly the camera rotates to look at the target. Lower values create a more noticeable delay.")]
    public float rotationSpeed = 3f;

    void Update()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera Follow Target is not assigned.", this);
            return;
        }

        Vector3 desiredPosition = target.position + positionOffset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        Quaternion desiredRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
    }
}
