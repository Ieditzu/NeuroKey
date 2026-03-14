using UnityEngine;

/// <summary>Spawns a simple first-person capsule if none exists so you can explore the map in FPS mode.</summary>
public static class FpsBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Spawn()
    {
        if (Object.FindObjectOfType<FirstPersonControllerSimple>() != null)
        {
            return;
        }

        Vector3 spawnPos = GuessSpawnPosition();
        GameObject player = new GameObject("FPS_Player");
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.height = 1.42f;
        cc.radius = 0.18f;
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);

        player.transform.position = spawnPos;
        player.AddComponent<FirstPersonControllerSimple>();

        // Keep legacy sphere disabled/hidden so only FPS player is visible at spawn.
        SphereController sphere = Object.FindObjectOfType<SphereController>();
        if (sphere != null)
        {
            sphere.gameObject.SetActive(false);
        }
    }

    private static Vector3 GuessSpawnPosition()
    {
        // Prefer to start near the sphere's initial position if present.
        SphereController sphere = Object.FindObjectOfType<SphereController>();
        if (sphere != null)
        {
            Vector3 pos = sphere.transform.position + new Vector3(0f, 2.0f, -2.5f);
            return pos;
        }

        GameObject easyPath = GameObject.Find("PathEasy");
        if (easyPath != null)
        {
            return easyPath.transform.position + new Vector3(0f, 2.0f, -4f);
        }

        // Default spawn if no hints found.
        return new Vector3(75f, 1.0f, 222f);
    }
}
