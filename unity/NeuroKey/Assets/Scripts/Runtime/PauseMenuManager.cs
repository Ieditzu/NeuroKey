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
    private GameObject mainPanel;
    private GameObject tasksPanel;
    
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
        root.AddComponent<GameClient>(); 
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
            if (actionResp.RequestPacketId == 8 && actionResp.Success) 
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => {
                    GameClient.Instance.SendPacket(new FetchChildStatsPacket()); 
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
        if (canvas == null) BuildUi();
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

        canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject dimmer = CreateUiObject("Dimmer", canvas.transform);
        dimmer.AddComponent<Image>().color = new Color(0.03f, 0.04f, 0.08f, 0.82f);
        StretchToFullscreen(dimmer.GetComponent<RectTransform>());

        // MAIN PANEL
        mainPanel = CreateUiObject("MainPanel", canvas.transform);
        RectTransform mainRect = mainPanel.GetComponent<RectTransform>();
        mainRect.sizeDelta = new Vector2(640f, 600f);
        mainRect.anchoredPosition = Vector2.zero;
        mainPanel.AddComponent<Image>().color = new Color(0.10f, 0.13f, 0.19f, 0.97f);
        mainPanel.AddComponent<Outline>().effectColor = new Color(0.27f, 0.78f, 0.94f, 0.45f);

        CreateText("PauseTitle", mainPanel.transform, "PAUSED", 38, FontStyle.Bold, TextAnchor.MiddleCenter, new Color(0.93f, 0.97f, 1f, 1f), new Vector2(0f, 240f), new Vector2(420f, 56f));
        
        CreateSensitivitySection(mainPanel.transform);

        GameObject qrSection = CreateUiObject("QrSection", mainPanel.transform);
        RectTransform qrRect = qrSection.GetComponent<RectTransform>();
        qrRect.sizeDelta = new Vector2(540f, 100f);
        qrRect.anchoredPosition = new Vector2(0f, 40f);

        qrStatusText = CreateText("QrStatus", qrSection.transform, 
            loggedInChildId == -1 ? "Not logged in" : loggedInChildName + " | " + loggedInChildPoints + " pts", 
            16, FontStyle.Italic, TextAnchor.MiddleCenter, Color.white, new Vector2(0, 30), new Vector2(500, 30));

        qrButton = CreateButton(qrSection.transform, "QrButton", "Generate QR Login", new Vector2(0f, -10f), new Color(0.4f, 0.2f, 0.8f, 1f));
        qrButton.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 40f);
        qrButton.onClick.AddListener(GenerateQrLogin);
        if (loggedInChildId != -1) qrButton.interactable = false;

        Button tasksBtn = CreateButton(mainPanel.transform, "TasksBtn", "View Tasks", new Vector2(0f, -60f), new Color(0.2f, 0.6f, 0.8f, 1f));
        tasksBtn.onClick.AddListener(() => ShowPanel(tasksPanel));

        Button resumeButton = CreateButton(mainPanel.transform, "ResumeButton", "Resume", new Vector2(0f, -130f), new Color(0.18f, 0.63f, 0.43f, 1f));
        resumeButton.onClick.AddListener(ResumeGame);

        Button saveButton = CreateButton(mainPanel.transform, "SaveButton", "Save Settings", new Vector2(0f, -200f), new Color(0.14f, 0.44f, 0.80f, 1f));
        saveButton.onClick.AddListener(SaveSettings);

        Button quitButton = CreateButton(mainPanel.transform, "QuitButton", "Quit Game", new Vector2(0f, -270f), new Color(0.72f, 0.24f, 0.26f, 1f));
        quitButton.onClick.AddListener(QuitGame);

        // TASKS PANEL
        tasksPanel = CreateUiObject("TasksPanel", canvas.transform);
        RectTransform tasksRect = tasksPanel.GetComponent<RectTransform>();
        tasksRect.sizeDelta = new Vector2(640f, 500f);
        tasksRect.anchoredPosition = Vector2.zero;
        tasksPanel.AddComponent<Image>().color = new Color(0.05f, 0.1f, 0.2f, 0.98f);
        tasksPanel.AddComponent<Outline>().effectColor = Color.cyan;

        CreateText("TasksTitle", tasksPanel.transform, "AVAILABLE TASKS", 30, FontStyle.Bold, TextAnchor.MiddleCenter, Color.cyan, new Vector2(0, 200), new Vector2(400, 50));

        taskListContainer = CreateUiObject("TaskList", tasksPanel.transform);
        RectTransform tlRect = taskListContainer.GetComponent<RectTransform>();
        tlRect.sizeDelta = new Vector2(550, 300);
        tlRect.anchoredPosition = new Vector2(0, 0);

        Button backBtn = CreateButton(tasksPanel.transform, "BackBtn", "Back", new Vector2(0, -200), new Color(0.4f, 0.4f, 0.4f));
        backBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        backBtn.onClick.AddListener(() => ShowPanel(mainPanel));

        tasksPanel.SetActive(false);
        canvasObject.SetActive(false);
        ApplySavedSensitivity();
        RebuildTaskList();
    }

    private void ShowPanel(GameObject panel)
    {
        mainPanel.SetActive(false);
        tasksPanel.SetActive(false);
        panel.SetActive(true);
    }

    private void RebuildTaskList()
    {
        if (taskListContainer == null) return;
        foreach (Transform child in taskListContainer.transform) Destroy(child.gameObject);

        float y = 130;
        foreach (var task in availableTasks)
        {
            GameObject item = CreateUiObject("TaskItem_" + task.Id, taskListContainer.transform);
            RectTransform iRect = item.GetComponent<RectTransform>();
            iRect.sizeDelta = new Vector2(520, 50);
            iRect.anchoredPosition = new Vector2(0, y);
            item.AddComponent<Image>().color = new Color(1,1,1,0.05f);

            CreateText("Label", item.transform, task.Title + " (" + task.Points + " pts)", 18, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white, new Vector2(-100, 0), new Vector2(300, 40));
            
            Button completeBtn = CreateButton(item.transform, "Btn", "Complete", new Vector2(180, 0), new Color(0.2f, 0.6f, 0.3f));
            completeBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 40);
            completeBtn.GetComponentInChildren<Text>().fontSize = 16;
            long tid = task.Id;
            completeBtn.onClick.AddListener(() => {
                if (loggedInChildId != -1)
                    GameClient.Instance.SendPacket(new CompleteTaskPacket(loggedInChildId, tid));
            });
            y -= 60;
        }
    }

    private void CreateSensitivitySection(Transform parent)
    {
        GameObject card = CreateUiObject("SensitivityCard", parent);
        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(540f, 150f);
        cardRect.anchoredPosition = new Vector2(0f, 150f);
        card.AddComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f, 0.96f);

        CreateText("SensitivityLabel", card.transform, "Mouse Sensitivity", 24, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white, new Vector2(-160f, 42f), new Vector2(280f, 36f));
        sensitivityValueText = CreateText("SensitivityValue", card.transform, "1.80", 22, FontStyle.Bold, TextAnchor.MiddleRight, Color.cyan, new Vector2(160f, 42f), new Vector2(120f, 36f));

        GameObject sliderObject = CreateUiObject("SensitivitySlider", card.transform);
        sliderObject.GetComponent<RectTransform>().sizeDelta = new Vector2(440f, 36f);
        sliderObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -18f);

        GameObject background = CreateUiObject("Background", sliderObject.transform);
        background.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.25f); background.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.75f);
        background.GetComponent<RectTransform>().offsetMin = Vector2.zero; background.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        background.AddComponent<Image>().color = new Color(0.24f, 0.28f, 0.36f, 1f);

        GameObject fill = CreateUiObject("Fill", CreateUiObject("Fill Area", sliderObject.transform).transform);
        fill.AddComponent<Image>().color = new Color(0.30f, 0.84f, 0.97f, 1f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero; fillRect.offsetMax = Vector2.zero;
        fill.transform.parent.GetComponent<RectTransform>().anchorMin = new Vector2(0f, 0.25f); fill.transform.parent.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 0.75f);

        GameObject handle = CreateUiObject("Handle", CreateUiObject("Handle Slide Area", sliderObject.transform).transform);
        handle.AddComponent<Image>().color = Color.white;
        handle.GetComponent<RectTransform>().sizeDelta = new Vector2(22f, 36f);
        handle.transform.parent.GetComponent<RectTransform>().anchorMin = Vector2.zero; handle.transform.parent.GetComponent<RectTransform>().anchorMax = Vector2.one;

        sensitivitySlider = sliderObject.AddComponent<Slider>();
        sensitivitySlider.minValue = 0.2f; sensitivitySlider.maxValue = 6f;
        sensitivitySlider.targetGraphic = handle.GetComponent<Image>();
        sensitivitySlider.fillRect = fillRect; sensitivitySlider.handleRect = handle.GetComponent<RectTransform>();
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void GenerateQrLogin()
    {
        if (GameClient.Instance != null && GameClient.Instance.IsConnected)
        {
            qrStatusText.text = "Generating...";
            GameClient.Instance.SendPacket(new GenerateQRLoginPacket());
        }
        else if (GameClient.Instance != null) { _ = GameClient.Instance.Connect(); }
    }

    private void PauseGame()
    {
        RebuildIfNeeded();
        ReacquireControllerIfNeeded();
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsGamePaused = true;
        if (canvas != null) { canvas.gameObject.SetActive(true); ShowPanel(mainPanel); }
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
