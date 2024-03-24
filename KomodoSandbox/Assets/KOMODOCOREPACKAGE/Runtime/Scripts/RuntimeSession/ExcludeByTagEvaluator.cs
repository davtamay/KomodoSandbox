using System;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class ExcludeByTagEvaluator : XRTargetEvaluator
{
    public string tagToExclude;

    protected override float CalculateNormalizedScore(IXRInteractor interactor, IXRInteractable target)
    {
        return target.transform.CompareTag(tagToExclude) ? 0f : 1f;
    }
}