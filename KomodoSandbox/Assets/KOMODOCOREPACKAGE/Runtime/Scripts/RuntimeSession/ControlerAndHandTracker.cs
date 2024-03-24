using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

[System.Serializable]
public struct PoseData
{
    public Vector3 pos;
    public Quaternion rot;
    
}
public class ControlerAndHandTracker : MonoBehaviour
{
    public PoseData lControler_PosData;
    public PoseData rControler_PosData;

    public ActionBasedController l_Controller_ActionBasedController;
    public ActionBasedController r_Controller_ActionBasedController;

    public GameObject lControler;
    public GameObject rControler;

    public GameObject lHand;
    public GameObject rHand;

    public bool isUsingHands;
    public bool isUsingControls;
    public void ActivateHandModality()
    {
        isUsingHands = true;
    }
    public void DeactivateHandModality()
    {
        isUsingHands = false;
    }

    public void ActivateControlerModality()
    {
        isUsingControls = true;
    }
    public void DeactivateControlerModality()
    {
        isUsingControls = false;
    }

    public Vector3 ReturnCurrentActiveInputPosition_LEFT()
    {
        if (isUsingHands)
        {
            return lHand.transform.position;
        }
        else if (isUsingControls)
        {
            return lControler.transform.position;
        }
        else
            return Vector3.zero;

    }
    public Vector3 ReturnCurrentActiveInputPosition_Right()
    {
        if (isUsingHands)
        {
            return rHand.transform.position;
        }
        else if (isUsingControls)
        {
            return rControler.transform.position;
        }
        else
            return Vector3.zero;

    }

    public Transform ReturnCurrentActiveInputTransform_LEFT()
    {
        if (isUsingHands)
        {
            return lHand.transform;
        }
        else if (isUsingControls)
        {
            return lControler.transform;
        }
        else
            return null;

    }
    public Transform ReturnCurrentActiveInputTransform_Right()
    {
        if (isUsingHands)
        {
            return rHand.transform;
        }
        else if (isUsingControls)
        {
            return rControler.transform;
        }
        else
            return null ;

    }
    public void Start()
    {
        l_Controller_ActionBasedController.positionAction.action.performed += (c) => { lControler_PosData.pos = c.ReadValue<Vector3>();  };
        l_Controller_ActionBasedController.rotationAction.action.performed += (c) => { lControler_PosData.rot = c.ReadValue<Quaternion>();  };

        r_Controller_ActionBasedController.positionAction.action.performed += (c) => { rControler_PosData.pos = c.ReadValue<Vector3>(); };
        r_Controller_ActionBasedController.rotationAction.action.performed += (c) => { rControler_PosData.rot = c.ReadValue<Quaternion>(); };

    }
  

  
}
