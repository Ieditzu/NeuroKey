using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PlayerSkinController : MonoBehaviour
{
    public enum SkinType
    {
        NeonOrbit = 0,
        MagentaStrata = 1,
        MintGrid = 2,
        AmberPulse = 3
    }

    [SerializeField] private SkinType defaultSkin = SkinType.NeonOrbit;

    private static Mesh cachedSphereMesh;
    private static Material[] cachedSkinMaterials;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private SphereController sphereController;
    private SkinType currentSkin;
    private bool hasAppliedSkin;

    private void Awake()
    {
        EnsureInitialized();
        ApplyDefaultSkin();
    }

    public void ApplyDefaultSkin()
    {
        ApplySkin(defaultSkin);
    }

    public void ApplySkin(SkinType skin)
    {
        EnsureInitialized();
        if (hasAppliedSkin && currentSkin == skin)
        {
            return;
        }

        EnsureSphereBody();
        RemoveLegacyDetailChildren(transform);
        meshRenderer.sharedMaterial = GetOrCreateSkinMaterial(skin);
        currentSkin = skin;
        hasAppliedSkin = true;

        if (sphereController != null)
        {
            sphereController.RefreshRollingRadius();
        }
    }

    public static void ApplyPreviewSkin(GameObject previewObject, SkinType skin)
    {
        if (previewObject == null)
        {
            return;
        }

        var filter = previewObject.GetComponent<MeshFilter>();
        if (filter == null)
        {
            filter = previewObject.AddComponent<MeshFilter>();
        }

        var renderer = previewObject.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = previewObject.AddComponent<MeshRenderer>();
        }

        filter.sharedMesh = GetSphereMesh();

        var collider = previewObject.GetComponent<Collider>();
        if (collider != null)
        {
            DestroySafe(collider);
        }

        RemoveLegacyDetailChildren(previewObject.transform);
        renderer.sharedMaterial = GetOrCreateSkinMaterial(skin);
    }

    public SkinType CurrentSkin => currentSkin;

    private void EnsureInitialized()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (sphereController == null)
        {
            sphereController = GetComponent<SphereController>();
        }
    }

    private void EnsureSphereBody()
    {
        var boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            DestroySafe(boxCollider);
        }

        var sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider == null)
        {
            sphereCollider = gameObject.AddComponent<SphereCollider>();
        }

        sphereCollider.radius = 0.5f;
        meshFilter.sharedMesh = GetSphereMesh();
    }

    private static Mesh GetSphereMesh()
    {
        if (cachedSphereMesh != null)
        {
            return cachedSphereMesh;
        }

        var temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cachedSphereMesh = temp.GetComponent<MeshFilter>()?.sharedMesh;
        DestroySafe(temp);
        return cachedSphereMesh;
    }

    private static void GetPalette(SkinType skin, out Color colorA, out Color colorB)
    {
        switch (skin)
        {
            case SkinType.MagentaStrata:
                colorA = new Color(0.31f, 0.12f, 0.32f, 1f);
                colorB = new Color(0.95f, 0.34f, 0.78f, 1f);
                break;
            case SkinType.MintGrid:
                colorA = new Color(0.1f, 0.31f, 0.27f, 1f);
                colorB = new Color(0.32f, 0.95f, 0.72f, 1f);
                break;
            case SkinType.AmberPulse:
                colorA = new Color(0.34f, 0.21f, 0.08f, 1f);
                colorB = new Color(0.98f, 0.75f, 0.26f, 1f);
                break;
            default:
                colorA = new Color(0.08f, 0.25f, 0.38f, 1f);
                colorB = new Color(0.2f, 0.9f, 0.98f, 1f);
                break;
        }
    }

    private static Material CreateFuturisticDualToneMaterial(Color colorA, Color colorB, int pattern)
    {
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            return null;
        }

        var material = new Material(shader);

        var texture = CreateDualToneSphereTexture(colorA, colorB, pattern);
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;

        material.mainTexture = texture;
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }

        Color baseBlend = Color.Lerp(colorA, colorB, 0.5f);
        material.color = baseBlend;
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", baseBlend);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", 0.72f);
        }

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", 0.93f);
        }
        else if (material.HasProperty("_Glossiness"))
        {
            material.SetFloat("_Glossiness", 0.93f);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", colorB * 0.38f);
        }

        return material;
    }

    private static Material GetOrCreateSkinMaterial(SkinType skin)
    {
        int index = (int)skin;
        if (index < 0)
        {
            index = 0;
        }

        if (cachedSkinMaterials == null || cachedSkinMaterials.Length < 4)
        {
            cachedSkinMaterials = new Material[4];
        }

        if (index >= cachedSkinMaterials.Length)
        {
            index = 0;
        }

        if (cachedSkinMaterials[index] != null)
        {
            return cachedSkinMaterials[index];
        }

        Color colorA;
        Color colorB;
        GetPalette((SkinType)index, out colorA, out colorB);
        cachedSkinMaterials[index] = CreateFuturisticDualToneMaterial(colorA, colorB, index);
        return cachedSkinMaterials[index];
    }

    private static Texture2D CreateDualToneSphereTexture(Color colorA, Color colorB, int pattern)
    {
        const int width = 512;
        const int height = 256;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        for (int y = 0; y < height; y++)
        {
            float v = y / (float)(height - 1);
            for (int x = 0; x < width; x++)
            {
                float u = x / (float)(width - 1);

                float blend;
                switch (pattern)
                {
                    case 1:
                        // layered horizontal strata with slight drift
                        blend = 0.5f + (Mathf.Sin((v * 14f) + (u * 2.4f)) * 0.38f);
                        break;
                    case 2:
                        // grid-wave fusion
                        float g1 = Mathf.Sin(u * 18f);
                        float g2 = Mathf.Sin(v * 12f);
                        blend = 0.5f + ((g1 * g2) * 0.34f);
                        break;
                    case 3:
                        // spiral pulse bands
                        float spiral = Mathf.Sin((u * 16f) - (v * 9f));
                        blend = 0.5f + (spiral * 0.4f);
                        break;
                    default:
                        // diagonal orbit split with soft transition
                        blend = 0.5f + (Mathf.Sin((u * 9f) + (v * 5f)) * 0.32f);
                        break;
                }

                blend = Mathf.Clamp01(blend);
                Color c = Color.Lerp(colorA, colorB, blend);

                // subtle polish variation to keep modern look without clutter
                float polish = 0.92f + (Mathf.Sin((u * 6.28318f) + (v * 3.14159f)) * 0.06f);
                c *= polish;
                c.a = 1f;

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        return texture;
    }

    private static void RemoveLegacyDetailChildren(Transform target)
    {
        if (target == null)
        {
            return;
        }

        for (int i = target.childCount - 1; i >= 0; i--)
        {
            var child = target.GetChild(i);
            if (child.name == "SkinDetails" || child.name.EndsWith("Detail"))
            {
                DestroySafe(child.gameObject);
            }
        }
    }

    private void EnsureLobbySkinSelectionAreaRuntime()
    {
        EnsureMainFloorShadowlessRuntime();
        EnsureSpawnFloorFlatRuntime();
        RemoveLegacyDualPitFloorRuntime();
        var existingRoot = GameObject.Find("LobbySkinSelection");
        if (existingRoot != null)
        {
            for (int i = existingRoot.transform.childCount - 1; i >= 0; i--)
            {
                DestroySafe(existingRoot.transform.GetChild(i).gameObject);
            }
        }

        var root = existingRoot != null ? existingRoot : new GameObject("LobbySkinSelection");

        var pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = "SkinSelectionPad";
        pad.transform.SetParent(root.transform);
        pad.transform.localPosition = new Vector3(8.95f, 0.03f, 7.55f);
        pad.transform.localScale = new Vector3(6.5f, 0.04f, 2.15f);
        var padRenderer = pad.GetComponent<Renderer>();
        if (padRenderer != null)
        {
            padRenderer.material = CreateFuturisticDualToneMaterial(
                new Color(0.03f, 0.04f, 0.08f, 1f),
                new Color(0.08f, 0.22f, 0.35f, 1f),
                0);
        }

        const float startX = 6.95f;
        const float spacing = 1.3f;
        const float z = 7.55f;
        float sampleY = 0.07f + (Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z) * 0.5f);

        CreateRuntimeSkinSample(root.transform, "SkinSampleNeonOrbit", SkinType.NeonOrbit, new Vector3(startX + (spacing * 0f), sampleY, z), transform.localScale);
        CreateRuntimeSkinSample(root.transform, "SkinSampleMagentaStrata", SkinType.MagentaStrata, new Vector3(startX + (spacing * 1f), sampleY, z), transform.localScale);
        CreateRuntimeSkinSample(root.transform, "SkinSampleMintGrid", SkinType.MintGrid, new Vector3(startX + (spacing * 2f), sampleY, z), transform.localScale);
        CreateRuntimeSkinSample(root.transform, "SkinSampleAmberPulse", SkinType.AmberPulse, new Vector3(startX + (spacing * 3f), sampleY, z), transform.localScale);

        EnsureSkinSelectionLabelRuntime(root.transform);
    }

    private static void CreateRuntimeSkinSample(
        Transform parent,
        string name,
        SkinType skinType,
        Vector3 localPosition,
        Vector3 modelScale)
    {
        var preview = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        preview.name = name;
        preview.transform.SetParent(parent);
        preview.transform.localPosition = localPosition;
        preview.transform.localRotation = Quaternion.identity;
        preview.transform.localScale = modelScale;

        ApplyPreviewSkin(preview, skinType);

        var triggerObject = new GameObject(name + "Trigger");
        triggerObject.transform.SetParent(parent);
        triggerObject.transform.localPosition = localPosition + new Vector3(0f, 0.2f, -0.95f);
        triggerObject.transform.localRotation = Quaternion.identity;
        triggerObject.transform.localScale = Vector3.one;

        var triggerCollider = triggerObject.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(1f, 1.25f, 0.9f);
        triggerCollider.center = new Vector3(0f, 0.45f, 0f);

        var trigger = triggerObject.AddComponent<SkinSelectionTrigger>();
        trigger.SetSkin(skinType);
    }

    private static void EnsureMainFloorShadowlessRuntime()
    {
        var floor = GameObject.Find("Floor");
        if (floor == null)
        {
            return;
        }

        var renderer = floor.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader != null)
        {
            var material = new Material(shader);
            var uniformFloorPathColor = new Color(0.24f, 0.24f, 0.24f, 1f);
            material.color = uniformFloorPathColor;
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", uniformFloorPathColor);
            }
            renderer.material = material;
        }

        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }

    private static void EnsureSpawnFloorFlatRuntime()
    {
        var spawnFloor = GameObject.Find("SpawnLobbyFloor");
        if (spawnFloor == null)
        {
            spawnFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spawnFloor.name = "SpawnLobbyFloor";
        }

        spawnFloor.transform.position = new Vector3(0f, -0.05f, 0f);
        spawnFloor.transform.rotation = Quaternion.identity;
        spawnFloor.transform.localScale = new Vector3(12f, 0.1f, 9f);

        var renderer = spawnFloor.GetComponent<Renderer>();
        if (renderer != null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Color");
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader != null)
            {
                var material = new Material(shader);
                var uniformFloorPathColor = new Color(0.24f, 0.24f, 0.24f, 1f);
                material.color = uniformFloorPathColor;
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", uniformFloorPathColor);
                }
                renderer.material = material;
            }

            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
    }

    private static void EnsureSkinSelectionLabelRuntime(Transform parent)
    {
        var textObj = new GameObject("SkinSelectionLabelText");
        textObj.transform.SetParent(parent);
        textObj.transform.localPosition = new Vector3(8.95f, 0.04f, 5.35f);
        textObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        textObj.transform.localScale = Vector3.one;

        var textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = "Change Skin";
        textMesh.fontSize = 88;
        textMesh.characterSize = 0.075f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
    }

    private static void RemoveLegacyDualPitFloorRuntime()
    {
        var legacyDualPitFloor = GameObject.Find("RectangularFloorWithDualPits");
        if (legacyDualPitFloor != null)
        {
            DestroySafe(legacyDualPitFloor);
        }

        var allTransforms = Object.FindObjectsOfType<Transform>();
        foreach (var t in allTransforms)
        {
            if (t == null || t.gameObject == null)
            {
                continue;
            }

            string n = t.gameObject.name.ToLowerInvariant();
            bool isLegacyPit = n == "pita" || n == "pitb" || n == "pitleft" || n == "pitright";
            bool isHoleLike = n.Contains("hole") || n.Contains("pit") || n.Contains("crater");
            if (!isLegacyPit && !isHoleLike)
            {
                continue;
            }

            if (Mathf.Abs(t.position.x) <= 13f && Mathf.Abs(t.position.z) <= 10f)
            {
                DestroySafe(t.gameObject);
            }
        }
    }

    private static void DestroySafe(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}
