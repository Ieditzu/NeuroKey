using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class QuestionHardPadWhiteScreen : MonoBehaviour
{
    [SerializeField] private float burstDuration = 0.35f;
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float textDelay = 0.12f;

    private static Canvas overlayCanvas;
    private static Image burstA;
    private static Image burstB;
    private static Image burstC;
    private static Image whiteImage;
    private static Text centerText;

    private Collider triggerCollider;
    private bool running;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            triggerCollider.isTrigger = true;
        }

        DisableLegacyLogic();
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

        if (triggerCollider != null)
        {
            triggerCollider.enabled = false;
        }

        DisableLegacyLogic();
        EnsureOverlay();
        ResetOverlay();
        overlayCanvas.gameObject.SetActive(true);
        SetPlayerLockState(sphere, fps, true, true);

        if (fps != null)
        {
            fps.SetCameraControlEnabled(false);
        }

        SetCursorVisible(false);
        yield return PlayCenterBurst();
        yield return FadeWhite(0f, 1f, fadeDuration);

        if (textDelay > 0f)
        {
            yield return new WaitForSeconds(textDelay);
        }

        centerText.text = "testet";
        centerText.gameObject.SetActive(true);
    }

    private IEnumerator PlayCenterBurst()
    {
        burstA.gameObject.SetActive(true);
        burstB.gameObject.SetActive(true);
        burstC.gameObject.SetActive(true);

        RectTransform rectA = burstA.rectTransform;
        RectTransform rectB = burstB.rectTransform;
        RectTransform rectC = burstC.rectTransform;

        float time = 0f;
        while (time < burstDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / Mathf.Max(0.01f, burstDuration));
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            float alpha = 1f - t;

            SetBurst(rectA, burstA, Mathf.Lerp(20f, 500f, eased), Mathf.Lerp(20f, 180f, eased), 0f, alpha);
            SetBurst(rectB, burstB, Mathf.Lerp(20f, 420f, eased), Mathf.Lerp(20f, 140f, eased), 55f, alpha * 0.9f);
            SetBurst(rectC, burstC, Mathf.Lerp(20f, 420f, eased), Mathf.Lerp(20f, 140f, eased), -55f, alpha * 0.9f);
            yield return null;
        }

        burstA.gameObject.SetActive(false);
        burstB.gameObject.SetActive(false);
        burstC.gameObject.SetActive(false);
    }

    private static void SetBurst(RectTransform rect, Image image, float width, float height, float rotationZ, float alpha)
    {
        rect.sizeDelta = new Vector2(width, height);
        rect.localEulerAngles = new Vector3(0f, 0f, rotationZ);
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private IEnumerator FadeWhite(float from, float to, float duration)
    {
        Color color = whiteImage.color;
        color.a = from;
        whiteImage.color = color;
        whiteImage.gameObject.SetActive(true);

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / Mathf.Max(0.01f, duration));
            float smooth = t * t * (3f - (2f * t));
            color.a = Mathf.Lerp(from, to, smooth);
            whiteImage.color = color;
            yield return null;
        }

        color.a = to;
        whiteImage.color = color;
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

        Question2PadWhiteFadeSequence question2 = GetComponent<Question2PadWhiteFadeSequence>();
        if (question2 != null)
        {
            question2.enabled = false;
        }
    }

    private static bool TryGetPlayer(Collider other, out SphereController sphere, out FirstPersonControllerSimple fps)
    {
        sphere = other.GetComponent<SphereController>() ?? other.GetComponentInParent<SphereController>();
        fps = other.GetComponent<FirstPersonControllerSimple>() ?? other.GetComponentInParent<FirstPersonControllerSimple>();
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
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = visible;

        if (EventSystem.current != null && !visible)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    private static void EnsureOverlay()
    {
        if (overlayCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("QuestionHardPadOverlay");
        overlayCanvas = canvasObject.AddComponent<Canvas>();
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 5000;
        canvasObject.AddComponent<GraphicRaycaster>();

        whiteImage = CreateImage("WhiteFade", canvasObject.transform, Color.white);
        StretchFullScreen(whiteImage.rectTransform);

        burstA = CreateImage("BurstA", canvasObject.transform, Color.white);
        burstB = CreateImage("BurstB", canvasObject.transform, Color.white);
        burstC = CreateImage("BurstC", canvasObject.transform, Color.white);
        ConfigureCenteredBurst(burstA.rectTransform);
        ConfigureCenteredBurst(burstB.rectTransform);
        ConfigureCenteredBurst(burstC.rectTransform);

        centerText = CreateText("CenterText", canvasObject.transform, "testet", 62, TextAnchor.MiddleCenter);
        StretchFullScreen(centerText.rectTransform);
    }

    private static void ResetOverlay()
    {
        EnsureOverlay();
        overlayCanvas.gameObject.SetActive(false);

        whiteImage.gameObject.SetActive(false);
        burstA.gameObject.SetActive(false);
        burstB.gameObject.SetActive(false);
        burstC.gameObject.SetActive(false);
        centerText.gameObject.SetActive(false);

        Color white = Color.white;
        white.a = 0f;
        whiteImage.color = white;

        Color burstColor = Color.white;
        burstColor.a = 0f;
        burstA.color = burstColor;
        burstB.color = burstColor;
        burstC.color = burstColor;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text CreateText(string name, Transform parent, string textValue, int fontSize, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text text = textObject.AddComponent<Text>();
        text.text = textValue;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        text.raycastTarget = false;
        return text;
    }

    private static void StretchFullScreen(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void ConfigureCenteredBurst(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(20f, 20f);
    }
}

public static class QuestionHardPadWhiteScreenBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachToQuestionHardPad()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>(true);
        for (int i = 0; i < allObjects.Length; i++)
        {
            if (!string.Equals(allObjects[i].name, "questionhard", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (allObjects[i].GetComponent<QuestionHardPadWhiteScreen>() == null)
            {
                allObjects[i].AddComponent<QuestionHardPadWhiteScreen>();
            }
        }
    }
}
