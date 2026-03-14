using UnityEngine;

[ExecuteAlways]
public class SakuraAreaColliderApplier : MonoBehaviour
{
    private void OnEnable()
    {
        ApplyColliders();
    }

    private void OnValidate()
    {
        ApplyColliders();
    }

    [ContextMenu("Apply Sakura Area Colliders")]
    public void ApplyColliders()
    {
        ApplyToRootAndChildren(GameObject.Find("sakura2"), false);
        ApplyWalkableTopSurface(GameObject.Find("low_poly_treesNXT_5flat"));
        ApplySketchfabWalkableSurfaces();
        ApplyWalkableBridgeColliders();
        ApplyWalkableTopSurface("plasnapython");
        ApplyWalkableTopSurface("Plane.001");
    }

    private static void ApplyWalkableTopSurface(string objectName)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].name == objectName)
            {
                ApplyWalkableTopSurface(allObjects[i]);
            }
        }
    }

    private static void ApplyToRootAndChildren(GameObject root, bool preferWalkable)
    {
        if (root == null)
        {
            return;
        }

        AddBestCollider(root, preferWalkable);

        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i] == null)
            {
                continue;
            }

            AddBestCollider(
                meshFilters[i].gameObject,
                preferWalkable || meshFilters[i].name == "Plane.001" || meshFilters[i].name == "plasnapython");
        }
    }

    private static void ApplyWalkableBridgeColliders()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        for (int i = 0; i < allObjects.Length; i++)
        {
            if (allObjects[i].name != "BranchBridge")
            {
                continue;
            }

            BuildSimpleWalkSurface(allObjects[i]);
        }
    }

    private static void ApplySketchfabWalkableSurfaces()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>(true);
        for (int i = 0; i < allObjects.Length; i++)
        {
            string name = allObjects[i].name;
            if (!name.StartsWith("Sketchfab_"))
            {
                continue;
            }

            BuildSimpleWalkSurface(allObjects[i]);
        }
    }

    private static void ApplyWalkableTopSurface(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        BuildSimpleWalkSurface(root);
    }

    private static void BuildSimpleWalkSurface(GameObject bridgeRoot)
    {
        if (bridgeRoot == null)
        {
            return;
        }

        Transform existing = bridgeRoot.transform.Find("WalkSurfaceCollider");
        if (existing != null)
        {
            DestroySafe(existing.gameObject);
        }

        Collider[] childColliders = bridgeRoot.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < childColliders.Length; i++)
        {
            if (childColliders[i] == null)
            {
                continue;
            }

            if (childColliders[i].gameObject == bridgeRoot)
            {
                DestroySafe(childColliders[i]);
                continue;
            }

            DestroySafe(childColliders[i]);
        }

        Bounds? localBounds = CalculateCombinedLocalBounds(bridgeRoot.transform);
        if (!localBounds.HasValue)
        {
            return;
        }

        Bounds bounds = localBounds.Value;
        float skin = 0.08f;
        float surfaceHeight = Mathf.Clamp(bounds.size.y * 0.18f, 0.08f, 0.18f);

        GameObject walkSurface = new GameObject("WalkSurfaceCollider");
        walkSurface.transform.SetParent(bridgeRoot.transform, false);
        walkSurface.transform.localPosition = new Vector3(
            bounds.center.x,
            bounds.max.y - (surfaceHeight * 0.5f),
            bounds.center.z);
        walkSurface.transform.rotation = Quaternion.identity;

        BoxCollider box = walkSurface.AddComponent<BoxCollider>();
        box.isTrigger = false;
        box.size = new Vector3(
            Mathf.Max(0.2f, bounds.size.x - skin),
            surfaceHeight,
            Mathf.Max(0.2f, bounds.size.z - skin));
        box.center = Vector3.zero;
    }

    private static Bounds? CalculateCombinedLocalBounds(Transform root)
    {
        MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
        bool hasBounds = false;
        Bounds combined = default;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter == null || meshFilter.sharedMesh == null)
            {
                continue;
            }

            Bounds meshBounds = meshFilter.sharedMesh.bounds;
            Vector3[] corners = GetBoundsCorners(meshBounds);
            for (int cornerIndex = 0; cornerIndex < corners.Length; cornerIndex++)
            {
                Vector3 worldPoint = meshFilter.transform.TransformPoint(corners[cornerIndex]);
                Vector3 localPoint = root.InverseTransformPoint(worldPoint);

                if (!hasBounds)
                {
                    combined = new Bounds(localPoint, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    combined.Encapsulate(localPoint);
                }
            }
        }

        return hasBounds ? combined : null;
    }

    private static Vector3[] GetBoundsCorners(Bounds bounds)
    {
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        return new[]
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z),
        };
    }

    private static void AddBestCollider(GameObject target, bool preferWalkable)
    {
        if (target == null)
        {
            return;
        }

        if (target.GetComponent<Collider>() != null)
        {
            return;
        }

        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        Renderer renderer = target.GetComponent<Renderer>();
        if (meshFilter == null || meshFilter.sharedMesh == null || renderer == null)
        {
            return;
        }

        Bounds bounds = renderer.bounds;
        Vector3 size = bounds.size;

        if (preferWalkable && size.x > 0.5f && size.z > 0.5f)
        {
            BoxCollider box = target.AddComponent<BoxCollider>();
            box.isTrigger = false;
            return;
        }

        MeshCollider meshCollider = target.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = meshFilter.sharedMesh;
        meshCollider.convex = false;
        meshCollider.isTrigger = false;
    }

    private static void DestroySafe(Object obj)
    {
        if (obj == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(obj);
        }
        else
        {
            Object.DestroyImmediate(obj);
        }
    }
}
