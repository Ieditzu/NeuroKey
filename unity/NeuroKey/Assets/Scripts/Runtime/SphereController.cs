using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SphereController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float rollResponsiveness = 18f;
    [SerializeField] private float gravityMultiplier = 3f;
    [SerializeField] private float fallYThreshold = -6f;

    private Rigidbody rb;
    private Vector3 input;
    private float radius;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private int timesFallen;
    private bool movementLocked;
    private bool hardFreeze;
    private bool isSprinting;

    /// <summary>
    /// Allows disabling keyboard steering (WASD/arrow keys) while keeping mobile/touch input active.
    /// </summary>
    public static bool KeyboardInputEnabled { get; set; } = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (GetComponent<PlayerSkinController>() == null)
        {
            gameObject.AddComponent<PlayerSkinController>();
        }

        RefreshRollingRadius();

        startPosition = transform.position;
        startRotation = transform.rotation;
        timesFallen = 0;
        FallCounterDisplay.SetCount(timesFallen);
    }

    public void RefreshRollingRadius()
    {
        var sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            radius = sphereCollider.radius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);
            return;
        }

        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Vector3 scaledSize = Vector3.Scale(boxCollider.size, transform.localScale);
            radius = Mathf.Max(scaledSize.x, scaledSize.y, scaledSize.z) * 0.5f;
            return;
        }

        radius = 0.5f;
    }

    private void Update()
    {
        if (hardFreeze)
        {
            input = Vector3.zero;
            isSprinting = false;
            return;
        }

        isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

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
        if (hardFreeze)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        if (transform.position.y < fallYThreshold)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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
        Vector3 horizontalVelocity = input * currentSpeed;
        rb.velocity = new Vector3(horizontalVelocity.x, rb.velocity.y, horizontalVelocity.z);

        // Apply extra downward force so the ball feels heavier and sticks to ground better.
        if (gravityMultiplier > 1f)
        {
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        Vector3 planarVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if (planarVelocity.sqrMagnitude > 0.0001f)
        {
            Vector3 moveDir = planarVelocity.normalized;
            Vector3 rollAxis = Vector3.Cross(Vector3.up, moveDir);
            float angularSpeed = planarVelocity.magnitude / Mathf.Max(radius, 0.0001f);
            Vector3 targetAngularVelocity = rollAxis * angularSpeed;
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, targetAngularVelocity, rollResponsiveness * Time.fixedDeltaTime);
        }
        else
        {
            rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, rollResponsiveness * Time.fixedDeltaTime);
        }
    }

    private void RespawnNow()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
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
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
