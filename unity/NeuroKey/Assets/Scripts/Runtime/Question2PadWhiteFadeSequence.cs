using System.Collections;
using System.Collections.Generic;
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

    private struct DebugQuestion
    {
        public string PromptRo;
        public string PromptEn;
        public string TitleRo;
        public string TitleEn;
        public string Code;
        public string ExpectedCode;
        public string HintRo;
        public string HintEn;

        public DebugQuestion(string titleRo, string titleEn, string promptRo, string promptEn, string code, string expectedCode, string hintRo, string hintEn)
        {
            TitleRo = titleRo;
            TitleEn = titleEn;
            PromptRo = promptRo;
            PromptEn = promptEn;
            Code = code;
            ExpectedCode = expectedCode;
            HintRo = hintRo;
            HintEn = hintEn;
        }
    }

    private static readonly DebugQuestion[] Questions =
    {
        new DebugQuestion(
            "Debugging 1",
            "Debugging 1",
            "Ce nu merge:\n- functia trebuie sa dubleze valoarea\n- acum `doubled` primeste doar valoarea initiala\n\nCe faci:\n- repara calculul din functie\n- nu schimba numele functiei\n- apasa `Verify` dupa editare",
            "What is broken:\n- the function should double the value\n- right now `doubled` only gets the original value\n\nWhat to do:\n- fix the calculation inside the function\n- do not rename the function\n- press `Verify` after editing",
            "#include <iostream>\nusing namespace std;\n\nint MultiplyByTwo(int value)\n{\n    int doubled = value;\n    return doubled;\n}\n\nint main()\n{\n    cout << MultiplyByTwo(6) << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nint MultiplyByTwo(int value)\n{\n    int doubled = value * 2;\n    return doubled;\n}\n\nint main()\n{\n    cout << MultiplyByTwo(6) << endl;\n    return 0;\n}",
            "Hint:\nBugul este in `MultiplyByTwo`.\n`doubled` trebuie sa foloseasca `value * 2`.\n`main` ramane la fel.\n\nCod corect:\nint MultiplyByTwo(int value)\n{\n    int doubled = value * 2;\n    return doubled;\n}",
            "Hint:\nThe bug is in `MultiplyByTwo`.\n`doubled` must use `value * 2`.\n`main` stays the same.\n\nCorrect code:\nint MultiplyByTwo(int value)\n{\n    int doubled = value * 2;\n    return doubled;\n}"),
        new DebugQuestion(
            "Debugging 2",
            "Debugging 2",
            "Ce nu merge:\n- functia trebuie sa faca suma\n- acum foloseste `-` in loc de `+`\n\nCe faci:\n- schimba doar operatorul din `return`\n- lasa restul codului la fel",
            "What is broken:\n- the function should compute a sum\n- right now it uses `-` instead of `+`\n\nWhat to do:\n- change only the operator in `return`\n- leave the rest of the code as it is",
            "#include <iostream>\nusing namespace std;\n\nint Sum(int a, int b)\n{\n    return a - b;\n}\n\nint main()\n{\n    int total = Sum(4, 6);\n    cout << total << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nint Sum(int a, int b)\n{\n    return a + b;\n}\n\nint main()\n{\n    int total = Sum(4, 6);\n    cout << total << endl;\n    return 0;\n}",
            "Hint:\nProblema este operatorul din `return`.\nPentru suma trebuie folosit `+`.\n\nCod corect:\nint Sum(int a, int b)\n{\n    return a + b;\n}",
            "Hint:\nThe issue is the operator in `return`.\nFor a sum, you must use `+`.\n\nCorrect code:\nint Sum(int a, int b)\n{\n    return a + b;\n}"),
        new DebugQuestion(
            "Debugging 3",
            "Debugging 3",
            "Ce nu merge:\n- functia trebuie sa intoarca `true` pentru numere pare\n- acum verificarea este inversata\n\nCe faci:\n- corecteaza comparatia dupa `% 2`\n- nu schimba restul functiei",
            "What is broken:\n- the function should return `true` for even numbers\n- right now the check is reversed\n\nWhat to do:\n- fix the comparison after `% 2`\n- do not change the rest of the function",
            "#include <iostream>\nusing namespace std;\n\nbool IsEven(int number)\n{\n    return number % 2 != 0;\n}\n\nint main()\n{\n    cout << (IsEven(8) ? \"even\" : \"odd\") << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nbool IsEven(int number)\n{\n    return number % 2 == 0;\n}\n\nint main()\n{\n    cout << (IsEven(8) ? \"even\" : \"odd\") << endl;\n    return 0;\n}",
            "Hint:\nUn numar par are restul `0` la impartirea cu `2`.\nComparatia trebuie sa verifice `== 0`.\n\nCod corect:\nbool IsEven(int number)\n{\n    return number % 2 == 0;\n}",
            "Hint:\nAn even number has remainder `0` when divided by `2`.\nThe comparison should check `== 0`.\n\nCorrect code:\nbool IsEven(int number)\n{\n    return number % 2 == 0;\n}")
    };

    [Header("Intro")]
    [SerializeField] private float burstDuration = 0.28f;
    [SerializeField] private float burstMaxScale = 0.32f;
    [SerializeField] private float whiteFadeDuration = 0.35f;

    [Header("UI")]
    [SerializeField] private float panelFadeDuration = 0.22f;
    [SerializeField] private int titleSize = 34;
    [SerializeField] private int bodySize = 18;
    [SerializeField] private int codeTextSize = 18;
    [SerializeField] private float reenterCooldown = 3f;
    [SerializeField] private int attemptsPerQuestion = 4;

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
    private static Text summaryText;
    private static Text hintText;
    private static InputField codeInput;
    private static Text codePlaceholder;
    private static Button leaveButton;
    private static Text leaveButtonText;
    private static Button verifyButton;
    private static Text verifyButtonText;
    private static Button nextButton;
    private static Text nextButtonText;
    private static Button hintButton;
    private static Text hintButtonText;
    private static Button backToQuestionButton;
    private static Text backToQuestionButtonText;
    private static Button retryWrongButton;
    private static Text retryWrongButtonText;
    private static Button languageRoButton;
    private static Text languageRoButtonText;
    private static Button languageEnButton;
    private static Text languageEnButtonText;

    private Collider triggerCollider;
    private bool running;
    private bool leaveRequested;
    private bool verifyRequested;
    private bool nextRequested;
    private bool hintRequested;
    private bool backToQuestionRequested;
    private bool retryWrongRequested;
    private bool languageChosen;
    private UiLanguage selectedLanguage = UiLanguage.Romanian;
    private SphereController currentSphere;
    private FirstPersonControllerSimple currentFps;
    private bool[] answeredCorrectly = new bool[Questions.Length];
    private readonly List<int> wrongQuestionIndices = new List<int>();
    private int activeAttemptsRemaining;
    private bool activeQuestionSolved;

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
        nextRequested = false;
        hintRequested = false;
        backToQuestionRequested = false;
        retryWrongRequested = false;
        languageChosen = false;
        selectedLanguage = UiLanguage.Romanian;
        currentSphere = sphere;
        currentFps = fps;
        answeredCorrectly = new bool[Questions.Length];
        wrongQuestionIndices.Clear();

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
            int[] allQuestionIndices = new int[Questions.Length];
            for (int i = 0; i < Questions.Length; i++)
            {
                allQuestionIndices[i] = i;
            }

            yield return RunQuestionSet(allQuestionIndices, false);
        }

        while (!leaveRequested)
        {
            yield return null;
        }

        CleanupAndRestore();
        StartCoroutine(ReenableTriggerAfterDelay());
    }

    private IEnumerator RunQuestionSet(int[] indices, bool retryMode)
    {
        wrongQuestionIndices.Clear();

        for (int i = 0; i < indices.Length && !leaveRequested; i++)
        {
            int questionIndex = indices[i];
            bool correct = false;
            yield return RunSingleQuestion(questionIndex, i + 1, indices.Length, retryMode, value => correct = value);

            if (leaveRequested)
            {
                yield break;
            }

            if (!correct)
            {
                wrongQuestionIndices.Add(questionIndex);
            }
            else
            {
                answeredCorrectly[questionIndex] = true;
            }
        }

        if (leaveRequested)
        {
            yield break;
        }

        yield return ShowSummary(retryMode);
    }

    private IEnumerator RunSingleQuestion(int questionIndex, int visibleIndex, int totalVisible, bool retryMode, System.Action<bool> onComplete)
    {
        DebugQuestion question = Questions[questionIndex];
        int attemptsRemaining = attemptsPerQuestion;
        bool solved = false;

        activeAttemptsRemaining = attemptsRemaining;
        activeQuestionSolved = false;
        SetupQuestionScreen(questionIndex, totalVisible, visibleIndex, question, attemptsRemaining, retryMode);
        yield return FadePanel(true);

        while (!leaveRequested && !solved)
        {
            while (!verifyRequested && !hintRequested && !leaveRequested)
            {
                yield return null;
            }

            if (leaveRequested)
            {
                onComplete(false);
                yield break;
            }

            if (hintRequested)
            {
                hintRequested = false;
                yield return ShowHintScreen(question);
                continue;
            }

            verifyRequested = false;
            bool correct = NormalizeCode(codeInput.text) == NormalizeCode(question.ExpectedCode);
            if (correct)
            {
                solved = true;
                activeQuestionSolved = true;
                feedbackText.text = selectedLanguage == UiLanguage.Romanian ? "Raspuns bun." : "Correct answer.";
                feedbackText.color = new Color(0.10f, 0.5f, 0.22f, 1f);
                attemptsText.text = selectedLanguage == UiLanguage.Romanian ? "Rezolvat corect" : "Solved correctly";
                verifyButton.gameObject.SetActive(false);
                hintButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                nextButtonText.text = visibleIndex < totalVisible
                    ? (selectedLanguage == UiLanguage.Romanian ? "Urmatoarea intrebare" : "Next question")
                    : (selectedLanguage == UiLanguage.Romanian ? "Vezi rezultatul" : "See result");

                nextRequested = false;
                while (!nextRequested && !leaveRequested)
                {
                    yield return null;
                }

                if (leaveRequested)
                {
                    onComplete(true);
                    yield break;
                }

                nextRequested = false;
                onComplete(true);
                yield break;
            }

            attemptsRemaining = Mathf.Max(0, attemptsRemaining - 1);
            activeAttemptsRemaining = attemptsRemaining;
            attemptsText.text = selectedLanguage == UiLanguage.Romanian
                ? "Incercari ramase: " + attemptsRemaining + " / " + attemptsPerQuestion
                : "Attempts left: " + attemptsRemaining + " / " + attemptsPerQuestion;
            feedbackText.text = selectedLanguage == UiLanguage.Romanian ? "Raspuns gresit." : "Wrong answer.";
            feedbackText.color = new Color(0.76f, 0.18f, 0.16f, 1f);

            if (attemptsRemaining == 0)
            {
                verifyButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
                nextButtonText.text = visibleIndex < totalVisible
                    ? (selectedLanguage == UiLanguage.Romanian ? "Continua" : "Continue")
                    : (selectedLanguage == UiLanguage.Romanian ? "Vezi rezultatul" : "See result");
                feedbackText.text = selectedLanguage == UiLanguage.Romanian
                    ? "Nu mai ai incercari. Foloseste Hint sau continua."
                    : "No attempts left. Use Hint or continue.";

                nextRequested = false;
                while (!nextRequested && !hintRequested && !leaveRequested)
                {
                    yield return null;
                }

                if (leaveRequested)
                {
                    onComplete(false);
                    yield break;
                }

                if (hintRequested)
                {
                    hintRequested = false;
                    yield return ShowHintScreen(question);
                    continue;
                }

                nextRequested = false;
                onComplete(false);
                yield break;
            }
        }
    }

    private void SetupQuestionScreen(int questionIndex, int totalVisible, int visibleIndex, DebugQuestion question, int attemptsRemaining, bool retryMode)
    {
        titleText.text = retryMode
            ? (selectedLanguage == UiLanguage.Romanian ? "Question2 // Refa intrebarile gresite" : "Question2 // Retry Wrong Questions")
            : "Question2 // Debugging C++ #" + (questionIndex + 1);
        subtitleText.text = selectedLanguage == UiLanguage.Romanian ? "Editor C++" : "C++ Editor";
        promptText.text = (selectedLanguage == UiLanguage.Romanian ? question.TitleRo : question.TitleEn)
            + "\n\n"
            + (selectedLanguage == UiLanguage.Romanian ? question.PromptRo : question.PromptEn);
        attemptsText.text = selectedLanguage == UiLanguage.Romanian
            ? "Intrebarea " + visibleIndex + " / " + totalVisible + " | Incercari: " + attemptsRemaining + " / " + attemptsPerQuestion
            : "Question " + visibleIndex + " / " + totalVisible + " | Attempts: " + attemptsRemaining + " / " + attemptsPerQuestion;
        feedbackText.text = selectedLanguage == UiLanguage.Romanian
            ? "Verifica atent ce nu merge si repara doar partea gresita."
            : "Read what is broken carefully and fix only the wrong part.";
        feedbackText.color = new Color(0.14f, 0.14f, 0.16f, 1f);
        codePlaceholder.text = selectedLanguage == UiLanguage.Romanian
            ? "Scrie aici varianta corecta..."
            : "Write the corrected version here...";
        codeInput.text = question.Code;

        panelImage.gameObject.SetActive(true);
        titleText.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(true);
        promptText.gameObject.SetActive(true);
        attemptsText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        codeInput.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        verifyButton.gameObject.SetActive(true);
        hintButton.gameObject.SetActive(true);
        nextButton.gameObject.SetActive(false);
        summaryText.gameObject.SetActive(false);
        retryWrongButton.gameObject.SetActive(false);
        languagePromptText.gameObject.SetActive(false);
        languageRoButton.gameObject.SetActive(false);
        languageEnButton.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        backToQuestionButton.gameObject.SetActive(false);

        leaveButtonText.text = "Leave";
        verifyButtonText.text = "Verify";
        hintButtonText.text = "Hint";

        leaveButton.onClick.RemoveAllListeners();
        verifyButton.onClick.RemoveAllListeners();
        hintButton.onClick.RemoveAllListeners();
        nextButton.onClick.RemoveAllListeners();

        leaveButton.onClick.AddListener(OnLeaveClicked);
        verifyButton.onClick.AddListener(OnVerifyClicked);
        hintButton.onClick.AddListener(OnHintClicked);
        nextButton.onClick.AddListener(OnNextClicked);

        codeInput.ActivateInputField();
    }

    private IEnumerator ShowSummary(bool retryMode)
    {
        int correctCount = CountCorrectAnswers();
        int wrongCount = Questions.Length - correctCount;

        titleText.text = selectedLanguage == UiLanguage.Romanian ? "Rezultat Final" : "Final Result";
        subtitleText.text = retryMode
            ? (selectedLanguage == UiLanguage.Romanian ? "Scor dupa retry" : "Score after retry")
            : (selectedLanguage == UiLanguage.Romanian ? "Scor" : "Score");
        summaryText.text = selectedLanguage == UiLanguage.Romanian
            ? "Raspunsuri corecte: " + correctCount + " / " + Questions.Length + "\nRaspunsuri gresite: " + wrongCount
            : "Correct answers: " + correctCount + " / " + Questions.Length + "\nWrong answers: " + wrongCount;

        titleText.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(true);
        summaryText.gameObject.SetActive(true);
        promptText.gameObject.SetActive(false);
        attemptsText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        codeInput.gameObject.SetActive(false);
        verifyButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        backToQuestionButton.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(true);

        retryWrongButton.gameObject.SetActive(wrongCount > 0);
        retryWrongButtonText.text = selectedLanguage == UiLanguage.Romanian ? "Refa intrebarile gresite" : "Retry wrong questions";

        leaveButton.onClick.RemoveAllListeners();
        retryWrongButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveClicked);
        retryWrongButton.onClick.AddListener(OnRetryWrongClicked);

        if (wrongCount == 0)
        {
            summaryText.text += selectedLanguage == UiLanguage.Romanian
                ? "\n\nAi terminat toate intrebarile corect."
                : "\n\nYou solved all questions correctly.";
        }
        else
        {
            summaryText.text += selectedLanguage == UiLanguage.Romanian
                ? "\n\nPoti apasa butonul de mai jos ca sa refaci doar intrebarile la care ai gresit."
                : "\n\nYou can press the button below to retry only the questions you missed.";
        }

        retryWrongRequested = false;
        while (!leaveRequested && !(wrongCount > 0 && retryWrongRequested))
        {
            yield return null;
        }

        if (leaveRequested || wrongCount == 0)
        {
            yield break;
        }

        retryWrongRequested = false;
        int[] retryIndices = wrongQuestionIndices.ToArray();
        yield return RunQuestionSet(retryIndices, true);
    }

    private IEnumerator ShowHintScreen(DebugQuestion question)
    {
        titleText.gameObject.SetActive(false);
        subtitleText.gameObject.SetActive(false);
        promptText.gameObject.SetActive(false);
        attemptsText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        codeInput.gameObject.SetActive(false);
        verifyButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        summaryText.gameObject.SetActive(false);
        retryWrongButton.gameObject.SetActive(false);
        languagePromptText.gameObject.SetActive(false);
        languageRoButton.gameObject.SetActive(false);
        languageEnButton.gameObject.SetActive(false);

        hintText.text = selectedLanguage == UiLanguage.Romanian ? question.HintRo : question.HintEn;
        hintText.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);
        backToQuestionButton.gameObject.SetActive(true);
        backToQuestionButtonText.text = selectedLanguage == UiLanguage.Romanian ? "Inapoi la intrebare" : "Back to the question";

        leaveButton.onClick.RemoveAllListeners();
        backToQuestionButton.onClick.RemoveAllListeners();
        leaveButton.onClick.AddListener(OnLeaveClicked);
        backToQuestionButton.onClick.AddListener(OnBackToQuestionClicked);

        backToQuestionRequested = false;
        while (!backToQuestionRequested && !leaveRequested)
        {
            yield return null;
        }

        if (leaveRequested)
        {
            yield break;
        }

        backToQuestionRequested = false;
        hintText.gameObject.SetActive(false);
        backToQuestionButton.gameObject.SetActive(false);
        titleText.gameObject.SetActive(true);
        subtitleText.gameObject.SetActive(true);
        promptText.gameObject.SetActive(true);
        attemptsText.gameObject.SetActive(true);
        feedbackText.gameObject.SetActive(true);
        codeInput.gameObject.SetActive(true);
        verifyButton.gameObject.SetActive(!activeQuestionSolved && activeAttemptsRemaining > 0);
        hintButton.gameObject.SetActive(!activeQuestionSolved);
        nextButton.gameObject.SetActive(activeQuestionSolved || activeAttemptsRemaining <= 0);
    }

    private IEnumerator ShowLanguageSelection()
    {
        panelImage.gameObject.SetActive(true);
        titleText.gameObject.SetActive(false);
        subtitleText.gameObject.SetActive(false);
        promptText.gameObject.SetActive(false);
        attemptsText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        summaryText.gameObject.SetActive(false);
        hintText.gameObject.SetActive(false);
        codeInput.gameObject.SetActive(false);
        verifyButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        nextButton.gameObject.SetActive(false);
        retryWrongButton.gameObject.SetActive(false);
        backToQuestionButton.gameObject.SetActive(false);

        languagePromptText.text = "Choose language / Alege limba";
        languagePromptText.gameObject.SetActive(true);
        languageRoButton.gameObject.SetActive(true);
        languageEnButton.gameObject.SetActive(true);
        leaveButton.gameObject.SetActive(true);

        languageRoButtonText.text = "Romana";
        languageEnButtonText.text = "English";
        leaveButtonText.text = "Leave";

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

    private int CountCorrectAnswers()
    {
        int count = 0;
        for (int i = 0; i < answeredCorrectly.Length; i++)
        {
            if (answeredCorrectly[i])
            {
                count++;
            }
        }

        return count;
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
        DestroyOverlayByName("Question2PadFadeCanvas");
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
        panelImage = EnsureImage(root, "Panel", new Color(1f, 1f, 1f, 0f));

        titleText = EnsureText(panelImage.transform, "TitleText", new Vector2(0.18f, 0.90f), new Vector2(420f, 48f), titleSize, FontStyle.Bold, new Color(0.08f, 0.08f, 0.1f, 1f), TextAnchor.MiddleLeft);
        subtitleText = EnsureText(panelImage.transform, "SubtitleText", new Vector2(0.72f, 0.90f), new Vector2(320f, 42f), 20, FontStyle.Bold, new Color(0.0f, 0.44f, 0.7f, 1f), TextAnchor.MiddleLeft);
        languagePromptText = EnsureText(panelImage.transform, "LanguagePromptText", new Vector2(0.5f, 0.63f), new Vector2(760f, 70f), 28, FontStyle.Bold, new Color(0.08f, 0.08f, 0.1f, 1f), TextAnchor.MiddleCenter);
        promptText = EnsureText(panelImage.transform, "PromptText", new Vector2(0.24f, 0.61f), new Vector2(420f, 250f), bodySize, FontStyle.Normal, new Color(0.12f, 0.12f, 0.15f, 1f), TextAnchor.UpperLeft);
        attemptsText = EnsureText(panelImage.transform, "AttemptsText", new Vector2(0.24f, 0.28f), new Vector2(420f, 34f), 18, FontStyle.Bold, new Color(0.08f, 0.38f, 0.62f, 1f), TextAnchor.MiddleLeft);
        feedbackText = EnsureText(panelImage.transform, "FeedbackText", new Vector2(0.24f, 0.20f), new Vector2(420f, 56f), 18, FontStyle.Bold, new Color(0.14f, 0.14f, 0.16f, 1f), TextAnchor.UpperLeft);
        summaryText = EnsureText(panelImage.transform, "SummaryText", new Vector2(0.5f, 0.5f), new Vector2(760f, 220f), 26, FontStyle.Bold, new Color(0.08f, 0.08f, 0.1f, 1f), TextAnchor.MiddleCenter);
        hintText = EnsureText(panelImage.transform, "HintText", new Vector2(0.5f, 0.60f), new Vector2(900f, 300f), bodySize, FontStyle.Normal, new Color(0.12f, 0.12f, 0.15f, 1f), TextAnchor.UpperLeft);
        codeInput = EnsureCodeInput(panelImage.transform);
        codePlaceholder = codeInput.placeholder as Text;

        leaveButton = EnsureButton(root, "LeaveButton", new Vector2(0.11f, 0.09f), new Vector2(170f, 52f), new Color(0.11f, 0.11f, 0.14f, 0.94f), "Leave", 24);
        verifyButton = EnsureButton(panelImage.transform, "VerifyButton", new Vector2(0.79f, 0.09f), new Vector2(160f, 48f), new Color(0.0f, 0.49f, 0.76f, 0.96f), "Verify", 24);
        nextButton = EnsureButton(panelImage.transform, "NextButton", new Vector2(0.61f, 0.09f), new Vector2(200f, 48f), new Color(0.12f, 0.56f, 0.32f, 0.96f), "Next Question", 22);
        hintButton = EnsureButton(panelImage.transform, "HintButton", new Vector2(0.44f, 0.09f), new Vector2(140f, 48f), new Color(0.68f, 0.48f, 0.08f, 0.96f), "Hint", 22);
        backToQuestionButton = EnsureButton(root, "BackToQuestionButton", new Vector2(0.31f, 0.09f), new Vector2(230f, 52f), new Color(0.0f, 0.49f, 0.76f, 0.96f), "Back to the question", 22);
        retryWrongButton = EnsureButton(panelImage.transform, "RetryWrongButton", new Vector2(0.5f, 0.18f), new Vector2(290f, 52f), new Color(0.12f, 0.56f, 0.32f, 0.96f), "Retry wrong questions", 22);
        languageRoButton = EnsureButton(panelImage.transform, "LanguageRoButton", new Vector2(0.40f, 0.43f), new Vector2(210f, 62f), new Color(0.0f, 0.49f, 0.76f, 0.96f), "Romana", 24);
        languageEnButton = EnsureButton(panelImage.transform, "LanguageEnButton", new Vector2(0.60f, 0.43f), new Vector2(210f, 62f), new Color(0.14f, 0.58f, 0.42f, 0.96f), "English", 24);

        leaveButtonText = leaveButton.GetComponentInChildren<Text>(true);
        verifyButtonText = verifyButton.GetComponentInChildren<Text>(true);
        nextButtonText = nextButton.GetComponentInChildren<Text>(true);
        hintButtonText = hintButton.GetComponentInChildren<Text>(true);
        backToQuestionButtonText = backToQuestionButton.GetComponentInChildren<Text>(true);
        retryWrongButtonText = retryWrongButton.GetComponentInChildren<Text>(true);
        languageRoButtonText = languageRoButton.GetComponentInChildren<Text>(true);
        languageEnButtonText = languageEnButton.GetComponentInChildren<Text>(true);

        StretchFullscreen(whiteImage.rectTransform);
        StretchFullscreen(panelImage.rectTransform);
        LayoutPanel();
        ConfigureTextSizing();
        EnsureEventSystem();
    }

    private void LayoutPanel()
    {
        panelImage.rectTransform.localScale = Vector3.one;
        Outline panelOutline = panelImage.GetComponent<Outline>() ?? panelImage.gameObject.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0f, 0f, 0.08f);
        panelOutline.effectDistance = new Vector2(2f, -2f);

        codeInput.GetComponent<RectTransform>().anchorMin = new Vector2(0.73f, 0.60f);
        codeInput.GetComponent<RectTransform>().anchorMax = new Vector2(0.73f, 0.60f);
        codeInput.GetComponent<RectTransform>().sizeDelta = new Vector2(600f, 320f);
        codeInput.GetComponent<Image>().color = new Color(0.96f, 0.97f, 1f, 0.96f);
        Outline editorOutline = codeInput.GetComponent<Outline>() ?? codeInput.gameObject.AddComponent<Outline>();
        editorOutline.effectColor = new Color(0f, 0.48f, 0.74f, 0.25f);
        editorOutline.effectDistance = new Vector2(2f, -2f);
    }

    private void ConfigureTextSizing()
    {
        ConfigureResizableText(promptText, 16, bodySize);
        ConfigureResizableText(feedbackText, 16, 18);
        ConfigureResizableText(hintText, 15, bodySize);
        ConfigureResizableText(summaryText, 20, 26);
        ConfigureResizableText(languagePromptText, 22, 28);

        if (codePlaceholder != null)
        {
            codePlaceholder.resizeTextForBestFit = true;
            codePlaceholder.resizeTextMinSize = 14;
            codePlaceholder.resizeTextMaxSize = codeTextSize;
        }
    }

    private static void ConfigureResizableText(Text text, int minSize, int maxSize)
    {
        if (text == null)
        {
            return;
        }

        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = minSize;
        text.resizeTextMaxSize = maxSize;
        text.lineSpacing = 1.05f;
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
        rect.anchorMin = new Vector2(0.73f, 0.56f);
        rect.anchorMax = new Vector2(0.73f, 0.56f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(620f, 380f);

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
            panelImage.color = new Color(1f, 1f, 1f, 0f);
        }

        HideText(titleText);
        HideText(subtitleText);
        HideText(languagePromptText);
        HideText(promptText);
        HideText(attemptsText);
        HideText(feedbackText);
        HideText(summaryText);
        HideText(hintText);

        if (codeInput != null)
        {
            codeInput.text = string.Empty;
            codeInput.gameObject.SetActive(false);
        }

        HideButton(leaveButton);
        HideButton(verifyButton);
        HideButton(nextButton);
        HideButton(hintButton);
        HideButton(backToQuestionButton);
        HideButton(retryWrongButton);
        HideButton(languageRoButton);
        HideButton(languageEnButton);
    }

    private static void HideText(Text text)
    {
        if (text != null)
        {
            text.gameObject.SetActive(false);
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
            panelImage.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        panelImage.color = new Color(1f, 1f, 1f, to);
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

    private void OnNextClicked()
    {
        nextRequested = true;
    }

    private void OnHintClicked()
    {
        hintRequested = true;
    }

    private void OnBackToQuestionClicked()
    {
        backToQuestionRequested = true;
    }

    private void OnRetryWrongClicked()
    {
        retryWrongRequested = true;
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
