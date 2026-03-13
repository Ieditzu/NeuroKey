using UnityEngine;

public class SkinSelectionTrigger : MonoBehaviour
{
    [SerializeField] private PlayerSkinController.SkinType skin = PlayerSkinController.SkinType.NeonOrbit;
    [SerializeField] private bool useTubeTravelAnimation;
    [SerializeField] private Transform sourceTube;
    [SerializeField] private SkinTubeTravelController travelController;

    public void SetSkin(PlayerSkinController.SkinType newSkin)
    {
        skin = newSkin;
    }

    public void SetTubeTravel(SkinTubeTravelController controller, Transform tube, bool enabled)
    {
        travelController = controller;
        sourceTube = tube;
        useTubeTravelAnimation = enabled;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryApplySkin(other);
    }

    private void TryApplySkin(Collider other)
    {
        var sphere = other.GetComponent<SphereController>();
        if (sphere == null)
        {
            return;
        }

        var skinController = sphere.GetComponent<PlayerSkinController>();
        if (skinController == null)
        {
            return;
        }

        if (!useTubeTravelAnimation && skinController.CurrentSkin == skin)
        {
            return;
        }

        if (useTubeTravelAnimation && travelController != null && sourceTube != null)
        {
            travelController.PlayTravel(sphere, skinController, skin, sourceTube);
            return;
        }

        skinController.ApplySkin(skin);
    }
}
