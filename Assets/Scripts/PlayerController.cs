using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonPlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main Camera.")]
    [SerializeField] private Transform cameraTransform;
    [Tooltip("Rigidbody.")]
    [SerializeField] private Rigidbody rb;
    [Tooltip("Animator.")]
    [SerializeField] private Animator animator;

    [Header("Movement config")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float rotationSpeed = 15f;

    private Vector3 moveDirection = Vector3.zero;
    private float currentSpeed = 0f;

    void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator Not found.");
                enabled = false;
                return;
            }
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody Not found.");
                enabled = false;
                return;
            }
        }

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (cameraTransform == null)
        {
            cameraTransform = Camera.main?.transform;
            if (cameraTransform == null)
            {
                Debug.LogError("Main Camera Not found.");
                enabled = false;
            }
        }
    }

    void Update()
    {
        HandleInteraction();
        HandleAnimation();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        float targetSpeed = inputDir.magnitude * moveSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDirection = moveDir.normalized * currentSpeed;
        }
        else
        {
            moveDirection = Vector3.zero;
        }

        Vector3 velocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
        rb.linearVelocity = velocity;
    }

    private void HandleAnimation()
    {
        bool isWalking = currentSpeed > 0.1f;
        animator.SetBool("Walking", isWalking);
    }

    private void HandleInteraction()
    {
        if (Input.GetButtonDown("Interact"))
        {
            animator.SetTrigger("Interact");
        }
    }
}
