using Komodo.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NetworkGrabInteractable : SingletonComponent<NetworkGrabInteractable>
{
    public static NetworkGrabInteractable Instance
    {
        get { return ((NetworkGrabInteractable)_Instance); }
        set { _Instance = value; }
    }

    Transform leftGrabbedObject;
    Transform rightGrabbedObject;

    private bool ShouldTrackPhysics(Rigidbody rb)
    {
        // Corrected the condition to properly return false when rb is null or rb.isKinematic is true
        if (rb == null || rb.isKinematic)
            return false;

        return true;
    }

    public void SelectObject(SelectEnterEventArgs seet, NetworkedGameObject netObj = null, Group group = null)
    {
        Debug.Log($"using: {seet.interactorObject.transform.parent.name}======  selected : {seet.interactableObject.transform.name}");

        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;

        Rigidbody rb = seet.interactableObject.transform.GetComponent<Rigidbody>();

        // Check which hand is grabbing the object and set the corresponding variable.
        if ("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor)
        {
            leftGrabbedObject = seet.interactableObject.transform;
            // Check if the other hand is not holding the same object
            if (rightGrabbedObject == null || rightGrabbedObject.GetInstanceID() != leftGrabbedObject.GetInstanceID())
            {
               
                
                if (ShouldTrackPhysics(rb))
                {
                    NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(netObj);
                }
                else
                {
                   if(group)
                        ImpressMainClientUpdater.Instance.AddUpdatable(group);
                    else
                       MainClientUpdater.Instance.AddUpdatable(netObj);
                }
            }
        }
        else if ("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor)
        {
            rightGrabbedObject = seet.interactableObject.transform;
            // Check if the other hand is not holding the same object
            if (leftGrabbedObject == null || leftGrabbedObject.GetInstanceID() != rightGrabbedObject.GetInstanceID())
            {

                if (ShouldTrackPhysics(rb))
                {
                    NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(netObj);
                }
                else
                {
                    if (group)
                        ImpressMainClientUpdater.Instance.AddUpdatable(group);
                    else
                        MainClientUpdater.Instance.AddUpdatable(netObj);
                    //MainClientUpdater.Instance.AddUpdatable(netObj);
                }
                //MainClientUpdater.Instance.AddUpdatable(netObj);
            }
        }
    }

    public void DeselectObject(SelectExitEventArgs seet, NetworkedGameObject netObj = null, Group group = null)
    {
        Debug.Log($"Deselected: {seet.interactableObject.transform.name}");

        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;
        Rigidbody rb = seet.interactableObject.transform.GetComponent<Rigidbody>();

        // Determine which hand is releasing the object
        if (("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor) && leftGrabbedObject != null)
        {
            if (rightGrabbedObject == null || rightGrabbedObject.GetInstanceID() != leftGrabbedObject.GetInstanceID())
            {
                // Only remove from updatable if it's not being held by the other hand or if it's a different object
                UpdateUpdatableStatus(netObj, rb);
                
                if (group)
                    ImpressMainClientUpdater.Instance.RemoveUpdatable(group);
                else
                    MainClientUpdater.Instance.RemoveUpdatable(netObj);
            }
            leftGrabbedObject = null;
        }
        else if (("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor) && rightGrabbedObject != null)
        {
            if (leftGrabbedObject == null || leftGrabbedObject.GetInstanceID() != rightGrabbedObject.GetInstanceID())
            {
                // Only remove from updatable if it's not being held by the other hand or if it's a different object
                UpdateUpdatableStatus(netObj, rb);

                if (group)
                    ImpressMainClientUpdater.Instance.RemoveUpdatable(group);
                else
                    MainClientUpdater.Instance.RemoveUpdatable(netObj);

            }
            rightGrabbedObject = null;
        }
    }

    private void UpdateUpdatableStatus(NetworkedGameObject netObj, Rigidbody rb)
    {
           // MainClientUpdater.Instance.RemoveUpdatable(netObj);

        if (ShouldTrackPhysics(rb))
        {
         //   NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(netObj);
        }
       
        
    }
}
