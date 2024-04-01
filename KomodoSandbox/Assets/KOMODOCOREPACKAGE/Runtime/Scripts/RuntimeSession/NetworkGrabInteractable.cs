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
    // Dictionary to track deselected objects with their Rigidbody for later removal.
 //   Dictionary<NetworkedGameObject, Rigidbody> deselectedObjects = new Dictionary<NetworkedGameObject, Rigidbody>();

    //private void FixedUpdate()
    //{
    //    var objectsToRemove = new List<NetworkedGameObject>();

    //    foreach (var pair in deselectedObjects)
    //    {
    //        NetworkedGameObject netObj = pair.Key;
    //        Rigidbody rb = pair.Value;

    //        // Check if the object can be considered "at rest"
    //        if (IsObjectAtRest(rb))
    //        {
    //            objectsToRemove.Add(netObj);
    //            MainClientUpdater.Instance.RemoveUpdatable(netObj);
    //        }
    //    }

    //    // Clean up objects that have been processed
    //    foreach (var obj in objectsToRemove)
    //    {
    //        deselectedObjects.Remove(obj);
    //    }
    //}

    // Checks if the Rigidbody can be considered "at rest"
    private bool ShouldTrackPhysics(Rigidbody rb)
    {
        if (rb == null && rb.isKinematic) 
            return false;

            return true;
        // Consider an object at rest if it's sleeping or if its velocity and angular velocity are below thresholds
    //    return rb.IsSleeping() || (rb.velocity.sqrMagnitude < 0.0001f && rb.angularVelocity.sqrMagnitude < 0.0001f);
    }

    public void SelectObject(SelectEnterEventArgs seet, NetworkedGameObject netObj)
    {
        Debug.Log($"using: {seet.interactorObject.transform.parent.name}======  selected : {seet.interactableObject.transform.name}");

        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;

        // Check which hand is grabbing the object and set the corresponding variable.
        if ("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor)
        {
            if (leftGrabbedObject == null || leftGrabbedObject != seet.interactableObject.transform)
            {
                leftGrabbedObject = seet.interactableObject.transform;
                // Only add to updatable if this object is not already being updated (grabbed by the other hand).
                if (rightGrabbedObject != leftGrabbedObject)
                {
                    MainClientUpdater.Instance.AddUpdatable(netObj);
                }
            }
        }
        else if ("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor)
        {
            if (rightGrabbedObject == null || rightGrabbedObject != seet.interactableObject.transform)
            {
                rightGrabbedObject = seet.interactableObject.transform;
                // Only add to updatable if this object is not already being updated (grabbed by the other hand).
                if (leftGrabbedObject != rightGrabbedObject)
                {
                    MainClientUpdater.Instance.AddUpdatable(netObj);
                }
            }
        }
    }

    public void DeselectObject(SelectExitEventArgs seet, NetworkedGameObject netObj)
    {
        Debug.Log($"Deselected: {seet.interactableObject.transform.name}");

        var parentNameOfInteractor = seet.interactorObject.transform.parent.name;
        Rigidbody rb = seet.interactableObject.transform.GetComponent<Rigidbody>();

        // Directly remove if the object doesn't require physics tracking
        if (ShouldTrackPhysics(rb))
        {
          //  NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(netObj);
            // Track the Rigidbody to monitor its rest state
           // deselectedObjects[netObj] = rb;
        }
        else
        {
        }
            MainClientUpdater.Instance.RemoveUpdatable(netObj);

        // Reset grab references
        if ("Left Controller" == parentNameOfInteractor || "Left Hand" == parentNameOfInteractor)
        {
            leftGrabbedObject = null;
        }
        else if ("Right Controller" == parentNameOfInteractor || "Right Hand" == parentNameOfInteractor)
        {
            rightGrabbedObject = null;
        }
    }



}
