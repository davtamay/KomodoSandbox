//#define TESTING_BEFORE_BUILDING

using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using Komodo.Utilities;

//namespace Komodo.Runtime
//{
    /// <summary>
    /// Used to establish gameobject as a reference to be used in networking
    /// </summary>
    //add interfaces to invoke eventsystem interactions (look start, look end) for our net objects
    public class NetworkedGameObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        //Register object to reference lists on clientspawnmanager to be refered to for synchronization
        [Tooltip("Entity_Data is created on Instantiate")]

        public bool usePhysics;

        [ShowOnly] [SerializeField] private bool isRegistered = false;

        //used to keep track of what UI element  does this object belongs to (for using with rendering and locking UI buttons)
        [ShowOnly] public int buttonIndex = -1;

        [ShowOnly] public int thisEntityID;

        private Rigidbody thisRigidBody;

        //entity used to access our data through entityManager
        public Entity entity;

        private EntityManager entityManager;

        public bool isTransformLocked;

        public void SetTransformLockedStated (bool isLocked)
        {
             isTransformLocked = isLocked;
        }
        public bool GetTransformLockedState() => isTransformLocked;

        public void Start()
        {
            //if we consider it a physics element we either get its rigidbody component, if it does not have one we add a new one
            InitializePhysicsComponentsIfNeeded();

           // yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            //InstantiateIfNeeded();
        }

        private void InstantiateIfNeeded()
        {
            //if this object was not instantiated early we make sure to instantiate it whenever it is active in scene
            if (isRegistered)
            {
                return;
            }

           // Debug.Log("INS IF NEEDED: ");

            Instantiate();
        }

        private void InitializePhysicsComponentsIfNeeded()
        {
            if (usePhysics)
            {
                thisRigidBody = GetComponent<Rigidbody>();

                if (!thisRigidBody)
                {
                    gameObject.AddComponent<Rigidbody>();
                }
            }
        }

        /// <summary>
        /// Instantiate this object to be referenced through the network
        /// </summary>
        /// <param name="importIndex"> used to assoicate our entity with a UI index to reference it with buttons</param>
        /// <param name="uniqueEntityID">if we give this paramater, we set it as the entity ID instead of giving it a default id</param>
        public void Instantiate(int importIndex = -1, int uniqueEntityID = -1)
        {
           // Debug.Log("ss: " + uniqueEntityID);
          
            
            //// get our entitymanager to get access to the entity world
            entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
         
            
            if (buttonIndex == -1)
                buttonIndex = importIndex;




            //if(uniqueEntityID == -1)
            //{
            //    NetworkedObjectsManager.Instance.net_GO_pendingRegistrationList.Enqueue(this);
            //    SocketIOJSLib.RequestUUIDFromServer();
            //}
            //else
            //{
            //    Register(uniqueEntityID);
            //}




//            //set custom id if we are not given a specified id when instantiating this network associated object
            int EntityID = (uniqueEntityID == -1) ? NetworkedObjectsManager.Instance.GenerateUniqueEntityID() : uniqueEntityID;

            if (this.thisEntityID == -1)
             this.thisEntityID = EntityID;


            Register(EntityID);

            //            thisEntityID = EntityID;

            //#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            ////do nothing
            //#else
            //            //entityManager.SetName(Entity, gameObject.name);
            //#endif
            //            if (buttonIndex == -1)
            //            buttonIndex = importIndex;


            //            NetworkedObjectsManager.Instance.Register(EntityID, this);

            //                //TODO: evaluate how good this solution is.
            //            //check to see if the gameObject is the main object or a subobject. If it's a main object, link it to the button.
            //            if (this.name == buttonIndex.ToString())
            //            {
            //                NetworkedObjectsManager.Instance.LinkNetObjectToButton(EntityID, this);
            //            }

            //            isRegistered = true;
        }

        public void Register(int uniqueEntityID)
        {
            //set custom id if we are not given a specified id when instantiating this network associated object
         //   int EntityID = uniqueEntityID;//(uniqueEntityID == -1) ? NetworkedObjectsManager.Instance.GenerateUniqueEntityID() : uniqueEntityID;

          //  thisEntityID = uniqueEntityID;

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
//do nothing
#else
            //entityManager.SetName(Entity, gameObject.name);
#endif
            //if (buttonIndex == -1)
            //    buttonIndex = importIndex;


            NetworkedObjectsManager.Instance.Register(uniqueEntityID, this);

            //TODO: evaluate how good this solution is.
            //check to see if the gameObject is the main object or a subobject. If it's a main object, link it to the button.
            if (this.name == buttonIndex.ToString())
            {
                NetworkedObjectsManager.Instance.LinkNetObjectToButton(uniqueEntityID, this);
            }

            isRegistered = true;


        }


        #region Physics Interaction Events (Add to network on collision)
        //if this object is a physics object detect when it collides to mark it to send its position information
        public void OnCollisionEnter(Collision collision)
        {
            //check if other object interacting has a rigidbody
            if (!collision.rigidbody)
            {
                return;
            }

            if (!usePhysics || !collision.rigidbody.CompareTag(TagList.interactable))
            {
                return;
            }

            if (!NetworkedPhysicsManager.Instance.physics_networkedEntities.Contains(this))
            {
                NetworkedPhysicsManager.Instance.physics_networkedEntities.Add(this);
            }

            if (entityManager.HasComponent<SendNetworkUpdateTag>(entity))
            {
                return;
            }

            entityManager.AddComponent<SendNetworkUpdateTag>(entity);
        }
        #endregion

        private void SendLookStartInteraction()
        {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(
               new Interaction
               {
                   interactionType = (int)INTERACTIONS.LOOK,

                   sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                   targetEntity_id = thisEntityID,
               }
            );
        }

        private void SendLookEndInteraction()
        {
            NetworkUpdateHandler.Instance.SendSyncInteractionMessage(
               new Interaction
               {
                   interactionType = (int)INTERACTIONS.LOOK_END,

                   sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                   targetEntity_id = thisEntityID,
               }
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            SendLookStartInteraction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SendLookEndInteraction();
        }
    }
//}