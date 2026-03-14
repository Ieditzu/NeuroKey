using UnityEngine;

/// <summary>
/// Minimal bottom-right overlay that appears after a coin is collected.
/// Lets you set the bean's jump power and toggle whether a target box can be pushed.
/// </summary>
public class PickupUIController : MonoBehaviour
{
    public static PickupUIController Instance { get; private set; }

    [SerializeField] private BeanController beanPlayer;
    [SerializeField] private FirstPersonControllerSimple fpsPlayer;
    [SerializeField] private Rigidbody targetBox;

    [SerializeField] private bool showOnStart = false;

    private bool visible;
    private string jumpInput = "0";
    private bool boxPushable = false;
    private string boxInput = "false";

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
            targetBox.isKinematic = true; // off by default
            boxPushable = false;
            boxInput = "false";
        }
    }

    public void Show()
    {
        visible = true;
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
                if (beanPlayer == null) beanPlayer = FindObjectOfType<BeanController>();
                if (fpsPlayer == null) fpsPlayer = FindObjectOfType<FirstPersonControllerSimple>();

                if (beanPlayer != null) beanPlayer.SetJumpForce(jp);
                if (fpsPlayer != null) fpsPlayer.SetJumpVelocity(jp);
            }
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
                if (targetBox != null)
                {
                    targetBox.isKinematic = !boxPushable;
                }
            }
        }

        GUILayout.EndArea();

        if (enterPressed)
        {
            GUI.FocusControl(null); // exit any focused field when Enter is pressed
        }
    }
}
