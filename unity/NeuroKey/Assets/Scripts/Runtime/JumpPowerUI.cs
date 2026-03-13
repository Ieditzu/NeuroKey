using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Adds a small UI in the top-right to expose jump power and cursor toggle.
/// Tab unlocks the cursor (stops camera look) so the slider can be adjusted.
/// </summary>
public class JumpPowerUI : MonoBehaviour
{
    private FirstPersonControllerSimple player;
    private Canvas canvas;
    private GameObject panel;
    private Slider slider;
    private Text valueLabel;
    private bool popupUnlocked;

    private const float ShowThresholdX = 150f; // after crossing bridge toward island two

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        GameObject go = new GameObject("JumpPowerUI");
        DontDestroyOnLoad(go);
        go.AddComponent<JumpPowerUI>();
    }

    private void Awake()
    {
        EnsureEventSystem();
        CreateCanvas();
        BuildPanel();
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<FirstPersonControllerSimple>();
            if (player != null)
            {
                slider.value = player.GetJumpPower();
                UpdateLabel(slider.value);
            }
        }

        if (!popupUnlocked && player != null && player.transform.position.x > ShowThresholdX)
        {
            popupUnlocked = true;
            panel.SetActive(true);
        }

        if (player != null)
        {
            // Keep the label synced even if jump power changes elsewhere.
            UpdateLabel(player.GetJumpPower());
        }
    }

    private void CreateCanvas()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 2000;
        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();
    }

    private void BuildPanel()
    {
        panel = new GameObject("JumpPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-18f, -18f);
        rt.sizeDelta = new Vector2(240f, 88f);

        Image bg = panel.GetComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.35f);
        bg.raycastTarget = true;

        GameObject labelObj = CreateText("JumpLabel", panel.transform, "jumpPower = 0.0", 18, TextAnchor.UpperRight);
        RectTransform lrt = labelObj.GetComponent<RectTransform>();
        lrt.anchorMin = new Vector2(0f, 0.5f);
        lrt.anchorMax = new Vector2(1f, 1f);
        lrt.offsetMin = new Vector2(10f, -6f);
        lrt.offsetMax = new Vector2(-10f, -8f);
        valueLabel = labelObj.GetComponent<Text>();

        GameObject sliderObj = new GameObject("JumpSlider", typeof(RectTransform), typeof(Slider));
        sliderObj.transform.SetParent(panel.transform, false);
        RectTransform srt = sliderObj.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0f, 0f);
        srt.anchorMax = new Vector2(1f, 0.5f);
        srt.offsetMin = new Vector2(12f, 10f);
        srt.offsetMax = new Vector2(-12f, 5f);

        slider = sliderObj.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 8f;
        slider.value = 0f;
        slider.onValueChanged.AddListener(OnSliderChanged);

        // Add simple visuals to the slider (background + fill + handle)
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fart = fillArea.GetComponent<RectTransform>();
        fart.anchorMin = new Vector2(0f, 0.25f);
        fart.anchorMax = new Vector2(1f, 0.75f);
        fart.offsetMin = new Vector2(10f, 0f);
        fart.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform frt = fill.GetComponent<RectTransform>();
        frt.anchorMin = new Vector2(0f, 0f);
        frt.anchorMax = new Vector2(1f, 1f);
        frt.offsetMin = Vector2.zero;
        frt.offsetMax = Vector2.zero;
        Image fillImg = fill.GetComponent<Image>();
        fillImg.color = new Color(0.18f, 0.62f, 0.92f, 0.85f);

        slider.fillRect = frt;

        GameObject background = new GameObject("Background", typeof(RectTransform), typeof(Image));
        background.transform.SetParent(sliderObj.transform, false);
        RectTransform brt = background.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0f, 0.25f);
        brt.anchorMax = new Vector2(1f, 0.75f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = Vector2.zero;
        background.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        background.transform.SetAsFirstSibling();

        GameObject handle = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handle.transform.SetParent(fillArea.transform, false);
        RectTransform hrt = handle.GetComponent<RectTransform>();
        hrt.sizeDelta = new Vector2(16f, 26f);
        Image handleImg = handle.GetComponent<Image>();
        handleImg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
        slider.handleRect = hrt;
        slider.targetGraphic = handleImg;
        slider.transition = Selectable.Transition.ColorTint;

        panel.SetActive(false);
    }

    private GameObject CreateText(string name, Transform parent, string text, int size, TextAnchor anchor)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        Text t = go.GetComponent<Text>();
        t.text = text;
        t.fontSize = size;
        t.alignment = anchor;
        t.color = Color.white;
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return go;
    }

    private void OnSliderChanged(float value)
    {
        if (player != null)
        {
            player.SetJumpPower(value);
        }
        UpdateLabel(value);
    }

    private void UpdateLabel(float value)
    {
        if (valueLabel != null)
        {
            valueLabel.text = $"jumpPower = {value:0.0}";
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject es = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        DontDestroyOnLoad(es);
    }
}
