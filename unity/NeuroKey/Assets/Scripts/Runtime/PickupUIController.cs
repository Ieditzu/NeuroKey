using UnityEngine;

/// <summary>
/// Minimal bottom-right overlay that appears after a coin is collected.
/// Lets you set the bean's jump power and toggle whether a target box can be pushed.
/// </summary>
public class PickupUIController : MonoBehaviour
{
    public static PickupUIController Instance { get; private set; }

    [SerializeField] private BeanController player;
    [SerializeField] private Rigidbody targetBox;

    [SerializeField] private bool showOnStart = false;

    private bool visible;
    private string jumpInput = "0";
    private bool boxPushable;

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
        if (player == null)
        {
            player = FindObjectOfType<BeanController>();
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
            boxPushable = !targetBox.isKinematic;
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

        const float width = 200f;
        const float height = 120f;
        Rect rect = new Rect(Screen.width - width - 16f, Screen.height - height - 16f, width, height);
        GUI.Box(rect, "Pickup Settings");

        GUILayout.BeginArea(new Rect(rect.x + 10f, rect.y + 25f, rect.width - 20f, rect.height - 35f));

        GUILayout.Label($"jumpPower = {jumpInput}");
        jumpInput = GUILayout.TextField(jumpInput, 8);
        if (GUILayout.Button("Apply jumpPower") && player != null)
        {
            if (float.TryParse(jumpInput, out float jp))
            {
                player.SetJumpForce(jp);
            }
        }

        bool newPushable = GUILayout.Toggle(boxPushable, "boxRigidbody pushable");
        if (newPushable != boxPushable)
        {
            boxPushable = newPushable;
            if (targetBox != null)
            {
                targetBox.isKinematic = !boxPushable;
            }
        }

        GUILayout.EndArea();
    }
}
