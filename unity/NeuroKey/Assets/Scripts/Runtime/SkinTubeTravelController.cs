using System.Collections;
using UnityEngine;

public class SkinTubeTravelController : MonoBehaviour
{
    [SerializeField] private Transform exitTube;
    [SerializeField] private Transform ejectTarget;
    [SerializeField] private float alignToHoleDuration = 0.2f;
    [SerializeField] private float descendIntoHoleDuration = 0.55f;
    [SerializeField] private float travelUndergroundDuration = 0.72f;
    [SerializeField] private float riseFromExitDuration = 0.28f;
    [SerializeField] private float holeMouthHeight = 0.18f;
    [SerializeField] private float shaftBottomY = -0.72f;
    [SerializeField] private float undergroundY = -1.35f;
    [SerializeField] private float exitSpeed = 8.2f;

    private Coroutine activeTravel;

    public void Configure(Transform exitTubeRef, Transform ejectTargetRef)
    {
        exitTube = exitTubeRef;
        ejectTarget = ejectTargetRef;
    }

    public void PlayTravel(
        SphereController sphere,
        PlayerSkinController skinController,
        PlayerSkinController.SkinType selectedSkin,
        Transform sourceTube)
    {
        if (sphere == null || skinController == null || sourceTube == null || exitTube == null || ejectTarget == null)
        {
            return;
        }

        if (activeTravel != null)
        {
            StopCoroutine(activeTravel);
        }

        activeTravel = StartCoroutine(PlayTravelRoutine(sphere, skinController, selectedSkin, sourceTube));
    }

    private IEnumerator PlayTravelRoutine(
        SphereController sphere,
        PlayerSkinController skinController,
        PlayerSkinController.SkinType selectedSkin,
        Transform sourceTube)
    {
        var rb = sphere.GetComponent<Rigidbody>();
        sphere.SetMovementLocked(true);
        sphere.SetHardFreeze(true);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Vector3 start = sphere.transform.position;
        Vector3 sourceMouth = sourceTube.position + (Vector3.up * holeMouthHeight);
        Vector3 exitMouth = exitTube.position + (Vector3.up * holeMouthHeight);
        Vector3 sourceShaftBottom = new Vector3(sourceTube.position.x, shaftBottomY, sourceTube.position.z);
        Vector3 exitShaftBottom = new Vector3(exitTube.position.x, shaftBottomY, exitTube.position.z);
        Vector3 sourceUnderground = new Vector3(sourceTube.position.x, undergroundY, sourceTube.position.z);
        Vector3 exitUnderground = new Vector3(exitTube.position.x, undergroundY, exitTube.position.z);

        float t = 0f;
        while (t < alignToHoleDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.001f, alignToHoleDuration));
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(start, sourceMouth, smooth);
            yield return null;
        }

        t = 0f;
        while (t < descendIntoHoleDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.001f, descendIntoHoleDuration));
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(sourceMouth, sourceShaftBottom, smooth);
            yield return null;
        }

        skinController.ApplySkin(selectedSkin);

        t = 0f;
        const float dipToUndergroundDuration = 0.16f;
        while (t < dipToUndergroundDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / dipToUndergroundDuration);
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(sourceShaftBottom, sourceUnderground, smooth);
            yield return null;
        }

        t = 0f;
        while (t < travelUndergroundDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.001f, travelUndergroundDuration));
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(sourceUnderground, exitUnderground, smooth);
            yield return null;
        }

        t = 0f;
        const float riseToShaftDuration = 0.16f;
        while (t < riseToShaftDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / riseToShaftDuration);
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(exitUnderground, exitShaftBottom, smooth);
            yield return null;
        }

        t = 0f;
        while (t < riseFromExitDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.001f, riseFromExitDuration));
            float smooth = k * k * (3f - (2f * k));
            sphere.transform.position = Vector3.Lerp(exitShaftBottom, exitMouth, smooth);
            yield return null;
        }

        sphere.transform.position = exitMouth;
        sphere.SetHardFreeze(false);
        sphere.SetMovementLocked(false);

        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 ejectDir = (ejectTarget.position - exitMouth);
            ejectDir.y = 0f;
            ejectDir = ejectDir.sqrMagnitude > 0.0001f ? ejectDir.normalized : Vector3.left;
            rb.velocity = (ejectDir * exitSpeed) + (Vector3.up * 3.8f);
        }

        activeTravel = null;
    }
}
