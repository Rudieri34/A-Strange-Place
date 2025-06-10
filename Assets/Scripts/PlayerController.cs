using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Main Camera.")]
    [SerializeField] private Transform _cameraTransform;
    [Tooltip("Rigidbody.")]
    [SerializeField] private Rigidbody _rb;
    [Tooltip("Animator.")]
    [SerializeField] private Animator _animator;

    [Header("Movement config")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _rotationSpeed = 15f;

    private Vector3 _moveDirection = Vector3.zero;
    private float _currentSpeed = 0f;

    void Start()
    {
        if (_rb == null)
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("Rigidbody Not found.");
                enabled = false;
                return;
            }
        }

       // _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
       // _rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main?.transform;
            if (_cameraTransform == null)
            {
                Debug.LogError("Main Camera Not found.");
                enabled = false;
            }
        }

        if (SaveManager.Instance.CheckIfSaveGameExists())
            transform.position = SaveManager.Instance.PlayerPosition;

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
        if (GameManager.Instance.IsInventoryOpen)
            return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 inputDir = new Vector3(horizontal, 0f, vertical).normalized;

        float targetSpeed = inputDir.magnitude * _moveSpeed;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, _acceleration * Time.fixedDeltaTime);

        if (inputDir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, _rotationSpeed * Time.fixedDeltaTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _moveDirection = moveDir.normalized * _currentSpeed;
        }
        else
        {
            _moveDirection = Vector3.zero;
        }

        Vector3 velocity = new Vector3(_moveDirection.x, _rb.linearVelocity.y, _moveDirection.z);
        _rb.linearVelocity = velocity;
    }

    private void HandleAnimation()
    {

        bool isWalking = _currentSpeed > 0.1f && !GameManager.Instance.IsInventoryOpen;
        _animator.SetBool("Walking", isWalking);
    }

    private void HandleInteraction()
    {
        if (GameManager.Instance.IsInventoryOpen)
            return;

        if (Input.GetButtonDown("Interact"))
        {
            _animator.SetTrigger("Interact");
        }
    }
}
