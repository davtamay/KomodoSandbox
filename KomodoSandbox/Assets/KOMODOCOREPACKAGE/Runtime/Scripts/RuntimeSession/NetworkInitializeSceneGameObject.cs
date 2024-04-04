using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Komodo.Runtime;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

public class NetworkInitializeSceneGameObject : MonoBehaviour
{
    NetworkedGameObject networkedGameObject;
    private void Awake()
    {
        networkedGameObject = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(gameObject, customEntityID: gameObject.GetInstanceID(), modelType: MODEL_TYPE.Physics);

        EnsureRigidbodyExists();
        SetupGeneralGrab();
        SetupInteractable();
    }

    private void EnsureRigidbodyExists()
    {
        if (!gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
        }
    }

    public void OnDestroy()
    {
        //send to server that this object is destroyed
    }

    private void SetupGeneralGrab()
    {
        if (TryGetComponent<XRGeneralGrabTransformer>(out XRGeneralGrabTransformer GENGrab))
        {

        }
        else
        {
            GENGrab = gameObject.AddComponent<XRGeneralGrabTransformer>();
            GENGrab.constrainedAxisDisplacementMode = XRGeneralGrabTransformer.ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp;
            GENGrab.allowTwoHandedRotation = XRGeneralGrabTransformer.TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand;
            GENGrab.allowOneHandedScaling = true;
            GENGrab.allowTwoHandedScaling = true;
            GENGrab.clampScaling = false;
            GENGrab.maximumScaleRatio = 10;
            GENGrab.scaleMultiplier = 1;

        }
    }

    private void SetupInteractable()
    {
        XRGrabInteractable interactable = default;

        if (TryGetComponent<XRGrabInteractable>(out XRGrabInteractable inter))
        {
            interactable = inter;
        }
        else
        {
            interactable = gameObject.AddComponent<XRGrabInteractable>();
            interactable.selectMode = InteractableSelectMode.Multiple;
            interactable.useDynamicAttach = true;
        }

        interactable.selectEntered.AddListener((ctx) => NetworkGrabInteractable.Instance.SelectObject(ctx, networkedGameObject));
        interactable.selectExited.AddListener((ctx) => NetworkGrabInteractable.Instance.DeselectObject(ctx, networkedGameObject));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Determine the impact force of the collision
        float impactForce = collision.relativeVelocity.magnitude;

        // Define your threshold for "observable difference"
        float threshold = 1.0f; // Adjust this value based on your needs

        if (impactForce > threshold)
        {
           
            if (networkedGameObject != null)
            {
                // Add to the physics manager's list for network updates
                if (!NetworkedPhysicsManager.Instance.physics_networkedEntities.Contains(networkedGameObject))
                    NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(networkedGameObject);
            }
        }
    }
}
