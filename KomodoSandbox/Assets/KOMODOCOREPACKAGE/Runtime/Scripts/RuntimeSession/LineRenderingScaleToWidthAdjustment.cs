using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LineRenderingScaleToWidthAdjustment : MonoBehaviour
{
    Coroutine applyScaleCoroutine_L;
    Coroutine applyScaleCoroutine_R;

    Transform leftScaledObject;
    Transform rightScaledObject;

    LineRenderer[] currentL;
    int[] NetIDs_L;
    float[] currentLInitialValues_L;

    LineRenderer[] currentR;
    int[] NetIDs_R;
    float[] currentRInitialValues_R;

    private Vector3 initialLeftScale = Vector3.one;
    private Vector3 initialRightScale = Vector3.one;
    private bool leftScaleSet = false;
    private bool rightScaleSet = false;


    public void SelectObject(SelectEnterEventArgs seet)
    {
        Debug.Log("using: " + seet.interactorObject.transform.parent.name + "======  selected : " + seet.interactableObject.transform.name);

        if (seet.interactorObject.interactablesSelected.Count > 1)
        {
            Debug.Log("GRABBED Multiple : " + seet.interactorObject.interactablesSelected.Count);
        }
        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;

      
        if ("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor)
        {
            if (rightScaledObject && rightScaledObject.gameObject.GetInstanceID() == seet.interactableObject.transform.gameObject.GetInstanceID())
            {
                return; // Exit if this object is already being scaled by the other hand.
            }

            currentL = seet.interactableObject.transform.GetComponentsInChildren<LineRenderer>();
            Debug.Log("CAPTURED LINE RENDERERS: " + currentL.Length);
            currentLInitialValues_L = new float[currentL.Length];
            NetIDs_L = new int[currentL.Length];

            for (int i = 0; i < currentL.Length; i++)
            {
                currentLInitialValues_L[i] = currentL[i].widthMultiplier;
                NetIDs_L[i] = currentL[i].GetComponentInParent<NetworkedGameObject>().thisEntityID;
            }

            leftScaledObject = seet.interactableObject.transform;

            if (!leftScaleSet)
            {
                initialLeftScale = leftScaledObject.localScale; // Capture initial scale at first grab
                leftScaleSet = true;
            }

            applyScaleCoroutine_L = StartCoroutine(ApplyLineScaling(currentL, NetIDs_L, currentLInitialValues_L, leftScaledObject));
        }
        else if ("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor)
        {
            if (leftScaledObject && leftScaledObject.gameObject.GetInstanceID() == seet.interactableObject.transform.gameObject.GetInstanceID())
            {
                return; // Exit if this object is already being scaled by the other hand.
            }

            currentR = seet.interactableObject.transform.GetComponentsInChildren<LineRenderer>();
            Debug.Log("CAPTURED LINE RENDERERS: " + currentR.Length);
            currentRInitialValues_R = new float[currentR.Length];
            NetIDs_R = new int[currentR.Length];

            for (int i = 0; i < currentR.Length; i++)
            {
                currentRInitialValues_R[i] = currentR[i].widthMultiplier;
                NetIDs_R[i] = currentR[i].GetComponentInParent<NetworkedGameObject>().thisEntityID;
            }

            rightScaledObject = seet.interactableObject.transform;

            if (!rightScaleSet)
            {
                initialRightScale = rightScaledObject.localScale; // Capture initial scale at first grab
                rightScaleSet = true;
            }

            applyScaleCoroutine_R = StartCoroutine(ApplyLineScaling(currentR, NetIDs_R, currentRInitialValues_R, rightScaledObject));
        }
    }


    public void DeselectObject(SelectExitEventArgs seet)
    {
        Debug.Log("using: " + seet.interactorObject.transform.parent.name + "======  Deselected : " + seet.interactableObject.transform.name);

        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;

        if ("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor)
        {
            if (applyScaleCoroutine_L != null)
                StopCoroutine(applyScaleCoroutine_L);

            currentL = null;
            currentLInitialValues_L = null;
            leftScaledObject = null;
            leftScaleSet = false; // Reset scale set flag
        }
        else if ("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor)
        {
            if (applyScaleCoroutine_R != null)
                StopCoroutine(applyScaleCoroutine_R);

            currentR = null;
            currentRInitialValues_R = null;
            rightScaledObject = null;
            rightScaleSet = false; // Reset scale set flag
        }
    }

    public IEnumerator ApplyLineScaling(LineRenderer[] lineRenderers, int[] netIDs, float[] initialValues, Transform scaledObject)
    {
        while (true)
        {
            Vector3 scaleChange = scaledObject.localScale;
            if (scaledObject == leftScaledObject && leftScaleSet)
            {
                scaleChange = scaleChange.x / initialLeftScale.x * Vector3.one;
            }
            else if (scaledObject == rightScaledObject && rightScaleSet)
            {
                scaleChange = scaleChange.x / initialRightScale.x * Vector3.one;
            }

            for (int i = 0; i < lineRenderers.Length; i++)
            {
                // Apply proportional scaling based on the initial scale captured
                lineRenderers[i].widthMultiplier = initialValues[i] * scaleChange.x;
            }
            yield return null;
        }
    }








    public void ActivateObject(ActivateEventArgs aea)
    {
        Debug.Log("using: " + aea.interactorObject.transform.parent.name + "======  selected : " + aea.interactableObject.transform.name);
        //seet.interactorObject.interactablesSelected
        //  Debug.Log("Activated : " + aea.interactableObject.transform.name);

    }














}
