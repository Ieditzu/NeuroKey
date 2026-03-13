using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class MediumQuestionPadStartSequence : MonoBehaviour
{
    private enum QuestionSetPreset
    {
        Medium = 0,
        Hard = 1
    }

    private const string MediumLeftPromptExact =
        "The following C++ code is supposed to increment the original value, but it does not.\n" +
        "Fix the bug.";
    private const string MediumFirstInitialCode =
        "#include <iostream>\n\n" +
        "void increment(int n) {\n" +
        "    n++;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    int value = 5;\n" +
        "    increment(value);\n" +
        "    std::cout << value << std::endl;\n" +
        "}";
    private const string MediumFirstCorrectCode =
        "#include <iostream>\n\n" +
        "void increment(int& n) {\n" +
        "    n++;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    int value = 5;\n" +
        "    increment(value);\n" +
        "    std::cout << value << std::endl;\n" +
        "}";
    private const string MediumSecondPromptExact =
        "Question:\n" +
        "This C++ function should return the sum of all elements in the vector, but it has undefined behavior.\n" +
        "Fix the bug.";
    private const string MediumSecondInitialCode =
        "#include <vector>\n\n" +
        "int sumVector(const std::vector<int>& nums) {\n" +
        "    int sum;\n" +
        "    for (size_t i = 0; i <= nums.size(); ++i) {\n" +
        "        sum += nums[i];\n" +
        "    }\n" +
        "    return sum;\n" +
        "}";
    private const string MediumSecondCorrectCode =
        "#include <vector>\n\n" +
        "int sumVector(const std::vector<int>& nums) {\n" +
        "    int sum = 0;\n" +
        "    for (size_t i = 0; i < nums.size(); ++i) {\n" +
        "        sum += nums[i];\n" +
        "    }\n" +
        "    return sum;\n" +
        "}";
    private const string MediumSecondHintText =
        "Step by step:\n" +
        "1) Initialize `sum` before using it.\n" +
        "2) The last valid index is `size() - 1`, so loop with `i < nums.size()`.\n" +
        "3) Keep the rest of the function unchanged.\n\n" +
        "Correct answer:\n" +
        "#include <vector>\n\n" +
        "int sumVector(const std::vector<int>& nums) {\n" +
        "    int sum = 0;\n" +
        "    for (size_t i = 0; i < nums.size(); ++i) {\n" +
        "        sum += nums[i];\n" +
        "    }\n" +
        "    return sum;\n" +
        "}";
    private const string MediumThirdPromptExact =
        "Question:\n" +
        "This C++ code should print the string in reverse order, but the loop is broken.\n" +
        "Find and fix the bug.";
    private const string MediumThirdInitialCode =
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "int main() {\n" +
        "    std::string s = \"debug\";\n" +
        "    std::string reversed;\n\n" +
        "    for (size_t i = s.size() - 1; i >= 0; --i) {\n" +
        "        reversed += s[i];\n" +
        "    }\n\n" +
        "    std::cout << reversed << std::endl;\n" +
        "}";
    private const string MediumThirdCorrectCode =
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "int main() {\n" +
        "    std::string s = \"debug\";\n" +
        "    std::string reversed;\n\n" +
        "    for (int i = static_cast<int>(s.size()) - 1; i >= 0; --i) {\n" +
        "        reversed += s[i];\n" +
        "    }\n\n" +
        "    std::cout << reversed << std::endl;\n" +
        "}";
    private const string MediumThirdHintText =
        "Step by step:\n" +
        "1) `size_t` is unsigned, so `i >= 0` is always true.\n" +
        "2) Use a signed index for a countdown loop.\n" +
        "3) Cast `s.size()` to `int` before subtracting 1.\n\n" +
        "Correct answer:\n" +
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "int main() {\n" +
        "    std::string s = \"debug\";\n" +
        "    std::string reversed;\n\n" +
        "    for (int i = static_cast<int>(s.size()) - 1; i >= 0; --i) {\n" +
        "        reversed += s[i];\n" +
        "    }\n\n" +
        "    std::cout << reversed << std::endl;\n" +
        "}";
    private const string MediumFourthPromptExact =
        "Question:\n" +
        "This code should call `Circle::area()` through a base pointer, but it calls the wrong function.\n" +
        "Fix the bug.";
    private const string MediumFourthInitialCode =
        "#include <iostream>\n\n" +
        "class Shape {\n" +
        "public:\n" +
        "    virtual double area() const { return 0.0; }\n" +
        "};\n\n" +
        "class Circle : public Shape {\n" +
        "public:\n" +
        "    explicit Circle(double r) : radius(r) {}\n" +
        "    double area() { return 3.14159 * radius * radius; }\n" +
        "private:\n" +
        "    double radius;\n" +
        "};\n\n" +
        "int main() {\n" +
        "    Circle c(2.0);\n" +
        "    Shape* s = &c;\n" +
        "    std::cout << s->area() << std::endl;\n" +
        "}";
    private const string MediumFourthCorrectCode =
        "#include <iostream>\n\n" +
        "class Shape {\n" +
        "public:\n" +
        "    virtual double area() const { return 0.0; }\n" +
        "};\n\n" +
        "class Circle : public Shape {\n" +
        "public:\n" +
        "    explicit Circle(double r) : radius(r) {}\n" +
        "    double area() const override { return 3.14159 * radius * radius; }\n" +
        "private:\n" +
        "    double radius;\n" +
        "};\n\n" +
        "int main() {\n" +
        "    Circle c(2.0);\n" +
        "    Shape* s = &c;\n" +
        "    std::cout << s->area() << std::endl;\n" +
        "}";
    private const string MediumFourthHintText =
        "Step by step:\n" +
        "1) Base method is `area() const`, so the derived signature must match exactly.\n" +
        "2) Add `const` to `Circle::area()`.\n" +
        "3) Add `override` to catch signature mismatches early.\n\n" +
        "Correct answer:\n" +
        "#include <iostream>\n\n" +
        "class Shape {\n" +
        "public:\n" +
        "    virtual double area() const { return 0.0; }\n" +
        "};\n\n" +
        "class Circle : public Shape {\n" +
        "public:\n" +
        "    explicit Circle(double r) : radius(r) {}\n" +
        "    double area() const override { return 3.14159 * radius * radius; }\n" +
        "private:\n" +
        "    double radius;\n" +
        "};\n\n" +
        "int main() {\n" +
        "    Circle c(2.0);\n" +
        "    Shape* s = &c;\n" +
        "    std::cout << s->area() << std::endl;\n" +
        "}";
    private const string MediumFifthPromptExact =
        "Question:\n" +
        "This function should return a valid C-string, but it returns a dangling pointer.\n" +
        "Fix the bug.";
    private const string MediumFifthInitialCode =
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "const char* getMessage() {\n" +
        "    std::string text = \"Level complete\";\n" +
        "    return text.c_str();\n" +
        "}\n\n" +
        "int main() {\n" +
        "    std::cout << getMessage() << std::endl;\n" +
        "}";
    private const string MediumFifthCorrectCode =
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "std::string getMessage() {\n" +
        "    std::string text = \"Level complete\";\n" +
        "    return text;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    std::cout << getMessage() << std::endl;\n" +
        "}";
    private const string MediumFifthHintText =
        "Step by step:\n" +
        "1) `text` is a local variable, so it is destroyed when the function ends.\n" +
        "2) Returning `text.c_str()` gives a pointer to invalid memory.\n" +
        "3) Return `std::string` by value instead.\n\n" +
        "Expected answer:\n" +
        "#include <iostream>\n" +
        "#include <string>\n\n" +
        "std::string getMessage() {\n" +
        "    std::string text = \"Level complete\";\n" +
        "    return text;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    std::cout << getMessage() << std::endl;\n" +
        "}";
    private const string MediumHintTutorialExact =
        "Step by step:\n" +
        "1) `increment(int n)` receives a copy, not the original variable.\n" +
        "2) Incrementing `n` changes only the local copy.\n" +
        "3) Use `int& n` to modify the caller's value.\n\n" +
        "Correct answer:\n" +
        "#include <iostream>\n\n" +
        "void increment(int& n) {\n" +
        "    n++;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    int value = 5;\n" +
        "    increment(value);\n" +
        "    std::cout << value << std::endl;\n" +
        "}";
    private const string HardFirstPromptExact =
        "Question:\n" +
        "Write C++ code for `IsEven(int n)` that returns `true` if `n` is even, otherwise `false`.";
    private const string HardFirstInitialCode =
        "bool IsEven(int n) {\n" +
        "    // Write your code here\n" +
        "    return false;\n" +
        "}";
    private const string HardFirstCorrectCode =
        "bool IsEven(int n) {\n" +
        "    return n % 2 == 0;\n" +
        "}";
    private const string HardFirstHintText =
        "Hint:\n" +
        "Use `% 2` to check if the remainder is 0.\n\n" +
        "Expected answer:\n" +
        "bool IsEven(int n) {\n" +
        "    return n % 2 == 0;\n" +
        "}";
    private const string HardSecondPromptExact =
        "Question:\n" +
        "Write C++ code for `Add(int a, int b)` that returns the sum of `a` and `b`.";
    private const string HardSecondInitialCode =
        "int Add(int a, int b) {\n" +
        "    // Write your code here\n" +
        "    return 0;\n" +
        "}";
    private const string HardSecondCorrectCode =
        "int Add(int a, int b) {\n" +
        "    return a + b;\n" +
        "}";
    private const string HardSecondHintText =
        "Hint:\n" +
        "Return `a + b`.\n\n" +
        "Expected answer:\n" +
        "int Add(int a, int b) {\n" +
        "    return a + b;\n" +
        "}";
    private const string HardThirdPromptExact =
        "Question:\n" +
        "Write C++ code for `CountPositive` that returns how many positive numbers are in a vector.";
    private const string HardThirdInitialCode =
        "#include <vector>\n\n" +
        "int CountPositive(const std::vector<int>& nums) {\n" +
        "    // Write your code here\n" +
        "    return 0;\n" +
        "}";
    private const string HardThirdCorrectCode =
        "#include <vector>\n\n" +
        "int CountPositive(const std::vector<int>& nums) {\n" +
        "    int count = 0;\n" +
        "    for (int x : nums) {\n" +
        "        if (x > 0) count++;\n" +
        "    }\n" +
        "    return count;\n" +
        "}";
    private const string HardThirdHintText =
        "Hint:\n" +
        "Use a counter and increment it when `x > 0`.\n\n" +
        "Expected answer:\n" +
        "#include <vector>\n\n" +
        "int CountPositive(const std::vector<int>& nums) {\n" +
        "    int count = 0;\n" +
        "    for (int x : nums) {\n" +
        "        if (x > 0) count++;\n" +
        "    }\n" +
        "    return count;\n" +
        "}";
    private const string HardFourthPromptExact =
        "Question:\n" +
        "Write C++ code for `Factorial(int n)` (n >= 0) using a loop.";
    private const string HardFourthInitialCode =
        "int Factorial(int n) {\n" +
        "    // Write your code here\n" +
        "    return 0;\n" +
        "}";
    private const string HardFourthCorrectCode =
        "int Factorial(int n) {\n" +
        "    int result = 1;\n" +
        "    for (int i = 2; i <= n; i++) {\n" +
        "        result *= i;\n" +
        "    }\n" +
        "    return result;\n" +
        "}";
    private const string HardFourthHintText =
        "Hint:\n" +
        "Start with `result = 1` and multiply from 2 to n.\n\n" +
        "Expected answer:\n" +
        "int Factorial(int n) {\n" +
        "    int result = 1;\n" +
        "    for (int i = 2; i <= n; i++) {\n" +
        "        result *= i;\n" +
        "    }\n" +
        "    return result;\n" +
        "}";
    private const string HardFifthPromptExact =
        "Question:\n" +
        "Write C++ code for `MaxOfThree(int a, int b, int c)` that returns the largest value.";
    private const string HardFifthInitialCode =
        "int MaxOfThree(int a, int b, int c) {\n" +
        "    // Write your code here\n" +
        "    return 0;\n" +
        "}";
    private const string HardFifthCorrectCode =
        "int MaxOfThree(int a, int b, int c) {\n" +
        "    int maxValue = a;\n" +
        "    if (b > maxValue) maxValue = b;\n" +
        "    if (c > maxValue) maxValue = c;\n" +
        "    return maxValue;\n" +
        "}";
    private const string HardFifthHintText =
        "Hint:\n" +
        "Start with `maxValue = a`, then compare with `b` and `c`.\n\n" +
        "Expected answer:\n" +
        "int MaxOfThree(int a, int b, int c) {\n" +
        "    int maxValue = a;\n" +
        "    if (b > maxValue) maxValue = b;\n" +
        "    if (c > maxValue) maxValue = c;\n" +
        "    return maxValue;\n" +
        "}";
    [Header("Blink")]
    [SerializeField] private float blinkDuration = 1.1f;
    [SerializeField] private float blinkFrequency = 7.5f;
    [SerializeField] private float maxEmissionMultiplier = 4.8f;

    [Header("Explosion")]
    [SerializeField] private ParticleSystem whiteExplosionPrefab;

    [Header("Pre-Explosion Motion")]
    [SerializeField] private float preExplosionMotionDuration = 1.25f;
    [SerializeField] private float liftHeight = 2.4f;
    [SerializeField] private float spinSpeed = 620f;
    [SerializeField] private float orbitRadius = 0.42f;
    [SerializeField] private float totalTurnsBeforeExplosion = 3f;
    [SerializeField] private float centerOnTurnIndex = 3f;
    [SerializeField] private float finalCameraDepth = 0.22f;
    [Header("Hard Portal Sequence")]
    [SerializeField] private float hardPortalOpenDuration = 0.45f;
    [SerializeField] private float hardPortalPullDuration = 0.7f;
    [SerializeField] private float hardPortalCollapseDuration = 0.2f;
    [SerializeField] private float hardPortalDistance = 2.4f;
    [SerializeField] private float hardPortalUpOffset = 1.1f;
    [SerializeField] private float hardPortalRadius = 1.05f;
    [SerializeField] private float hardPortalTwistSpeed = 240f;

    [Header("White Screen")]
    [SerializeField] private float fadeToWhiteDuration = 0.55f;
    [SerializeField] private bool autoCreateOverlay = true;
    [SerializeField] private Canvas overlayCanvas;
    [SerializeField] private Image whiteImage;
    [SerializeField] private Text startText;
    [TextArea(2, 5)]
    [SerializeField] private string mediumPromptTextContent =
        "The following C++ code is supposed to increment the original value, but it does not.\nFix the bug.";
    [SerializeField] private Text mediumPromptText;
    [SerializeField] private float mediumPromptFadeDuration = 0.32f;
    [SerializeField] private float mediumPromptTypeDelay = 0.018f;
    [TextArea(3, 8)]
    [SerializeField] private string mediumCssInitialCode =
        "#include <iostream>\n\nvoid increment(int n) {\n    n++;\n}\n\nint main() {\n    int value = 5;\n    increment(value);\n    std::cout << value << std::endl;\n}";
    [TextArea(3, 8)]
    [SerializeField] private string mediumCssCorrectCode =
        "#include <iostream>\n\nvoid increment(int& n) {\n    n++;\n}\n\nint main() {\n    int value = 5;\n    increment(value);\n    std::cout << value << std::endl;\n}";
    [SerializeField] private Image mediumCodePanel;
    [SerializeField] private InputField mediumCodeInputField;
    [SerializeField] private Button mediumVerifyButton;
    [SerializeField] private Text mediumVerifyResultText;
    [SerializeField] private Button mediumLeaveButton;
    [SerializeField] private Button mediumContinueButton;
    [SerializeField] private Button mediumHintButton;
    [SerializeField] private Image mediumTutorialPanel;
    [SerializeField] private Text mediumTutorialTitleText;
    [SerializeField] private Text mediumTutorialBodyText;
    [SerializeField] private Button mediumBackToQuestionButton;
    [TextArea(5, 12)]
    [SerializeField] private string mediumTutorialContent =
        "Step by step:\n" +
        "1) `increment(int n)` receives a copy, not the original variable.\n" +
        "2) Incrementing `n` changes only the local copy.\n" +
        "3) Use `int& n` to modify the caller's value.\n\n" +
        "Correct answer:\n" +
        "#include <iostream>\n\n" +
        "void increment(int& n) {\n" +
        "    n++;\n" +
        "}\n\n" +
        "int main() {\n" +
        "    int value = 5;\n" +
        "    increment(value);\n" +
        "    std::cout << value << std::endl;\n" +
        "}";
    [SerializeField] private float mediumTutorialTypeDelay = 0.012f;
    [SerializeField] private float mediumResultPulseDuration = 0.28f;
    [SerializeField] private float mediumResultHoldDuration = 0.32f;
    [SerializeField] private float mediumReentryCooldownSeconds = 4f;
    [SerializeField] private string nextLevelSceneName = string.Empty;
    [Header("Medium Progression")]
    [SerializeField] private QuestionSetPreset questionSetPreset = QuestionSetPreset.Medium;
    [SerializeField] private bool isFollowupTestPad;
    [SerializeField] private string followupTestMessage = "test";
    [SerializeField] private bool continueBuildsNextMediumPath = true;
    [SerializeField] private string nextPathRootName = "MediumNextTestPath";
    [SerializeField] private int mediumQuestionStage = 1;
    [SerializeField] private int nextPathSegments = 6;
    [SerializeField] private float nextPathSegmentLength = 2.6f;
    [SerializeField] private float nextPathStartOffset = 0f;
    [SerializeField] private Vector3 nextPathSegmentScale = new Vector3(2.2f, 0.28f, 2.6f);
    [SerializeField] private int nextPadSegmentIndex = -1;
    [SerializeField] private Vector3 nextPadScale = new Vector3(3.2f, 0.2f, 2.4f);
    [SerializeField] private bool freezeCameraDuringSequence = true;

    private bool running;
    private bool buttonPressed;
    private bool leavePressed;
    private bool continuePressed;
    private bool mediumSequenceCompleted;
    private bool tutorialOpen;
    private bool nextPathSpawned;
    private Button watchedButton;
    private float nextAllowedTriggerTime;
    private Coroutine verifyPulseRoutine;
    private Coroutine tutorialTypeRoutine;
    private readonly List<RendererState> rendererStates = new List<RendererState>();
    private TopDownCameraFollow frozenCameraFollow;
    private Transform frozenCameraTransform;
    private Vector3 frozenCameraPosition;
    private Quaternion frozenCameraRotation;
    private Vector3 hardCameraVelocity;
    private Collider padCollider;

    private struct RendererState
    {
        public Renderer renderer;
        public bool rendererEnabled;
        public Material material;
        public bool hasEmission;
        public Color emissionColor;
    }

    private void Awake()
    {
        padCollider = GetComponent<Collider>();
        EnsureOverlay();
        HideOverlayImmediate();
    }

    private void Update()
    {
        if (!CanStartSequence() || padCollider == null || padCollider.isTrigger)
        {
            return;
        }

        FirstPersonControllerSimple fps = Object.FindObjectOfType<FirstPersonControllerSimple>();
        if (fps == null)
        {
            return;
        }

        Collider playerCollider = fps.GetComponent<CharacterController>();
        if (playerCollider == null)
        {
            playerCollider = fps.GetComponent<Collider>();
        }

        if (playerCollider == null || !IsPlayerTouchingPad(playerCollider))
        {
            return;
        }

        StartCoroutine(PlaySequence(null, fps));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!CanStartSequence())
        {
            return;
        }

        if (!TryGetPlayer(collision.collider, out SphereController sphere, out FirstPersonControllerSimple fps))
        {
            return;
        }

        StartCoroutine(PlaySequence(sphere, fps));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!CanStartSequence())
        {
            return;
        }

        if (!TryGetPlayer(other, out SphereController sphere, out FirstPersonControllerSimple fps))
        {
            return;
        }

        StartCoroutine(PlaySequence(sphere, fps));
    }

    private bool CanStartSequence()
    {
        return !mediumSequenceCompleted && !running && Time.time >= nextAllowedTriggerTime;
    }

    private bool IsPlayerTouchingPad(Collider playerCollider)
    {
        if (padCollider == null || playerCollider == null)
        {
            return false;
        }

        Vector3 direction;
        float distance;
        bool overlapping = Physics.ComputePenetration(
            padCollider, padCollider.transform.position, padCollider.transform.rotation,
            playerCollider, playerCollider.transform.position, playerCollider.transform.rotation,
            out direction, out distance);

        return overlapping && distance > 0f;
    }

    private static bool TryGetPlayer(Collider other, out SphereController sphere, out FirstPersonControllerSimple fps)
    {
        sphere = null;
        fps = null;
        if (other == null)
        {
            return false;
        }

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

    private IEnumerator PlaySequence(SphereController sphere, FirstPersonControllerSimple fps)
    {
        running = true;
        HideAllSequenceOverlays();
        FreezeCameraIfNeeded();
        Vector3 originalSphereScale = sphere != null ? sphere.transform.localScale : Vector3.one;

        Rigidbody rb = sphere != null ? sphere.GetComponent<Rigidbody>() : null;
        if (sphere != null)
        {
            sphere.SetMovementLocked(true);
            sphere.SetHardFreeze(true);
        }
        if (fps != null)
        {
            fps.SetMovementLocked(true);
            fps.SetHardFreeze(true);
        }

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (sphere != null && questionSetPreset == QuestionSetPreset.Hard)
        {
            CacheRendererStates(sphere);
            hardCameraVelocity = Vector3.zero;
            yield return PlayHardPortalAbsorptionMotion(sphere);
        }
        else if (sphere != null)
        {
            CacheRendererStates(sphere);

            float t = 0f;
            float duration = Mathf.Max(0.01f, blinkDuration);
            float frequency = Mathf.Max(0.5f, blinkFrequency);

            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duration);
                bool visible = Mathf.Repeat(t * frequency, 1f) > 0.3f;
                ApplyBlinkState(visible, k * maxEmissionMultiplier);
                HoldFrozenCameraPose();
                yield return null;
            }

            ApplyBlinkState(true, maxEmissionMultiplier);
            yield return PlayPreExplosionMotion(sphere);
            SpawnWhiteExplosion(sphere.transform.position);
        }
        else
        {
            yield return FadeWhite(0f, 1f, fadeToWhiteDuration);
        }

        SetOverlayVisible(true);
        if (sphere != null && questionSetPreset == QuestionSetPreset.Hard)
        {
            // Hard transition is pre-lit during portal absorption; avoid a visible second fade.
            SetWhiteAlpha(1f);
        }
        else if (sphere != null)
        {
            yield return FadeWhite(0f, 1f, fadeToWhiteDuration);
        }

        // Keep the screen fully white for one frame before any question UI appears.
        yield return null;

        if (isFollowupTestPad)
        {
            SetCodeExerciseVisible(false);
            SetMediumPromptVisible(true);
            mediumPromptText.text = string.Empty;
            yield return TypeTextInto(mediumPromptText, string.IsNullOrWhiteSpace(followupTestMessage) ? "test" : followupTestMessage, 0.03f);
            yield return new WaitForSecondsRealtime(1.2f);

            RestoreRendererStates();
            SetStartTextVisible(false);
            SetOverlayVisible(false);
            ReleasePlayer(sphere, fps, originalSphereScale);
            RestoreCameraIfNeeded();
            running = false;
            yield break;
        }

        SetupCodeExerciseForDisplay();
        yield return AnimateMediumPromptText();

        buttonPressed = false;
        leavePressed = false;
        continuePressed = false;
        TryHookAbandonButton();
        while (!buttonPressed && !leavePressed && !continuePressed)
        {
            if (watchedButton == null || !watchedButton.gameObject.activeInHierarchy)
            {
                TryHookAbandonButton();
            }

            HoldFrozenCameraPose();
            yield return null;
        }

        UnwatchButton();

        if (continuePressed)
        {
            if (sphere != null && questionSetPreset == QuestionSetPreset.Hard)
            {
                MoveSphereToSafePadSpot(sphere);
            }
            if (sphere != null)
            {
                sphere.transform.localScale = originalSphereScale;
            }
            HandleContinueToNextLevel(sphere, fps);
            yield break;
        }

        if (leavePressed)
        {
            if (sphere != null && questionSetPreset == QuestionSetPreset.Hard)
            {
                MoveSphereToSafePadSpot(sphere);
            }
            nextAllowedTriggerTime = Time.time + Mathf.Max(0f, mediumReentryCooldownSeconds);
        }

        RestoreRendererStates();
        SetStartTextVisible(false);
        SetOverlayVisible(false);

        ReleasePlayer(sphere, fps, originalSphereScale);
        RestoreCameraIfNeeded();
        running = false;
    }

    private void ReleasePlayer(SphereController sphere, FirstPersonControllerSimple fps, Vector3 originalSphereScale)
    {
        if (sphere != null)
        {
            sphere.transform.localScale = originalSphereScale;
            sphere.SetHardFreeze(false);
            sphere.SetMovementLocked(false);
        }

        if (fps != null)
        {
            fps.SetHardFreeze(false);
            fps.SetMovementLocked(false);
        }
    }

    private void TryHookAbandonButton()
    {
        Button[] buttons = Resources.FindObjectsOfTypeAll<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            Button candidate = buttons[i];
            if (candidate == null)
            {
                continue;
            }

            GameObject candidateObject = candidate.gameObject;
            if (candidateObject == null || !candidateObject.activeInHierarchy)
            {
                continue;
            }

            Text label = candidate.GetComponentInChildren<Text>(true);
            if (label == null)
            {
                continue;
            }

            string txt = (label.text ?? string.Empty).Trim();
            if (!txt.Equals("Abandon Question", System.StringComparison.OrdinalIgnoreCase) &&
                !txt.Equals("Forfait Question", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            UnwatchButton();

            watchedButton = candidate;
            watchedButton.onClick.RemoveListener(OnAbandonPressed);
            watchedButton.onClick.AddListener(OnAbandonPressed);
            return;
        }
    }

    private void OnAbandonPressed()
    {
        buttonPressed = true;
    }

    private void UnwatchButton()
    {
        if (watchedButton != null && watchedButton.gameObject != null)
        {
            watchedButton.onClick.RemoveListener(OnAbandonPressed);
        }

        watchedButton = null;
    }

    private void CacheRendererStates(SphereController sphere)
    {
        rendererStates.Clear();
        Renderer[] renderers = sphere.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null)
            {
                continue;
            }

            Material[] materials = r.materials;
            for (int m = 0; m < materials.Length; m++)
            {
                Material mat = materials[m];
                if (mat == null)
                {
                    continue;
                }

                RendererState state = new RendererState
                {
                    renderer = r,
                    rendererEnabled = r.enabled,
                    material = mat,
                    hasEmission = mat.HasProperty("_EmissionColor")
                };

                if (state.hasEmission)
                {
                    mat.EnableKeyword("_EMISSION");
                    state.emissionColor = mat.GetColor("_EmissionColor");
                }

                rendererStates.Add(state);
            }
        }
    }

    private void ApplyBlinkState(bool visible, float emissionMultiplier)
    {
        for (int i = 0; i < rendererStates.Count; i++)
        {
            RendererState state = rendererStates[i];
            if (state.renderer != null)
            {
                state.renderer.enabled = visible;
            }

            if (state.hasEmission && state.material != null)
            {
                state.material.SetColor("_EmissionColor", state.emissionColor * emissionMultiplier);
            }
        }
    }

    private void RestoreRendererStates()
    {
        for (int i = 0; i < rendererStates.Count; i++)
        {
            RendererState state = rendererStates[i];
            if (state.renderer != null)
            {
                state.renderer.enabled = state.rendererEnabled;
            }

            if (state.hasEmission && state.material != null)
            {
                state.material.SetColor("_EmissionColor", state.emissionColor);
            }
        }

        rendererStates.Clear();
    }

    private IEnumerator PlayHardPortalAbsorptionMotion(SphereController sphere)
    {
        if (sphere == null)
        {
            yield break;
        }

        Camera cam = Camera.main;
        Vector3 start = sphere.transform.position;
        Vector3 forward = transform.forward;
        Vector3 side = transform.right;
        if (cam != null)
        {
            forward = cam.transform.forward;
            side = cam.transform.right;
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        if (side.sqrMagnitude < 0.0001f)
        {
            side = Vector3.right;
        }

        forward.Normalize();
        side.Normalize();
        Vector3 up = Vector3.up;
        float distance = Mathf.Max(1.8f, hardPortalDistance + 1.2f);
        Vector3 portalPos = start + (side * distance) + (up * Mathf.Max(0.8f, hardPortalUpOffset + 0.2f)) + (forward * 0.45f);
        Vector3 lookDir = (start - portalPos).normalized;
        if (lookDir.sqrMagnitude < 0.0001f)
        {
            lookDir = -side;
        }

        Quaternion portalRot = Quaternion.LookRotation(lookDir, up);
        Transform outerRing;
        Transform innerRing;
        Transform core;
        Light glow;
        GameObject portalRoot = CreateHardPortalVisual(portalPos, portalRot, out outerRing, out innerRing, out core, out glow);
        Renderer coreRenderer = core != null ? core.GetComponent<Renderer>() : null;
        Material coreMaterial = coreRenderer != null ? coreRenderer.material : null;
        float baseRadius = Mathf.Max(0.4f, hardPortalRadius);
        Vector3 outerTargetScale = new Vector3(baseRadius * 2.2f, 0.06f, baseRadius * 2.2f);
        Vector3 innerTargetScale = new Vector3(baseRadius * 1.5f, 0.04f, baseRadius * 1.5f);
        Vector3 coreTargetScale = new Vector3(baseRadius * 1.35f, baseRadius * 1.35f, 1f);
        Vector3 sphereStartScale = sphere.transform.localScale;

        float openDuration = Mathf.Max(0.15f, hardPortalOpenDuration);
        float tOpen = 0f;
        while (tOpen < openDuration)
        {
            tOpen += Time.deltaTime;
            float k = Mathf.Clamp01(tOpen / openDuration);
            float smooth = k * k * (3f - (2f * k));
            float pulse = 1f + (Mathf.Sin(Time.time * 7f) * 0.08f);
            outerRing.localScale = outerTargetScale * smooth * pulse;
            innerRing.localScale = innerTargetScale * smooth * (2f - pulse);
            core.localScale = coreTargetScale * smooth;
            AnimatePortalVisual(outerRing, innerRing, core, glow, 1f);
            UpdateHardZoomCameraPose(sphere.transform.position, portalPos, 0f);
            yield return null;
        }

        float pullDuration = Mathf.Max(0.3f, hardPortalPullDuration);
        float tPull = 0f;
        Vector3 control = Vector3.Lerp(start, portalPos, 0.5f) + (up * 1.25f) + (forward * 0.2f);
        while (tPull < pullDuration)
        {
            tPull += Time.deltaTime;
            float k = Mathf.Clamp01(tPull / pullDuration);
            float smooth = k * k * (3f - (2f * k));
            Vector3 arc = ((1f - smooth) * (1f - smooth) * start) +
                          (2f * (1f - smooth) * smooth * control) +
                          (smooth * smooth * portalPos);
            float swirl = (1f - smooth) * 0.24f;
            float angle = smooth * Mathf.PI * 5f;
            arc += (side * Mathf.Sin(angle) + up * Mathf.Cos(angle)) * swirl;
            sphere.transform.position = arc;

            float sphereSpin = (hardPortalTwistSpeed * 1.15f) * Time.deltaTime;
            sphere.transform.Rotate(up, sphereSpin, Space.World);
            sphere.transform.Rotate(side, sphereSpin * 0.72f, Space.World);

            float pullBoost = Mathf.Lerp(1f, 1.35f, smooth);
            AnimatePortalVisual(outerRing, innerRing, core, glow, pullBoost);
            UpdateHardZoomCameraPose(sphere.transform.position, portalPos, smooth);
            yield return null;
        }

        float chargeDuration = 0.5f;
        Light sphereChargeLight = CreateSphereChargeLight();
        Transform camTr = frozenCameraTransform != null ? frozenCameraTransform : (Camera.main != null ? Camera.main.transform : null);
        Vector3 orbitStartOffset = camTr != null ? (camTr.position - sphere.transform.position) : new Vector3(0f, 3f, -5f);
        Vector3 orbitStartFlat = Vector3.ProjectOnPlane(orbitStartOffset, Vector3.up);
        if (orbitStartFlat.sqrMagnitude < 0.001f)
        {
            orbitStartFlat = Vector3.back * 5f;
        }

        float orbitRadius = Mathf.Clamp(orbitStartFlat.magnitude, 4f, 6.8f);
        float orbitHeight = Mathf.Clamp(orbitStartOffset.y, 2.4f, 4.8f);
        Vector3 orbitStartDir = orbitStartFlat.normalized;
        float tCharge = 0f;
        Vector3 chargeStartPos = sphere.transform.position;
        Vector3 chargeStartScale = sphere.transform.localScale;
        while (tCharge < chargeDuration)
        {
            tCharge += Time.deltaTime;
            float k = Smooth01(tCharge / chargeDuration);
            float inv = 1f - k;
            float pulse = Mathf.Lerp(1f, 1.52f, k);
            outerRing.localScale = outerTargetScale * pulse;
            innerRing.localScale = innerTargetScale * Mathf.Lerp(1.95f, 1.28f, k);
            core.localScale = coreTargetScale * Mathf.Lerp(1.08f, 1.82f, k);
            if (glow != null)
            {
                glow.intensity = Mathf.Lerp(4.2f, 13.5f, k);
                glow.range = Mathf.Lerp(7.4f, 17.2f, k);
            }
            ApplyPortalCoreBlackHoleLook(coreMaterial, k);

            Vector3 chargeDrift = (side * Mathf.Sin(Time.time * 8.2f) + up * Mathf.Cos(Time.time * 6.8f)) * (0.08f * inv);
            sphere.transform.position = Vector3.Lerp(chargeStartPos, portalPos, k) + chargeDrift;
            sphere.transform.localScale = Vector3.Lerp(chargeStartScale, sphereStartScale * 0.16f, k);
            ApplySphereChargeGlow(Mathf.Lerp(2.4f, 11f, k));
            if (sphereChargeLight != null)
            {
                sphereChargeLight.transform.position = sphere.transform.position;
                sphereChargeLight.intensity = Mathf.Lerp(0.2f, 14f, k);
                sphereChargeLight.range = Mathf.Lerp(1.4f, 18f, k);
            }

            AnimatePortalVisual(outerRing, innerRing, core, glow, Mathf.Lerp(0.95f, 0.25f, k));
            UpdateHardEntryOrbitCamera(sphere.transform.position, portalPos, orbitStartDir, orbitRadius, orbitHeight, Mathf.Lerp(0f, 0.58f, k));
            // Keep charge visible; full white is applied later.
            SetWhiteAlpha(Mathf.Lerp(0f, 0.35f, k));
            yield return null;
        }

        float crushDuration = 0.38f;
        float tCrush = 0f;
        Vector3 crushStartPos = sphere.transform.position;
        Vector3 crushStartScale = sphere.transform.localScale;
        while (tCrush < crushDuration)
        {
            tCrush += Time.deltaTime;
            float k = Smooth01(tCrush / crushDuration);
            sphere.transform.position = Vector3.Lerp(crushStartPos, portalPos, k);
            Vector3 crushedLocal = new Vector3(
                Mathf.Lerp(crushStartScale.x, sphereStartScale.x * 0.045f, k),
                Mathf.Lerp(crushStartScale.y * 1.15f, sphereStartScale.y * 0.02f, k),
                Mathf.Lerp(crushStartScale.z, sphereStartScale.z * 0.045f, k));
            sphere.transform.localScale = crushedLocal;

            ApplySphereChargeGlow(Mathf.Lerp(11f, 15f, k));
            if (glow != null)
            {
                glow.intensity = Mathf.Lerp(13.5f, 16.8f, k);
                glow.range = Mathf.Lerp(17.2f, 19.5f, k);
            }
            ApplyPortalCoreBlackHoleLook(coreMaterial, Mathf.Lerp(1f, 1.2f, k));
            UpdateHardEntryOrbitCamera(sphere.transform.position, portalPos, orbitStartDir, orbitRadius, orbitHeight, Mathf.Lerp(0.58f, 0.9f, k));
            SetWhiteAlpha(Mathf.Lerp(0.35f, 0.55f, k));
            yield return null;
        }

        float finalOrbitDuration = 0.28f;
        float tFinalOrbit = 0f;
        while (tFinalOrbit < finalOrbitDuration)
        {
            tFinalOrbit += Time.deltaTime;
            float k = Smooth01(tFinalOrbit / finalOrbitDuration);
            UpdateHardEntryOrbitCamera(sphere.transform.position, portalPos, orbitStartDir, orbitRadius, orbitHeight, Mathf.Lerp(0.9f, 1f, k));
            yield return null;
        }

        if (sphereChargeLight != null)
        {
            Destroy(sphereChargeLight.gameObject);
        }

        SpawnBlackHoleImplosionFx(portalPos);
        SpawnMegaPortalFinale(cam, portalPos);
        yield return new WaitForSecondsRealtime(0.06f);
        if (portalRoot != null)
        {
            Destroy(portalRoot);
        }

        sphere.transform.position = portalPos;
        sphere.transform.localScale = sphereStartScale * 0.05f;
    }

    private void UpdateHardZoomCameraPose(Vector3 spherePos, Vector3 portalPos, float zoomK)
    {
        Transform camTr = frozenCameraTransform != null ? frozenCameraTransform : (Camera.main != null ? Camera.main.transform : null);
        if (camTr == null)
        {
            return;
        }

        Vector3 pullDir = (portalPos - spherePos).normalized;
        if (pullDir.sqrMagnitude < 0.0001f)
        {
            pullDir = transform.forward.sqrMagnitude > 0.001f ? transform.forward.normalized : Vector3.forward;
        }

        Vector3 side = Vector3.Cross(Vector3.up, pullDir).normalized;
        if (side.sqrMagnitude < 0.0001f)
        {
            side = Vector3.right;
        }

        float k = Mathf.Clamp01(zoomK);
        float backOffset = Mathf.Lerp(7.2f, 3.4f, k);
        float sideOffset = Mathf.Lerp(3.6f, 1.35f, k);
        float heightOffset = Mathf.Lerp(4.0f, 2.0f, k);
        Vector3 targetPos = spherePos - (pullDir * backOffset) + (side * sideOffset) + (Vector3.up * heightOffset);

        float followSmoothTime = Mathf.Lerp(0.28f, 0.14f, k);
        camTr.position = Vector3.SmoothDamp(camTr.position, targetPos, ref hardCameraVelocity, followSmoothTime, Mathf.Infinity, Time.deltaTime);

        Quaternion targetRot = Quaternion.LookRotation((spherePos + (pullDir * 0.15f)) - camTr.position, Vector3.up);
        float rotLerp = 1f - Mathf.Exp(-5.8f * Time.deltaTime);
        camTr.rotation = Quaternion.Slerp(camTr.rotation, targetRot, rotLerp);
    }

    private static float Smooth01(float t)
    {
        float k = Mathf.Clamp01(t);
        return k * k * k * (k * (6f * k - 15f) + 10f);
    }

    private void UpdateHardEntryOrbitCamera(
        Vector3 spherePos,
        Vector3 portalPos,
        Vector3 orbitStartDir,
        float orbitRadius,
        float orbitHeight,
        float orbitProgress)
    {
        Transform camTr = frozenCameraTransform != null ? frozenCameraTransform : (Camera.main != null ? Camera.main.transform : null);
        if (camTr == null)
        {
            return;
        }

        float k = Mathf.Clamp01(orbitProgress);
        float angle = Mathf.Lerp(0f, 360f, k);
        Vector3 orbitDir = Quaternion.AngleAxis(angle, Vector3.up) * orbitStartDir;
        Vector3 toPortal = (portalPos - spherePos).normalized;
        if (toPortal.sqrMagnitude < 0.0001f)
        {
            toPortal = orbitStartDir.sqrMagnitude > 0.0001f ? orbitStartDir : Vector3.forward;
        }

        Vector3 rightBiasDir = Vector3.Cross(Vector3.up, toPortal).normalized;
        if (rightBiasDir.sqrMagnitude < 0.0001f)
        {
            rightBiasDir = Vector3.right;
        }

        Vector3 orbitPivot = portalPos + (Vector3.up * 0.22f);
        Vector3 targetPos = orbitPivot + (orbitDir * orbitRadius) + (Vector3.up * orbitHeight) + (rightBiasDir * 0.18f);
        float posFollow = 1f - Mathf.Exp(-Mathf.Lerp(8f, 4.2f, Mathf.Clamp01(orbitProgress)) * Time.deltaTime);
        camTr.position = Vector3.Lerp(camTr.position, targetPos, posFollow);

        Vector3 lookTarget = orbitPivot;
        Quaternion lookRot = Quaternion.LookRotation(lookTarget - camTr.position, Vector3.up);
        camTr.rotation = lookRot;
    }

    private void ApplyPortalCoreBlackHoleLook(Material mat, float amount)
    {
        if (mat == null)
        {
            return;
        }

        float k = Mathf.Clamp01(amount);
        Color baseColor = Color.Lerp(new Color(0.12f, 0.2f, 0.55f, 1f), new Color(0.01f, 0.01f, 0.02f, 1f), k);
        mat.color = baseColor;
        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            Color rim = Color.Lerp(new Color(0.28f, 0.5f, 1f, 1f), new Color(0.95f, 1f, 1f, 1f), Mathf.Clamp01(k * 0.8f));
            float emissionPow = Mathf.Lerp(1.2f, 4.4f, k);
            mat.SetColor("_EmissionColor", rim * emissionPow);
        }
    }

    private Light CreateSphereChargeLight()
    {
        GameObject lightObj = new GameObject("HardSphereChargeLight");
        Light l = lightObj.AddComponent<Light>();
        l.type = LightType.Point;
        l.color = new Color(0.55f, 0.92f, 1f, 1f);
        l.intensity = 0.2f;
        l.range = 1.4f;
        return l;
    }

    private void SpawnBlackHoleImplosionFx(Vector3 position)
    {
        GameObject root = new GameObject("HardBlackHoleImplosion");
        root.transform.position = position;
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.2f, 0.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.16f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.05f, 0.05f, 0.08f, 1f),
            new Color(0.26f, 0.42f, 1f, 0.9f));
        main.maxParticles = 1200;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 700),
            new ParticleSystem.Burst(0.04f, 450)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;

        ps.Play();
        Destroy(root, 1.2f);
    }

    private void ApplySphereChargeGlow(float intensityMultiplier)
    {
        for (int i = 0; i < rendererStates.Count; i++)
        {
            RendererState state = rendererStates[i];
            if (state.renderer == null)
            {
                continue;
            }

            state.renderer.enabled = true;
            if (state.hasEmission && state.material != null)
            {
                state.material.EnableKeyword("_EMISSION");
                state.material.SetColor("_EmissionColor", state.emissionColor * intensityMultiplier);
            }
        }
    }

    private void AnimatePortalVisual(Transform outerRing, Transform innerRing, Transform core, Light glow, float intensityMultiplier)
    {
        if (outerRing != null)
        {
            outerRing.Rotate(Vector3.forward, hardPortalTwistSpeed * intensityMultiplier * Time.deltaTime, Space.Self);
        }

        if (innerRing != null)
        {
            innerRing.Rotate(Vector3.forward, -hardPortalTwistSpeed * 1.35f * intensityMultiplier * Time.deltaTime, Space.Self);
        }

        if (core != null)
        {
            core.Rotate(Vector3.forward, hardPortalTwistSpeed * 0.45f * intensityMultiplier * Time.deltaTime, Space.Self);
        }

        if (glow != null)
        {
            glow.intensity = Mathf.Lerp(2.0f, 3.2f, (Mathf.Sin(Time.time * 8f) + 1f) * 0.5f) * intensityMultiplier;
            glow.range = Mathf.Lerp(3.6f, 5.8f, (Mathf.Cos(Time.time * 6.2f) + 1f) * 0.5f);
        }
    }

    private void SpawnMegaPortalFinale(Camera cam, Vector3 portalPosition)
    {
        SpawnPortalBurst(portalPosition);

        Vector3 fxPos = portalPosition;
        if (cam != null)
        {
            fxPos = cam.transform.position + (cam.transform.forward * 5f);
        }

        GameObject root = new GameObject("HardPortalMegaFinale");
        root.transform.position = fxPos;

        ParticleSystem wide = root.AddComponent<ParticleSystem>();
        wide.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = wide.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.6f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(4.5f, 11.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.38f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.68f, 1f, 1f, 1f),
            new Color(0.4f, 0.65f, 1f, 0.95f));
        main.maxParticles = 3200;

        var emission = wide.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 1450),
            new ParticleSystem.Burst(0.06f, 900),
            new ParticleSystem.Burst(0.12f, 650)
        });

        var shape = wide.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.35f;

        var colorOverLifetime = wide.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient g = new Gradient();
        g.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.8f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.44f, 0.78f, 1f), 0.45f),
                new GradientColorKey(new Color(0.12f, 0.24f, 0.82f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = g;

        var noise = wide.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.18f, 0.52f);
        noise.frequency = 0.8f;
        noise.scrollSpeed = 0.9f;

        wide.Play();
        Destroy(root, 2.8f);
    }

    private GameObject CreateHardPortalVisual(
        Vector3 position,
        Quaternion rotation,
        out Transform outerRing,
        out Transform innerRing,
        out Transform core,
        out Light glow)
    {
        GameObject root = new GameObject("HardPortalVisual");
        root.transform.position = position;
        root.transform.rotation = rotation;

        Material outerMat = CreateRuntimeMaterial(new Color(0.08f, 0.84f, 1f, 1f));
        Material innerMat = CreateRuntimeMaterial(new Color(0.2f, 0.45f, 1f, 1f));
        Material coreMat = CreateRuntimeMaterial(new Color(0.12f, 0.2f, 0.55f, 1f));

        if (outerMat != null)
        {
            outerMat.EnableKeyword("_EMISSION");
            outerMat.SetColor("_EmissionColor", new Color(0.14f, 0.95f, 1f, 1f) * 2.2f);
        }

        if (innerMat != null)
        {
            innerMat.EnableKeyword("_EMISSION");
            innerMat.SetColor("_EmissionColor", new Color(0.35f, 0.6f, 1f, 1f) * 2.5f);
        }

        if (coreMat != null)
        {
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_EmissionColor", new Color(0.28f, 0.5f, 1f, 1f) * 1.3f);
        }

        GameObject outer = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        outer.name = "PortalOuterRing";
        outer.transform.SetParent(root.transform, false);
        outer.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        outerRing = outer.transform;
        Collider outerCollider = outer.GetComponent<Collider>();
        if (outerCollider != null)
        {
            Destroy(outerCollider);
        }

        AssignMaterial(outer, outerMat);

        GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        inner.name = "PortalInnerRing";
        inner.transform.SetParent(root.transform, false);
        inner.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        innerRing = inner.transform;
        Collider innerCollider = inner.GetComponent<Collider>();
        if (innerCollider != null)
        {
            Destroy(innerCollider);
        }

        AssignMaterial(inner, innerMat);

        GameObject coreObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        coreObject.name = "PortalCore";
        coreObject.transform.SetParent(root.transform, false);
        coreObject.transform.localRotation = Quaternion.identity;
        core = coreObject.transform;
        Collider coreCollider = coreObject.GetComponent<Collider>();
        if (coreCollider != null)
        {
            Destroy(coreCollider);
        }

        AssignMaterial(coreObject, coreMat);

        GameObject haloObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
        haloObject.name = "PortalHalo";
        haloObject.transform.SetParent(root.transform, false);
        haloObject.transform.localRotation = Quaternion.identity;
        haloObject.transform.localPosition = new Vector3(0f, 0f, 0.01f);
        haloObject.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
        Collider haloCollider = haloObject.GetComponent<Collider>();
        if (haloCollider != null)
        {
            Destroy(haloCollider);
        }

        Material haloMat = CreateRuntimeMaterial(new Color(0.18f, 0.82f, 1f, 1f));
        if (haloMat != null)
        {
            haloMat.EnableKeyword("_EMISSION");
            haloMat.SetColor("_EmissionColor", new Color(0.45f, 0.95f, 1f, 1f) * 2.8f);
            AssignMaterial(haloObject, haloMat);
        }

        ParticleSystem ringParticles = CreatePortalParticles(root.transform);
        ringParticles.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        ringParticles.Play();

        ParticleSystem sparkParticles = CreatePortalSparkParticles(root.transform);
        sparkParticles.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        sparkParticles.Play();

        glow = root.AddComponent<Light>();
        glow.type = LightType.Point;
        glow.color = new Color(0.35f, 0.76f, 1f, 1f);
        glow.intensity = 0.1f;
        glow.range = 1.5f;

        outerRing.localScale = Vector3.zero;
        innerRing.localScale = Vector3.zero;
        core.localScale = Vector3.zero;
        return root;
    }

    private ParticleSystem CreatePortalParticles(Transform parent)
    {
        GameObject particlesObject = new GameObject("PortalParticles");
        particlesObject.transform.SetParent(parent, false);
        ParticleSystem ps = particlesObject.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.03f, 0.1f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.35f, 0.95f, 1f, 0.95f),
            new Color(0.1f, 0.35f, 1f, 0.78f));
        main.maxParticles = 320;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 220f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Max(0.36f, hardPortalRadius * 0.68f);
        shape.radiusThickness = 0.88f;

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);
        noise.frequency = 0.65f;
        noise.scrollSpeed = 0.85f;

        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.x = new ParticleSystem.MinMaxCurve(-0.65f, 0.65f);
        force.y = new ParticleSystem.MinMaxCurve(-0.45f, 0.45f);
        force.z = new ParticleSystem.MinMaxCurve(-0.65f, 0.65f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.55f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.32f, 0.58f, 1f), 0.6f),
                new GradientColorKey(new Color(0.08f, 0.17f, 0.72f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(0.7f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve();
        sizeCurve.AddKey(0f, 0.15f);
        sizeCurve.AddKey(0.2f, 1f);
        sizeCurve.AddKey(1f, 0.1f);
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        return ps;
    }

    private ParticleSystem CreatePortalSparkParticles(Transform parent)
    {
        GameObject particlesObject = new GameObject("PortalSparkParticles");
        particlesObject.transform.SetParent(parent, false);
        ParticleSystem ps = particlesObject.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.42f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.8f, 4.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.72f, 1f, 1f, 0.95f),
            new Color(0.45f, 0.7f, 1f, 0.8f));
        main.maxParticles = 260;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 140f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Max(0.2f, hardPortalRadius * 0.5f);
        shape.radiusThickness = 0.2f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.25f, 0.25f);

        return ps;
    }

    private void SpawnPortalBurst(Vector3 position)
    {
        GameObject root = new GameObject("HardPortalCollapseBurst");
        root.transform.position = position;
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2.8f, 5.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.15f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(0.46f, 1f, 1f, 1f),
            new Color(0.45f, 0.62f, 1f, 0.9f));
        main.maxParticles = 240;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 130),
            new ParticleSystem.Burst(0.04f, 70)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = Mathf.Max(0.15f, hardPortalRadius * 0.2f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.64f, 1f, 1f), 0f),
                new GradientColorKey(new Color(0.3f, 0.55f, 1f), 0.8f),
                new GradientColorKey(new Color(0.12f, 0.22f, 0.7f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.45f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = gradient;

        ps.Play();
        Destroy(root, 1.4f);
    }

    private IEnumerator PlayPreExplosionMotion(SphereController sphere)
    {
        if (sphere == null)
        {
            yield break;
        }

        Camera cam = Camera.main;
        Vector3 start = sphere.transform.position;
        Vector3 target = start + (Vector3.up * Mathf.Max(0.2f, liftHeight));
        float startCameraDepth = 0f;
        float targetCameraDepth = 0f;
        bool hasCameraCenterPath = false;
        Vector3 orbitAxisA = Vector3.right;
        Vector3 orbitAxisB = Vector3.forward;
        float totalTurns = Mathf.Max(1f, totalTurnsBeforeExplosion);
        float centerStartsAfterTurns = Mathf.Clamp(centerOnTurnIndex - 1f, 0f, totalTurns);

        if (cam != null)
        {
            Vector3 camPos = cam.transform.position;
            Vector3 camForward = cam.transform.forward.normalized;
            startCameraDepth = Vector3.Dot(start - camPos, camForward);
            if (startCameraDepth > 0.1f)
            {
                targetCameraDepth = Mathf.Clamp(finalCameraDepth, 0.08f, startCameraDepth - 0.05f);
                hasCameraCenterPath = true;
                orbitAxisA = cam.transform.right.normalized;
                orbitAxisB = cam.transform.up.normalized;
            }
        }

        float t = 0f;
        float duration = Mathf.Max(0.05f, preExplosionMotionDuration);
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - (2f * k));
            float turnProgress = totalTurns * smooth;
            float angle = turnProgress * Mathf.PI * 2f;

            Vector3 nextPos = Vector3.Lerp(start, target, smooth);
            Vector3 orbitCenter = nextPos;
            if (hasCameraCenterPath)
            {
                float depth = Mathf.Lerp(startCameraDepth, targetCameraDepth, smooth);
                orbitCenter = cam.transform.position + (cam.transform.forward.normalized * depth);
            }

            float orbitT = 0f;
            if (centerStartsAfterTurns > 0.001f)
            {
                orbitT = Mathf.Clamp01(1f - (turnProgress / centerStartsAfterTurns));
            }

            float radius = Mathf.Max(0f, orbitRadius) * orbitT;
            Vector3 orbitOffset = (orbitAxisA * Mathf.Cos(angle) + orbitAxisB * Mathf.Sin(angle)) * radius;
            nextPos = orbitCenter + orbitOffset;
            sphere.transform.position = nextPos;

            float spin = spinSpeed * Time.deltaTime;
            sphere.transform.Rotate(Vector3.up, spin, Space.World);
            sphere.transform.Rotate(Vector3.right, spin * 0.58f, Space.Self);
            HoldFrozenCameraPose();
            yield return null;
        }

        if (hasCameraCenterPath)
        {
            sphere.transform.position = cam.transform.position + (cam.transform.forward.normalized * targetCameraDepth);
        }
        else
        {
            sphere.transform.position = target;
        }
    }

    private void SpawnWhiteExplosion(Vector3 position)
    {
        if (whiteExplosionPrefab != null)
        {
            ParticleSystem fx = Instantiate(whiteExplosionPrefab, position, Quaternion.identity);
            fx.Play();
            float cleanup = fx.main.duration + fx.main.startLifetime.constantMax + 0.25f;
            Destroy(fx.gameObject, cleanup);
            return;
        }

        GameObject root = new GameObject("MediumPadWhiteExplosion");
        root.transform.position = position;
        ParticleSystem ps = root.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.22f, 0.45f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1.2f, 3.2f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.24f);
        main.startColor = Color.white;
        main.maxParticles = 240;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[]
        {
            new ParticleSystem.Burst(0f, 140),
            new ParticleSystem.Burst(0.06f, 90)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.15f;

        ps.Play();
        Destroy(root, 1.2f);
    }

    private IEnumerator FadeWhite(float from, float to, float duration)
    {
        SetWhiteAlpha(from);

        if (duration <= 0.001f)
        {
            SetWhiteAlpha(to);
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float smooth = k * k * (3f - (2f * k));
            SetWhiteAlpha(Mathf.Lerp(from, to, smooth));
            HoldFrozenCameraPose();
            yield return null;
        }

        SetWhiteAlpha(to);
    }

    private void EnsureOverlay()
    {
        if ((!autoCreateOverlay && (whiteImage == null || startText == null || mediumPromptText == null || mediumCodeInputField == null || mediumVerifyButton == null || mediumVerifyResultText == null || mediumLeaveButton == null || mediumContinueButton == null || mediumHintButton == null || mediumTutorialPanel == null || mediumBackToQuestionButton == null || mediumTutorialBodyText == null)) ||
            (whiteImage != null && startText != null && mediumPromptText != null && mediumCodeInputField != null && mediumVerifyButton != null && mediumVerifyResultText != null && mediumLeaveButton != null && mediumContinueButton != null && mediumHintButton != null && mediumTutorialPanel != null && mediumBackToQuestionButton != null && mediumTutorialBodyText != null))
        {
            return;
        }

        if (whiteImage != null && startText != null)
        {
            if (overlayCanvas == null)
            {
                overlayCanvas = whiteImage.GetComponentInParent<Canvas>();
            }

            if (overlayCanvas != null)
            {
                if (mediumPromptText == null)
                {
                    mediumPromptText = CreateLeftPromptText(overlayCanvas.transform);
                }

                if (mediumCodeInputField == null || mediumVerifyButton == null || mediumVerifyResultText == null || mediumLeaveButton == null || mediumContinueButton == null || mediumHintButton == null || mediumTutorialPanel == null || mediumBackToQuestionButton == null || mediumTutorialBodyText == null)
                {
                    CreateRightCodeExerciseUI(overlayCanvas.transform);
                    EnsureEventSystemExists();
                }
                return;
            }
        }

        GameObject canvasObj = new GameObject("MediumPadStartOverlayCanvas");
        overlayCanvas = canvasObj.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 11000;
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject bgObj = new GameObject("WhiteScreen");
        bgObj.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        whiteImage = bgObj.AddComponent<Image>();
        whiteImage.color = new Color(0.985f, 0.99f, 1f, 1f);
        whiteImage.raycastTarget = false;

        GameObject topTintObj = new GameObject("TopTint");
        topTintObj.transform.SetParent(canvasObj.transform, false);
        RectTransform topTintRect = topTintObj.AddComponent<RectTransform>();
        topTintRect.anchorMin = new Vector2(0f, 0.78f);
        topTintRect.anchorMax = new Vector2(1f, 1f);
        topTintRect.offsetMin = Vector2.zero;
        topTintRect.offsetMax = Vector2.zero;
        Image topTint = topTintObj.AddComponent<Image>();
        topTint.color = new Color(0.90f, 0.94f, 1f, 0.28f);
        topTint.raycastTarget = false;

        GameObject txtObj = new GameObject("StartLabel");
        txtObj.transform.SetParent(canvasObj.transform, false);
        RectTransform txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = new Vector2(0.5f, 0.5f);
        txtRect.anchorMax = new Vector2(0.5f, 0.5f);
        txtRect.anchoredPosition = Vector2.zero;
        txtRect.sizeDelta = new Vector2(560f, 140f);

        startText = txtObj.AddComponent<Text>();
        startText.text = string.Empty;
        startText.alignment = TextAnchor.MiddleCenter;
        startText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        startText.fontSize = 110;
        startText.fontStyle = FontStyle.Bold;
        startText.color = Color.black;
        startText.raycastTarget = false;

        mediumPromptText = CreateLeftPromptText(canvasObj.transform);
        CreateRightCodeExerciseUI(canvasObj.transform);
        EnsureEventSystemExists();
    }

    private void SetOverlayVisible(bool visible)
    {
        if (overlayCanvas != null)
        {
            overlayCanvas.enabled = visible;
        }

        SetStartTextVisible(false);
        SetMediumPromptVisible(false);
        SetCodeExerciseVisible(false);

        if (!visible)
        {
            SetWhiteAlpha(0f);
        }
    }

    private void HideOverlayImmediate()
    {
        SetOverlayVisible(false);
        SetWhiteAlpha(0f);
        SetStartTextVisible(false);
        SetMediumPromptVisible(false);
    }

    private static void HideAllSequenceOverlays()
    {
        MediumQuestionPadStartSequence[] sequences = Object.FindObjectsOfType<MediumQuestionPadStartSequence>();
        for (int i = 0; i < sequences.Length; i++)
        {
            MediumQuestionPadStartSequence sequence = sequences[i];
            if (sequence == null)
            {
                continue;
            }

            sequence.HideOverlayImmediate();
        }
    }

    private void SetWhiteAlpha(float alpha)
    {
        if (whiteImage == null)
        {
            return;
        }

        Color c = whiteImage.color;
        c.a = Mathf.Clamp01(alpha);
        whiteImage.color = c;
        bool canRender = overlayCanvas == null || overlayCanvas.enabled;
        whiteImage.enabled = canRender && c.a > 0.0001f;
    }

    private void SetStartTextVisible(bool visible)
    {
        if (startText != null)
        {
            startText.enabled = visible;
        }
    }

    private void SetMediumPromptVisible(bool visible)
    {
        if (mediumPromptText == null)
        {
            return;
        }

        Transform promptRoot = mediumPromptText.transform.parent;
        if (promptRoot != null)
        {
            promptRoot.gameObject.SetActive(visible);
        }

        mediumPromptText.enabled = visible;
        if (!visible)
        {
            mediumPromptText.text = string.Empty;
            Color c = mediumPromptText.color;
            c.a = 0f;
            mediumPromptText.color = c;
        }
    }

    private void SetCodeExerciseVisible(bool visible)
    {
        if (!visible && verifyPulseRoutine != null)
        {
            StopCoroutine(verifyPulseRoutine);
            verifyPulseRoutine = null;
        }
        if (!visible && tutorialTypeRoutine != null)
        {
            StopCoroutine(tutorialTypeRoutine);
            tutorialTypeRoutine = null;
        }

        SetQuestionUiVisible(visible);
        ShowTutorialUi(false, false);
    }

    private void SetupCodeExerciseForDisplay()
    {
        SetCodeExerciseVisible(true);
        tutorialOpen = false;
        ShowTutorialUi(false, false);
        if (mediumCodeInputField != null)
        {
            mediumCodeInputField.text = mediumCssInitialCode;
            mediumCodeInputField.ActivateInputField();
        }

        if (mediumContinueButton != null)
        {
            mediumContinueButton.gameObject.SetActive(false);
        }

        if (mediumVerifyResultText != null)
        {
            mediumVerifyResultText.text = string.Empty;
        }
    }

    private void SetQuestionUiVisible(bool visible)
    {
        if (mediumPromptText != null)
        {
            mediumPromptText.enabled = visible;
        }

        if (mediumCodePanel != null && mediumCodePanel.gameObject != null)
        {
            mediumCodePanel.gameObject.SetActive(visible);
        }

        if (mediumCodeInputField != null)
        {
            mediumCodeInputField.gameObject.SetActive(visible);
        }

        if (mediumVerifyButton != null)
        {
            mediumVerifyButton.gameObject.SetActive(visible);
            mediumVerifyButton.onClick.RemoveListener(OnMediumVerifyPressed);
            if (visible)
            {
                mediumVerifyButton.onClick.AddListener(OnMediumVerifyPressed);
            }
        }

        if (mediumLeaveButton != null)
        {
            mediumLeaveButton.gameObject.SetActive(visible);
            mediumLeaveButton.onClick.RemoveListener(OnLeavePressed);
            if (visible)
            {
                mediumLeaveButton.onClick.AddListener(OnLeavePressed);
            }
        }

        if (mediumHintButton != null)
        {
            mediumHintButton.gameObject.SetActive(visible);
            mediumHintButton.onClick.RemoveListener(OnHintPressed);
            if (visible)
            {
                mediumHintButton.onClick.AddListener(OnHintPressed);
            }
        }

        if (mediumContinueButton != null)
        {
            mediumContinueButton.onClick.RemoveListener(OnContinuePressed);
            mediumContinueButton.gameObject.SetActive(false);
            if (visible)
            {
                mediumContinueButton.onClick.AddListener(OnContinuePressed);
            }
        }

        if (mediumVerifyResultText != null)
        {
            mediumVerifyResultText.gameObject.SetActive(visible);
            if (visible)
            {
                mediumVerifyResultText.text = string.Empty;
            }
        }
    }

    private void ShowTutorialUi(bool show, bool animate)
    {
        tutorialOpen = show;

        if (tutorialTypeRoutine != null)
        {
            StopCoroutine(tutorialTypeRoutine);
            tutorialTypeRoutine = null;
        }

        if (mediumTutorialPanel != null && mediumTutorialPanel.gameObject != null)
        {
            mediumTutorialPanel.gameObject.SetActive(show);
        }

        if (mediumBackToQuestionButton != null)
        {
            mediumBackToQuestionButton.gameObject.SetActive(show);
            mediumBackToQuestionButton.onClick.RemoveListener(OnBackToQuestionPressed);
            if (show)
            {
                mediumBackToQuestionButton.onClick.AddListener(OnBackToQuestionPressed);
            }
        }

        if (show)
        {
            SetQuestionUiVisible(false);
            if (mediumTutorialTitleText != null)
            {
                mediumTutorialTitleText.text = "Quick Hint";
                mediumTutorialTitleText.rectTransform.localScale = Vector3.one;
            }

            if (mediumTutorialBodyText != null)
            {
                mediumTutorialBodyText.text = string.Empty;
            }

            if (animate)
            {
                tutorialTypeRoutine = StartCoroutine(AnimateTutorialText());
            }
            else if (mediumTutorialBodyText != null)
            {
                mediumTutorialBodyText.text = mediumTutorialContent;
            }
            return;
        }

        if (mediumTutorialBodyText != null)
        {
            mediumTutorialBodyText.text = string.Empty;
        }
        if (mediumTutorialTitleText != null)
        {
            mediumTutorialTitleText.rectTransform.localScale = Vector3.one;
        }
    }

    private void OnHintPressed()
    {
        ShowTutorialUi(true, true);
    }

    private void OnBackToQuestionPressed()
    {
        ShowTutorialUi(false, false);
        SetQuestionUiVisible(true);
        if (mediumContinueButton != null)
        {
            mediumContinueButton.gameObject.SetActive(false);
        }
    }

    private IEnumerator AnimateTutorialText()
    {
        if (mediumTutorialBodyText == null)
        {
            yield break;
        }

        string full = string.IsNullOrWhiteSpace(mediumTutorialContent)
            ? MediumHintTutorialExact
            : mediumTutorialContent;

        mediumTutorialBodyText.text = string.Empty;
        float delay = Mathf.Max(0.004f, mediumTutorialTypeDelay);
        float pulse = 0f;
        RectTransform titleRect = mediumTutorialTitleText != null ? mediumTutorialTitleText.rectTransform : null;
        Vector3 titleBaseScale = titleRect != null ? titleRect.localScale : Vector3.one;
        for (int i = 0; i < full.Length; i++)
        {
            if (!tutorialOpen)
            {
                yield break;
            }

            mediumTutorialBodyText.text += full[i];
            pulse += delay * 3.2f;
            if (titleRect != null)
            {
                float s = 1f + (Mathf.Sin(pulse) * 0.03f);
                titleRect.localScale = titleBaseScale * s;
            }
            if ((i % 3) == 0)
            {
                HoldFrozenCameraPose();
            }

            yield return new WaitForSecondsRealtime(delay);
        }

        if (titleRect != null)
        {
            titleRect.localScale = titleBaseScale;
        }

        tutorialTypeRoutine = null;
    }

    private void OnMediumVerifyPressed()
    {
        if (mediumCodeInputField == null || mediumVerifyResultText == null)
        {
            return;
        }

        bool correct = NormalizeCode(mediumCodeInputField.text) == NormalizeCode(mediumCssCorrectCode);
        mediumVerifyResultText.text = correct ? "Correct" : "Incorrect";
        mediumVerifyResultText.color = correct ? new Color(0.08f, 0.58f, 0.12f, 1f) : new Color(0.76f, 0.12f, 0.12f, 1f);

        if (mediumContinueButton != null)
        {
            mediumContinueButton.gameObject.SetActive(correct);
        }

        if (verifyPulseRoutine != null)
        {
            StopCoroutine(verifyPulseRoutine);
        }

        verifyPulseRoutine = StartCoroutine(AnimateVerifyResultPulse());
    }

    private IEnumerator AnimateVerifyResultPulse()
    {
        if (mediumVerifyResultText == null)
        {
            yield break;
        }

        RectTransform rt = mediumVerifyResultText.rectTransform;
        Vector3 baseScale = Vector3.one;
        if (rt != null)
        {
            baseScale = rt.localScale;
        }

        Color baseColor = mediumVerifyResultText.color;
        float half = Mathf.Max(0.04f, mediumResultPulseDuration) * 0.5f;
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);
            if (rt != null)
            {
                rt.localScale = Vector3.Lerp(baseScale, baseScale * 1.08f, k);
            }

            Color c = baseColor;
            c.a = Mathf.Lerp(0.45f, 1f, k);
            mediumVerifyResultText.color = c;
            yield return null;
        }

        float hold = Mathf.Max(0f, mediumResultHoldDuration);
        if (hold > 0f)
        {
            yield return new WaitForSecondsRealtime(hold);
        }

        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / half);
            if (rt != null)
            {
                rt.localScale = Vector3.Lerp(baseScale * 1.08f, baseScale, k);
            }

            mediumVerifyResultText.color = baseColor;
            yield return null;
        }

        if (rt != null)
        {
            rt.localScale = baseScale;
        }

        mediumVerifyResultText.color = baseColor;
        verifyPulseRoutine = null;
    }

    private void OnLeavePressed()
    {
        leavePressed = true;
        HideAllSequenceOverlays();
    }

    private void OnContinuePressed()
    {
        continuePressed = true;
    }

    private void HandleContinueToNextLevel(SphereController sphere, FirstPersonControllerSimple fps)
    {
        mediumSequenceCompleted = true;
        RestoreRendererStates();
        SetStartTextVisible(false);
        SetOverlayVisible(false);
        UnwatchButton();
        if (sphere != null)
        {
            if (questionSetPreset == QuestionSetPreset.Hard)
            {
                MoveSphereToSafePadSpot(sphere);
            }
            sphere.SetHardFreeze(false);
            sphere.SetMovementLocked(false);
        }
        if (fps != null)
        {
            fps.SetHardFreeze(false);
            fps.SetMovementLocked(false);
        }
        RestoreCameraIfNeeded();
        running = false;

        if (continueBuildsNextMediumPath)
        {
            BuildAndActivateNextMediumTestPath();
            return;
        }

        if (!string.IsNullOrWhiteSpace(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName.Trim());
            return;
        }

        Scene active = SceneManager.GetActiveScene();
        int nextIndex = active.buildIndex + 1;
        if (nextIndex >= 0 && nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextIndex);
            return;
        }

        // Fallback: no next scene configured. Keep gameplay running but do not allow re-entering this sequence.
        if (mediumCodeInputField != null && mediumCodeInputField.gameObject.activeInHierarchy)
        {
            mediumCodeInputField.DeactivateInputField();
        }
    }

    private void MoveSphereToSafePadSpot(SphereController sphere)
    {
        if (sphere == null)
        {
            return;
        }

        Vector3 safePos = transform.position + (Vector3.up * 1.05f) + (transform.forward * 0.35f);
        Collider padCollider = GetComponent<Collider>();
        if (padCollider != null)
        {
            Bounds b = padCollider.bounds;
            safePos = new Vector3(b.center.x, b.max.y + 0.7f, b.center.z) + (transform.forward * 0.35f);
        }

        Rigidbody rb = sphere.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = safePos;
        }

        sphere.transform.position = safePos;
    }

    private void BuildAndActivateNextMediumTestPath()
    {
        if (nextPathSpawned)
        {
            return;
        }

        int nextStage = mediumQuestionStage + 1;
        string stageRootName = nextPathRootName + "_Q" + nextStage;
        Transform existing = transform.parent != null ? transform.parent.Find(stageRootName) : null;
        if (existing != null)
        {
            nextPathSpawned = true;
            return;
        }

        GameObject root = new GameObject(stageRootName);
        if (transform.parent != null)
        {
            root.transform.SetParent(transform.parent, true);
        }

        Vector3 segScale = new Vector3(
            Mathf.Max(0.6f, nextPathSegmentScale.x),
            Mathf.Max(0.12f, nextPathSegmentScale.y),
            Mathf.Max(0.6f, nextPathSegmentScale.z));
        int segments = Mathf.Max(2, nextPathSegments);
        float segLen = Mathf.Max(0.5f, Mathf.Max(nextPathSegmentLength, segScale.z));
        int padSegIndex = nextPadSegmentIndex < 0 ? (segments - 1) : Mathf.Clamp(nextPadSegmentIndex, 0, segments - 1);
        // Start exactly after the first activated pad so the road is continuous (no gap).
        Vector3 basePos = transform.position + (transform.forward * segLen);

        Material roadMat = ResolveRoadMaterial();
        if (roadMat == null)
        {
            roadMat = CreateRuntimeMaterial(new Color(0.36f, 0.42f, 0.5f, 1f));
        }

        Color padColor = questionSetPreset == QuestionSetPreset.Hard
            ? new Color(0.9f, 0.28f, 0.28f, 1f)
            : new Color(1f, 0.8f, 0.18f, 1f);
        Material padMat = CreateRuntimeMaterial(padColor);

        Vector3 padAnchorPos = basePos;
        for (int i = 0; i < segments; i++)
        {
            GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            seg.name = "MediumNextRoadSegment_" + i;
            seg.transform.SetParent(root.transform, true);
            Vector3 segPos = basePos + (transform.forward * (i * segLen));
            seg.transform.position = segPos;
            seg.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            seg.transform.localScale = segScale;
            AssignMaterial(seg, roadMat);

            if (i == padSegIndex)
            {
                padAnchorPos = segPos;
            }
        }

        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "MediumQuestionPad_Test";
        pad.transform.SetParent(root.transform, true);
        float roadTop = padAnchorPos.y + (segScale.y * 0.5f);
        Vector3 padScale = new Vector3(
            Mathf.Max(1.2f, nextPadScale.x),
            Mathf.Max(0.08f, nextPadScale.y),
            Mathf.Max(1.2f, nextPadScale.z));
        pad.transform.position = new Vector3(padAnchorPos.x, roadTop + (padScale.y * 0.5f) - 0.02f, padAnchorPos.z);
        pad.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        pad.transform.localScale = padScale;
        AssignMaterial(pad, padMat);

        MediumQuestionPadStartSequence nextPadSequence = pad.AddComponent<MediumQuestionPadStartSequence>();
        string prompt;
        string initialCode;
        string correctCode;
        string hintText;
        bool allowNext;
        GetQuestionConfigForStage(nextStage, out prompt, out initialCode, out correctCode, out hintText, out allowNext);
        nextPadSequence.questionSetPreset = questionSetPreset;
        nextPadSequence.nextPathRootName = nextPathRootName;
        nextPadSequence.nextPathSegments = nextPathSegments;
        nextPadSequence.nextPathSegmentLength = nextPathSegmentLength;
        nextPadSequence.nextPathSegmentScale = nextPathSegmentScale;
        nextPadSequence.nextPadSegmentIndex = nextPadSegmentIndex;
        nextPadSequence.nextPadScale = nextPadScale;
        nextPadSequence.ConfigureAsQuestion(
            prompt,
            initialCode,
            correctCode,
            hintText,
            allowNext);
        nextPadSequence.mediumQuestionStage = nextStage;
        nextPadSequence.autoCreateOverlay = true;

        nextPathSpawned = true;
    }

    private void GetQuestionConfigForStage(int stage, out string prompt, out string initialCode, out string correctCode, out string hintText, out bool allowNext)
    {
        if (questionSetPreset == QuestionSetPreset.Hard)
        {
            if (stage <= 1)
            {
                prompt = HardFirstPromptExact;
                initialCode = HardFirstInitialCode;
                correctCode = HardFirstCorrectCode;
                hintText = HardFirstHintText;
                allowNext = true;
                return;
            }

            if (stage == 2)
            {
                prompt = HardSecondPromptExact;
                initialCode = HardSecondInitialCode;
                correctCode = HardSecondCorrectCode;
                hintText = HardSecondHintText;
                allowNext = true;
                return;
            }

            if (stage == 3)
            {
                prompt = HardThirdPromptExact;
                initialCode = HardThirdInitialCode;
                correctCode = HardThirdCorrectCode;
                hintText = HardThirdHintText;
                allowNext = true;
                return;
            }

            if (stage == 4)
            {
                prompt = HardFourthPromptExact;
                initialCode = HardFourthInitialCode;
                correctCode = HardFourthCorrectCode;
                hintText = HardFourthHintText;
                allowNext = true;
                return;
            }

            if (stage == 5)
            {
                prompt = HardFifthPromptExact;
                initialCode = HardFifthInitialCode;
                correctCode = HardFifthCorrectCode;
                hintText = HardFifthHintText;
                allowNext = false;
                return;
            }

            prompt = "Question:\nWrite valid C++ for the requested task.";
            initialCode = "int main() {\n    return 0;\n}";
            correctCode = "int main() {\n    return 0;\n}";
            hintText = "Hint: use the exact function signature from the prompt.";
            allowNext = false;
            return;
        }

        if (stage <= 1)
        {
            prompt = MediumLeftPromptExact;
            initialCode = MediumFirstInitialCode;
            correctCode = MediumFirstCorrectCode;
            hintText = MediumHintTutorialExact;
            allowNext = true;
            return;
        }

        if (stage == 2)
        {
            prompt = MediumSecondPromptExact;
            initialCode = MediumSecondInitialCode;
            correctCode = MediumSecondCorrectCode;
            hintText = MediumSecondHintText;
            allowNext = true;
            return;
        }

        if (stage == 3)
        {
            prompt = MediumThirdPromptExact;
            initialCode = MediumThirdInitialCode;
            correctCode = MediumThirdCorrectCode;
            hintText = MediumThirdHintText;
            allowNext = true;
            return;
        }

        if (stage == 4)
        {
            prompt = MediumFourthPromptExact;
            initialCode = MediumFourthInitialCode;
            correctCode = MediumFourthCorrectCode;
            hintText = MediumFourthHintText;
            allowNext = true;
            return;
        }

        if (stage == 5)
        {
            prompt = MediumFifthPromptExact;
            initialCode = MediumFifthInitialCode;
            correctCode = MediumFifthCorrectCode;
            hintText = MediumFifthHintText;
            allowNext = false;
            return;
        }

        prompt = "Question:\nFix the C++ code.";
        initialCode = "int main() {\n    return 0;\n}";
        correctCode = "int main() {\n    return 0;\n}";
        hintText = "Hint: read each line and function signature carefully.";
        allowNext = false;
    }

    public void ConfigureAsHardStart()
    {
        questionSetPreset = QuestionSetPreset.Hard;
        mediumQuestionStage = 1;
        nextPathRootName = "HardNextTestPath";

        string prompt;
        string initialCode;
        string correctCode;
        string hintText;
        bool allowNext;
        GetQuestionConfigForStage(mediumQuestionStage, out prompt, out initialCode, out correctCode, out hintText, out allowNext);

        ConfigureAsQuestion(prompt, initialCode, correctCode, hintText, allowNext);
    }

    private static Material CreateRuntimeMaterial(Color c)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            return null;
        }

        Material mat = new Material(shader);
        mat.color = c;
        return mat;
    }

    private static void AssignMaterial(GameObject go, Material mat)
    {
        if (go == null || mat == null)
        {
            return;
        }

        Renderer r = go.GetComponent<Renderer>();
        if (r != null)
        {
            r.sharedMaterial = mat;
        }
    }

    private Material ResolveRoadMaterial()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 4f);
        float best = float.MaxValue;
        Material bestMat = null;
        for (int i = 0; i < hits.Length; i++)
        {
            Collider c = hits[i];
            if (c == null || c.transform == transform)
            {
                continue;
            }

            Renderer r = c.GetComponent<Renderer>();
            if (r == null || r.sharedMaterial == null)
            {
                continue;
            }

            string n = c.gameObject.name;
            if (string.IsNullOrEmpty(n))
            {
                continue;
            }

            if (n.IndexOf("Road", System.StringComparison.OrdinalIgnoreCase) < 0 &&
                n.IndexOf("Path", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < best)
            {
                best = d;
                bestMat = r.sharedMaterial;
            }
        }

        return bestMat;
    }

    private static string NormalizeCode(string input)
    {
        string value = (input ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
        string[] lines = value.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimEnd();
        }

        return string.Join("\n", lines).Trim();
    }

    private IEnumerator AnimateMediumPromptText()
    {
        if (mediumPromptText == null)
        {
            yield break;
        }

        string fullText = string.IsNullOrWhiteSpace(mediumPromptTextContent)
            ? MediumLeftPromptExact
            : mediumPromptTextContent;

        SetMediumPromptVisible(true);
        mediumPromptText.text = string.Empty;

        float fadeDuration = Mathf.Max(0.01f, mediumPromptFadeDuration);
        float tFade = 0f;
        while (tFade < fadeDuration)
        {
            tFade += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(tFade / fadeDuration);
            Color c = mediumPromptText.color;
            c.a = k;
            mediumPromptText.color = c;
            HoldFrozenCameraPose();
            yield return null;
        }

        float delay = Mathf.Max(0.001f, mediumPromptTypeDelay);
        float timer = 0f;
        int index = 0;
        while (index < fullText.Length)
        {
            timer += Time.unscaledDeltaTime;
            while (timer >= delay && index < fullText.Length)
            {
                mediumPromptText.text += fullText[index];
                index++;
                timer -= delay;
            }

            HoldFrozenCameraPose();
            yield return null;
        }
    }

    private IEnumerator TypeTextInto(Text target, string text, float charDelay)
    {
        if (target == null)
        {
            yield break;
        }

        string full = text ?? string.Empty;
        float delay = Mathf.Max(0.001f, charDelay);
        target.text = string.Empty;
        for (int i = 0; i < full.Length; i++)
        {
            target.text += full[i];
            HoldFrozenCameraPose();
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    public void ConfigureAsFollowupTest(string message)
    {
        isFollowupTestPad = true;
        followupTestMessage = string.IsNullOrWhiteSpace(message) ? "test" : message;
        continueBuildsNextMediumPath = false;
    }

    public void ConfigureAsQuestion(string prompt, string initialCode, string correctCode, string hintText, bool allowPathContinuation)
    {
        isFollowupTestPad = false;
        if (!string.IsNullOrWhiteSpace(prompt))
        {
            mediumPromptTextContent = prompt;
        }

        if (!string.IsNullOrWhiteSpace(initialCode))
        {
            mediumCssInitialCode = initialCode;
        }

        if (!string.IsNullOrWhiteSpace(correctCode))
        {
            mediumCssCorrectCode = correctCode;
        }

        if (!string.IsNullOrWhiteSpace(hintText))
        {
            mediumTutorialContent = hintText;
        }

        continueBuildsNextMediumPath = allowPathContinuation;
    }

    private Text CreateLeftPromptText(Transform parent)
    {
        if (parent == null)
        {
            return null;
        }
        bool isHard = questionSetPreset == QuestionSetPreset.Hard;

        GameObject promptCardObj = new GameObject("MediumPromptLeftCard");
        promptCardObj.transform.SetParent(parent, false);
        RectTransform promptCardRect = promptCardObj.AddComponent<RectTransform>();
        promptCardRect.anchorMin = new Vector2(0.05f, 0.22f);
        promptCardRect.anchorMax = new Vector2(0.49f, 0.78f);
        promptCardRect.offsetMin = Vector2.zero;
        promptCardRect.offsetMax = Vector2.zero;
        Image promptCard = promptCardObj.AddComponent<Image>();
        promptCard.color = isHard
            ? new Color(0.06f, 0.1f, 0.16f, 0.9f)
            : new Color(1f, 1f, 1f, 0.82f);
        Outline promptOutline = promptCardObj.AddComponent<Outline>();
        promptOutline.effectColor = isHard
            ? new Color(0.2f, 0.78f, 1f, 0.42f)
            : new Color(0f, 0f, 0f, 0.14f);
        promptOutline.effectDistance = new Vector2(2f, -2f);
        Shadow promptShadow = promptCardObj.AddComponent<Shadow>();
        promptShadow.effectColor = isHard
            ? new Color(0.03f, 0.5f, 0.8f, 0.24f)
            : new Color(0f, 0f, 0f, 0.12f);
        promptShadow.effectDistance = new Vector2(0f, -6f);

        GameObject promptObj = new GameObject("MediumPromptLeft");
        promptObj.transform.SetParent(promptCardObj.transform, false);
        RectTransform promptRect = promptObj.AddComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0.08f, 0.12f);
        promptRect.anchorMax = new Vector2(0.92f, 0.88f);
        promptRect.offsetMin = Vector2.zero;
        promptRect.offsetMax = Vector2.zero;
        Text prompt = promptObj.AddComponent<Text>();
        prompt.text = string.Empty;
        prompt.alignment = TextAnchor.MiddleCenter;
        prompt.horizontalOverflow = HorizontalWrapMode.Wrap;
        prompt.verticalOverflow = VerticalWrapMode.Overflow;
        prompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        prompt.fontSize = 40;
        prompt.fontStyle = isHard ? FontStyle.Normal : FontStyle.Bold;
        prompt.color = isHard
            ? new Color(0.82f, 0.94f, 1f, 0f)
            : new Color(0f, 0f, 0f, 0f);
        prompt.lineSpacing = 1.1f;
        prompt.raycastTarget = false;
        return prompt;
    }

    private void CreateRightCodeExerciseUI(Transform parent)
    {
        if (parent == null)
        {
            return;
        }
        bool isHard = questionSetPreset == QuestionSetPreset.Hard;

        GameObject panelObj = new GameObject("MediumCodePanel");
        panelObj.transform.SetParent(parent, false);
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.54f, 0.18f);
        panelRect.anchorMax = new Vector2(0.95f, 0.82f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        mediumCodePanel = panelObj.AddComponent<Image>();
        mediumCodePanel.color = isHard
            ? new Color(0.03f, 0.07f, 0.12f, 0.96f)
            : new Color(0.94f, 0.965f, 1f, 0.98f);

        Outline outline = panelObj.AddComponent<Outline>();
        outline.effectColor = isHard
            ? new Color(0.14f, 0.82f, 1f, 0.35f)
            : new Color(0f, 0f, 0f, 0.22f);
        outline.effectDistance = new Vector2(3f, -3f);

        Shadow panelShadow = panelObj.AddComponent<Shadow>();
        panelShadow.effectColor = isHard
            ? new Color(0.03f, 0.22f, 0.36f, 0.28f)
            : new Color(0f, 0f, 0f, 0.16f);
        panelShadow.effectDistance = new Vector2(0f, -10f);

        GameObject panelAccentObj = new GameObject("PanelAccent");
        panelAccentObj.transform.SetParent(panelObj.transform, false);
        RectTransform panelAccentRect = panelAccentObj.AddComponent<RectTransform>();
        panelAccentRect.anchorMin = new Vector2(0f, 0.97f);
        panelAccentRect.anchorMax = new Vector2(1f, 1f);
        panelAccentRect.offsetMin = Vector2.zero;
        panelAccentRect.offsetMax = Vector2.zero;
        Image panelAccent = panelAccentObj.AddComponent<Image>();
        panelAccent.color = isHard
            ? new Color(0.02f, 0.88f, 1f, 0.96f)
            : new Color(0.28f, 0.48f, 0.86f, 0.9f);
        panelAccent.raycastTarget = false;

        GameObject titleObj = new GameObject("MediumCodeTitle");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.06f, 0.86f);
        titleRect.anchorMax = new Vector2(0.94f, 0.97f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        Text title = titleObj.AddComponent<Text>();
        title.text = isHard ? "HARD // C++ CONSOLE" : "C++ Code";
        title.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        title.fontSize = 28;
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleLeft;
        title.color = isHard
            ? new Color(0.82f, 0.95f, 1f, 1f)
            : new Color(0.08f, 0.08f, 0.08f, 1f);
        title.raycastTarget = false;

        GameObject editorBgObj = new GameObject("MediumCodeEditorBg");
        editorBgObj.transform.SetParent(panelObj.transform, false);
        RectTransform editorBgRect = editorBgObj.AddComponent<RectTransform>();
        editorBgRect.anchorMin = new Vector2(0.06f, 0.23f);
        editorBgRect.anchorMax = new Vector2(0.94f, 0.84f);
        editorBgRect.offsetMin = Vector2.zero;
        editorBgRect.offsetMax = Vector2.zero;
        Image editorBg = editorBgObj.AddComponent<Image>();
        editorBg.color = isHard
            ? new Color(0.08f, 0.12f, 0.18f, 0.98f)
            : new Color(1f, 1f, 1f, 1f);
        Outline editorOutline = editorBgObj.AddComponent<Outline>();
        editorOutline.effectColor = isHard
            ? new Color(0.2f, 0.9f, 1f, 0.2f)
            : new Color(0f, 0f, 0f, 0.09f);
        editorOutline.effectDistance = new Vector2(1f, -1f);

        GameObject inputObj = new GameObject("MediumCodeInput");
        inputObj.transform.SetParent(editorBgObj.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchorMin = Vector2.zero;
        inputRect.anchorMax = Vector2.one;
        inputRect.offsetMin = new Vector2(14f, 14f);
        inputRect.offsetMax = new Vector2(-14f, -14f);
        mediumCodeInputField = inputObj.AddComponent<InputField>();
        mediumCodeInputField.lineType = InputField.LineType.MultiLineNewline;
        mediumCodeInputField.textComponent = CreateInputText(inputObj.transform);
        mediumCodeInputField.placeholder = CreatePlaceholderText(inputObj.transform);
        mediumCodeInputField.text = mediumCssInitialCode;
        if (isHard && mediumCodeInputField.textComponent != null)
        {
            mediumCodeInputField.textComponent.color = new Color(0.82f, 0.96f, 1f, 1f);
        }
        Text hardPlaceholder = mediumCodeInputField.placeholder as Text;
        if (isHard && hardPlaceholder != null)
        {
            hardPlaceholder.color = new Color(0.45f, 0.8f, 0.9f, 0.45f);
        }

        GameObject btnObj = new GameObject("MediumVerifyButton");
        btnObj.transform.SetParent(panelObj.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.28f, 0.09f);
        btnRect.anchorMax = new Vector2(0.72f, 0.18f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        Image btnImage = btnObj.AddComponent<Image>();
        btnImage.color = isHard
            ? new Color(0.0f, 0.55f, 0.78f, 0.94f)
            : new Color(0.12f, 0.2f, 0.42f, 0.95f);
        mediumVerifyButton = btnObj.AddComponent<Button>();
        mediumVerifyButton.targetGraphic = btnImage;
        mediumVerifyButton.onClick.RemoveListener(OnMediumVerifyPressed);
        mediumVerifyButton.onClick.AddListener(OnMediumVerifyPressed);
        ColorBlock verifyColors = mediumVerifyButton.colors;
        verifyColors.normalColor = isHard ? new Color(0.0f, 0.55f, 0.78f, 0.94f) : new Color(0.12f, 0.2f, 0.42f, 0.95f);
        verifyColors.highlightedColor = isHard ? new Color(0.06f, 0.68f, 0.92f, 1f) : new Color(0.18f, 0.28f, 0.54f, 1f);
        verifyColors.pressedColor = isHard ? new Color(0.0f, 0.42f, 0.62f, 1f) : new Color(0.08f, 0.15f, 0.32f, 1f);
        verifyColors.selectedColor = verifyColors.highlightedColor;
        verifyColors.fadeDuration = 0.08f;
        mediumVerifyButton.colors = verifyColors;

        GameObject btnLabelObj = new GameObject("MediumVerifyButtonLabel");
        btnLabelObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnLabelRect = btnLabelObj.AddComponent<RectTransform>();
        btnLabelRect.anchorMin = Vector2.zero;
        btnLabelRect.anchorMax = Vector2.one;
        btnLabelRect.offsetMin = Vector2.zero;
        btnLabelRect.offsetMax = Vector2.zero;
        Text btnLabel = btnLabelObj.AddComponent<Text>();
        btnLabel.text = "Verify";
        btnLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnLabel.fontSize = 28;
        btnLabel.fontStyle = FontStyle.Bold;
        btnLabel.alignment = TextAnchor.MiddleCenter;
        btnLabel.color = Color.white;
        btnLabel.raycastTarget = false;

        GameObject continueObj = new GameObject("MediumContinueButton");
        continueObj.transform.SetParent(panelObj.transform, false);
        RectTransform continueRect = continueObj.AddComponent<RectTransform>();
        continueRect.anchorMin = new Vector2(0.06f, 0.09f);
        continueRect.anchorMax = new Vector2(0.27f, 0.18f);
        continueRect.offsetMin = Vector2.zero;
        continueRect.offsetMax = Vector2.zero;
        Image continueImage = continueObj.AddComponent<Image>();
        continueImage.color = isHard
            ? new Color(0.02f, 0.62f, 0.36f, 0.96f)
            : new Color(0.1f, 0.46f, 0.18f, 0.96f);
        mediumContinueButton = continueObj.AddComponent<Button>();
        mediumContinueButton.targetGraphic = continueImage;
        mediumContinueButton.onClick.RemoveListener(OnContinuePressed);
        mediumContinueButton.onClick.AddListener(OnContinuePressed);
        ColorBlock continueColors = mediumContinueButton.colors;
        continueColors.normalColor = isHard ? new Color(0.02f, 0.62f, 0.36f, 0.96f) : new Color(0.1f, 0.46f, 0.18f, 0.96f);
        continueColors.highlightedColor = isHard ? new Color(0.07f, 0.76f, 0.45f, 1f) : new Color(0.16f, 0.56f, 0.24f, 1f);
        continueColors.pressedColor = isHard ? new Color(0.02f, 0.46f, 0.28f, 1f) : new Color(0.07f, 0.34f, 0.13f, 1f);
        continueColors.selectedColor = continueColors.highlightedColor;
        continueColors.fadeDuration = 0.08f;
        mediumContinueButton.colors = continueColors;

        GameObject continueLabelObj = new GameObject("MediumContinueButtonLabel");
        continueLabelObj.transform.SetParent(continueObj.transform, false);
        RectTransform continueLabelRect = continueLabelObj.AddComponent<RectTransform>();
        continueLabelRect.anchorMin = Vector2.zero;
        continueLabelRect.anchorMax = Vector2.one;
        continueLabelRect.offsetMin = Vector2.zero;
        continueLabelRect.offsetMax = Vector2.zero;
        Text continueLabel = continueLabelObj.AddComponent<Text>();
        continueLabel.text = "Continue";
        continueLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        continueLabel.fontSize = 22;
        continueLabel.fontStyle = FontStyle.Bold;
        continueLabel.alignment = TextAnchor.MiddleCenter;
        continueLabel.color = Color.white;
        continueLabel.raycastTarget = false;
        mediumContinueButton.gameObject.SetActive(false);

        GameObject leaveObj = new GameObject("MediumLeaveButton");
        leaveObj.transform.SetParent(parent, false);
        RectTransform leaveRect = leaveObj.AddComponent<RectTransform>();
        leaveRect.anchorMin = new Vector2(0.03f, 0.03f);
        leaveRect.anchorMax = new Vector2(0.18f, 0.1f);
        leaveRect.offsetMin = Vector2.zero;
        leaveRect.offsetMax = Vector2.zero;
        Image leaveImage = leaveObj.AddComponent<Image>();
        leaveImage.color = isHard
            ? new Color(0.12f, 0.16f, 0.22f, 0.94f)
            : new Color(0.16f, 0.16f, 0.2f, 0.92f);
        mediumLeaveButton = leaveObj.AddComponent<Button>();
        mediumLeaveButton.targetGraphic = leaveImage;
        mediumLeaveButton.onClick.RemoveListener(OnLeavePressed);
        mediumLeaveButton.onClick.AddListener(OnLeavePressed);
        ColorBlock leaveColors = mediumLeaveButton.colors;
        leaveColors.normalColor = isHard ? new Color(0.12f, 0.16f, 0.22f, 0.94f) : new Color(0.16f, 0.16f, 0.2f, 0.92f);
        leaveColors.highlightedColor = isHard ? new Color(0.18f, 0.23f, 0.32f, 1f) : new Color(0.22f, 0.22f, 0.28f, 1f);
        leaveColors.pressedColor = isHard ? new Color(0.09f, 0.12f, 0.18f, 1f) : new Color(0.12f, 0.12f, 0.16f, 1f);
        leaveColors.selectedColor = leaveColors.highlightedColor;
        leaveColors.fadeDuration = 0.08f;
        mediumLeaveButton.colors = leaveColors;

        GameObject leaveLabelObj = new GameObject("MediumLeaveButtonLabel");
        leaveLabelObj.transform.SetParent(leaveObj.transform, false);
        RectTransform leaveLabelRect = leaveLabelObj.AddComponent<RectTransform>();
        leaveLabelRect.anchorMin = Vector2.zero;
        leaveLabelRect.anchorMax = Vector2.one;
        leaveLabelRect.offsetMin = Vector2.zero;
        leaveLabelRect.offsetMax = Vector2.zero;
        Text leaveLabel = leaveLabelObj.AddComponent<Text>();
        leaveLabel.text = "Leave";
        leaveLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        leaveLabel.fontSize = 24;
        leaveLabel.fontStyle = FontStyle.Bold;
        leaveLabel.alignment = TextAnchor.MiddleCenter;
        leaveLabel.color = Color.white;
        leaveLabel.raycastTarget = false;

        GameObject hintObj = new GameObject("MediumHintButton");
        hintObj.transform.SetParent(parent, false);
        RectTransform hintRect = hintObj.AddComponent<RectTransform>();
        hintRect.anchorMin = new Vector2(0.20f, 0.03f);
        hintRect.anchorMax = new Vector2(0.35f, 0.1f);
        hintRect.offsetMin = Vector2.zero;
        hintRect.offsetMax = Vector2.zero;
        Image hintImage = hintObj.AddComponent<Image>();
        hintImage.color = isHard
            ? new Color(0.36f, 0.2f, 0.05f, 0.96f)
            : new Color(0.48f, 0.35f, 0.08f, 0.94f);
        mediumHintButton = hintObj.AddComponent<Button>();
        mediumHintButton.targetGraphic = hintImage;
        mediumHintButton.onClick.RemoveListener(OnHintPressed);
        mediumHintButton.onClick.AddListener(OnHintPressed);
        ColorBlock hintColors = mediumHintButton.colors;
        hintColors.normalColor = isHard ? new Color(0.36f, 0.2f, 0.05f, 0.96f) : new Color(0.48f, 0.35f, 0.08f, 0.94f);
        hintColors.highlightedColor = isHard ? new Color(0.5f, 0.28f, 0.08f, 1f) : new Color(0.6f, 0.45f, 0.12f, 1f);
        hintColors.pressedColor = isHard ? new Color(0.28f, 0.14f, 0.04f, 1f) : new Color(0.38f, 0.28f, 0.07f, 1f);
        hintColors.selectedColor = hintColors.highlightedColor;
        hintColors.fadeDuration = 0.08f;
        mediumHintButton.colors = hintColors;

        GameObject hintLabelObj = new GameObject("MediumHintButtonLabel");
        hintLabelObj.transform.SetParent(hintObj.transform, false);
        RectTransform hintLabelRect = hintLabelObj.AddComponent<RectTransform>();
        hintLabelRect.anchorMin = Vector2.zero;
        hintLabelRect.anchorMax = Vector2.one;
        hintLabelRect.offsetMin = Vector2.zero;
        hintLabelRect.offsetMax = Vector2.zero;
        Text hintLabel = hintLabelObj.AddComponent<Text>();
        hintLabel.text = "Hint";
        hintLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintLabel.fontSize = 24;
        hintLabel.fontStyle = FontStyle.Bold;
        hintLabel.alignment = TextAnchor.MiddleCenter;
        hintLabel.color = Color.white;
        hintLabel.raycastTarget = false;

        GameObject tutorialObj = new GameObject("MediumHintTutorialPanel");
        tutorialObj.transform.SetParent(parent, false);
        RectTransform tutorialRect = tutorialObj.AddComponent<RectTransform>();
        tutorialRect.anchorMin = new Vector2(0.08f, 0.12f);
        tutorialRect.anchorMax = new Vector2(0.92f, 0.9f);
        tutorialRect.offsetMin = Vector2.zero;
        tutorialRect.offsetMax = Vector2.zero;
        mediumTutorialPanel = tutorialObj.AddComponent<Image>();
        mediumTutorialPanel.color = isHard
            ? new Color(0.04f, 0.09f, 0.15f, 0.985f)
            : new Color(0.95f, 0.975f, 1f, 0.985f);

        Outline tutOutline = tutorialObj.AddComponent<Outline>();
        tutOutline.effectColor = isHard
            ? new Color(0.16f, 0.78f, 0.96f, 0.24f)
            : new Color(0f, 0f, 0f, 0.18f);
        tutOutline.effectDistance = new Vector2(3f, -3f);

        GameObject tutTitleObj = new GameObject("MediumHintTutorialTitle");
        tutTitleObj.transform.SetParent(tutorialObj.transform, false);
        RectTransform tutTitleRect = tutTitleObj.AddComponent<RectTransform>();
        tutTitleRect.anchorMin = new Vector2(0.05f, 0.86f);
        tutTitleRect.anchorMax = new Vector2(0.95f, 0.97f);
        tutTitleRect.offsetMin = Vector2.zero;
        tutTitleRect.offsetMax = Vector2.zero;
        mediumTutorialTitleText = tutTitleObj.AddComponent<Text>();
        mediumTutorialTitleText.text = "Quick Hint";
        mediumTutorialTitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mediumTutorialTitleText.fontSize = 42;
        mediumTutorialTitleText.fontStyle = FontStyle.Bold;
        mediumTutorialTitleText.alignment = TextAnchor.MiddleCenter;
        mediumTutorialTitleText.color = isHard
            ? new Color(0.78f, 0.95f, 1f, 1f)
            : new Color(0.08f, 0.1f, 0.16f, 1f);
        mediumTutorialTitleText.raycastTarget = false;

        GameObject tutBodyObj = new GameObject("MediumHintTutorialBody");
        tutBodyObj.transform.SetParent(tutorialObj.transform, false);
        RectTransform tutBodyRect = tutBodyObj.AddComponent<RectTransform>();
        tutBodyRect.anchorMin = new Vector2(0.07f, 0.18f);
        tutBodyRect.anchorMax = new Vector2(0.93f, 0.83f);
        tutBodyRect.offsetMin = Vector2.zero;
        tutBodyRect.offsetMax = Vector2.zero;
        mediumTutorialBodyText = tutBodyObj.AddComponent<Text>();
        mediumTutorialBodyText.text = string.Empty;
        mediumTutorialBodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mediumTutorialBodyText.fontSize = 28;
        mediumTutorialBodyText.fontStyle = FontStyle.Normal;
        mediumTutorialBodyText.alignment = TextAnchor.UpperLeft;
        mediumTutorialBodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        mediumTutorialBodyText.verticalOverflow = VerticalWrapMode.Overflow;
        mediumTutorialBodyText.lineSpacing = 1.18f;
        mediumTutorialBodyText.color = isHard
            ? new Color(0.78f, 0.9f, 1f, 1f)
            : new Color(0.1f, 0.1f, 0.1f, 1f);
        mediumTutorialBodyText.raycastTarget = false;

        GameObject backObj = new GameObject("MediumBackToQuestionButton");
        backObj.transform.SetParent(tutorialObj.transform, false);
        RectTransform backRect = backObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.34f, 0.05f);
        backRect.anchorMax = new Vector2(0.66f, 0.14f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        Image backImage = backObj.AddComponent<Image>();
        backImage.color = isHard
            ? new Color(0.0f, 0.45f, 0.7f, 0.96f)
            : new Color(0.09f, 0.27f, 0.48f, 0.96f);
        mediumBackToQuestionButton = backObj.AddComponent<Button>();
        mediumBackToQuestionButton.targetGraphic = backImage;
        mediumBackToQuestionButton.onClick.RemoveListener(OnBackToQuestionPressed);
        mediumBackToQuestionButton.onClick.AddListener(OnBackToQuestionPressed);
        ColorBlock backColors = mediumBackToQuestionButton.colors;
        backColors.normalColor = isHard ? new Color(0.0f, 0.45f, 0.7f, 0.96f) : new Color(0.09f, 0.27f, 0.48f, 0.96f);
        backColors.highlightedColor = isHard ? new Color(0.06f, 0.62f, 0.88f, 1f) : new Color(0.13f, 0.34f, 0.58f, 1f);
        backColors.pressedColor = isHard ? new Color(0.0f, 0.3f, 0.52f, 1f) : new Color(0.06f, 0.2f, 0.38f, 1f);
        backColors.selectedColor = backColors.highlightedColor;
        backColors.fadeDuration = 0.08f;
        mediumBackToQuestionButton.colors = backColors;

        GameObject backLabelObj = new GameObject("MediumBackToQuestionLabel");
        backLabelObj.transform.SetParent(backObj.transform, false);
        RectTransform backLabelRect = backLabelObj.AddComponent<RectTransform>();
        backLabelRect.anchorMin = Vector2.zero;
        backLabelRect.anchorMax = Vector2.one;
        backLabelRect.offsetMin = Vector2.zero;
        backLabelRect.offsetMax = Vector2.zero;
        Text backLabel = backLabelObj.AddComponent<Text>();
        backLabel.text = "Back to Question";
        backLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        backLabel.fontSize = 24;
        backLabel.fontStyle = FontStyle.Bold;
        backLabel.alignment = TextAnchor.MiddleCenter;
        backLabel.color = Color.white;
        backLabel.raycastTarget = false;
        tutorialObj.SetActive(false);

        GameObject resultObj = new GameObject("MediumVerifyResultText");
        resultObj.transform.SetParent(panelObj.transform, false);
        RectTransform resultRect = resultObj.AddComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.08f, 0.01f);
        resultRect.anchorMax = new Vector2(0.92f, 0.08f);
        resultRect.offsetMin = Vector2.zero;
        resultRect.offsetMax = Vector2.zero;
        mediumVerifyResultText = resultObj.AddComponent<Text>();
        mediumVerifyResultText.text = string.Empty;
        mediumVerifyResultText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        mediumVerifyResultText.fontSize = 26;
        mediumVerifyResultText.fontStyle = FontStyle.Bold;
        mediumVerifyResultText.alignment = TextAnchor.MiddleCenter;
        mediumVerifyResultText.color = isHard
            ? new Color(0.76f, 0.93f, 1f, 1f)
            : new Color(0.12f, 0.12f, 0.12f, 1f);
        mediumVerifyResultText.raycastTarget = false;
    }

    private static Text CreateInputText(Transform parent)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 26;
        text.fontStyle = FontStyle.Normal;
        text.supportRichText = false;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        return text;
    }

    private static Graphic CreatePlaceholderText(Transform parent)
    {
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(parent, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        Text placeholder = placeholderObj.AddComponent<Text>();
        placeholder.text = "// Write your C++ fix here";
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 25;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.alignment = TextAnchor.UpperLeft;
        placeholder.color = new Color(0f, 0f, 0f, 0.32f);
        placeholder.raycastTarget = false;
        return placeholder;
    }

    private static void EnsureEventSystemExists()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
    }

    private void FreezeCameraIfNeeded()
    {
        if (!freezeCameraDuringSequence)
        {
            return;
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        frozenCameraTransform = cam.transform;
        frozenCameraPosition = frozenCameraTransform.position;
        frozenCameraRotation = frozenCameraTransform.rotation;

        frozenCameraFollow = cam.GetComponent<TopDownCameraFollow>();
        if (frozenCameraFollow != null)
        {
            frozenCameraFollow.enabled = false;
        }
    }

    private void HoldFrozenCameraPose()
    {
        if (!freezeCameraDuringSequence || frozenCameraTransform == null)
        {
            return;
        }

        frozenCameraTransform.position = frozenCameraPosition;
        frozenCameraTransform.rotation = frozenCameraRotation;
    }

    private void RestoreCameraIfNeeded()
    {
        HoldFrozenCameraPose();
        if (frozenCameraFollow != null)
        {
            frozenCameraFollow.enabled = true;
        }

        frozenCameraFollow = null;
        frozenCameraTransform = null;
        hardCameraVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        RestoreCameraIfNeeded();
    }
}

public static class MediumQuestionPadStartSequenceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachIfNeeded()
    {
        AttachToPad("MediumQuestionPad");
        AttachToPad("HardQuestionPad");
    }

    private static void AttachToPad(string padName)
    {
        GameObject pad = GameObject.Find(padName);
        if (pad == null)
        {
            return;
        }

        if (padName == "MediumQuestionPad" || padName == "HardQuestionPad")
        {
            return;
        }

        if (pad.GetComponent<MediumQuestionPadStartSequence>() == null)
        {
            pad.AddComponent<MediumQuestionPadStartSequence>();
        }
    }
}
