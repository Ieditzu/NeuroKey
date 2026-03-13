using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class CodeChallengePadCinematic : MonoBehaviour
{
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

        public CodeChallenge(string prompt, string initialCode, string expectedCode, string hint)
        {
            Prompt = prompt;
            InitialCode = initialCode;
            ExpectedCode = expectedCode;
            Hint = hint;
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
    [SerializeField] private float reenterCooldown = 2f;
    [SerializeField] private float exitHeightOffset = 0.55f;
    [SerializeField] private float exitBackOffset = 0.15f;
    [SerializeField] private float exitMoveDuration = 0.24f;

    [Header("Screen")]
    [SerializeField] private float fadeToWhiteDuration = 0.34f;
    [SerializeField] private float panelFadeDuration = 0.2f;

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
            "Debugging 1\n\nScop: functia trebuie sa modifice valoarea originala din `main`.\n\nCe trebuie sa observi:\n- variabila `value` este trimisa in functie\n- dupa apel, la afisare, valoarea ar trebui sa fie marita\n- in forma actuala, functia modifica doar o copie\n\nCerinta:\nRepara functia astfel incat incrementarea sa afecteze variabila originala.",
            "#include <iostream>\n\nvoid increment(int n) {\n    n++;\n}\n\nint main() {\n    int value = 5;\n    increment(value);\n    std::cout << value << std::endl;\n}",
            "#include <iostream>\n\nvoid increment(int& n) {\n    n++;\n}\n\nint main() {\n    int value = 5;\n    increment(value);\n    std::cout << value << std::endl;\n}",
            "Explicatie pas cu pas:\n\nIn C++, parametrul `int n` inseamna ca functia primeste o copie a valorii. Asta inseamna ca in interiorul functiei tu modifici doar copia locala, nu si variabila originala din `main`.\n\nDe aceea, dupa apelul `increment(value)`, variabila `value` ramane neschimbata.\n\nCa sa modifici direct variabila originala, trebuie sa trimiti parametrul prin referinta:\n`int& n`\n\nCand folosesti referinta, `n` devine un alt nume pentru variabila originala, iar `n++` modifica exact valoarea din `main`.\n\nRaspuns corect:\nvoid increment(int& n)\n{\n    n++;\n}"),
        new CodeChallenge(
            "Debugging 2\n\nScop: functia trebuie sa intoarca suma tuturor elementelor din vector.\n\nCe trebuie sa observi:\n- variabila `sum` este folosita inainte sa primeasca o valoare initiala\n- bucla trece prea departe, peste ultimul index valid\n\nCerinta:\nRepara functia astfel incat sa calculeze corect suma si sa nu iasa din vector.",
            "#include <vector>\n\nint sumVector(const std::vector<int>& nums) {\n    int sum;\n    for (size_t i = 0; i <= nums.size(); ++i) {\n        sum += nums[i];\n    }\n    return sum;\n}",
            "#include <vector>\n\nint sumVector(const std::vector<int>& nums) {\n    int sum = 0;\n    for (size_t i = 0; i < nums.size(); ++i) {\n        sum += nums[i];\n    }\n    return sum;\n}",
            "Explicatie pas cu pas:\n\nAici sunt doua probleme diferite.\n\n1. `sum` nu este initializat.\nAsta inseamna ca porneste cu o valoare necunoscuta din memorie. Cand aduni peste ea, rezultatul devine imprevizibil.\n\n2. Conditia `i <= nums.size()` este gresita.\nUltimul index valid intr-un vector este `nums.size() - 1`, deci bucla trebuie sa mearga cat timp `i < nums.size()`.\nDaca folosesti `<=`, la ultimul pas incerci sa accesezi o pozitie care nu exista.\n\nForma corecta este:\n- `int sum = 0;`\n- `for (size_t i = 0; i < nums.size(); ++i)`\n\nAsa calculezi sigur suma fara comportament nedefinit."),
        new CodeChallenge(
            "Debugging 3\n\nScop: codul trebuie sa afiseze textul inversat.\n\nCe trebuie sa observi:\n- bucla porneste de la ultimul caracter\n- indexul scade pana la 0\n- tipul folosit pentru index nu este potrivit pentru o bucla descrescatoare\n\nCerinta:\nRepara bucla astfel incat programul sa construiasca corect stringul inversat.",
            "#include <iostream>\n#include <string>\n\nint main() {\n    std::string s = \"debug\";\n    std::string reversed;\n\n    for (size_t i = s.size() - 1; i >= 0; --i) {\n        reversed += s[i];\n    }\n\n    std::cout << reversed << std::endl;\n}",
            "#include <iostream>\n#include <string>\n\nint main() {\n    std::string s = \"debug\";\n    std::string reversed;\n\n    for (int i = static_cast<int>(s.size()) - 1; i >= 0; --i) {\n        reversed += s[i];\n    }\n\n    std::cout << reversed << std::endl;\n}",
            "Explicatie pas cu pas:\n\n`size_t` este un tip unsigned, adica nu poate retine valori negative.\n\nIntr-o bucla descrescatoare, dupa ce ajungi la 0 si mai scazi o data, valoarea nu devine `-1`, ci se transforma intr-un numar foarte mare. De aceea conditia `i >= 0` nu functioneaza cum te astepti.\n\nSolutia este sa folosesti un tip semnat, de exemplu `int`, pentru indexul care merge inapoi.\n\nCum `s.size()` intoarce `size_t`, este bine sa faci conversia explicita la `int`:\n`static_cast<int>(s.size()) - 1`\n\nAsa bucla porneste de la ultimul caracter si se opreste corect cand trece de 0."),
        new CodeChallenge(
            "Debugging 4\n\nScop: apelul prin pointer de baza trebuie sa execute metoda suprascrisa din clasa derivata.\n\nCe trebuie sa observi:\n- in clasa de baza, metoda `area` este `const`\n- in clasa derivata, semnatura nu este identica\n- daca semnaturile nu coincid, override-ul nu functioneaza corect\n\nCerinta:\nCorecteaza metoda din `Circle` astfel incat apelul `s->area()` sa foloseasca versiunea potrivita.",
            "#include <iostream>\n\nclass Shape {\npublic:\n    virtual double area() const { return 0.0; }\n};\n\nclass Circle : public Shape {\npublic:\n    explicit Circle(double r) : radius(r) {}\n    double area() { return 3.14159 * radius * radius; }\nprivate:\n    double radius;\n};\n\nint main() {\n    Circle c(2.0);\n    Shape* s = &c;\n    std::cout << s->area() << std::endl;\n}",
            "#include <iostream>\n\nclass Shape {\npublic:\n    virtual double area() const { return 0.0; }\n};\n\nclass Circle : public Shape {\npublic:\n    explicit Circle(double r) : radius(r) {}\n    double area() const override { return 3.14159 * radius * radius; }\nprivate:\n    double radius;\n};\n\nint main() {\n    Circle c(2.0);\n    Shape* s = &c;\n    std::cout << s->area() << std::endl;\n}",
            "Explicatie pas cu pas:\n\nCand suprascrii o metoda virtuala, semnatura trebuie sa fie identica cu cea din clasa de baza.\n\nIn `Shape`, metoda este:\n`double area() const`\n\nIn `Circle`, metoda a fost scrisa fara `const`.\nAsta inseamna ca nu mai este exact aceeasi semnatura, deci override-ul nu este cel asteptat.\n\nTrebuie sa adaugi `const` si este foarte bine sa folosesti si `override`, pentru ca acesta te ajuta sa vezi imediat in compilare daca semnatura nu se potriveste.\n\nForma corecta este:\n`double area() const override`\n\nAsa, apelul prin `Shape*` ajunge in metoda din `Circle`."),
        new CodeChallenge(
            "Debugging 5\n\nScop: functia trebuie sa intoarca un rezultat valid, fara pointer dangling.\n\nCe trebuie sa observi:\n- `text` este o variabila locala\n- la finalul functiei, variabila este distrusa\n- `c_str()` intoarce un pointer catre memoria interna a acelui string\n\nCerinta:\nSchimba functia astfel incat valoarea returnata sa ramana valida dupa iesirea din functie.",
            "#include <iostream>\n#include <string>\n\nconst char* getMessage() {\n    std::string text = \"Level complete\";\n    return text.c_str();\n}\n\nint main() {\n    std::cout << getMessage() << std::endl;\n}",
            "#include <iostream>\n#include <string>\n\nstd::string getMessage() {\n    std::string text = \"Level complete\";\n    return text;\n}\n\nint main() {\n    std::cout << getMessage() << std::endl;\n}",
            "Explicatie pas cu pas:\n\nVariabila `text` exista doar in interiorul functiei. In momentul in care functia se termina, obiectul este distrus.\n\nMetoda `c_str()` intoarce un pointer catre memoria interna a acelui string. Daca stringul nu mai exista, pointerul ramane suspendat, adica devine invalid.\n\nDe aceea apare problema numita `dangling pointer`.\n\nSolutia simpla si corecta aici este sa returnezi direct `std::string` prin valoare. In C++, asta este sigur si normal.\n\nAstfel, mesajul returnat ramane valid si poate fi afisat fara probleme.")
    };

    private static readonly CodeChallenge[] HardChallenges =
    {
        new CodeChallenge(
            "Cerinta 1\n\nScrie functia `bool IsEven(int n)`.\n\nCe trebuie sa faca:\n- primeste un numar intreg\n- intoarce `true` daca numarul este par\n- intoarce `false` daca numarul este impar\n\nScrie doar corpul corect al functiei in editorul din dreapta.",
            "bool IsEven(int n)\n{\n    \n}",
            "bool IsEven(int n)\n{\n    return n % 2 == 0;\n}",
            "Explicatie:\n\nUn numar par este un numar care se imparte exact la 2.\nAsta inseamna ca restul impartirii la 2 este 0.\n\nIn C++, restul impartirii se verifica cu operatorul `%`.\n\nDeci verificarea corecta este:\n`n % 2 == 0`\n\nDaca expresia este adevarata, functia intoarce `true`."),
        new CodeChallenge(
            "Cerinta 2\n\nScrie functia `int MaxOfTwo(int a, int b)`.\n\nCe trebuie sa faca:\n- compara cele doua valori primite\n- intoarce numarul mai mare\n\nRezolva direct in editorul din dreapta.",
            "int MaxOfTwo(int a, int b)\n{\n    \n}",
            "int MaxOfTwo(int a, int b)\n{\n    return a > b ? a : b;\n}",
            "Explicatie:\n\nTrebuie sa compari valorile `a` si `b`.\nDaca `a` este mai mare, returnezi `a`.\nIn caz contrar, returnezi `b`.\n\nO forma scurta si corecta este cu operatorul ternar:\n`a > b ? a : b`\n\nAsta inseamna:\n- daca `a > b`, rezultatul este `a`\n- altfel, rezultatul este `b`"),
        new CodeChallenge(
            "Cerinta 3\n\nScrie functia `int Square(int x)`.\n\nCe trebuie sa faca:\n- primeste un numar `x`\n- intoarce patratul lui, adica valoarea inmultita cu ea insasi",
            "int Square(int x)\n{\n    \n}",
            "int Square(int x)\n{\n    return x * x;\n}",
            "Explicatie:\n\nPatratul unui numar inseamna numarul inmultit cu el insusi.\n\nDaca ai `x`, atunci patratul lui este:\n`x * x`\n\nDe aceea instructiunea corecta de return este:\n`return x * x;`"),
        new CodeChallenge(
            "Cerinta 4\n\nScrie functia `int Sum3(int a, int b, int c)`.\n\nCe trebuie sa faca:\n- primeste trei numere intregi\n- intoarce suma lor",
            "int Sum3(int a, int b, int c)\n{\n    \n}",
            "int Sum3(int a, int b, int c)\n{\n    return a + b + c;\n}",
            "Explicatie:\n\nFunctia primeste trei parametri: `a`, `b` si `c`.\nCa sa obtii suma totala, trebuie sa le aduni pe toate trei.\n\nForma corecta este:\n`return a + b + c;`\n\nNu trebuie altceva in aceasta cerinta."),
        new CodeChallenge(
            "Cerinta 5\n\nScrie functia `int Factorial3()`.\n\nCe trebuie sa faca:\n- intoarce factorialul lui 3\n\nAmintire:\nfactorialul unui numar inseamna produsul numerelor de la acel numar pana la 1.",
            "int Factorial3()\n{\n    \n}",
            "int Factorial3()\n{\n    return 3 * 2 * 1;\n}",
            "Explicatie:\n\nFactorialul lui 3 se calculeaza asa:\n`3 * 2 * 1`\n\nPentru aceasta cerinta nu ai nevoie de bucla sau variabile suplimentare.\nPoti intoarce direct rezultatul prin:\n`return 3 * 2 * 1;`")
    };

    private static Canvas overlayCanvas;
    private static Image whiteImage;
    private static Image panelImage;
    private static Text titleText;
    private static Text counterText;
    private static Text promptText;
    private static Text feedbackText;
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
    private int score;
    private GameObject activePortal;
    private bool overlayInteractionActive;

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

        MediumQuestionPadStartSequence oldSequence = GetComponent<MediumQuestionPadStartSequence>();
        if (oldSequence != null)
        {
            oldSequence.enabled = false;
        }
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

    private void LateUpdate()
    {
        if (overlayInteractionActive)
        {
            SetCursorVisible(true);
        }
    }

    private IEnumerator PlaySequence(SphereController sphere, FirstPersonControllerSimple fps)
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
        ShowMainUi(true);
        ShowLeaveButton(true);
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
        if (triggerCollider != null)
        {
            StartCoroutine(ReenableTriggerAfterDelay());
        }
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
            attemptsLeft[i] = 4;
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

            string actual = NormalizeCode(codeInput.text);
            string expected = NormalizeCode(challenge.ExpectedCode);
            bool correct = actual == expected;
            answers[current] = codeInput.text;

            if (correct)
            {
                if (!solved[current])
                {
                    solved[current] = true;
                    score++;
                }

                feedbackText.text = "Corect. Apasa pe Continuare.";
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
                        feedbackText.text = "Corect. Apasa pe Continuare.";
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
                feedbackText.text = "Incorect. Mai ai " + attemptsLeft[current] + " incercari.";
                feedbackText.color = wrongColor;
            }
            else
            {
                feedbackText.text = "Ai ramas fara incercari. Trecem automat la urmatoarea intrebare.";
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
                    feedbackText.text = "Incorect. Mai ai " + attemptsLeft[current] + " incercari.";
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

        titleText.text = mode == ChallengeMode.Medium ? "Medium complete" : "Hard complete";
        counterText.text = "Rezultat";
        promptText.text = "Ai rezolvat corect " + score + " din " + challenges.Length + " provocari.";
        feedbackText.text = "Poti reface intrebarile gresite sau poti iesi.";
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
                retryAttempts[writeIndex] = 4;
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
        titleText.text = mode == ChallengeMode.Medium ? "Medium C++ Debugging" : "Hard C++ Coding";
        counterText.text = "Challenge " + (currentIndex + 1) + " / " + total;
        promptText.text = challenge.Prompt;
        codeInput.text = currentAnswer;
        feedbackText.text = "Incercari ramase: " + attemptsLeft + " / 4";
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

            titleText.text = "Refacere intrebari gresite";
            counterText.text = "Retry " + (current + 1) + " / " + retryChallenges.Length;
            promptText.text = challenge.Prompt;
            codeInput.text = retryAnswers[current];
            feedbackText.text = "Incercari ramase: " + retryAttempts[current] + " / 4";
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

            string actual = NormalizeCode(codeInput.text);
            string expected = NormalizeCode(challenge.ExpectedCode);
            retryAnswers[current] = codeInput.text;
            if (actual == expected)
            {
                retrySolved[current] = true;
                solved[retrySourceIndex[current]] = true;
                feedbackText.text = "Corect. Apasa pe Continuare.";
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
                feedbackText.text = "Incorect. Mai ai " + retryAttempts[current] + " incercari.";
                feedbackText.color = wrongColor;
                continue;
            }

            feedbackText.text = "Ai ramas fara incercari. Trecem automat la urmatoarea intrebare.";
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

    private void RestorePlayerState(SphereController sphere, FirstPersonControllerSimple fps)
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

    private static void TeleportPlayer(SphereController sphere, FirstPersonControllerSimple fps, Vector3 position, Quaternion rotation)
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

    private static bool TryGetPlayer(Collider other, out SphereController sphere, out FirstPersonControllerSimple fps)
    {
        sphere = other.GetComponent<SphereController>() ?? other.GetComponentInParent<SphereController>();
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
        hintScreenText = EnsureText(root, "HintScreenText", new Vector2(0.5f, 0.58f), new Vector2(1020f, 360f), questionSize, FontStyle.Bold, textColor);
        codeInput = EnsureCodeInput(panelImage.transform);
        codeInputText = codeInput.textComponent;
        codePlaceholder = codeInput.placeholder as Text;

        leaveButton = EnsureButton(root, "LeaveButton", new Vector2(0.11f, 0.11f), new Vector2(180f, 56f), buttonColor, "Leave", buttonTextSize);
        hintScreenBackButton = EnsureButton(root, "HintScreenBackButton", new Vector2(0.27f, 0.11f), new Vector2(180f, 56f), buttonColor, "Back", buttonTextSize);
        verifyButton = EnsureButton(panelImage.transform, "VerifyButton", new Vector2(0.80f, 0.17f), new Vector2(160f, 52f), accentColor, "Verificare", buttonTextSize);
        backButton = EnsureButton(panelImage.transform, "BackButton", new Vector2(0.58f, 0.17f), new Vector2(150f, 52f), buttonColor, "Back", buttonTextSize);
        hintButton = EnsureButton(panelImage.transform, "HintButton", new Vector2(0.69f, 0.17f), new Vector2(150f, 52f), secondaryAccentColor, "Hint", buttonTextSize);
        continueButton = EnsureButton(panelImage.transform, "ContinueButton", new Vector2(0.82f, 0.16f), new Vector2(160f, 48f), correctColor, "Continuare", buttonTextSize);
        retryWrongButton = EnsureButton(panelImage.transform, "RetryWrongButton", new Vector2(0.72f, 0.16f), new Vector2(260f, 48f), secondaryAccentColor, "Refa gresite", buttonTextSize);

        leaveButtonText = leaveButton.GetComponentInChildren<Text>(true);
        hintScreenBackButtonText = hintScreenBackButton.GetComponentInChildren<Text>(true);
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
        codeInput.gameObject.SetActive(false);
        hintScreenText.gameObject.SetActive(false);
        leaveButton.gameObject.SetActive(false);
        hintScreenBackButton.gameObject.SetActive(false);
        verifyButton.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        hintButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        retryWrongButton.gameObject.SetActive(false);
        SetOverlayVisible(false);
    }

    private void SetOverlayVisible(bool visible)
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(visible);
        }
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
        TearDownOverlay();
        SetCursorVisible(false);
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
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
}

public static class CodeChallengePadCinematicBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachIfNeeded()
    {
        Attach("MediumQuestionPad", 0);
        Attach("HardQuestionPad", 0);
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

        component.ConfigureForPad(mode == 1);
    }
}
