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

    //public ActionBasedController l_Controller_ActionBasedController;
    //public ActionBasedController r_Controller_ActionBasedController;
    // Reference to input actions for left and right controllers
    public InputActionReference lController_PositionAction;
    public InputActionReference lController_RotationAction;
 
    public InputActionReference rController_PositionAction;
    public InputActionReference rController_RotationAction;


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

    public Quaternion ReturnCurrentActiveInputRotation_LEFT()
    {
        if (isUsingHands)
        {
            return lHand.transform.rotation;
        }
        else if (isUsingControls)
        {
            return lControler.transform.rotation;
        }
        else
            return Quaternion.identity;

    }
    public Quaternion ReturnCurrentActiveInputRotation_Right()
    {
        if (isUsingHands)
        {
            return rHand.transform.rotation;
        }
        else if (isUsingControls)
        {
            return rControler.transform.rotation;
        }
        else
            return Quaternion.identity;

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
            return lControler.transform;

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
            return rControler.transform; 

    }
    public void Start()
    {
        lController_PositionAction.action.performed += (c) => { lControler_PosData.pos = c.ReadValue<Vector3>(); };
        lController_RotationAction.action.performed += (c) => { lControler_PosData.rot = c.ReadValue<Quaternion>(); };

        rController_PositionAction.action.performed += (c) => { rControler_PosData.pos = c.ReadValue<Vector3>(); };
        rController_RotationAction.action.performed += (c) => { rControler_PosData.rot = c.ReadValue<Quaternion>(); };
        //l_Controller_ActionBasedController.positionAction.action.performed += (c) => { lControler_PosData.pos = c.ReadValue<Vector3>();  };
        //l_Controller_ActionBasedController.rotationAction.action.performed += (c) => { lControler_PosData.rot = c.ReadValue<Quaternion>();  };

        //r_Controller_ActionBasedController.positionAction.action.performed += (c) => { rControler_PosData.pos = c.ReadValue<Vector3>(); };
        //r_Controller_ActionBasedController.rotationAction.action.performed += (c) => { rControler_PosData.rot = c.ReadValue<Quaternion>(); };

    }
  

  
}
