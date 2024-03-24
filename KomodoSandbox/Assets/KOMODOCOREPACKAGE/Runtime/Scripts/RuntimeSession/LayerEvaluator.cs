using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;


public class LayerEvaluator : XRTargetEvaluator
{
    public LayerMask preferredLayers;

    protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
    {
        int targetLayer = 1 << target.transform.gameObject.layer;
        bool isPreferredLayer = (preferredLayers.value & targetLayer) != 0;
        return isPreferredLayer ? 1f : 0f;
    }
}
