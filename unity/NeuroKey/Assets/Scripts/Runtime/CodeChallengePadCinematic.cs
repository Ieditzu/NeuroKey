using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class CodeChallengePadCinematic : MonoBehaviour
{
    private enum QuizLanguage
    {
        Romanian = 0,
        English = 1
    }

    private enum ChallengeMode
    {
        Medium = 0,
        Hard = 1
    }

    private struct CodeChallenge
    {
        public string Prompt;
        public string InitialCode;
        public string ExpectedCode;
        public string Hint;
        public string ValidationId;

        public CodeChallenge(string prompt, string initialCode, string expectedCode, string hint, string validationId = "")
        {
            Prompt = prompt;
            InitialCode = initialCode;
            ExpectedCode = expectedCode;
            Hint = hint;
            ValidationId = validationId;
        }
    }

    [SerializeField] private ChallengeMode mode = ChallengeMode.Medium;

    [Header("Portal")]
    [SerializeField] private float portalForwardDistance = 1.35f;
    [SerializeField] private float portalSideDistance = 2.6f;
    [SerializeField] private float portalHeightOffset = 2.7f;
    [SerializeField] private float portalSpawnDuration = 0.42f;
    [SerializeField] private float lookAtPortalDuration = 0.5f;
    [SerializeField] private float waitBeforeSuction = 0.45f;
    [SerializeField] private float suctionDuration = 0.22f;
    [SerializeField] private float portalCollapseDuration = 0.28f;

    [Header("Exit")]
    [SerializeField] private float reenterCooldown = 0f;
    [SerializeField] private float exitHeightOffset = 0.55f;
    [SerializeField] private float exitBackOffset = 0.15f;
    [SerializeField] private float exitMoveDuration = 0.24f;

    [Header("Screen")]
    [SerializeField] private float fadeToWhiteDuration = 0.34f;

    [Header("Theme")]
    [SerializeField] private Color pageTint = new Color(0.98f, 0.98f, 0.96f, 1f);
    [SerializeField] private Color cardColor = new Color(1f, 1f, 1f, 0f);
    [SerializeField] private Color editorColor = new Color(1f, 1f, 1f, 0.96f);
    [SerializeField] private Color textColor = new Color(0.1f, 0.1f, 0.1f, 1f);
    [SerializeField] private Color editorTextColor = new Color(0.08f, 0.09f, 0.12f, 1f);
    [SerializeField] private Color accentColor = new Color(0.12f, 0.55f, 0.98f, 1f);
    [SerializeField] private Color secondaryAccentColor = new Color(0.0f, 0.72f, 0.74f, 1f);
    [SerializeField] private Color correctColor = new Color(0.18f, 0.62f, 0.32f, 1f);
    [SerializeField] private Color wrongColor = new Color(0.83f, 0.22f, 0.22f, 1f);
    [SerializeField] private Color buttonColor = new Color(0.14f, 0.14f, 0.14f, 0.92f);
    [SerializeField] private Color pulseColor = new Color(0.82f, 0.96f, 1f, 1f);

    [Header("Typography")]
    [SerializeField] private int titleSize = 40;
    [SerializeField] private int questionSize = 26;
    [SerializeField] private int bodySize = 22;
    [SerializeField] private int buttonTextSize = 24;
    [SerializeField] private int codeTextSize = 22;

    private static readonly CodeChallenge[] MediumChallenges =
    {
        new CodeChallenge(
            "Debugging 1\n\nCodul trebuie sa dubleze valoarea primita si sa afiseze rezultatul.\n\nProblema:\n- functia intoarce doar valoarea initiala\n- lipseste inmultirea cu 2\n\nRepara codul din editorul din dreapta fara sa schimbi numele functiei.",
            "#include <iostream>\nusing namespace std;\n\nint MultiplyByTwo(int value)\n{\n    int doubled = value;\n    return doubled;\n}\n\nint main()\n{\n    cout << MultiplyByTwo(6) << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nint MultiplyByTwo(int value)\n{\n    int doubled = value * 2;\n    return doubled;\n}\n\nint main()\n{\n    cout << MultiplyByTwo(6) << endl;\n    return 0;\n}",
            "Hint:\n\nBugul este in functia `MultiplyByTwo`.\nVariabila `doubled` trebuie sa primeasca `value * 2`, nu doar `value`.\n\nPartea corecta este:\n`int doubled = value * 2;`",
            "medium_multiply"),
        new CodeChallenge(
            "Debugging 2\n\nCodul trebuie sa calculeze suma a doua numere.\n\nProblema:\n- functia foloseste operatorul gresit\n- in loc de suma face scadere\n\nSchimba doar linia gresita din functie.",
            "#include <iostream>\nusing namespace std;\n\nint Sum(int a, int b)\n{\n    return a - b;\n}\n\nint main()\n{\n    int total = Sum(4, 6);\n    cout << total << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nint Sum(int a, int b)\n{\n    return a + b;\n}\n\nint main()\n{\n    int total = Sum(4, 6);\n    cout << total << endl;\n    return 0;\n}",
            "Hint:\n\nProblema este in instructiunea `return`.\nPentru suma trebuie folosit operatorul `+`, nu `-`.\n\nLinia corecta este:\n`return a + b;`",
            "medium_sum"),
        new CodeChallenge(
            "Debugging 3\n\nCodul trebuie sa verifice daca un numar este par.\n\nProblema:\n- expresia logica este inversata\n- functia intoarce `true` pentru numere impare\n\nCorecteaza doar comparatia.",
            "#include <iostream>\nusing namespace std;\n\nbool IsEven(int number)\n{\n    return number % 2 != 0;\n}\n\nint main()\n{\n    cout << (IsEven(8) ? \"even\" : \"odd\") << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nbool IsEven(int number)\n{\n    return number % 2 == 0;\n}\n\nint main()\n{\n    cout << (IsEven(8) ? \"even\" : \"odd\") << endl;\n    return 0;\n}",
            "Hint:\n\nUn numar par are restul `0` la impartirea la 2.\nComparatia corecta este `== 0`.\n\nLinia buna este:\n`return number % 2 == 0;`",
            "medium_even"),
        new CodeChallenge(
            "Debugging 4\n\nCodul trebuie sa incrementeze variabila originala cu 1.\n\nProblema:\n- functia primeste parametrul prin valoare\n- modificarea nu ajunge inapoi in `main`\n\nRepara semnatura functiei.",
            "#include <iostream>\nusing namespace std;\n\nvoid Increment(int n)\n{\n    n++;\n}\n\nint main()\n{\n    int value = 5;\n    Increment(value);\n    cout << value << endl;\n    return 0;\n}",
            "#include <iostream>\nusing namespace std;\n\nvoid Increment(int& n)\n{\n    n++;\n}\n\nint main()\n{\n    int value = 5;\n    Increment(value);\n    cout << value << endl;\n    return 0;\n}",
            "Hint:\n\nDaca vrei sa modifici variabila originala, parametrul trebuie trimis prin referinta.\nAsta inseamna ca functia trebuie sa primeasca `int& n`.\n\nPartea corecta este:\n`void Increment(int& n)`",
            "medium_increment")
    };

    private static readonly CodeChallenge[] HardChallenges =
    {
        new CodeChallenge(
            "Cerinta 1\n\nScrie functia `bool IsEven(int n)`.\n\nCe trebuie sa faca:\n- primeste un numar intreg\n- intoarce `true` daca numarul este par\n- intoarce `false` daca numarul este impar\n\nPoti rezolva in mai multe moduri, dar functia trebuie sa mearga corect.",
            "bool IsEven(int n)\n{\n    \n}",
            "bool IsEven(int n)\n{\n    return n % 2 == 0;\n}",
            "Explicatie:\n\nUn numar par are restul 0 la impartirea la 2.\nPoti scrie direct `return n % 2 == 0;` sau poti folosi un `if` care intoarce `true` si `false`.\nImportant este sa verifici corect paritatea.\n\nO rezolvare buna este:\n\nbool IsEven(int n)\n{\n    return n % 2 == 0;\n}",
            "hard_is_even"),
        new CodeChallenge(
            "Cerinta 2\n\nScrie functia `int MaxOfTwo(int a, int b)`.\n\nCe trebuie sa faca:\n- compara cele doua valori primite\n- intoarce numarul mai mare\n\nRezolva direct in editorul din dreapta.",
            "int MaxOfTwo(int a, int b)\n{\n    \n}",
            "int MaxOfTwo(int a, int b)\n{\n    return a > b ? a : b;\n}",
            "Explicatie:\n\nPoti rezolva cu operatorul ternar sau cu `if/else`.\nImportant este ca functia sa intoarca valoarea mai mare dintre `a` si `b`.\n\nO rezolvare buna este:\n\nint MaxOfTwo(int a, int b)\n{\n    return a > b ? a : b;\n}",
            "hard_max"),
        new CodeChallenge(
            "Cerinta 3\n\nScrie functia `int Square(int x)`.\n\nCe trebuie sa faca:\n- primeste un numar `x`\n- intoarce patratul lui\n\nScrie functia complet.",
            "int Square(int x)\n{\n    \n}",
            "int Square(int x)\n{\n    return x * x;\n}",
            "Explicatie:\n\nPatratul unui numar inseamna numarul inmultit cu el insusi.\nPoti scrie direct `return x * x;` sau poti folosi o variabila intermediara si apoi `return`.\n\nO rezolvare buna este:\n\nint Square(int x)\n{\n    return x * x;\n}",
            "hard_square"),
        new CodeChallenge(
            "Cerinta 4\n\nScrie functia `int Sum3(int a, int b, int c)`.\n\nCe trebuie sa faca:\n- primeste trei numere intregi\n- intoarce suma lor\n\nNu exista o singura forma corecta.",
            "int Sum3(int a, int b, int c)\n{\n    \n}",
            "int Sum3(int a, int b, int c)\n{\n    return a + b + c;\n}",
            "Explicatie:\n\nPoti face suma direct in `return` sau printr-o variabila auxiliara.\nImportant este ca rezultatul final sa fie `a + b + c`.\n\nO rezolvare buna este:\n\nint Sum3(int a, int b, int c)\n{\n    return a + b + c;\n}",
            "hard_sum3"),
        new CodeChallenge(
            "Cerinta 5\n\nScrie functia `int Factorial3()`.\n\nCe trebuie sa faca:\n- intoarce factorialul lui 3\n\nAmintire:\nfactorialul unui numar inseamna produsul numerelor de la acel numar pana la 1.",
            "int Factorial3()\n{\n    \n}",
            "int Factorial3()\n{\n    return 3 * 2 * 1;\n}",
            "Explicatie:\n\nPoti intoarce direct `6`, poti scrie `3 * 2 * 1`, sau poti folosi o variabila si sa o inmultesti pe rand.\nImportant este ca functia sa intoarca rezultatul corect pentru factorialul lui 3.\n\nO rezolvare buna este:\n\nint Factorial3()\n{\n    return 3 * 2 * 1;\n}",
            "hard_factorial3")
    };

    private static Canvas overlayCanvas;
    private static Image whiteImage;
    private static Image panelImage;
    private static Text titleText;
    private static Text counterText;
    private static Text promptText;
    private static Text feedbackText;
    private static Text languagePromptText;
    private static InputField codeInput;
    private static Text codeInputText;
    private static Text codePlaceholder;
    private static Text hintScreenText;
    private static Button leaveButton;
    private static Text leaveButtonText;
    private static Button verifyButton;
    private static Text verifyButtonText;
    private static Button backButton;
    private static Text backButtonText;
    private static Button hintButton;
    private static Text hintButtonText;
    private static Button continueButton;
    private static Text continueButtonText;
    private static Button retryWrongButton;
    private static Text retryWrongButtonText;
    private static Button languageRoButton;
    private static Text languageRoButtonText;
    private static Button languageEnButton;
    private static Text languageEnButtonText;
    private static Button hintScreenBackButton;
    private static Text hintScreenBackButtonText;

    private Collider triggerCollider;
    private bool running;
    private bool leaveRequested;
    private bool verifyRequested;
    private bool backRequested;
    private bool hintRequested;
    private bool continueRequested;
    private bool retryWrongRequested;
    private bool hintScreenBackRequested;
    private bool languageChosen;
    private int score;
    private GameObject activePortal;
    private bool overlayInteractionActive;
    private QuizLanguage selectedLanguage = QuizLanguage.Romanian;

    public void ConfigureForPad(bool useHardMode)
    {
        mode = useHardMode ? ChallengeMode.Hard : ChallengeMode.Medium;
    }

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        TryStartSequence(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryStartSequence(other);
    }

    private void TryStartSequence(Collider other)
    {
        if (running)
        {
            return;
        }

        if (!TryGetPlayer(other, out BeanController sphere, out FirstPersonControllerSimple fps))
        {
            return;
        }

        StartCoroutine(PlaySequence(sphere, fps));
    }

    private void LateUpdate()
    {
        if (overlayInteractionActive)
        {
            SetCursorVisible(true);
        }
    }

    private IEnumerator PlaySequence(BeanController sphere, FirstPersonControllerSimple fps)
    {
        running = true;
        leaveRequested = false;

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        SetPlayerLockState(sphere, fps, true, true);
        if (fps != null)
        {
            fps.SetCameraControlEnabled(false);
        }

        Transform playerRoot = fps != null ? fps.transform : sphere != null ? sphere.transform : null;
        Camera activeCamera = ResolveCamera(fps);
        Transform camTransform = activeCamera != null ? activeCamera.transform : null;
        if (playerRoot == null || camTransform == null)
        {
            RestorePlayerState(sphere, fps);
            yield break;
        }

        Vector3 startPosition = playerRoot.position;
        Quaternion startRotation = playerRoot.rotation;
        Vector3 startCameraLocalPosition = camTransform.localPosition;
        Quaternion startCameraLocalRotation = camTransform.localRotation;
        float startFov = activeCamera.fieldOfView;

        Vector3 portalPosition = GetPortalPosition(playerRoot, startPosition);
        Quaternion portalRotation = Quaternion.LookRotation((startPosition + Vector3.up) - portalPosition, Vector3.up);

        activePortal = CreatePortal(portalPosition, portalRotation);
        yield return AnimatePortalAppear(activePortal.transform);
        yield return TurnPlayerTowardPortal(playerRoot, camTransform, activeCamera, startCameraLocalPosition, startCameraLocalRotation, startFov, portalPosition);
        yield return new WaitForSeconds(waitBeforeSuction);
        yield return PullPlayerToPortal(playerRoot, camTransform, activeCamera, startCameraLocalPosition, startCameraLocalRotation, startFov, portalPosition);

        EnsureOverlay();
        ResetOverlay();
        SetOverlayVisible(true);
        yield return FadeImage(whiteImage, 0f, 1f, fadeToWhiteDuration, pageTint);
        whiteImage.color = new Color(pageTint.r, pageTint.g, pageTint.b, 1f);
        yield return null;
        ShowLeaveButton(true);
        yield return PromptForLanguageSelection();
        if (!leaveRequested)
        {
            ShowMainUi(true);
        }
        yield return RunChallenges();
        if (!leaveRequested)
        {
            ShowMainUi(false);
            ShowLeaveButton(false);
            ResetOverlay();
        }

        Vector3 exitPosition = GetSafeExitPosition(startPosition, playerRoot);
        if (activePortal == null)
        {
            activePortal = CreatePortal(portalPosition, portalRotation);
            yield return AnimatePortalAppear(activePortal.transform);
        }

        Vector3 facing = exitPosition - portalPosition;
        if (facing.sqrMagnitude < 0.0001f)
        {
            facing = playerRoot.forward;
        }

        TeleportPlayer(sphere, fps, portalPosition - facing.normalized * 0.15f, Quaternion.LookRotation(facing.normalized, Vector3.up));
        camTransform.localPosition = startCameraLocalPosition;
        camTransform.localRotation = startCameraLocalRotation;
        activeCamera.fieldOfView = startFov;

        yield return FadeImage(whiteImage, 1f, 0f, 0.2f, pageTint);
        yield return MovePlayerOut(playerRoot, exitPosition, startRotation);
        yield return CollapsePortal(activePortal);

        RestorePlayerState(sphere, fps);
        StartCoroutine(ReenableTriggerAfterDelay());
    }

    private IEnumerator RunChallenges()
    {
        CodeChallenge[] challenges = mode == ChallengeMode.Medium ? MediumChallenges : HardChallenges;
        score = 0;
        string[] answers = new string[challenges.Length];
        bool[] solved = new bool[challenges.Length];
        int[] attemptsLeft = new int[challenges.Length];
        for (int i = 0; i < challenges.Length; i++)
        {
            answers[i] = challenges[i].InitialCode;
            attemptsLeft[i] = GetAttemptsAllowed();
        }

        int current = 0;

        while (current < challenges.Length && !leaveRequested)
        {
            CodeChallenge challenge = challenges[current];
            verifyRequested = false;
            backRequested = false;
            hintRequested = false;
            continueRequested = false;

            ApplyChallengeContent(challenge, current, challenges.Length, answers[current], attemptsLeft[current]);

            verifyButton.onClick.RemoveAllListeners();
            verifyButton.onClick.AddListener(OnVerifyClicked);
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
            hintButton.onClick.RemoveAllListeners();
            hintButton.onClick.AddListener(OnHintClicked);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinueClicked);
            retryWrongButton.onClick.RemoveAllListeners();
            retryWrongButton.onClick.AddListener(OnRetryWrongClicked);

            ShowMainUi(true);
            ShowChallengeButtons(current > 0, true, true, false, false);

            while (!verifyRequested && !backRequested && !leaveRequested)
            {
                if (hintRequested)
                {
                    hintRequested = false;
                    yield return ShowHintScreen(challenge.Hint);
                    if (leaveRequested)
                    {
                        yield break;
                    }
                    ShowMainUi(true);
                    ShowChallengeButtons(current > 0, true, true, false, false);
                }

                yield return null;
            }

            if (leaveRequested)
            {
                yield break;
            }

            if (backRequested)
            {
                answers[current] = codeInput.text;
                current = Mathf.Max(0, current - 1);
                continue;
            }

            bool correct = IsChallengeCorrect(challenge, codeInput.text);
            answers[current] = codeInput.text;

            if (correct)
            {
                if (!solved[current])
                {
                    solved[current] = true;
                    score++;
                }

                feedbackText.text = Localize("Raspuns corect. Apasa pe urmatoarea intrebare.", "Correct answer. Press next question.");
                feedbackText.color = correctColor;
                ShowChallengeButtons(current > 0, true, false, true, false);
                while (!continueRequested && !backRequested && !leaveRequested)
                {
                    if (hintRequested)
                    {
                        hintRequested = false;
                        yield return ShowHintScreen(challenge.Hint);
                        if (leaveRequested)
                        {
                            yield break;
                        }
                        ShowMainUi(true);
                        ShowChallengeButtons(current > 0, true, false, true, false);
                        feedbackText.text = Localize("Raspuns corect. Apasa pe urmatoarea intrebare.", "Correct answer. Press next question.");
                        feedbackText.color = correctColor;
                    }

                    yield return null;
                }

                if (leaveRequested)
                {
                    yield break;
                }

                if (backRequested)
                {
                    current = Mathf.Max(0, current - 1);
                    continue;
                }

                current++;
                continue;
            }

            attemptsLeft[current] = Mathf.Max(0, attemptsLeft[current] - 1);
            if (attemptsLeft[current] > 0)
            {
                feedbackText.text = Localize("Incorect. Mai ai ", "Incorrect. You have ") + attemptsLeft[current] + Localize(" incercari.", " attempts left.");
                feedbackText.color = wrongColor;
            }
            else
            {
                feedbackText.text = Localize("Ai ramas fara incercari. Trecem automat la urmatoarea intrebare.", "No attempts left. Moving to the next question.");
                feedbackText.color = wrongColor;
                yield return new WaitForSeconds(1f);
                current++;
                continue;
            }

            feedbackText.color = wrongColor;
            verifyRequested = false;
            while (!verifyRequested && !backRequested && !leaveRequested)
            {
                if (hintRequested)
                {
                    hintRequested = false;
                    yield return ShowHintScreen(challenge.Hint);
                    if (leaveRequested)
                    {
                        yield break;
                    }
                    ShowMainUi(true);
                    ShowChallengeButtons(current > 0, true, true, false, false);
                    feedbackText.text = Localize("Incorect. Mai ai ", "Incorrect. You have ") + attemptsLeft[current] + Localize(" incercari.", " attempts left.");
                    feedbackText.color = wrongColor;
                }

                yield return null;
            }

            if (backRequested)
            {
                answers[current] = codeInput.text;
                current = Mathf.Max(0, current - 1);
            }
        }

        titleText.text = mode == ChallengeMode.Medium
            ? Localize("Medium complet", "Medium complete")
            : Localize("Hard complet", "Hard complete");
        counterText.text = Localize("Rezultat", "Result");
        promptText.text = Localize("Ai rezolvat corect ", "You solved ") + score + Localize(" din ", " out of ") + challenges.Length + Localize(" provocari.", " challenges correctly.");
        feedbackText.text = Localize("Poti reface intrebarile gresite sau poti iesi.", "You can retry the wrong questions or leave.");
        feedbackText.color = correctColor;
        codeInput.gameObject.SetActive(false);
        ShowChallengeButtons(false, false, false, false, false);

        bool hasWrong = false;
        for (int i = 0; i < solved.Length; i++)
        {
            if (!solved[i])
            {
                hasWrong = true;
                break;
            }
        }

        retryWrongButton.gameObject.SetActive(hasWrong);
        retryWrongRequested = false;
        while (!leaveRequested && !(hasWrong && retryWrongRequested))
        {
            yield return null;
        }

        retryWrongButton.gameObject.SetActive(false);
        if (leaveRequested || !hasWrong)
        {
            yield break;
        }

        int retryCount = 0;
        for (int i = 0; i < solved.Length; i++)
        {
            if (!solved[i])
            {
                retryCount++;
            }
        }

        CodeChallenge[] retryChallenges = new CodeChallenge[retryCount];
        string[] retryAnswers = new string[retryCount];
        bool[] retrySolved = new bool[retryCount];
        int[] retryAttempts = new int[retryCount];
        int[] retrySourceIndex = new int[retryCount];
        int writeIndex = 0;
        for (int i = 0; i < solved.Length; i++)
        {
            if (!solved[i])
            {
                retryChallenges[writeIndex] = challenges[i];
                retryAnswers[writeIndex] = challenges[i].InitialCode;
                retrySolved[writeIndex] = false;
                retryAttempts[writeIndex] = GetAttemptsAllowed();
                retrySourceIndex[writeIndex] = i;
                writeIndex++;
            }
        }

        yield return RunRetryChallenges(retryChallenges, retryAnswers, retrySolved, retryAttempts, retrySourceIndex, solved);
    }

    private string NormalizeCode(string value)
    {
        string normalized = value.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimEnd();
        }

        return string.Join("\n", lines);
    }

    private int GetAttemptsAllowed()
    {
        return mode == ChallengeMode.Hard ? 5 : 4;
    }

    private bool IsChallengeCorrect(CodeChallenge challenge, string submittedCode)
    {
        string actual = NormalizeCode(submittedCode);
        string expected = NormalizeCode(challenge.ExpectedCode);

        if (string.IsNullOrEmpty(challenge.ValidationId))
        {
            return actual == expected;
        }

        string compact = CompactCode(actual);
        switch (challenge.ValidationId)
        {
            case "medium_multiply":
                return compact.Contains("intmultiplybytwo(intvalue){intdoubled=value*2;returndoubled;}");
            case "medium_sum":
                return compact.Contains("intsum(inta,intb){returna+b;}");
            case "medium_even":
                return compact.Contains("booliseven(intnumber){returnnumber%2==0;}");
            case "medium_increment":
                return compact.Contains("voidincrement(int&n){n++;}") || compact.Contains("voidincrement(int&n){++n;}");
            case "hard_is_even":
                return compact.Contains("booliseven(intn){returnn%2==0;}")
                    || compact.Contains("booliseven(intn){return!(n%2);}")
                    || compact.Contains("booliseven(intn){if(n%2==0)returntrue;returnfalse;}")
                    || compact.Contains("booliseven(intn){if(n%2==0){returntrue;}returnfalse;}");
            case "hard_max":
                return compact.Contains("intmaxoftwo(inta,intb){returna>b?a:b;}")
                    || compact.Contains("intmaxoftwo(inta,intb){if(a>b)returna;returnb;}")
                    || compact.Contains("intmaxoftwo(inta,intb){if(a>b){returna;}else{returnb;}}");
            case "hard_square":
                return compact.Contains("intsquare(intx){returnx*x;}")
                    || compact.Contains("intsquare(intx){intresult=x*x;returnresult;}");
            case "hard_sum3":
                return compact.Contains("intsum3(inta,intb,intc){returna+b+c;}")
                    || compact.Contains("intsum3(inta,intb,intc){inttotal=a+b+c;returntotal;}");
            case "hard_factorial3":
                return compact.Contains("intfactorial3(){return3*2*1;}")
                    || compact.Contains("intfactorial3(){return6;}")
                    || compact.Contains("intfactorial3(){intresult=3*2*1;returnresult;}");
            default:
                return actual == expected;
        }
    }

    private string CompactCode(string value)
    {
        char[] chars = value.ToCharArray();
        System.Text.StringBuilder builder = new System.Text.StringBuilder(chars.Length);
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsWhiteSpace(chars[i]))
            {
                builder.Append(char.ToLowerInvariant(chars[i]));
            }
        }

        return builder.ToString();
    }

    private IEnumerator ShowHintScreen(string hintText)
    {
        hintScreenBackRequested = false;
        SetOverlayVisible(true);
        ShowMainUi(false);
        ShowChallengeButtons(false, false, false, false, false);

        hintScreenText.text = hintText;
        LayoutHintScreen();
        hintScreenText.gameObject.SetActive(true);
        hintScreenBackButton.onClick.RemoveAllListeners();
        hintScreenBackButton.onClick.AddListener(OnHintScreenBackClicked);
        hintScreenBackButton.gameObject.SetActive(true);
        SetCursorVisible(true);

        while (!hintScreenBackRequested && !leaveRequested)
        {
            yield return null;
        }

        hintScreenText.gameObject.SetActive(false);
        hintScreenBackButton.gameObject.SetActive(false);
        if (leaveRequested)
        {
            yield break;
        }
    }

    private void ShowMainUi(bool visible)
    {
        if (panelImage == null || titleText == null || counterText == null || promptText == null || feedbackText == null || codeInput == null)
        {
            return;
        }

        if (visible)
        {
            SetOverlayVisible(true);
        }

        overlayInteractionActive = visible;
        panelImage.gameObject.SetActive(visible);
        titleText.gameObject.SetActive(visible);
        counterText.gameObject.SetActive(visible);
        promptText.gameObject.SetActive(visible);
        feedbackText.gameObject.SetActive(visible);
        codeInput.gameObject.SetActive(visible);
        if (visible)
        {
            ApplyLocalizedStaticTexts();
            LayoutMainScreen();
            SetCursorVisible(true);
            if (codeInput != null)
            {
                RectTransform codeRect = codeInput.GetComponent<RectTransform>();
                if (codeRect != null)
                {
                    codeRect.localScale = Vector3.one;
                }
                codeInput.ActivateInputField();
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(codeInput.gameObject);
                }
            }
        }
    }

    private void ShowChallengeButtons(bool showBack, bool showHint, bool showVerify, bool showContinue, bool showRetryWrong)
    {
        if (backButton == null || hintButton == null || verifyButton == null || continueButton == null || retryWrongButton == null)
        {
            return;
        }

        backButton.gameObject.SetActive(showBack);
        hintButton.gameObject.SetActive(showHint);
        verifyButton.gameObject.SetActive(showVerify);
        continueButton.gameObject.SetActive(showContinue);
        retryWrongButton.gameObject.SetActive(showRetryWrong);
    }

    private void ApplyChallengeContent(CodeChallenge challenge, int currentIndex, int total, string currentAnswer, int attemptsLeft)
    {
        ApplyLocalizedStaticTexts();
        titleText.text = mode == ChallengeMode.Medium
            ? Localize("Medium C++ Debugging", "Medium C++ Debugging")
            : Localize("Hard C++ Coding", "Hard C++ Coding");
        counterText.text = Localize("Intrebarea ", "Question ") + (currentIndex + 1) + " / " + total;
        promptText.text = challenge.Prompt;
        codeInput.text = currentAnswer;
        feedbackText.text = Localize("Incercari ramase: ", "Attempts left: ") + attemptsLeft + " / " + GetAttemptsAllowed();
        feedbackText.color = textColor;
    }

    private IEnumerator RunRetryChallenges(CodeChallenge[] retryChallenges, string[] retryAnswers, bool[] retrySolved, int[] retryAttempts, int[] retrySourceIndex, bool[] solved)
    {
        int current = 0;
        while (current < retryChallenges.Length && !leaveRequested)
        {
            CodeChallenge challenge = retryChallenges[current];
            verifyRequested = false;
            backRequested = false;
            hintRequested = false;
            continueRequested = false;

            ApplyLocalizedStaticTexts();
            titleText.text = Localize("Refacere intrebari gresite", "Retry wrong questions");
            counterText.text = Localize("Refacere ", "Retry ") + (current + 1) + " / " + retryChallenges.Length;
            promptText.text = challenge.Prompt;
            codeInput.text = retryAnswers[current];
            feedbackText.text = Localize("Incercari ramase: ", "Attempts left: ") + retryAttempts[current] + " / " + GetAttemptsAllowed();
            feedbackText.color = textColor;

            ShowMainUi(true);
            ShowChallengeButtons(current > 0, true, true, false, false);

            while (!verifyRequested && !backRequested && !leaveRequested)
            {
                if (hintRequested)
                {
                    hintRequested = false;
                    yield return ShowHintScreen(challenge.Hint);
                    if (leaveRequested)
                    {
                        yield break;
                    }
                    ShowMainUi(true);
                    ShowChallengeButtons(current > 0, true, true, false, false);
                }

                yield return null;
            }

            if (leaveRequested)
            {
                yield break;
            }

            if (backRequested)
            {
                retryAnswers[current] = codeInput.text;
                current = Mathf.Max(0, current - 1);
                continue;
            }

            retryAnswers[current] = codeInput.text;
            if (IsChallengeCorrect(challenge, codeInput.text))
            {
                retrySolved[current] = true;
                solved[retrySourceIndex[current]] = true;
                feedbackText.text = Localize("Raspuns corect. Apasa pe urmatoarea intrebare.", "Correct answer. Press next question.");
                feedbackText.color = correctColor;
                ShowChallengeButtons(current > 0, true, false, true, false);

                while (!continueRequested && !leaveRequested)
                {
                    yield return null;
                }

                if (leaveRequested)
                {
                    yield break;
                }

                current++;
                continue;
            }

            retryAttempts[current] = Mathf.Max(0, retryAttempts[current] - 1);
            if (retryAttempts[current] > 0)
            {
                feedbackText.text = Localize("Incorect. Mai ai ", "Incorrect. You have ") + retryAttempts[current] + Localize(" incercari.", " attempts left.");
                feedbackText.color = wrongColor;
                continue;
            }

            feedbackText.text = Localize("Ai ramas fara incercari. Trecem automat la urmatoarea intrebare.", "No attempts left. Moving to the next question.");
            feedbackText.color = wrongColor;
            yield return new WaitForSeconds(1f);
            current++;
        }
    }

    private void OnVerifyClicked() => verifyRequested = true;
    private void OnBackClicked() => backRequested = true;
    private void OnHintClicked() => hintRequested = true;
    private void OnContinueClicked() => continueRequested = true;
    private void OnRetryWrongClicked() => retryWrongRequested = true;
    private void OnHintScreenBackClicked() => hintScreenBackRequested = true;

    private IEnumerator ReenableTriggerAfterDelay()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, reenterCooldown));
        if (triggerCollider != null)
        {
            triggerCollider.enabled = true;
        }
        running = false;
    }

    private void RestorePlayerState(BeanController sphere, FirstPersonControllerSimple fps)
    {
        SetPlayerLockState(sphere, fps, false, false);
        if (fps != null)
        {
            fps.SetCameraControlEnabled(true);
        }
        overlayInteractionActive = false;
        TearDownOverlay();
        SetCursorVisible(false);
        running = false;
    }

    private Vector3 GetPortalPosition(Transform playerRoot, Vector3 playerPosition)
    {
        return playerPosition
            + (playerRoot.forward * portalForwardDistance)
            + (playerRoot.right * portalSideDistance)
            + (Vector3.up * portalHeightOffset);
    }

    private Vector3 GetSafeExitPosition(Vector3 fallbackPosition, Transform playerRoot)
    {
        Bounds padBounds = triggerCollider != null ? triggerCollider.bounds : new Bounds(transform.position, Vector3.one);
        Vector3 candidate = padBounds.center;
        candidate.y = padBounds.max.y + exitHeightOffset;
        candidate -= transform.forward * exitBackOffset;

        Vector3 rayOrigin = new Vector3(candidate.x, padBounds.max.y + 4f, candidate.z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 12f, ~0, QueryTriggerInteraction.Ignore))
        {
            candidate.y = hit.point.y + exitHeightOffset;
        }
        else
        {
            candidate = new Vector3(padBounds.center.x, padBounds.max.y + exitHeightOffset, padBounds.center.z);
        }

        CharacterController controller = playerRoot != null ? playerRoot.GetComponent<CharacterController>() : null;
        if (controller != null)
        {
            candidate.y += Mathf.Max(0.1f, controller.skinWidth + (controller.height * 0.08f));
        }

        if (float.IsNaN(candidate.x) || float.IsNaN(candidate.y) || float.IsNaN(candidate.z))
        {
            candidate = fallbackPosition + Vector3.up * exitHeightOffset;
        }

        return candidate;
    }

    private IEnumerator TurnPlayerTowardPortal(Transform playerRoot, Transform camTransform, Camera activeCamera, Vector3 baseCameraLocalPosition, Quaternion baseCameraLocalRotation, float baseFov, Vector3 portalPosition)
    {
        Vector3 lookDirection = portalPosition - playerRoot.position;
        lookDirection.y = 0f;
        if (lookDirection.sqrMagnitude < 0.0001f)
        {
            lookDirection = playerRoot.forward;
        }

        Quaternion fromRotation = playerRoot.rotation;
        Quaternion toRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        float elapsed = 0f;
        while (elapsed < lookAtPortalDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, lookAtPortalDuration));
            playerRoot.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
            camTransform.localPosition = Vector3.Lerp(baseCameraLocalPosition, baseCameraLocalPosition + new Vector3(0f, 0.012f, 0f), t);
            camTransform.localRotation = Quaternion.Slerp(baseCameraLocalRotation, baseCameraLocalRotation * Quaternion.Euler(1f, 0f, 1.4f), t);
            activeCamera.fieldOfView = Mathf.Lerp(baseFov, baseFov + 8f, t);
            yield return null;
        }
    }

    private IEnumerator PullPlayerToPortal(Transform playerRoot, Transform camTransform, Camera activeCamera, Vector3 baseCameraLocalPosition, Quaternion baseCameraLocalRotation, float baseFov, Vector3 portalPosition)
    {
        Vector3 fromPosition = playerRoot.position;
        Vector3 toPosition = portalPosition - ((portalPosition - fromPosition).normalized * 0.22f);
        Quaternion fromRotation = playerRoot.rotation;
        Quaternion toRotation = Quaternion.LookRotation((portalPosition - fromPosition).normalized, Vector3.up);
        float elapsed = 0f;
        while (elapsed < suctionDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, suctionDuration));
            playerRoot.position = Vector3.Lerp(fromPosition, toPosition, t) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * 0.08f);
            playerRoot.rotation = Quaternion.Slerp(fromRotation, toRotation, t);
            camTransform.localRotation = Quaternion.Slerp(baseCameraLocalRotation, baseCameraLocalRotation * Quaternion.Euler(0.8f, 0f, 1.6f), t);
            activeCamera.fieldOfView = Mathf.Lerp(baseFov + 3f, baseFov + 8f, t);
            yield return null;
        }
    }

    private IEnumerator MovePlayerOut(Transform playerRoot, Vector3 toPosition, Quaternion toRotation)
    {
        Vector3 startPosition = playerRoot.position;
        Quaternion startRotation = playerRoot.rotation;
        float elapsed = 0f;
        while (elapsed < exitMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, exitMoveDuration));
            playerRoot.position = Vector3.Lerp(startPosition, toPosition, t);
            playerRoot.rotation = Quaternion.Slerp(startRotation, toRotation, t);
            yield return null;
        }
    }

    private IEnumerator AnimatePortalAppear(Transform portalTransform)
    {
        Vector3 targetScale = portalTransform.localScale;
        portalTransform.localScale = Vector3.zero;
        float elapsed = 0f;
        while (elapsed < portalSpawnDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseOut(elapsed / Mathf.Max(0.01f, portalSpawnDuration));
            portalTransform.localScale = targetScale * t;
            yield return null;
        }
    }

    private IEnumerator CollapsePortal(GameObject portal)
    {
        if (portal == null)
        {
            yield break;
        }

        Transform portalTransform = portal.transform;
        Vector3 startScale = portalTransform.localScale;
        float elapsed = 0f;
        while (elapsed < portalCollapseDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, portalCollapseDuration));
            portalTransform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(portal);
        activePortal = null;
    }

    private static void TeleportPlayer(BeanController sphere, FirstPersonControllerSimple fps, Vector3 position, Quaternion rotation)
    {
        if (fps != null)
        {
            CharacterController controller = fps.GetComponent<CharacterController>();
            bool wasEnabled = controller != null && controller.enabled;
            if (controller != null)
            {
                controller.enabled = false;
            }
            fps.transform.SetPositionAndRotation(position, rotation);
            if (controller != null)
            {
                controller.enabled = wasEnabled;
            }
        }

        if (sphere != null)
        {
            Rigidbody rb = sphere.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.position = position;
                rb.rotation = rotation;
            }
            sphere.transform.SetPositionAndRotation(position, rotation);
        }
    }

    private static void SetPlayerLockState(BeanController sphere, FirstPersonControllerSimple fps, bool movementLocked, bool hardFreeze)
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

    private static Camera ResolveCamera(FirstPersonControllerSimple fps)
    {
        if (fps != null)
        {
            Camera childCamera = fps.GetComponentInChildren<Camera>(true);
            if (childCamera != null)
            {
                return childCamera;
            }
        }
        return Camera.main;
    }

    private static bool TryGetPlayer(Collider other, out BeanController sphere, out FirstPersonControllerSimple fps)
    {
        sphere = other.GetComponent<BeanController>() ?? other.GetComponentInParent<BeanController>();
        fps = other.GetComponent<FirstPersonControllerSimple>() ?? other.GetComponentInParent<FirstPersonControllerSimple>();
        return sphere != null || fps != null;
    }

    private static float EaseInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static float EaseOut(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private GameObject CreatePortal(Vector3 position, Quaternion rotation)
    {
        GameObject root = new GameObject(mode == ChallengeMode.Medium ? "MediumPortal" : "HardPortal");
        root.transform.SetPositionAndRotation(position, rotation);
        root.transform.localScale = Vector3.one;

        GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        core.name = "Core";
        core.transform.SetParent(root.transform, false);
        core.transform.localScale = new Vector3(1.45f, 2.05f, 0.16f);
        Destroy(core.GetComponent<Collider>());
        ApplyEmissionMaterial(core, accentColor, 2.8f);

        GameObject outerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        outerRing.name = "OuterRing";
        outerRing.transform.SetParent(root.transform, false);
        outerRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        outerRing.transform.localScale = new Vector3(1.35f, 0.04f, 1.35f);
        Destroy(outerRing.GetComponent<Collider>());
        ApplyEmissionMaterial(outerRing, secondaryAccentColor, 4.2f);

        GameObject innerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        innerRing.name = "InnerRing";
        innerRing.transform.SetParent(root.transform, false);
        innerRing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        innerRing.transform.localScale = new Vector3(1.05f, 0.025f, 1.05f);
        Destroy(innerRing.GetComponent<Collider>());
        ApplyEmissionMaterial(innerRing, pulseColor, 3.2f);

        GameObject lightRoot = new GameObject("PortalLight");
        lightRoot.transform.SetParent(root.transform, false);
        Light portalLight = lightRoot.AddComponent<Light>();
        portalLight.type = LightType.Point;
        portalLight.range = 10f;
        portalLight.intensity = 5.4f;
        portalLight.color = pulseColor;

        CreatePortalParticles(root.transform);

        PortalLoopAnimator animator = root.AddComponent<PortalLoopAnimator>();
        animator.Initialize(pulseColor);
        return root;
    }

    private void CreatePortalParticles(Transform parent)
    {
        GameObject fxRoot = new GameObject("PortalParticles");
        fxRoot.transform.SetParent(parent, false);
        fxRoot.SetActive(false);

        ParticleSystem ps = fxRoot.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.playOnAwake = false;
        main.loop = true;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.55f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.12f);
        main.maxParticles = 80;
        main.startColor = new ParticleSystem.MinMaxGradient(pulseColor, secondaryAccentColor);

        var emission = ps.emission;
        emission.rateOverTime = 34f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = 0.9f;
        shape.radiusThickness = 0.15f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.orbitalZ = new ParticleSystem.MinMaxCurve(1.25f);
        velocity.radial = new ParticleSystem.MinMaxCurve(0.04f);

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fxRoot.SetActive(true);
        ps.Play();
    }

    private static void ApplyEmissionMaterial(GameObject target, Color color, float emissionIntensity)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor("_EmissionColor", color * emissionIntensity);
        }
        material.EnableKeyword("_EMISSION");
        renderer.material = material;
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas == null)
        {
            GameObject canvasObject = new GameObject("CodeChallengePadCanvas");
            overlayCanvas = canvasObject.AddComponent<Canvas>();
            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = 13000;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            Object.DontDestroyOnLoad(canvasObject);
        }

        Transform root = overlayCanvas.transform;
        whiteImage = EnsureImage(root, "WhiteImage", pageTint);
        panelImage = EnsureImage(root, "Panel", cardColor);
        StretchFullscreen(panelImage.rectTransform);
        panelImage.raycastTarget = false;

        titleText = EnsureText(panelImage.transform, "TitleText", new Vector2(0.22f, 0.88f), new Vector2(460f, 56f), titleSize, FontStyle.Bold, textColor);
        counterText = EnsureText(panelImage.transform, "CounterText", new Vector2(0.22f, 0.82f), new Vector2(460f, 40f), bodySize, FontStyle.Bold, secondaryAccentColor);
        promptText = EnsureText(panelImage.transform, "PromptText", new Vector2(0.22f, 0.60f), new Vector2(460f, 230f), questionSize, FontStyle.Bold, textColor);
        feedbackText = EnsureText(panelImage.transform, "FeedbackText", new Vector2(0.22f, 0.28f), new Vector2(460f, 120f), bodySize, FontStyle.Bold, textColor);
        languagePromptText = EnsureText(root, "LanguagePromptText", new Vector2(0.5f, 0.60f), new Vector2(760f, 80f), questionSize, FontStyle.Bold, textColor);
        hintScreenText = EnsureText(root, "HintScreenText", new Vector2(0.5f, 0.58f), new Vector2(1020f, 360f), questionSize, FontStyle.Bold, textColor);
        codeInput = EnsureCodeInput(panelImage.transform);
        codeInputText = codeInput.textComponent;
        codePlaceholder = codeInput.placeholder as Text;

        leaveButton = EnsureButton(root, "LeaveButton", new Vector2(0.11f, 0.11f), new Vector2(180f, 56f), buttonColor, "Leave", buttonTextSize);
        hintScreenBackButton = EnsureButton(root, "HintScreenBackButton", new Vector2(0.27f, 0.11f), new Vector2(180f, 56f), buttonColor, "Back", buttonTextSize);
        languageRoButton = EnsureButton(root, "LanguageRoButton", new Vector2(0.39f, 0.45f), new Vector2(220f, 64f), accentColor, "Romana", buttonTextSize);
        languageEnButton = EnsureButton(root, "LanguageEnButton", new Vector2(0.61f, 0.45f), new Vector2(220f, 64f), secondaryAccentColor, "English", buttonTextSize);
        verifyButton = EnsureButton(panelImage.transform, "VerifyButton", new Vector2(0.80f, 0.17f), new Vector2(160f, 52f), accentColor, "Verificare", buttonTextSize);
        backButton = EnsureButton(panelImage.transform, "BackButton", new Vector2(0.58f, 0.17f), new Vector2(150f, 52f), buttonColor, "Back", buttonTextSize);
        hintButton = EnsureButton(panelImage.transform, "HintButton", new Vector2(0.69f, 0.17f), new Vector2(150f, 52f), secondaryAccentColor, "Hint", buttonTextSize);
        continueButton = EnsureButton(panelImage.transform, "ContinueButton", new Vector2(0.82f, 0.16f), new Vector2(160f, 48f), correctColor, "Continuare", buttonTextSize);
        retryWrongButton = EnsureButton(panelImage.transform, "RetryWrongButton", new Vector2(0.72f, 0.16f), new Vector2(260f, 48f), secondaryAccentColor, "Refa gresite", buttonTextSize);

        leaveButtonText = leaveButton.GetComponentInChildren<Text>(true);
        hintScreenBackButtonText = hintScreenBackButton.GetComponentInChildren<Text>(true);
        languageRoButtonText = languageRoButton.GetComponentInChildren<Text>(true);
        languageEnButtonText = languageEnButton.GetComponentInChildren<Text>(true);
        verifyButtonText = verifyButton.GetComponentInChildren<Text>(true);
        backButtonText = backButton.GetComponentInChildren<Text>(true);
        hintButtonText = hintButton.GetComponentInChildren<Text>(true);
        continueButtonText = continueButton.GetComponentInChildren<Text>(true);
        retryWrongButtonText = retryWrongButton.GetComponentInChildren<Text>(true);

        LayoutMainScreen();
        LayoutHintScreen();

        EnsureEventSystem();
    }

    private static Image EnsureImage(Transform parent, string name, Color color)
    {
        GameObject go = GetOrCreateUiObject(parent, name);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        Image image = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static void StretchFullscreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static Text EnsureText(Transform parent, string name, Vector2 anchor, Vector2 size, int fontSize, FontStyle fontStyle, Color color)
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
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private InputField EnsureCodeInput(Transform parent)
    {
        GameObject root = GetOrCreateUiObject(parent, "CodeInput");
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.71f, 0.56f);
        rect.anchorMax = new Vector2(0.71f, 0.56f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(640f, 360f);
        rect.localScale = Vector3.one;

        Image image = root.GetComponent<Image>() ?? root.AddComponent<Image>();
        image.color = editorColor;
        image.raycastTarget = true;
        Outline outline = root.GetComponent<Outline>() ?? root.AddComponent<Outline>();
        outline.effectColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.32f);
        outline.effectDistance = new Vector2(2f, -2f);
        RectMask2D mask = root.GetComponent<RectMask2D>() ?? root.AddComponent<RectMask2D>();
        mask.padding = Vector4.zero;

        InputField input = root.GetComponent<InputField>() ?? root.AddComponent<InputField>();
        input.lineType = InputField.LineType.MultiLineNewline;
        input.transition = Selectable.Transition.None;
        input.selectionColor = new Color(accentColor.r, accentColor.g, accentColor.b, 0.22f);
        input.caretColor = accentColor;
        input.customCaretColor = true;

        GameObject textObj = GetOrCreateUiObject(root.transform, "Text");
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(16f, 16f);
        textRect.offsetMax = new Vector2(-16f, -16f);
        textRect.localScale = Vector3.one;
        Text text = textObj.GetComponent<Text>() ?? textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = codeTextSize - 1;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.color = editorTextColor;
        text.supportRichText = false;
        text.resizeTextForBestFit = false;
        input.textComponent = text;

        GameObject placeholderObj = GetOrCreateUiObject(root.transform, "Placeholder");
        RectTransform placeRect = placeholderObj.GetComponent<RectTransform>();
        placeRect.anchorMin = new Vector2(0f, 0f);
        placeRect.anchorMax = new Vector2(1f, 1f);
        placeRect.offsetMin = new Vector2(16f, 16f);
        placeRect.offsetMax = new Vector2(-16f, -16f);
        placeRect.localScale = Vector3.one;
        Text placeholder = placeholderObj.GetComponent<Text>() ?? placeholderObj.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = codeTextSize - 1;
        placeholder.alignment = TextAnchor.UpperLeft;
        placeholder.color = new Color(editorTextColor.r, editorTextColor.g, editorTextColor.b, 0.45f);
        placeholder.text = "Scrie aici codul C++...";
        placeholder.resizeTextForBestFit = false;
        input.placeholder = placeholder;
        input.targetGraphic = image;
        Navigation navigation = input.navigation;
        navigation.mode = Navigation.Mode.None;
        input.navigation = navigation;
        return input;
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

        GameObject textObj = GetOrCreateUiObject(go.transform, "Label");
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text text = textObj.GetComponent<Text>() ?? textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.text = label;
        return button;
    }

    private static GameObject GetOrCreateUiObject(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            RectTransform existingRect = existing as RectTransform;
            if (existingRect != null)
            {
                existing.SetParent(parent, false);
                return existing.gameObject;
            }

            Object.Destroy(existing.gameObject);
        }

        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private void LayoutMainScreen()
    {
        StretchFullscreen(whiteImage.rectTransform);
        StretchFullscreen(panelImage.rectTransform);

        titleText.alignment = TextAnchor.MiddleLeft;
        counterText.alignment = TextAnchor.MiddleLeft;
        promptText.alignment = TextAnchor.UpperLeft;
        feedbackText.alignment = TextAnchor.UpperLeft;

        SetRect(titleText.rectTransform, new Vector2(0.22f, 0.89f), new Vector2(500f, 54f));
        SetRect(counterText.rectTransform, new Vector2(0.22f, 0.83f), new Vector2(500f, 38f));
        SetRect(promptText.rectTransform, new Vector2(0.24f, 0.58f), new Vector2(520f, 300f));
        SetRect(feedbackText.rectTransform, new Vector2(0.24f, 0.24f), new Vector2(520f, 120f));
        SetRect(codeInput.GetComponent<RectTransform>(), new Vector2(0.71f, 0.58f), new Vector2(620f, 340f));
        SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.60f, 0.16f), new Vector2(138f, 48f));
        SetRect(hintButton.GetComponent<RectTransform>(), new Vector2(0.71f, 0.16f), new Vector2(138f, 48f));
        SetRect(verifyButton.GetComponent<RectTransform>(), new Vector2(0.82f, 0.16f), new Vector2(160f, 48f));
        SetRect(continueButton.GetComponent<RectTransform>(), new Vector2(0.82f, 0.16f), new Vector2(160f, 48f));
        SetRect(retryWrongButton.GetComponent<RectTransform>(), new Vector2(0.72f, 0.16f), new Vector2(260f, 48f));
        SetRect(leaveButton.GetComponent<RectTransform>(), new Vector2(0.10f, 0.09f), new Vector2(156f, 48f));

        promptText.fontSize = 23;
        feedbackText.fontSize = 21;
    }

    private void LayoutHintScreen()
    {
        StretchFullscreen(whiteImage.rectTransform);
        SetRect(hintScreenText.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(980f, 420f));
        hintScreenText.alignment = TextAnchor.UpperLeft;
        hintScreenText.fontSize = 23;
        SetRect(leaveButton.GetComponent<RectTransform>(), new Vector2(0.10f, 0.09f), new Vector2(156f, 48f));
        SetRect(hintScreenBackButton.GetComponent<RectTransform>(), new Vector2(0.22f, 0.09f), new Vector2(156f, 48f));
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        rect.localScale = Vector3.one;
    }

    private static void EnsureEventSystem()
    {
        EventSystem existing = EventSystem.current != null ? EventSystem.current : Object.FindObjectOfType<EventSystem>();
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

        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
        Object.DontDestroyOnLoad(go);
    }

    private void ResetOverlay()
    {
        overlayInteractionActive = false;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        if (overlayCanvas == null || whiteImage == null || panelImage == null)
        {
            return;
        }
        whiteImage.color = new Color(pageTint.r, pageTint.g, pageTint.b, 0f);
        whiteImage.enabled = false;
        panelImage.color = new Color(cardColor.r, cardColor.g, cardColor.b, 0f);
        panelImage.gameObject.SetActive(false);
        titleText.gameObject.SetActive(false);
        counterText.gameObject.SetActive(false);
        promptText.gameObject.SetActive(false);
        feedbackText.gameObject.SetActive(false);
        languagePromptText.gameObject.SetActive(false);
        codeInput.gameObject.SetActive(false);
        hintScreenText.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        hintScreenBackButton.gameObject.SetActive(false);
        verifyButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        retryWrongButton.gameObject.SetActive(false);
        languageRoButton.gameObject.SetActive(false);
        languageEnButton.gameObject.SetActive(false);
        SetOverlayVisible(false);
    }

    private void SetOverlayVisible(bool visible)
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(visible);
        }
    }

    private void ForceSceneOnlyVisibility()
    {
        overlayInteractionActive = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(false);
        }

        SetCursorVisible(false);
    }

    private void TearDownOverlay()
    {
        overlayInteractionActive = false;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        if (overlayCanvas != null)
        {
            Destroy(overlayCanvas.gameObject);
        }

        overlayCanvas = null;
        whiteImage = null;
        panelImage = null;
        titleText = null;
        counterText = null;
        promptText = null;
        feedbackText = null;
        languagePromptText = null;
        codeInput = null;
        codeInputText = null;
        codePlaceholder = null;
        hintScreenText = null;
        leaveButton = null;
        leaveButtonText = null;
        verifyButton = null;
        verifyButtonText = null;
        backButton = null;
        backButtonText = null;
        hintButton = null;
        hintButtonText = null;
        continueButton = null;
        continueButtonText = null;
        retryWrongButton = null;
        retryWrongButtonText = null;
        languageRoButton = null;
        languageRoButtonText = null;
        languageEnButton = null;
        languageEnButtonText = null;
        hintScreenBackButton = null;
        hintScreenBackButtonText = null;
    }

    private void ShowLeaveButton(bool visible)
    {
        leaveButton.onClick.RemoveAllListeners();
        if (visible)
        {
            leaveButton.onClick.AddListener(OnLeaveClicked);
        }
        leaveButton.gameObject.SetActive(visible);
    }

    private void OnLeaveClicked()
    {
        leaveRequested = true;
        ForceSceneOnlyVisibility();
    }

    private IEnumerator FadeImage(Image image, float from, float to, float duration, Color color)
    {
        if (image == null)
        {
            yield break;
        }

        image.enabled = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, duration));
            image.color = new Color(color.r, color.g, color.b, Mathf.Lerp(from, to, t));
            yield return null;
        }
        image.color = new Color(color.r, color.g, color.b, to);
        image.enabled = to > 0.0001f;
    }

    private static void SetCursorVisible(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private IEnumerator PromptForLanguageSelection()
    {
        languageChosen = false;
        ShowMainUi(false);
        ShowChallengeButtons(false, false, false, false, false);
        languagePromptText.text = "Alege limba / Choose language";
        languagePromptText.gameObject.SetActive(true);
        languageRoButton.gameObject.SetActive(true);
        languageEnButton.gameObject.SetActive(true);
        languageRoButton.onClick.RemoveAllListeners();
        languageEnButton.onClick.RemoveAllListeners();
        languageRoButton.onClick.AddListener(() => SelectLanguage(QuizLanguage.Romanian));
        languageEnButton.onClick.AddListener(() => SelectLanguage(QuizLanguage.English));
        SetCursorVisible(true);

        while (!languageChosen && !leaveRequested)
        {
            yield return null;
        }

        languagePromptText.gameObject.SetActive(false);
        languageRoButton.gameObject.SetActive(false);
        languageEnButton.gameObject.SetActive(false);
    }

    private void SelectLanguage(QuizLanguage language)
    {
        selectedLanguage = language;
        languageChosen = true;
        ApplyLocalizedStaticTexts();
    }

    private void ApplyLocalizedStaticTexts()
    {
        if (leaveButtonText != null) leaveButtonText.text = "Leave";
        if (hintScreenBackButtonText != null) hintScreenBackButtonText.text = Localize("Inapoi", "Back");
        if (verifyButtonText != null) verifyButtonText.text = Localize("Verificare", "Verify");
        if (backButtonText != null) backButtonText.text = Localize("Inapoi", "Back");
        if (hintButtonText != null) hintButtonText.text = "Hint";
        if (continueButtonText != null) continueButtonText.text = Localize("Urmatoarea", "Next");
        if (retryWrongButtonText != null) retryWrongButtonText.text = Localize("Refa gresite", "Retry wrong");
        if (languageRoButtonText != null) languageRoButtonText.text = "Romana";
        if (languageEnButtonText != null) languageEnButtonText.text = "English";
    }

    private string Localize(string romanian, string english)
    {
        return selectedLanguage == QuizLanguage.Romanian ? romanian : english;
    }
}

public static class CodeChallengePadCinematicBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachIfNeeded()
    {
        Attach("Question2Pad", 0);
        Attach("HardQuestionPad", 1);
        Attach("questionhard", 1);
    }

    private static void Attach(string padName, int mode)
    {
        GameObject pad = GameObject.Find(padName);
        if (pad == null)
        {
            return;
        }

        CodeChallengePadCinematic component = pad.GetComponent<CodeChallengePadCinematic>();
        if (component == null)
        {
            component = pad.AddComponent<CodeChallengePadCinematic>();
        }

        Collider collider = pad.GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
            collider.enabled = true;
        }

        CppQuestionPadCinematic cppPad = pad.GetComponent<CppQuestionPadCinematic>();
        if (cppPad != null)
        {
            cppPad.enabled = false;
        }

        component.ConfigureForPad(mode == 1);
        component.enabled = true;
    }
}
