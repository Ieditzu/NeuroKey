using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider))]
public class CoinRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 120f;
    [SerializeField] private GameObject robotPrefab;
    [SerializeField] private Vector3 robotSpawnPosition = new Vector3(46f, 5f, 312f);
    [SerializeField] private Vector3 robotScale = Vector3.one * 0.2f;

    private void Awake()
    {
#if UNITY_EDITOR
        if (robotPrefab == null)
        {
            robotPrefab = LoadRobotFromAssets();
        }
#endif
        EnsureTrigger();
    }

    private void Reset()
    {
        EnsureTrigger();
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BeanController>() != null || other.GetComponent<CharacterController>() != null)
        {
            var ui = PickupUIController.Instance ?? FindObjectOfType<PickupUIController>();
            if (ui != null)
            {
                ui.Show();
            }

            SpawnRobot();
            gameObject.SetActive(false);
        }
    }

    private void SpawnRobot()
    {
        GameObject prefabToUse = robotPrefab ?? LoadRobotFromAssets();
        if (prefabToUse == null)
        {
            Debug.LogWarning("Robot prefab not found; assign it on CoinRotator or place one at Assets/robot-copernicus/source/low.obj");
            return;
        }

        var spawned = Instantiate(prefabToUse, robotSpawnPosition, Quaternion.identity);
        spawned.transform.localScale = robotScale;

        // Face the player horizontally and keep doing so.
        Transform player = ResolvePlayerTransform();
        if (player != null)
        {
            Vector3 dir = player.position - spawned.transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                spawned.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            }
        }
        if (spawned.GetComponent<RobotLookAt>() == null)
        {
            spawned.AddComponent<RobotLookAt>();
        }

    }

    private GameObject LoadRobotFromAssets()
    {
#if UNITY_EDITOR
        var loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/robot-copernicus/source/low.obj");
        if (loaded != null)
        {
            return loaded;
        }
#endif
        return null;
    }

    private Transform ResolvePlayerTransform()
    {
        var bean = FindObjectOfType<BeanController>();
        if (bean != null) return bean.transform;

        var fps = FindObjectOfType<FirstPersonControllerSimple>();
        if (fps != null) return fps.transform;

        return null;
    }

    private void EnsureTrigger()
    {
        var col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
            col.enabled = true;
        }
    }
}
