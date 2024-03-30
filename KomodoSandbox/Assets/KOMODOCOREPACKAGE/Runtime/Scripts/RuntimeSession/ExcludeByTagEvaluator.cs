using System;

using UnityEngine.XR.Interaction.Toolkit.Filtering;

[Serializable]
public class ExcludeByTagEvaluator : XRTargetEvaluator
{
    public string tagToExclude;

    protected override float CalculateNormalizedScore(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRInteractor interactor, UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable target)
    {
        return target.transform.CompareTag(tagToExclude) ? 0f : 1f;
    }
}