using UnityEngine;

/// <summary>
/// Minimal bottom-right overlay that appears after a coin is collected.
/// Lets you set the bean's jump power and toggle whether a target box can be pushed.
/// </summary>
public class PickupUIController : MonoBehaviour
{
    private const float MaxJumpValue = 10f;

    public static PickupUIController Instance { get; private set; }

    [SerializeField] private BeanController beanPlayer;
    [SerializeField] private FirstPersonControllerSimple fpsPlayer;
    [SerializeField] private Rigidbody targetBox;

    [SerializeField] private bool showOnStart = false;
    [SerializeField] private float pushableBoxMass = 8f;
    [SerializeField] private float pushableBoxDrag = 3.5f;
    [SerializeField] private float pushableBoxAngularDrag = 2f;
    [SerializeField] private float boxRespawnY = -20f;

    private bool visible;
    private string jumpInput = "0";
    private bool boxPushable = false;
    private string boxInput = "false";
    private string jumpValidationMessage = string.Empty;
    private CoinRotator activeCoin;
    private Vector3 targetBoxSpawnPosition;
    private Quaternion targetBoxSpawnRotation;
    private float targetBoxOriginalMass;
    private float targetBoxOriginalDrag;
    private float targetBoxOriginalAngularDrag;
    private bool targetBoxStateCaptured;
    private float defaultBeanJumpValue;
    private float defaultFpsJumpValue;
    private bool defaultsCaptured;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        visible = showOnStart;
        TryAutoAssign();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureExists()
    {
        if (Instance == null)
        {
            var go = new GameObject("PickupUIController");
            go.AddComponent<PickupUIController>();
        }
    }

    private void TryAutoAssign()
    {
        if (beanPlayer == null)
        {
            beanPlayer = FindObjectOfType<BeanController>();
        }

        if (fpsPlayer == null)
        {
            fpsPlayer = FindObjectOfType<FirstPersonControllerSimple>();
        }

        if (targetBox == null)
        {
            foreach (var rb in FindObjectsOfType<Rigidbody>())
            {
                string lower = rb.gameObject.name.ToLower();
                if (lower.Contains("box") || lower.Contains("movable"))
                {
                    targetBox = rb;
                    break;
                }
            }
        }

        if (targetBox != null)
        {
            CacheTargetBoxState();
            RestoreDefaults();
        }

        if (!defaultsCaptured)
        {
            if (beanPlayer != null)
            {
                defaultBeanJumpValue = beanPlayer.GetJumpForce();
            }

            if (fpsPlayer != null)
            {
                defaultFpsJumpValue = fpsPlayer.GetJumpVelocity();
            }

            defaultsCaptured = beanPlayer != null || fpsPlayer != null;
        }
    }

    private void Update()
    {
        if (targetBox == null)
        {
            TryAutoAssign();
        }

        if (boxPushable && targetBox.position.y < boxRespawnY)
        {
            RespawnTargetBox();
        }

        if (visible && Input.GetKeyDown(KeyCode.L))
        {
            ExitMode();
        }
    }

    private void CacheTargetBoxState()
    {
        if (targetBox == null || targetBoxStateCaptured)
        {
            return;
        }

        targetBoxSpawnPosition = targetBox.position;
        targetBoxSpawnRotation = targetBox.rotation;
        targetBoxOriginalMass = targetBox.mass;
        targetBoxOriginalDrag = targetBox.drag;
        targetBoxOriginalAngularDrag = targetBox.angularDrag;
        targetBoxStateCaptured = true;
    }

    private void ApplyTargetBoxPhysics()
    {
        if (targetBox == null)
        {
            return;
        }

        CacheTargetBoxState();

        targetBox.isKinematic = !boxPushable;
        if (boxPushable)
        {
            targetBox.mass = Mathf.Max(targetBoxOriginalMass, pushableBoxMass);
            targetBox.drag = Mathf.Max(targetBoxOriginalDrag, pushableBoxDrag);
            targetBox.angularDrag = Mathf.Max(targetBoxOriginalAngularDrag, pushableBoxAngularDrag);
        }
        else
        {
            targetBox.mass = targetBoxOriginalMass;
            targetBox.drag = targetBoxOriginalDrag;
            targetBox.angularDrag = targetBoxOriginalAngularDrag;
        }
    }

    private void RespawnTargetBox()
    {
        if (targetBox == null)
        {
            return;
        }

        CacheTargetBoxState();
        targetBox.velocity = Vector3.zero;
        targetBox.angularVelocity = Vector3.zero;
        targetBox.position = targetBoxSpawnPosition;
        targetBox.rotation = targetBoxSpawnRotation;
        targetBox.Sleep();
    }

