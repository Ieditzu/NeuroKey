using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SphereRiftPortalSequence : MonoBehaviour
{
    [SerializeField] private float portalHeightOffset = 0.48f;
    [SerializeField] private float triggerRadius = 0.75f;
    [SerializeField] private float pullRadius = 1.05f;
    [SerializeField] private float pullDuration = 0.38f;
    [SerializeField] private float spinSpeed = 82f;
    [SerializeField] private float pulseSpeed = 3.6f;
    [SerializeField] private float fadeToWhiteDuration = 0.55f;
    [SerializeField] private float textFadeDelay = 0.08f;
    [SerializeField] private Color riftCoreColor = new Color(0.94f, 0.98f, 1f, 0.98f);
    [SerializeField] private Color riftRingColor = new Color(0.58f, 0.9f, 1f, 0.96f);
    [SerializeField] private Color riftGlowColor = new Color(0.96f, 0.98f, 1f, 1f);

    private static Canvas overlayCanvas;
    private static Image whiteImage;
    private static Text centerText;

    private SphereCollider portalTrigger;
    private Transform portalRoot;
    private Transform arrowRoot;
    private Transform arrowBody;
    private Transform arrowHead;
    private Transform outerRing;
    private Transform innerRing;
    private Transform core;
    private Light portalLight;
    private bool running;

    private void Awake()
    {
        EnsurePortalVisual();
        EnsurePortalTrigger();
        EnsureOverlay();
        ResetOverlay();
    }

    private void Update()
    {
        AnimatePortalVisual();

        if (running)
        {
            return;
        }

        if (!TryGetPlayer(out BeanController sphere, out FirstPersonControllerSimple fps, out Transform playerRoot))
        {
            return;
        }

        Vector3 portalPosition = GetPortalCenter();
        float distance = Vector3.Distance(playerRoot.position, portalPosition);
        if (distance <= pullRadius)
        {
            StartCoroutine(PlaySequence(sphere, fps, playerRoot, portalPosition));
        }
    }

    private IEnumerator PlaySequence(BeanController sphere, FirstPersonControllerSimple fps, Transform playerRoot, Vector3 portalPosition)
    {
        running = true;
        if (portalTrigger != null)
        {
            portalTrigger.enabled = false;
        }

        EnsureOverlay();
        ResetOverlay();
        if (overlayCanvas != null)
        {
            overlayCanvas.gameObject.SetActive(true);
        }

        SetPlayerLockState(sphere, fps, true, true);
        if (fps != null)
        {
            fps.SetCameraControlEnabled(false);
        }

        Rigidbody rb = sphere != null ? sphere.GetComponent<Rigidbody>() : null;
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 start = playerRoot.position;
        Vector3 end = portalPosition;
        Quaternion startRotation = playerRoot.rotation;
        Quaternion targetRotation = Quaternion.LookRotation((portalPosition - start).normalized, Vector3.up);

        float elapsed = 0f;
        while (elapsed < pullDuration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, pullDuration));
            float swirl = Mathf.Sin(t * Mathf.PI) * 0.12f;
            Vector3 arc = transform.right * swirl;
            Vector3 position = Vector3.Lerp(start, end, t) + arc;
            SetPlayerPositionAndRotation(sphere, fps, position, Quaternion.Slerp(startRotation, targetRotation, t));
            yield return null;
        }

        SetPlayerPositionAndRotation(sphere, fps, end, targetRotation);
        SetPlayerVisible(sphere, false);
        yield return FadeWhite(0f, 1f, fadeToWhiteDuration);
        if (textFadeDelay > 0f)
        {
            yield return new WaitForSeconds(textFadeDelay);
        }

        if (centerText != null)
        {
            centerText.text = "test";
            centerText.gameObject.SetActive(true);
        }
    }

    private void EnsurePortalVisual()
    {
        if (portalRoot != null)
        {
            return;
        }

        Transform existing = transform.Find("RiftPortalVisual");
        if (existing != null)
        {
            portalRoot = existing;
            arrowRoot = existing.Find("GuideArrow");
            if (arrowRoot != null)
            {
                arrowBody = arrowRoot.Find("Body");
                arrowHead = arrowRoot.Find("Head");
            }
            outerRing = existing.Find("OuterRing");
            innerRing = existing.Find("InnerRing");
            core = existing.Find("Core");
            portalLight = existing.GetComponentInChildren<Light>(true);
            return;
        }

        portalRoot = new GameObject("RiftPortalVisual").transform;
        portalRoot.SetParent(transform, false);
        portalRoot.localPosition = new Vector3(0f, portalHeightOffset, 0f);
        portalRoot.localRotation = Quaternion.identity;

        core = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        core.name = "Core";
        core.SetParent(portalRoot, false);
        core.localScale = new Vector3(0.28f, 0.46f, 0.1f);
        Destroy(core.GetComponent<Collider>());
        ApplyEmissionMaterial(core.gameObject, riftCoreColor, 6.2f);

        outerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        outerRing.name = "OuterRing";
        outerRing.SetParent(portalRoot, false);
        outerRing.localRotation = Quaternion.Euler(90f, 0f, 0f);
        outerRing.localScale = new Vector3(0.36f, 0.022f, 0.36f);
        Destroy(outerRing.GetComponent<Collider>());
        ApplyEmissionMaterial(outerRing.gameObject, riftRingColor, 8.8f);

        innerRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder).transform;
        innerRing.name = "InnerRing";
        innerRing.SetParent(portalRoot, false);
        innerRing.localRotation = Quaternion.Euler(90f, 0f, 0f);
        innerRing.localScale = new Vector3(0.26f, 0.015f, 0.26f);
        Destroy(innerRing.GetComponent<Collider>());
        ApplyEmissionMaterial(innerRing.gameObject, riftGlowColor, 7.6f);

        GameObject lightRoot = new GameObject("RiftLight");
        lightRoot.transform.SetParent(portalRoot, false);
        portalLight = lightRoot.AddComponent<Light>();
        portalLight.type = LightType.Point;
        portalLight.range = 26f;
        portalLight.intensity = 15f;
        portalLight.color = riftGlowColor;

        arrowRoot = new GameObject("GuideMarker").transform;
        arrowRoot.SetParent(portalRoot, false);
        arrowRoot.localPosition = new Vector3(0f, 0.62f, 0f);
        arrowRoot.localRotation = Quaternion.identity;

        arrowBody = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        arrowBody.name = "MarkBody";
        arrowBody.SetParent(arrowRoot, false);
        arrowBody.localScale = new Vector3(0.012f, 0.34f, 0.008f);
        arrowBody.localPosition = new Vector3(0f, 0.06f, 0f);
        Destroy(arrowBody.GetComponent<Collider>());
        ApplyEmissionMaterial(arrowBody.gameObject, riftGlowColor, 4.8f);

        arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
        arrowHead.name = "MarkDot";
        arrowHead.SetParent(arrowRoot, false);
        arrowHead.localScale = new Vector3(0.02f, 0.02f, 0.02f);
        arrowHead.localPosition = new Vector3(0f, -0.18f, 0f);
        arrowHead.localRotation = Quaternion.identity;
        Destroy(arrowHead.GetComponent<Collider>());
        ApplyEmissionMaterial(arrowHead.gameObject, riftGlowColor, 5.8f);

    }

    private void AnimatePortalVisual()
    {
        if (portalRoot == null)
        {
            return;
        }

        float time = Time.time;
        portalRoot.localPosition = new Vector3(0f, portalHeightOffset + Mathf.Sin(time * 1.45f) * 0.08f, 0f);

        if (arrowRoot != null)
        {
            float arrowBob = Mathf.Sin(time * 2.2f) * 0.06f;
            float arrowPulse = 1f + Mathf.Sin(time * 3.5f) * 0.08f;
            arrowRoot.localPosition = new Vector3(0f, 0.62f + arrowBob, 0f);
            arrowRoot.localScale = Vector3.one * arrowPulse;
        }

        if (outerRing != null)
        {
            outerRing.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
        }

        if (innerRing != null)
        {
            innerRing.Rotate(Vector3.forward, -spinSpeed * 1.55f * Time.deltaTime, Space.Self);
        }

        if (core != null)
        {
            float pulse = 1f + Mathf.Sin(time * pulseSpeed) * 0.08f;
            core.localScale = new Vector3(0.28f, 0.46f, 0.1f) * pulse;
        }

        if (portalLight != null)
        {
            portalLight.intensity = 15f + Mathf.Sin(time * 4f) * 2.2f;
            portalLight.range = 26f + Mathf.Sin(time * 2.5f) * 1.8f;
        }
    }

    private Vector3 GetPortalCenter()
    {
        return portalRoot != null ? portalRoot.position : transform.position + Vector3.up * portalHeightOffset;
    }

    private void EnsurePortalTrigger()
    {
        portalTrigger = GetComponent<SphereCollider>();
        if (portalTrigger == null)
        {
            portalTrigger = gameObject.AddComponent<SphereCollider>();
        }

        portalTrigger.isTrigger = true;
        portalTrigger.enabled = true;
        portalTrigger.radius = triggerRadius;
        portalTrigger.center = new Vector3(0f, portalHeightOffset, 0f);
    }

    private static bool TryGetPlayer(out BeanController sphere, out FirstPersonControllerSimple fps, out Transform playerRoot)
    {
        sphere = Object.FindObjectOfType<BeanController>();
        fps = Object.FindObjectOfType<FirstPersonControllerSimple>();
        playerRoot = fps != null ? fps.transform : sphere != null ? sphere.transform : null;
        return playerRoot != null;
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

    private static void SetPlayerPositionAndRotation(BeanController sphere, FirstPersonControllerSimple fps, Vector3 position, Quaternion rotation)
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

    private static void SetPlayerVisible(BeanController sphere, bool visible)
    {
        if (sphere == null)
        {
            return;
        }

        Renderer[] renderers = sphere.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = visible;
        }
    }

    private void EnsureOverlay()
    {
        if (overlayCanvas != null && whiteImage != null && centerText != null)
        {
            return;
        }

        GameObject canvasObject = GameObject.Find("SphereRiftPortalCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("SphereRiftPortalCanvas");
        }

        overlayCanvas = canvasObject.GetComponent<Canvas>();
        if (overlayCanvas == null)
        {
            overlayCanvas = canvasObject.AddComponent<Canvas>();
        }

        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 16000;

        if (canvasObject.GetComponent<CanvasScaler>() == null)
        {
            canvasObject.AddComponent<CanvasScaler>();
        }

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
        {
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        whiteImage = EnsureImage(canvasObject.transform, "WhiteImage", Color.white);
        Stretch(whiteImage.rectTransform);

        centerText = EnsureText(canvasObject.transform, "CenterText", "test", 72, new Color(0.08f, 0.08f, 0.08f, 1f));
        Stretch(centerText.rectTransform);
    }

    private void ResetOverlay()
    {
        if (overlayCanvas == null || whiteImage == null || centerText == null)
        {
            return;
        }

        overlayCanvas.gameObject.SetActive(false);
        whiteImage.gameObject.SetActive(false);
        centerText.gameObject.SetActive(false);

        Color clear = Color.white;
        clear.a = 0f;
        whiteImage.color = clear;
    }

    private IEnumerator FadeWhite(float from, float to, float duration)
    {
        if (whiteImage == null)
        {
            yield break;
        }

        Color color = whiteImage.color;
        color.a = from;
        whiteImage.color = color;
        whiteImage.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = EaseInOut(elapsed / Mathf.Max(0.01f, duration));
            color.a = Mathf.Lerp(from, to, t);
            whiteImage.color = color;
            yield return null;
        }

        color.a = to;
        whiteImage.color = color;
    }

    private static Image EnsureImage(Transform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            go.transform.SetParent(parent, false);
        }

        Image image = go.GetComponent<Image>();
        if (image == null)
        {
            image = go.AddComponent<Image>();
        }

        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Text EnsureText(Transform parent, string name, string value, int fontSize, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject go = existing != null ? existing.gameObject : new GameObject(name);
        if (existing == null)
        {
            go.transform.SetParent(parent, false);
        }

        Text text = go.GetComponent<Text>();
        if (text == null)
        {
            text = go.AddComponent<Text>();
        }

        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = color;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
    }

    private static float EaseInOut(float t)
    {
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
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
}

public static class SphereRiftPortalSequenceBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AttachIfNeeded()
    {
        GameObject target = GameObject.Find("Sphere");
        if (target == null)
        {
            return;
        }

        if (target.GetComponent<SphereRiftPortalSequence>() == null)
        {
            target.AddComponent<SphereRiftPortalSequence>();
        }
    }
}
