using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class Question2PadWhiteFadeSequence : MonoBehaviour
{
    private enum UiLanguage
    {
        Romanian = 0,
        English = 1
    }

    private const string PromptRo =
        "Debugging C++\n\n" +
        "Corecteaza functia astfel incat sa intoarca dublul valorii primite.\n\n" +
        "Cerinta:\n" +
        "- citeste codul din dreapta\n" +
        "- modifica doar ce este gresit\n" +
        "- ai 4 incercari";

    private const string PromptEn =
        "C++ Debugging\n\n" +
        "Fix the function so it returns double the received value.\n\n" +
        "Task:\n" +
        "- read the code on the right\n" +
        "- change only what is wrong\n" +
        "- you have 4 attempts";

    private const string InitialCode =
        "int MultiplyByTwo(int value)\n" +
        "{\n" +
        "    int doubled = value;\n" +
        "    return doubled;\n" +
        "}";

    private const string ExpectedCode =
        "int MultiplyByTwo(int value)\n" +
        "{\n" +
        "    int doubled = value * 2;\n" +
        "    return doubled;\n" +
        "}";

    [Header("Intro")]
    [SerializeField] private float burstDuration = 0.28f;
    [SerializeField] private float burstMaxScale = 0.32f;
    [SerializeField] private float whiteFadeDuration = 0.35f;

    [Header("Ui")]
    [SerializeField] private float panelFadeDuration = 0.22f;
    [SerializeField] private int titleSize = 34;
    [SerializeField] private int bodySize = 22;
    [SerializeField] private int codeTextSize = 20;
    [SerializeField] private float reenterCooldown = 1.25f;

    private static Canvas overlayCanvas;
    private static Image burstImageA;
    private static Image burstImageB;
    private static Image burstImageC;
    private static Image whiteImage;
    private static Image panelImage;
    private static Text titleText;
    private static Text subtitleText;
    private static Text languagePromptText;
    private static Text promptText;
    private static Text attemptsText;
    private static Text feedbackText;
    private static InputField codeInput;
    private static Text codePlaceholder;
    private static Button leaveButton;
    private static Text leaveButtonText;
    private static Button verifyButton;
    private static Text verifyButtonText;
    private static Button languageRoButton;
    private static Text languageRoButtonText;
    private static Button languageEnButton;
    private static Text languageEnButtonText;

    private Collider triggerCollider;
    private bool running;
    private bool leaveRequested;
    private bool verifyRequested;
    private bool languageChosen;
    private UiLanguage selectedLanguage = UiLanguage.Romanian;
    private int attemptsRemaining;
    private SphereController currentSphere;
    private FirstPersonControllerSimple currentFps;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        DisableLegacyLogic();
        DestroyLegacyOverlays();
        EnsureOverlay();
        ResetOverlay();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (running)
        {
            return;
        }

        if (!TryGetPlayer(other, out SphereController sphere, out FirstPersonControllerSimple fps))
        {
            return;
        }

        StartCoroutine(PlaySequence(sphere, fps));
    }

    private IEnumerator PlaySequence(SphereController sphere, FirstPersonControllerSimple fps)
    {
        running = true;
        leaveRequested = false;
        verifyRequested = false;
        languageChosen = false;
        selectedLanguage = UiLanguage.Romanian;
        attemptsRemaining = 4;
        currentSphere = sphere;
        currentFps = fps;

        EnsureOverlay();
        ResetOverlay();
        overlayCanvas.gameObject.SetActive(true);

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        SetPlayerLockState(sphere, fps, true, true);
        if (fps != null)
        {
            fps.SetCameraControlEnabled(false);
        }
        SetCursorVisible(true);

        yield return PlayCenterBurst();
        yield return ExpandWhiteFromCenter();
        yield return ShowLanguageSelection();

        if (!leaveRequested)
        {
            yield return ShowChallengeScreen();
        }

        while (!leaveRequested)
        {
            yield return null;
        }

        CleanupAndRestore();
        StartCoroutine(ReenableTriggerAfterDelay());
    }

    private IEnumerator ShowLanguageSelection()
    {
        titleText.text = "Question 2";
        subtitleText.text = "Debugging C++";
        languagePromptText.text = "Alege limba / Choose language";

        titleText.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(true);
        languagePromptText.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        languageRoButton.gameObject.SetActive(true);
        languageEnButton.gameObject.SetActive(true);

        leaveButtonText.text = "Leave";
        languageRoButtonText.text = "Romana";
        languageEnButtonText.text = "English";

        leaveButton.onClick.RemoveAllListeners();
        languageRoButton.onClick.RemoveAllListeners();
        languageEnButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveClicked);
        languageRoButton.onClick.AddListener(() => SelectLanguage(UiLanguage.Romanian));
        languageEnButton.onClick.AddListener(() => SelectLanguage(UiLanguage.English));

        yield return FadePanel(true);

        while (!languageChosen && !leaveRequested)
        {
            yield return null;
        }

        languagePromptText.gameObject.SetActive(false);
        languageRoButton.gameObject.SetActive(false);
        languageEnButton.gameObject.SetActive(false);
    }

    private IEnumerator ShowChallengeScreen()
    {
        titleText.text = selectedLanguage == UiLanguage.Romanian ? "Question 2 // Debugging" : "Question 2 // Debugging";
        subtitleText.text = selectedLanguage == UiLanguage.Romanian ? "Editor C++" : "C++ Editor";
        promptText.text = selectedLanguage == UiLanguage.Romanian ? PromptRo : PromptEn;
        attemptsText.text = selectedLanguage == UiLanguage.Romanian ? "Incercari ramase: 4 / 4" : "Attempts left: 4 / 4";
        feedbackText.text = selectedLanguage == UiLanguage.Romanian
            ? "Corecteaza codul si apasa Verify."
            : "Fix the code and press Verify.";
        feedbackText.color = new Color(0.15f, 0.16f, 0.2f, 1f);

        promptText.gameObject.SetActive(true);
        attemptsText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        codeInput.gameObject.SetActive(true);
        verifyButton.gameObject.SetActive(true);

        verifyButtonText.text = "Verify";
        codePlaceholder.text = selectedLanguage == UiLanguage.Romanian
            ? "Scrie aici codul corect..."
            : "Write the corrected code here...";
        codeInput.text = InitialCode;
        codeInput.ActivateInputField();

        verifyButton.onClick.RemoveAllListeners();
        verifyButton.onClick.AddListener(OnVerifyClicked);

        bool solved = false;
        while (!leaveRequested && !solved)
        {
            while (!verifyRequested && !leaveRequested)
            {
                yield return null;
            }

            if (leaveRequested)
            {
                yield break;
            }

            verifyRequested = false;
            solved = EvaluateAnswer();
        }
    }

    private bool EvaluateAnswer()
    {
        bool correct = NormalizeCode(codeInput.text) == NormalizeCode(ExpectedCode);
        if (correct)
        {
            feedbackText.text = selectedLanguage == UiLanguage.Romanian ? "Raspuns bun." : "Correct answer.";
            feedbackText.color = new Color(0.12f, 0.52f, 0.24f, 1f);
            verifyButton.gameObject.SetActive(false);
            attemptsText.text = selectedLanguage == UiLanguage.Romanian ? "Completat" : "Completed";
            return true;
        }

        attemptsRemaining = Mathf.Max(0, attemptsRemaining - 1);
        attemptsText.text = selectedLanguage == UiLanguage.Romanian
            ? "Incercari ramase: " + attemptsRemaining + " / 4"
            : "Attempts left: " + attemptsRemaining + " / 4";

        if (attemptsRemaining > 0)
        {
            feedbackText.text = selectedLanguage == UiLanguage.Romanian ? "Raspuns gresit." : "Wrong answer.";
            feedbackText.color = new Color(0.74f, 0.22f, 0.18f, 1f);
            return false;
        }

        feedbackText.text = selectedLanguage == UiLanguage.Romanian
            ? "Nu mai ai incercari. Poti apasa Leave."
            : "No attempts left. You can press Leave.";
        feedbackText.color = new Color(0.74f, 0.22f, 0.18f, 1f);
        verifyButton.gameObject.SetActive(false);
        return false;
    }

    private void DisableLegacyLogic()
    {
        CodeChallengePadCinematic codeChallenge = GetComponent<CodeChallengePadCinematic>();
        if (codeChallenge != null)
        {
            codeChallenge.enabled = false;
        }

        CppQuestionPadCinematic cppCinematic = GetComponent<CppQuestionPadCinematic>();
        if (cppCinematic != null)
        {
            cppCinematic.enabled = false;
        }
    }

    private static void DestroyLegacyOverlays()
    {
        DestroyOverlayByName("CppQuestionPadCinematicCanvas");
        DestroyOverlayByName("CodeChallengePadCanvas");
        DestroyOverlayByName("MediumPadWhiteTestCanvas");
    }

    private static void DestroyOverlayByName(string objectName)
    {
        GameObject overlay = GameObject.Find(objectName);
        if (overlay != null)
        {
            Destroy(overlay);
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas == null)
        {
            GameObject canvasObject = GameObject.Find("Question2PadFadeCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Question2PadFadeCanvas");
                overlayCanvas = canvasObject.AddComponent<Canvas>();
                overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                overlayCanvas.sortingOrder = 15000;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObject);
            }
            else
            {
                overlayCanvas = canvasObject.GetComponent<Canvas>();
            }
        }

        Transform root = overlayCanvas.transform;
        burstImageA = EnsureBurstImage(root, "BurstA", 0f);
        burstImageB = EnsureBurstImage(root, "BurstB", 45f);
        burstImageC = EnsureBurstImage(root, "BurstC", 90f);
        whiteImage = EnsureImage(root, "WhiteImage", new Color(1f, 1f, 1f, 0f));
        panelImage = EnsureImage(root, "MainPanel", new Color(1f, 1f, 1f, 0f));
        titleText = EnsureText(panelImage.transform, "TitleText", new Vector2(0.22f, 0.88f), new Vector2(420f, 50f), titleSize, FontStyle.Bold, new Color(0.08f, 0.08f, 0.1f, 1f), TextAnchor.MiddleLeft);
        subtitleText = EnsureText(panelImage.transform, "SubtitleText", new Vector2(0.72f, 0.88f), new Vector2(340f, 44f), bodySize, FontStyle.Bold, new Color(0.0f, 0.45f, 0.7f, 1f), TextAnchor.MiddleLeft);
        languagePromptText = EnsureText(panelImage.transform, "LanguagePromptText", new Vector2(0.5f, 0.57f), new Vector2(700f, 70f), 30, FontStyle.Bold, new Color(0.08f, 0.08f, 0.1f, 1f), TextAnchor.MiddleCenter);
        promptText = EnsureText(panelImage.transform, "PromptText", new Vector2(0.24f, 0.54f), new Vector2(420f, 330f), bodySize, FontStyle.Normal, new Color(0.12f, 0.12f, 0.15f, 1f), TextAnchor.UpperLeft);
        attemptsText = EnsureText(panelImage.transform, "AttemptsText", new Vector2(0.24f, 0.21f), new Vector2(420f, 42f), 20, FontStyle.Bold, new Color(0.08f, 0.38f, 0.62f, 1f), TextAnchor.MiddleLeft);
        feedbackText = EnsureText(panelImage.transform, "FeedbackText", new Vector2(0.24f, 0.13f), new Vector2(420f, 84f), 20, FontStyle.Bold, new Color(0.14f, 0.14f, 0.16f, 1f), TextAnchor.UpperLeft);
        codeInput = EnsureCodeInput(panelImage.transform);
        codePlaceholder = codeInput.placeholder as Text;

        leaveButton = EnsureButton(root, "LeaveButton", new Vector2(0.12f, 0.09f), new Vector2(170f, 52f), new Color(0.11f, 0.11f, 0.14f, 0.94f), "Leave", 24);
        verifyButton = EnsureButton(panelImage.transform, "VerifyButton", new Vector2(0.76f, 0.14f), new Vector2(160f, 50f), new Color(0.0f, 0.49f, 0.76f, 0.96f), "Verify", 24);
        languageRoButton = EnsureButton(panelImage.transform, "LanguageRoButton", new Vector2(0.40f, 0.43f), new Vector2(200f, 62f), new Color(0.0f, 0.49f, 0.76f, 0.96f), "Romana", 24);
        languageEnButton = EnsureButton(panelImage.transform, "LanguageEnButton", new Vector2(0.60f, 0.43f), new Vector2(200f, 62f), new Color(0.14f, 0.58f, 0.42f, 0.96f), "English", 24);

        leaveButtonText = leaveButton.GetComponentInChildren<Text>(true);
        verifyButtonText = verifyButton.GetComponentInChildren<Text>(true);
        languageRoButtonText = languageRoButton.GetComponentInChildren<Text>(true);
        languageEnButtonText = languageEnButton.GetComponentInChildren<Text>(true);

        StretchFullscreen(whiteImage.rectTransform);
        StretchFullscreen(panelImage.rectTransform);
        LayoutPanel();
        EnsureEventSystem();
    }

    private void LayoutPanel()
    {
        panelImage.rectTransform.localScale = Vector3.one;

        Outline panelOutline = panelImage.GetComponent<Outline>() ?? panelImage.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0f, 0f, 0.08f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        codeInput.GetComponent<Image>().color = new Color(0.96f, 0.97f, 1f, 0.96f);
        Outline editorOutline = codeInput.GetComponent<Outline>() ?? codeInput.gameObject.AddComponent<Outline>();
        editorOutline.effectColor = new Color(0f, 0.48f, 0.74f, 0.25f);
        editorOutline.effectDistance = new Vector2(2f, -2f);
    }

    private static Image EnsureBurstImage(Transform parent, string name, float rotationZ)
    {
        GameObject go = GetOrCreateUiObject(parent, name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(40f, 280f);
        rect.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
        rect.localScale = Vector3.zero;

        Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        image.raycastTarget = false;
        return image;
    }

    private static Image EnsureImage(Transform parent, string name, Color color)
    {
        GameObject go = GetOrCreateUiObject(parent, name);
        Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text EnsureText(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        GameObject go = GetOrCreateUiObject(parent, name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        Text text = go.GetComponent<Text>() ?? go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static Button EnsureButton(Transform parent, string name, Vector2 anchor, Vector2 size, Color color, string label, int fontSize)
    {
        GameObject go = GetOrCreateUiObject(parent, name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;

        Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.color = color;
        Button button = go.GetComponent<Button>() ?? go.AddComponent<Button>();
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        GameObject labelObject = GetOrCreateUiObject(go.transform, "Label");
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text text = labelObject.GetComponent<Text>() ?? labelObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        text.raycastTarget = false;
        return button;
    }

    private InputField EnsureCodeInput(Transform parent)
    {
        GameObject root = GetOrCreateUiObject(parent, "CodeInput");
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.73f, 0.54f);
        rect.anchorMax = new Vector2(0.73f, 0.54f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600f, 360f);

        Image image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        image.color = new Color(0.96f, 0.97f, 1f, 0.96f);
        image.raycastTarget = true;

        InputField input = root.GetComponent<InputField>() ?? root.AddComponent<InputField>();
        input.lineType = InputField.LineType.MultiLineNewline;
        input.transition = Selectable.Transition.None;
        input.selectionColor = new Color(0.0f, 0.49f, 0.76f, 0.18f);
        input.caretColor = new Color(0.0f, 0.49f, 0.76f, 1f);
        input.customCaretColor = true;

        GameObject textObj = GetOrCreateUiObject(root.transform, "Text");
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 18f);
        textRect.offsetMax = new Vector2(-18f, -18f);

        Text text = textObj.GetComponent<Text>() ?? textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = codeTextSize;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.09f, 0.1f, 0.13f, 1f);
        text.supportRichText = false;
        input.textComponent = text;

        GameObject placeholderObj = GetOrCreateUiObject(root.transform, "Placeholder");
        RectTransform placeholderRect = placeholderObj.GetComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = new Vector2(18f, 18f);
        placeholderRect.offsetMax = new Vector2(-18f, -18f);

        Text placeholder = placeholderObj.GetComponent<Text>() ?? placeholderObj.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = codeTextSize;
        placeholder.alignment = TextAnchor.UpperLeft;
        placeholder.color = new Color(0.09f, 0.1f, 0.13f, 0.35f);
        placeholder.text = "Write code...";
        input.placeholder = placeholder;
        input.targetGraphic = image;
        return input;
    }

    private static GameObject GetOrCreateUiObject(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void StretchFullscreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = EventSystem.current != null ? EventSystem.current : Object.FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            if (existing.GetComponent<InputSystemUIInputModule>() == null)
            {
                existing.gameObject.AddComponent<InputSystemUIInputModule>();
            }
            return;
        }

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
        Object.DontDestroyOnLoad(go);
    }

    private void ResetOverlay()
    {
        HideBurstImage(burstImageA);
        HideBurstImage(burstImageB);
        HideBurstImage(burstImageC);

        if (whiteImage != null)
        {
            whiteImage.enabled = false;
            whiteImage.color = new Color(1f, 1f, 1f, 0f);
            whiteImage.rectTransform.localScale = Vector3.zero;
        }

        if (panelImage != null)
        {
            panelImage.gameObject.SetActive(false);
            panelImage.color = new Color(0.98f, 0.98f, 0.98f, 0f);
        }

        HideText(titleText);
        HideText(subtitleText);
        HideText(languagePromptText);
        HideText(promptText);
        HideText(attemptsText);
        HideText(feedbackText);

        if (codeInput != null)
        {
            codeInput.text = string.Empty;
            codeInput.gameObject.SetActive(false);
        }

        HideButton(leaveButton);
        HideButton(verifyButton);
        HideButton(languageRoButton);
        HideButton(languageEnButton);
    }

    private static void HideText(Text text)
    {
        if (text != null)
        {
            text.gameObject.SetActive(false);
            Color color = text.color;
            color.a = 0f;
            text.color = color;
        }
    }

    private static void HideButton(Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.gameObject.SetActive(false);
        }
    }

    private static void HideBurstImage(Image image)
    {
        if (image == null)
        {
            return;
        }

        image.enabled = false;
        image.color = new Color(1f, 1f, 1f, 0f);
        image.rectTransform.localScale = Vector3.zero;
    }

    private static IEnumerator FadeText(Text text, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, duration));
            Color color = text.color;
            color.a = Mathf.Lerp(from, to, t);
            text.color = color;
            yield return null;
        }

        Color finalColor = text.color;
        finalColor.a = to;
        text.color = finalColor;
    }

    private IEnumerator FadePanel(bool show)
    {
        panelImage.gameObject.SetActive(true);
        float from = show ? 0f : 1f;
        float to = show ? 1f : 0f;
        float elapsed = 0f;
        while (elapsed < panelFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, panelFadeDuration));
            float alpha = Mathf.Lerp(from, to, t);
            panelImage.color = new Color(0.98f, 0.98f, 0.98f, alpha);
            yield return null;
        }

        panelImage.color = new Color(0.98f, 0.98f, 0.98f, to);
        if (!show)
        {
            panelImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayCenterBurst()
    {
        burstImageA.enabled = true;
        burstImageB.enabled = true;
        burstImageC.enabled = true;

        float elapsed = 0f;
        while (elapsed < burstDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, burstDuration));
            float alpha = 1f - t;
            float scale = Mathf.Lerp(0.08f, burstMaxScale, t);
            AnimateBurstImage(burstImageA, scale, alpha);
            AnimateBurstImage(burstImageB, scale * 0.86f, alpha * 0.92f);
            AnimateBurstImage(burstImageC, scale * 0.72f, alpha * 0.84f);
            yield return null;
        }

        HideBurstImage(burstImageA);
        HideBurstImage(burstImageB);
        HideBurstImage(burstImageC);
    }

    private static void AnimateBurstImage(Image image, float scale, float alpha)
    {
        if (image == null)
        {
            return;
        }

        image.color = new Color(1f, 1f, 1f, alpha);
        image.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    private IEnumerator ExpandWhiteFromCenter()
    {
        whiteImage.enabled = true;
        whiteImage.color = new Color(1f, 1f, 1f, 1f);
        whiteImage.rectTransform.localScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < whiteFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, whiteFadeDuration));
            float scale = Mathf.Lerp(0.02f, 1.45f, t);
            whiteImage.rectTransform.localScale = new Vector3(scale, scale, 1f);
            yield return null;
        }

        whiteImage.rectTransform.localScale = Vector3.one * 1.45f;
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return string.Empty;
        }

        string[] lines = code.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        return string.Join("\n", lines).Trim();
    }

    private void SelectLanguage(UiLanguage language)
    {
        selectedLanguage = language;
        languageChosen = true;
    }

    private void OnVerifyClicked()
    {
        verifyRequested = true;
    }

    private void OnLeaveClicked()
    {
        leaveRequested = true;
    }

    private void CleanupAndRestore()
    {
        ResetOverlay();
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }

        SetCursorVisible(false);
        SetPlayerLockState(currentSphere, currentFps, false, false);
        if (currentFps != null)
        {
            currentFps.SetCameraControlEnabled(true);
        }

        currentSphere = null;
        currentFps = null;
        running = false;
    }

    private IEnumerator ReenableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, reenterCooldown));
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
    }

    private static bool TryGetPlayer(Collider other, out SphereController sphere, out FirstPersonControllerSimple fps)
    {
        sphere = other.GetComponent<SphereController>();
        if (sphere == null)
        {
            sphere = other.GetComponentInParent<SphereController>();
        }

        fps = other.GetComponent<FirstPersonControllerSimple>();
        if (fps == null)
        {
            fps = other.GetComponentInParent<FirstPersonControllerSimple>();
        }

        return sphere != null || fps != null;
    }

    private static void SetPlayerLockState(SphereController sphere, FirstPersonControllerSimple fps, bool movementLocked, bool hardFreeze)
    {
        if (sphere != null)
        {
            sphere.SetMovementLocked(movementLocked);
            sphere.SetHardFreeze(hardFreeze);
        }

        if (fps != null)
        {
            fps.SetMovementLocked(movementLocked);
            fps.SetHardFreeze(hardFreeze);
        }
    }

    private static void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }
}

public static class Question2PadWhiteFadeSequenceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachIfNeeded()
    {
        GameObject pad = GameObject.Find("Question2Pad");
        if (pad == null)
        {
            return;
        }

        if (pad.GetComponent<Question2PadWhiteFadeSequence>() == null)
        {
            pad.AddComponent<Question2PadWhiteFadeSequence>();
        }
    }
}