    public void Show(CoinRotator coin)
    {
        activeCoin = coin;
        RestoreDefaults();
        jumpInput = "0";

        if (beanPlayer == null)
        {
            beanPlayer = FindObjectOfType<BeanController>();
        }

        if (fpsPlayer == null)
        {
            fpsPlayer = FindObjectOfType<FirstPersonControllerSimple>();
        }

        if (beanPlayer != null)
        {
            beanPlayer.SetJumpForce(0f);
        }

        if (fpsPlayer != null)
        {
            fpsPlayer.SetJumpVelocity(0f);
        }

        visible = true;
    }

    private void RestoreDefaults()
    {
        if (beanPlayer == null)
        {
            beanPlayer = FindObjectOfType<BeanController>();
        }

        if (fpsPlayer == null)
        {
            fpsPlayer = FindObjectOfType<FirstPersonControllerSimple>();
        }

        if (!defaultsCaptured)
        {
            if (beanPlayer != null)
            {
                defaultBeanJumpValue = beanPlayer.GetJumpForce();
            }

            if (fpsPlayer != null)
            {
                defaultFpsJumpValue = fpsPlayer.GetJumpVelocity();
            }

            defaultsCaptured = beanPlayer != null || fpsPlayer != null;
        }

        if (beanPlayer != null)
        {
            beanPlayer.SetJumpForce(defaultBeanJumpValue);
            jumpInput = defaultBeanJumpValue.ToString("0.###");
        }
        else if (fpsPlayer != null)
        {
            jumpInput = defaultFpsJumpValue.ToString("0.###");
        }

        if (fpsPlayer != null)
        {
            fpsPlayer.SetJumpVelocity(defaultFpsJumpValue);
            if (beanPlayer == null)
            {
                jumpInput = defaultFpsJumpValue.ToString("0.###");
            }
        }

        boxPushable = false;
        boxInput = "false";
        jumpValidationMessage = string.Empty;
        ApplyTargetBoxPhysics();
        RespawnTargetBox();
    }

    private void ExitMode()
    {
        RestoreDefaults();
        visible = false;

        if (activeCoin != null)
        {
            activeCoin.ResetPickup();
            activeCoin = null;
        }
    }

    private void OnGUI()
    {
        if (!visible)
        {
            return;
        }

        const float width = 240f;
        const float height = 150f;
        Rect rect = new Rect(Screen.width - width - 16f, Screen.height - height - 16f, width, height);
        GUI.Box(rect, GUIContent.none);

        GUILayout.BeginArea(new Rect(rect.x + 10f, rect.y + 10f, rect.width - 20f, rect.height - 20f));

        bool enterPressed = Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return;

        GUI.SetNextControlName("JumpField");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"jumpVelocity =", GUILayout.Width(100f));
        jumpInput = GUILayout.TextField(jumpInput, 12);
        GUILayout.EndHorizontal();

        if (float.TryParse(jumpInput, out float jp))
        {
            bool applyNow = enterPressed ? GUI.GetNameOfFocusedControl() == "JumpField" : true;
            if (applyNow)
            {
                if (jp > MaxJumpValue)
                {
                    jumpValidationMessage = "max value 10";
                }
                else
                {
                    jumpValidationMessage = string.Empty;

                    if (beanPlayer == null) beanPlayer = FindObjectOfType<BeanController>();
                    if (fpsPlayer == null) fpsPlayer = FindObjectOfType<FirstPersonControllerSimple>();

                    if (beanPlayer != null) beanPlayer.SetJumpForce(jp);
                    if (fpsPlayer != null) fpsPlayer.SetJumpVelocity(jp);
                }
            }
        }

        if (!string.IsNullOrEmpty(jumpValidationMessage))
        {
            GUILayout.Label(jumpValidationMessage);
        }

        GUILayout.Space(6f);
        GUI.SetNextControlName("BoxField");
        GUILayout.BeginHorizontal();
        GUILayout.Label("boxRigidbody =", GUILayout.Width(100f));
        boxInput = GUILayout.TextField(boxInput, 8).ToLowerInvariant();
        GUILayout.EndHorizontal();
        if (enterPressed && GUI.GetNameOfFocusedControl() == "BoxField")
        {
            bool parsed = boxInput == "true";
            if (parsed != boxPushable)
            {
                boxPushable = parsed;
                ApplyTargetBoxPhysics();
            }
        }

        GUILayout.EndArea();

        if (enterPressed)
        {
            GUI.FocusControl(null); // exit any focused field when Enter is pressed
        }
    }
}
