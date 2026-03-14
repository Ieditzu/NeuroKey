using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BeanController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float fallYThreshold = -6f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float mouseSensitivity = 2f;

    private Rigidbody rb;
    private Vector3 input;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private int timesFallen;
    private bool movementLocked;
    private bool hardFreeze;
    private bool isSprinting;
    private bool isGrounded;

    private Transform camTransform;
    private float pitch;

    public static bool KeyboardInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Make it a bean, do not roll

        var sphereCol = GetComponent<SphereCollider>();
        if (sphereCol != null)
        {
            Destroy(sphereCol);
        }

        var capCol = GetComponent<CapsuleCollider>();
        if (capCol == null)
        {
            capCol = gameObject.AddComponent<CapsuleCollider>();
            capCol.height = 2f;
            capCol.radius = 0.5f;
            capCol.center = new Vector3(0, 1f, 0);
        }

        var filter = GetComponent<MeshFilter>();
        if (filter != null)
        {
            var temp = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            filter.sharedMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Destroy(temp);
        }

        var mainCam = Camera.main;
        if (mainCam != null)
        {
            // First-person: put the main camera inside the bean and drive it from this controller.
            var follow = mainCam.GetComponent<BeanCameraFollow>();
            if (follow != null)
            {
                Destroy(follow);
            }

            camTransform = mainCam.transform;
            camTransform.SetParent(transform, false);
            camTransform.localPosition = new Vector3(0f, 1.1f, 0f);
            camTransform.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        startPosition = transform.position;
        startRotation = transform.rotation;
        timesFallen = 0;
        FallCounterDisplay.SetCount(timesFallen);
    }

    private void Update()
    {
        if (hardFreeze)
        {
            input = Vector3.zero;
            isSprinting = false;
            return;
        }

        if (!movementLocked && KeyboardInputEnabled && camTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -85f, 85f);
            camTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !movementLocked && !hardFreeze && KeyboardInputEnabled)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        Vector2 mobileMove = MobileTouchInput.Move;
        if (mobileMove.sqrMagnitude > 0.0001f)
        {
            input = new Vector3(mobileMove.x, 0f, mobileMove.y);
            return;
        }

        if (!KeyboardInputEnabled)
        {
            input = Vector3.zero;
            return;
        }

        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) z -= 1f;

        input = new Vector3(x, 0f, z).normalized;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 0.2f);

        if (hardFreeze)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            return;
        }

        if (transform.position.y < fallYThreshold)
        {
            rb.velocity = Vector3.zero;
            timesFallen++;
            FallCounterDisplay.SetCount(timesFallen);
            RespawnNow();
            return;
        }

        if (movementLocked)
        {
            return;
        }

        float currentSpeed = isSprinting ? moveSpeed * sprintMultiplier : moveSpeed;
        
        Vector3 moveDir;
        if (camTransform != null)
        {
            moveDir = transform.right * input.x + transform.forward * input.z;
        }
        else
        {
            // If there's no camera, fall back to simple world or relative transform space
            moveDir = transform.right * input.x + transform.forward * input.z;
        }

        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        Vector3 targetVelocity = moveDir * currentSpeed;
        rb.velocity = new Vector3(targetVelocity.x, rb.velocity.y, targetVelocity.z);

        if (gravityMultiplier > 1f)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }
    }

    private void RespawnNow()
    {
        rb.velocity = Vector3.zero;
        rb.position = startPosition;
        rb.rotation = startRotation;
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
        if (movementLocked)
        {
            input = Vector3.zero;
            isSprinting = false;
        }
    }

    public void SetHardFreeze(bool frozen)
    {
        hardFreeze = frozen;
        if (hardFreeze)
        {
            input = Vector3.zero;
            isSprinting = false;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
    }
}
