using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NeuroKey.Network;
using System.Collections.Generic;

public class PauseMenuManager : MonoBehaviour
{
    private const string MouseSensitivityPrefKey = "MouseSensitivity";
    private static PauseMenuManager instance;

    public static bool IsGamePaused { get; private set; }

    private Canvas canvas;
    private GameObject panel;
    private Slider sensitivitySlider;
    private Text sensitivityValueText;
    private FirstPersonControllerSimple fpsController;
    private float previousTimeScale = 1f;
    private bool initialized;

    private Text qrStatusText;
    private Button qrButton;
    private long loggedInChildId = -1;
    private string loggedInChildName = "";
    private int loggedInChildPoints = 0;

    private GameObject taskListContainer;
    private List<FetchTasksResponsePacket.TaskDto> availableTasks = new List<FetchTasksResponsePacket.TaskDto>();

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
        root.AddComponent<GameClient>(); // Add network client
        DontDestroyOnLoad(root);
    }

    private void Start()
    {
        if (GameClient.Instance != null)
        {
            GameClient.Instance.OnPacketReceived += OnPacketReceived;
            _ = GameClient.Instance.Connect();
        }
    }

    private void OnPacketReceived(Packet packet)
    {
        if (packet is QRLoginResponsePacket qrResp)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (qrStatusText != null)
                    qrStatusText.text = "TOKEN: " + qrResp.Token;
            });
        }
        else if (packet is ChildAuthResponsePacket authResp)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                if (authResp.Success)
                {
                    loggedInChildId = authResp.ChildId;
                    loggedInChildName = authResp.ChildName;
                    
                    // Fetch stats and tasks
                    GameClient.Instance.SendPacket(new FetchChildStatsPacket());
                    GameClient.Instance.SendPacket(new FetchTasksPacket());

                    if (qrButton != null) qrButton.interactable = false;
                }
                else
                {
                    if (qrStatusText != null) qrStatusText.text = "LOGIN FAILED";
                }
            });
        }
        else if (packet is FetchChildStatsResponsePacket statsResp)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                loggedInChildPoints = statsResp.TotalPoints;
                if (qrStatusText != null)
                    qrStatusText.text = statsResp.Name + " | " + statsResp.TotalPoints + " pts";
            });
        }
        else if (packet is FetchTasksResponsePacket tasksResp)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                availableTasks = tasksResp.Tasks;
                RebuildTaskList();
            });
        }
        else if (packet is ActionResponsePacket actionResp)
        {
            if (actionResp.RequestPacketId == 8 && actionResp.Success) // CompleteTask
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    GameClient.Instance.SendPacket(new FetchChildStatsPacket()); // Refresh points
                });
            }
        }
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
            if (IsGamePaused) ResumeGame();
            else PauseGame();
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
        if (canvas != null) Destroy(canvas.gameObject);

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

        GameObject dimmer = CreateUiObject("Dimmer", canvas.transform);
        Image dimmerImage = dimmer.AddComponent<Image>();
        dimmerImage.color = new Color(0.03f, 0.04f, 0.08f, 0.82f);
        StretchToFullscreen(dimmer.GetComponent<RectTransform>());

        panel = CreateUiObject("Panel", canvas.transform);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(640f, 750f);
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
            new Color(0.93f, 0.97f, 1f, 1f), new Vector2(0f, 310f), new Vector2(420f, 56f));
        
        CreateSensitivitySection(panel.transform);

        GameObject qrSection = CreateUiObject("QrSection", panel.transform);
        RectTransform qrRect = qrSection.GetComponent<RectTransform>();
        qrRect.sizeDelta = new Vector2(540f, 100f);
        qrRect.anchoredPosition = new Vector2(0f, 100f);

        qrStatusText = CreateText("QrStatus", qrSection.transform, 
            loggedInChildId == -1 ? "Not logged in" : loggedInChildName + " | " + loggedInChildPoints + " pts", 
            16, FontStyle.Italic, TextAnchor.MiddleCenter, Color.white, new Vector2(0, 30), new Vector2(500, 30));

        qrButton = CreateButton(qrSection.transform, "QrButton", "Generate QR Login", new Vector2(0f, -10f), new Color(0.4f, 0.2f, 0.8f, 1f));
        qrButton.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 40f);
        qrButton.onClick.AddListener(GenerateQrLogin);
        if (loggedInChildId != -1) qrButton.interactable = false;

        // Tasks
        GameObject taskHeader = CreateUiObject("TaskHeader", panel.transform);
        RectTransform thRect = taskHeader.GetComponent<RectTransform>();
        thRect.anchoredPosition = new Vector2(0, -10);
        CreateText("TaskTitle", thRect, "AVAILABLE TASKS", 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.cyan, Vector2.zero, new Vector2(400, 30));

        taskListContainer = CreateUiObject("TaskList", panel.transform);
        RectTransform tlRect = taskListContainer.GetComponent<RectTransform>();
        tlRect.sizeDelta = new Vector2(500, 200);
        tlRect.anchoredPosition = new Vector2(0, -130);
        Image tlImg = taskListContainer.AddComponent<Image>();
        tlImg.color = new Color(0,0,0,0.2f);

        RebuildTaskList();

        Button resumeButton = CreateButton(panel.transform, "ResumeButton", "Resume", new Vector2(0f, -260f), new Color(0.18f, 0.63f, 0.43f, 1f));
        resumeButton.onClick.AddListener(ResumeGame);

        Button saveButton = CreateButton(panel.transform, "SaveButton", "Save Settings", new Vector2(0f, -320f), new Color(0.14f, 0.44f, 0.80f, 1f));
        saveButton.onClick.AddListener(SaveSettings);

        Button quitButton = CreateButton(panel.transform, "QuitButton", "Quit Game", new Vector2(0f, -380f), new Color(0.72f, 0.24f, 0.26f, 1f));
        quitButton.onClick.AddListener(QuitGame);

        canvasObject.SetActive(false);
        ApplySavedSensitivity();
    }

    private void RebuildTaskList()
    {
        if (taskListContainer == null) return;
        foreach (Transform child in taskListContainer.transform) Destroy(child.gameObject);

        float y = 80;
        foreach (var task in availableTasks)
        {
            GameObject item = CreateUiObject("TaskItem_" + task.Id, taskListContainer.transform);
            RectTransform iRect = item.GetComponent<RectTransform>();
            iRect.sizeDelta = new Vector2(480, 40);
            iRect.anchoredPosition = new Vector2(0, y);

            CreateText("Label", item.transform, task.Title + " (" + task.Points + " pts)", 14, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white, new Vector2(-100, 0), new Vector2(300, 30));
            
            Button completeBtn = CreateButton(item.transform, "Btn", "Complete", new Vector2(160, 0), new Color(0.2f, 0.5f, 0.2f));
            completeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 30);
            completeBtn.GetComponentInChildren<Text>().fontSize = 12;
            long tid = task.Id;
            completeBtn.onClick.AddListener(() => {
                if (loggedInChildId != -1)
                    GameClient.Instance.SendPacket(new CompleteTaskPacket(loggedInChildId, tid));
            });
            y -= 45;
        }
    }

    private void CreateSensitivitySection(Transform parent)
    {
        GameObject card = CreateUiObject("SensitivityCard", parent);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(540f, 150f);
        cardRect.anchoredPosition = new Vector2(0f, 210f);

        Image cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.15f, 0.18f, 0.25f, 0.96f);

        CreateText("SensitivityLabel", card.transform, "Mouse Sensitivity", 24, FontStyle.Bold, TextAnchor.MiddleLeft,
            new Color(0.92f, 0.96f, 1f, 1f), new Vector2(-160f, 42f), new Vector2(280f, 36f));
        sensitivityValueText = CreateText("SensitivityValue", card.transform, "1.80", 22, FontStyle.Bold, TextAnchor.MiddleRight,
            new Color(0.42f, 0.88f, 0.98f, 1f), new Vector2(160f, 42f), new Vector2(120f, 36f));

        GameObject sliderObject = CreateUiObject("SensitivitySlider", card.transform);
        RectTransform sliderRect = sliderObject.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(440f, 36f);
        sliderRect.anchoredPosition = new Vector2(0f, -18f);

        GameObject background = CreateUiObject("Background", sliderObject.transform);
        RectTransform backgroundRect = background.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero; backgroundRect.offsetMax = Vector2.zero;
        background.AddComponent<Image>().color = new Color(0.24f, 0.28f, 0.36f, 1f);

        GameObject fillArea = CreateUiObject("Fill Area", sliderObject.transform);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f); fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f); fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject fill = CreateUiObject("Fill", fillArea.transform);
        Image fillImage = fill.AddComponent<Image>();
        fillImage.color = new Color(0.30f, 0.84f, 0.97f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;

        GameObject handleArea = CreateUiObject("Handle Slide Area", sliderObject.transform);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero; handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f); handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject handle = CreateUiObject("Handle", handleArea.transform);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = new Color(0.96f, 0.98f, 1f, 1f);
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(22f, 36f);

        sensitivitySlider = sliderObject.AddComponent<Slider>();
        sensitivitySlider.minValue = 0.2f; sensitivitySlider.maxValue = 6f;
        sensitivitySlider.targetGraphic = handleImg;
        sensitivitySlider.fillRect = fillRect; sensitivitySlider.handleRect = handleRect;
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void GenerateQrLogin()
    {
        if (GameClient.Instance != null && GameClient.Instance.IsConnected)
        {
            qrStatusText.text = "Generating...";
            GameClient.Instance.SendPacket(new GenerateQRLoginPacket());
        }
        else if (GameClient.Instance != null)
        {
            qrStatusText.text = "Connecting...";
            _ = GameClient.Instance.Connect();
        }
    }

    private void PauseGame()
    {
        RebuildIfNeeded();
        ReacquireControllerIfNeeded();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsGamePaused = true;
        if (canvas != null) canvas.gameObject.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResumeGame()
    {
        if (canvas != null) canvas.gameObject.SetActive(false);
        Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        IsGamePaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ForceHiddenIfNotPaused() { if (!IsGamePaused && canvas != null) canvas.gameObject.SetActive(false); }

    private void SaveSettings()
    {
        float val = sensitivitySlider != null ? sensitivitySlider.value : PlayerPrefs.GetFloat(MouseSensitivityPrefKey, 1.8f);
        PlayerPrefs.SetFloat(MouseSensitivityPrefKey, val);
        PlayerPrefs.Save();
        if (fpsController != null) fpsController.SetMouseSensitivity(val);
        UpdateSensitivityLabel(val);
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

    private void OnSensitivityChanged(float v) { UpdateSensitivityLabel(v); if (fpsController != null) fpsController.SetMouseSensitivity(v); }

    private void UpdateSensitivityLabel(float v) { if (sensitivityValueText != null) sensitivityValueText.text = v.ToString("0.00"); }

    private void ReacquireControllerIfNeeded() { if (fpsController == null) fpsController = FindObjectOfType<FirstPersonControllerSimple>(); }

    private void ApplySavedSensitivity()
    {
        float v = PlayerPrefs.GetFloat(MouseSensitivityPrefKey, 1.8f);
        if (sensitivitySlider != null) sensitivitySlider.SetValueWithoutNotify(v);
        UpdateSensitivityLabel(v);
        if (fpsController != null) fpsController.SetMouseSensitivity(v);
    }

    private static void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null) return;
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject CreateUiObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void StretchToFullscreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
    }

    private static Text CreateText(string name, Transform parent, string content, int size, FontStyle style, TextAnchor align, Color col, Vector2 pos, Vector2 s)
    {
        GameObject obj = CreateUiObject(name, parent);
        RectTransform r = obj.GetComponent<RectTransform>();
        r.sizeDelta = s; r.anchoredPosition = pos;
        Text t = obj.AddComponent<Text>();
        t.text = content; t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size; t.fontStyle = style; t.alignment = align; t.color = col;
        return t;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, Color col)
    {
        GameObject obj = CreateUiObject(name, parent);
        RectTransform r = obj.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(420f, 58f); r.anchoredPosition = pos;
        Image img = obj.AddComponent<Image>(); img.color = col;
        Button b = obj.AddComponent<Button>(); b.targetGraphic = img;
        CreateText(name + "L", obj.transform, label, 20, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white, Vector2.zero, new Vector2(360, 36));
        return b;
    }
}
