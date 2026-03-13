using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    private const string MouseSensitivityPrefKey = "MouseSensitivity";
    private static PauseMenuManager instance;

    public static bool IsGamePaused { get; private set; }

    private Canvas canvas;
    private GameObject panel;
    private GameObject dimmer;
    private Button pauseButton;
    private Slider sensitivitySlider;
    private Text sensitivityValueText;
    private FirstPersonControllerSimple fpsController;
    private float previousTimeScale = 1f;
    private bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            instance.RebuildIfNeeded();
            return;
        }

        GameObject root = new GameObject("PauseMenuManager");
        instance = root.AddComponent<PauseMenuManager>();
        DontDestroyOnLoad(root);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        RebuildIfNeeded();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
            IsGamePaused = false;
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        ReacquireControllerIfNeeded();

        if (IsEscapePressed())
        {
            if (IsGamePaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private static bool IsEscapePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebuildIfNeeded();
        ForceHiddenIfNotPaused();
        ReacquireControllerIfNeeded();
        ApplySavedSensitivity();
    }

    private void RebuildIfNeeded()
    {
        if (!initialized)
        {
            BuildUi();
            initialized = true;
        }

        if (canvas == null)
        {
            BuildUi();
        }

        ForceHiddenIfNotPaused();
        EnsureEventSystem();
    }

    private void BuildUi()
    {
        if (canvas != null)
        {
            Destroy(canvas.gameObject);
        }

        GameObject canvasObject = new GameObject("PauseMenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 12000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        dimmer = CreateUiObject("Dimmer", canvas.transform);
        Image dimmerImage = dimmer.AddComponent<Image>();
        dimmerImage.color = new Color(0.03f, 0.04f, 0.08f, 0.82f);
        StretchToFullscreen(dimmer.GetComponent<RectTransform>());

        panel = CreateUiObject("Panel", canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(640f, 560f);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.10f, 0.13f, 0.19f, 0.97f);

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = new Color(0.27f, 0.78f, 0.94f, 0.45f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateText("PauseTitle", panel.transform, "PAUSED", 38, FontStyle.Bold, TextAnchor.MiddleCenter,
            new Color(0.93f, 0.97f, 1f, 1f), new Vector2(0f, 210f), new Vector2(420f, 56f));
        CreateText("PauseSubtitle", panel.transform, "Adjust controls and continue when ready.", 18, FontStyle.Normal, TextAnchor.MiddleCenter,
            new Color(0.70f, 0.80f, 0.90f, 1f), new Vector2(0f, 168f), new Vector2(500f, 32f));

        CreateSensitivitySection(panel.transform);

        Button resumeButton = CreateButton(panel.transform, "ResumeButton", "Resume", new Vector2(0f, -60f), new Color(0.18f, 0.63f, 0.43f, 1f));
        resumeButton.onClick.AddListener(ResumeGame);

        Button saveButton = CreateButton(panel.transform, "SaveButton", "Save Settings", new Vector2(0f, -140f), new Color(0.14f, 0.44f, 0.80f, 1f));
        saveButton.onClick.AddListener(SaveSettings);

        Button quitButton = CreateButton(panel.transform, "QuitButton", "Quit Game", new Vector2(0f, -220f), new Color(0.72f, 0.24f, 0.26f, 1f));
        quitButton.onClick.AddListener(QuitGame);

        pauseButton = CreateButton(canvas.transform, "PauseToggleButton", "II", new Vector2(-70f, -70f), new Color(0.12f, 0.4f, 0.8f, 0.9f));
        RectTransform pauseRect = pauseButton.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(1f, 1f);
        pauseRect.anchorMax = new Vector2(1f, 1f);
        pauseRect.pivot = new Vector2(1f, 1f);
        pauseRect.sizeDelta = new Vector2(82f, 82f);
        Text pauseLabel = pauseButton.GetComponentInChildren<Text>();
        if (pauseLabel != null)
        {
            pauseLabel.text = "II";
            pauseLabel.fontSize = 34;
        }
        pauseButton.onClick.AddListener(PauseGame);

        canvasObject.SetActive(true);
        SetMenuVisible(false);
        ApplySavedSensitivity();
    }

    private void CreateSensitivitySection(Transform parent)
    {
        GameObject card = CreateUiObject("SensitivityCard", parent);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(540f, 150f);
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.anchoredPosition = new Vector2(0f, 45f);

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.15f, 0.18f, 0.25f, 0.96f);

        CreateText("SensitivityLabel", card.transform, "Mouse Sensitivity", 24, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Color(0.92f, 0.96f, 1f, 1f), new Vector2(-160f, 42f), new Vector2(280f, 36f));
        sensitivityValueText = CreateText("SensitivityValue", card.transform, "1.80", 22, FontStyle.Bold, TextAnchor.MiddleRight,
            new Color(0.42f, 0.88f, 0.98f, 1f), new Vector2(160f, 42f), new Vector2(120f, 36f));

        GameObject sliderObject = CreateUiObject("SensitivitySlider", card.transform);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(440f, 36f);
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.pivot = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(0f, -18f);

        GameObject background = CreateUiObject("Background", sliderObject.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        Image backgroundImage = background.AddComponent<Image>();
        backgroundImage.color = new Color(0.24f, 0.28f, 0.36f, 1f);

        GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f);
        fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = CreateUiObject("Fill", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.30f, 0.84f, 0.97f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = CreateUiObject("Handle Slide Area", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = CreateUiObject("Handle", handleArea.transform);
        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.96f, 0.98f, 1f, 1f);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(22f, 36f);

        sensitivitySlider = sliderObject.AddComponent<Slider>();
        sensitivitySlider.minValue = 0.2f;
        sensitivitySlider.maxValue = 6f;
        sensitivitySlider.wholeNumbers = false;
        sensitivitySlider.targetGraphic = handleImage;
        sensitivitySlider.fillRect = fillRect;
        sensitivitySlider.handleRect = handleRect;
        sensitivitySlider.direction = Slider.Direction.LeftToRight;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);

        CreateText("SensitivityHint", card.transform, "Esc opens this menu. Save stores the current mouse sensitivity.", 15, FontStyle.Normal, TextAnchor.MiddleLeft,
            new Color(0.63f, 0.72f, 0.81f, 1f), new Vector2(0f, -54f), new Vector2(440f, 26f));
    }

    private void SetMenuVisible(bool visible)
    {
        if (dimmer != null)
        {
            dimmer.SetActive(visible);
        }

        if (panel != null)
        {
            panel.SetActive(visible);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(!visible);
        }

        if (canvas != null && !canvas.gameObject.activeSelf)
        {
            canvas.gameObject.SetActive(true);
        }
    }

    private void PauseGame()
    {
        RebuildIfNeeded();
        ReacquireControllerIfNeeded();

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsGamePaused = true;

        SetMenuVisible(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = fpsController != null ? fpsController.GetMouseSensitivity() : LoadSavedSensitivity();
            UpdateSensitivityLabel(sensitivitySlider.value);
        }
    }

    private void ResumeGame()
    {
        SetMenuVisible(false);

        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        IsGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ForceHiddenIfNotPaused()
    {
        if (!IsGamePaused)
        {
            SetMenuVisible(false);
        }
    }

    private void SaveSettings()
    {
        float value = sensitivitySlider != null ? sensitivitySlider.value : LoadSavedSensitivity();
        PlayerPrefs.SetFloat(MouseSensitivityPrefKey, value);
        PlayerPrefs.Save();

        if (fpsController != null)
        {
            fpsController.SetMouseSensitivity(value);
        }

        UpdateSensitivityLabel(value);
    }

    private void QuitGame()
    {
        SaveSettings();
        Time.timeScale = 1f;
        IsGamePaused = false;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnSensitivityChanged(float value)
    {
        UpdateSensitivityLabel(value);
        ReacquireControllerIfNeeded();
        if (fpsController != null)
        {
            fpsController.SetMouseSensitivity(value);
        }
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("0.00");
        }
    }

    private void ReacquireControllerIfNeeded()
    {
        if (fpsController == null)
        {
            fpsController = FindObjectOfType<FirstPersonControllerSimple>();
        }
    }

    private void ApplySavedSensitivity()
    {
        float value = LoadSavedSensitivity();
        if (sensitivitySlider != null)
        {
            sensitivitySlider.SetValueWithoutNotify(value);
        }

        UpdateSensitivityLabel(value);
        ReacquireControllerIfNeeded();
        if (fpsController != null)
        {
            fpsController.SetMouseSensitivity(value);
        }
    }

    private static float LoadSavedSensitivity()
    {
        return PlayerPrefs.GetFloat(MouseSensitivityPrefKey, 1.8f);
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            StandaloneInputModule legacyModule = existing.GetComponent<StandaloneInputModule>();
            if (legacyModule != null)
            {
                Object.Destroy(legacyModule);
            }

            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void StretchToFullscreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Text CreateText(string name, Transform parent, string content, int fontSize, FontStyle style,
        TextAnchor alignment, Color color, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = CreateUiObject(name, parent);
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        return text;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 position, Color color)
    {
        GameObject buttonObject = CreateUiObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(420f, 58f);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.14f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 0.8f);
        button.colors = colors;

        CreateText(name + "Label", buttonObject.transform, label, 23, FontStyle.Bold, TextAnchor.MiddleCenter,
            Color.white, Vector2.zero, new Vector2(360f, 36f));

        return button;
    }
}
