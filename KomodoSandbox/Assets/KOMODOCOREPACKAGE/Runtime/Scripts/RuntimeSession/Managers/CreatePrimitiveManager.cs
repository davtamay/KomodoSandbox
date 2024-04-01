//using Komodo.Runtime;
using Komodo.Utilities;
using System;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using UnityEngine.XR.Interaction.Toolkit.Transformers;
//using Komodo.IMPRESS;


//namespace Komodo.Runtime
//{


public class CreatePrimitiveManager : SingletonComponent<CreatePrimitiveManager>
    {
        public static CreatePrimitiveManager Instance
        {
            get { return (CreatePrimitiveManager)_Instance; }
            set { _Instance = value; }
        }

        public Transform ghostPrimitivesParent;

        public GameObject ghostSphere;

        public GameObject ghostCapsule;

        public GameObject ghostCylinder;

        public GameObject ghostPlane;

        public GameObject ghostCube;

       // public Toggle[] toggleButtons;

        public ToggleGroup toggleGroup;

        public Toggle currentToggle;

        public ImpressPlayer player;

        public ToolPlacement toolPlacement;

        private bool _isRightHanded;

        EntityManager entityManager;

        Transform primitiveCreationParent;

        private TriggerCreatePrimitive _primitiveTrigger;

        private int _primitiveID = 0;

        private int _strokeIndex = 0;

        private bool _isEnabled;

        private UnityAction _enable;

        private UnityAction _disable;

        private UnityAction _setLeftHanded;

        private UnityAction _setRightHanded;

        private UnityAction _selectSphere;

        private UnityAction _selectCylinder;

        private UnityAction _selectCube;

        private UnityAction _selectPlane;

        private UnityAction _selectCapsule;

        private UnityAction _deselectSphere;

        private UnityAction _deselectCylinder;

        private UnityAction _deselectCube;

        private UnityAction _deselectPlane;

        private UnityAction _deselectCapsule;

        private PrimitiveType _currentType;

    public Material materialToUse;

        // DELAYED FEATURE
        // public void OnValidate ()
        // {
        //     if (ghostCapsule == null)
        //     {
        //         throw new MissingReferenceException("ghostCapsule");
        //     }

        //     if (ghostCube == null)
        //     {
        //         throw new MissingReferenceException("ghostCube");
        //     }

        //     if (ghostCylinder == null)
        //     {
        //         throw new MissingReferenceException("ghostCylinder");
        //     }

        //     if (ghostPlane == null)
        //     {
        //         throw new MissingReferenceException("ghostPlane");
        //     }

        //     if (ghostSphere == null)
        //     {
        //         throw new MissingReferenceException("ghostSphere");
        //     }

        //     if (toolPlacement == null)
        //     {
        //         throw new MissingReferenceException("toolPlacement");
        //     }
        // }

        public void Awake()
        {
            _isEnabled = false;

            _isRightHanded = false;

            // force-create an instance
            var initManager = Instance;

           // entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            primitiveCreationParent = new GameObject("CreatedPrimitives").transform;

            InitializeTriggerAndGhost();

            InitializeListeners();

            GlobalMessageManager.Instance.Subscribe("primitive", (str) => ReceivePrimitiveUpdate(str));
        }

        bool canSetPrimitives = true;

        public bool GetPrimitiveUpdateStatus() => canSetPrimitives;
        public void Set_Primitive_UPDATE(bool active)
        {
            canSetPrimitives = active;
        }
        public void InitializeTriggerAndGhost ()
        {
            //ToolAnchor anchor = toolPlacement.GetCurrentToolAnchor();

            //if (!anchor || !anchor.transform)
            //{
            //    Debug.LogWarning("Could not find a proper tool parent, so setting it to the screen by default.");

            //    toolPlacement.SetCurrentToolAnchor(ToolAnchor.Kind.SCREEN);

            //    anchor = toolPlacement.GetCurrentToolAnchor();
            //}

            //ghostPrimitivesParent.SetParent(anchor.transform, false);

            //ghostPrimitivesParent.transform.localPosition = new Vector3(0, 0, 0);

            //if (anchor.kind == ToolAnchor.Kind.RIGHT_HANDED)
            //{
            //    _primitiveTrigger = player.triggerCreatePrimitiveRight;

            //    return;
            //}

            //if (anchor.kind == ToolAnchor.Kind.LEFT_HANDED)
            //{
            //    _primitiveTrigger = player.triggerCreatePrimitiveLeft;

            //    return;
            //}

            // TODO - use a screen-based tool trigger here, instead of this left-handed trigger.

            _primitiveTrigger = player.triggerCreatePrimitiveLeft;
        }

        private void _Enable ()
        {
            _isEnabled = true;

            InitializeTriggerAndGhost();

            _ToggleGhostPrimitive(true);

            _primitiveTrigger.gameObject.SetActive(true);
        }

        private void _Disable ()
        {
            _isEnabled = false;

            _currentType = default;

            InitializeTriggerAndGhost();

            _ToggleGhostPrimitive(false);

            _primitiveTrigger.gameObject.SetActive(false);
        }

        private void _DeselectSphere ()
        {
            ghostSphere.SetActive(false);
        }

        private void _DeselectCapsule ()
        {
            ghostCapsule.SetActive(false);
        }

        private void _DeselectCube ()
        {
            ghostCube.SetActive(false);
        }

        private void _DeselectPlane ()
        {
            ghostPlane.SetActive(false);
        }

        private void _DeselectCylinder ()
        {
            ghostCylinder.SetActive(false);
        }

        private void _SelectSphere ()
        {
            DeactivateAllChildren();

            _currentType = PrimitiveType.Sphere;

            ghostSphere.SetActive(true);
        }

        private void _SelectCapsule ()
        {
            DeactivateAllChildren();

            _currentType = PrimitiveType.Capsule;

            ghostCapsule.SetActive(true);
        }

        private void _SelectCube ()
        {
            DeactivateAllChildren();

            _currentType = PrimitiveType.Cube;

            ghostCube.SetActive(true);
        }

        private void _SelectPlane ()
        {
            DeactivateAllChildren();

            _currentType = PrimitiveType.Plane;

            ghostPlane.SetActive(true);
        }

        private void _SelectCylinder ()
        {
            DeactivateAllChildren();

            _currentType = PrimitiveType.Cylinder;

            ghostCylinder.SetActive(true);
        }


        public void SelectSphere()
        {
            ImpressEventManager.TriggerEvent("primitiveTool.selectSphere");
            //_currentType = PrimitiveType.Sphere;

            //ghostSphere.SetActive(true);
        }

        public void SelectCapsule()
        {
            ImpressEventManager.TriggerEvent("primitiveTool.selectCapsule");
            //_currentType = PrimitiveType.Capsule;

            //ghostCapsule.SetActive(true);
        }

        public void SelectCube()
        {
            ImpressEventManager.TriggerEvent("primitiveTool.selectCube");
            //_currentType = PrimitiveType.Cube;

            //ghostCube.SetActive(true);
        }

        public void SelectPlane()
        {
            ImpressEventManager.TriggerEvent("primitiveTool.selectPlane");
            //_currentType = PrimitiveType.Plane;

            //ghostPlane.SetActive(true);
        }

        public void SelectCylinder()
        {
            ImpressEventManager.TriggerEvent("primitiveTool.selectCylinder");
           
        }

        public void InitializeListeners ()
        {
            _enable += _Enable;

            ImpressEventManager.StartListening("primitiveTool.enable", _enable);

            _disable += _Disable;

            ImpressEventManager.StartListening("primitiveTool.disable", _disable);

            _selectSphere += _SelectSphere;

            ImpressEventManager.StartListening("primitiveTool.selectSphere", _selectSphere);

            _selectCapsule += _SelectCapsule;

            ImpressEventManager.StartListening("primitiveTool.selectCapsule", _selectCapsule);

            _selectCube += _SelectCube;

            ImpressEventManager.StartListening("primitiveTool.selectCube", _selectCube);

            _selectPlane += _SelectPlane;

            ImpressEventManager.StartListening("primitiveTool.selectPlane", _selectPlane);

            _selectCylinder += _SelectCylinder;

            ImpressEventManager.StartListening("primitiveTool.selectCylinder", _selectCylinder);

            _deselectSphere += _DeselectSphere;

            ImpressEventManager.StartListening("primitiveTool.deselectSphere", _deselectSphere);

            _deselectCapsule += _DeselectCapsule;

            ImpressEventManager.StartListening("primitiveTool.deselectCapsule", _deselectCapsule);

            _deselectCube += _DeselectCube;

            ImpressEventManager.StartListening("primitiveTool.deselectCube", _deselectCube);

            _deselectPlane += _DeselectPlane;

            ImpressEventManager.StartListening("primitiveTool.deselectPlane", _deselectPlane);

            _deselectCylinder += _DeselectCylinder;

            ImpressEventManager.StartListening("primitiveTool.deselectCylinder", _deselectCylinder);
        }

        private void TogglePrimitive (bool state, int index)
        {
            GetCurrentToggle(state);

            DeactivateAllChildren();

            ghostPrimitivesParent.GetChild(index).gameObject.SetActive(true);

            _primitiveTrigger.gameObject.SetActive(state);
        }

        private void _ToggleGhostPrimitive (bool state)
        {
            ghostPrimitivesParent.gameObject.SetActive(state);
        }

        public void GetCurrentToggle (bool state)
        {
            currentToggle = toggleGroup.GetFirstActiveToggle();
        }

        public void DeactivateAllChildren ()
        {
            foreach (Transform item in ghostPrimitivesParent)
            {
                item.gameObject.SetActive(false);
            }
        }

        public GameObject GetGhostPrimitive (PrimitiveType type)
        {
            if (type == PrimitiveType.Sphere)
            {

                return ghostSphere;
            }

            if (type == PrimitiveType.Cube)
            {
                return ghostCube;
            }

            if (type == PrimitiveType.Capsule)
            {
                return ghostCapsule;
            }

            if (type == PrimitiveType.Cylinder)
            {
                return ghostCylinder;
            }

            if (type == PrimitiveType.Plane)
            {
                return ghostPlane;
            }

            return null;
        }

        public GameObject CreatePrimitive ()
        {
            GameObject primitive = GetGhostPrimitive(_currentType);

            var rot = primitive.transform.rotation;

            var scale = primitive.transform.lossyScale;

            primitive = GameObject.CreatePrimitive(_currentType);

             primitive.GetComponent<MeshRenderer>().sharedMaterial = materialToUse;

            //add a box collider instead, cant grab objects with different colliders in WebGL build for some reason
            primitive.TryGetComponent<Collider>(out Collider col);

            Destroy(col);

            primitive.AddComponent<BoxCollider>();

      


            // //tag it to be used with ECS system
            // entityManager.AddComponentData(netObject.Entity, new PrimitiveTag());

            primitive.tag = "Interactable";

            primitive.transform.position = ghostPrimitivesParent.position;

            primitive.transform.SetGlobalScale(scale);

            primitive.transform.rotation = rot;

            primitive.transform.SetParent(primitiveCreationParent.transform, true);

              Guid myGUID = Guid.NewGuid();

             _primitiveID = myGUID.GetHashCode();//100000000 + 10000000 + (NetworkUpdateHandler.Instance.client_id * 10000) + _strokeIndex;


            var netObject = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(primitive, customEntityID: _primitiveID, modelType: MODEL_TYPE.Primitive);

        SetupXRToolkitGrabbable(netObject);

        Debug.Log("PRIMITIVE: " + netObject.transform.position + "  S:" + netObject.transform.localScale + "A : " + netObject.gameObject.activeInHierarchy  + "mat" + netObject.GetComponentInChildren<Renderer>().material.name);


            var rot2 = primitive.transform.rotation;

            SendPrimitiveUpdate(
                _primitiveID,
                (int)_currentType,
                primitive.transform.lossyScale.x,
                primitive.transform.position,
                new Vector4(rot2.x, rot2.y, rot2.z, rot2.w)
            );

            if (UndoRedoManager.IsAlive)
            {
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    primitive.SetActive(false);

                    //send network update call for everyone else
                    SendPrimitiveUpdate(_primitiveID, -9);
                });
            }

            return primitive;
        }

    public void SetupXRToolkitGrabbable(NetworkedGameObject nRGO)
    {

        nRGO.gameObject.AddComponent<Rigidbody>().isKinematic = true;
        var GENGrab = nRGO.gameObject.AddComponent<XRGeneralGrabTransformer>();
        GENGrab.constrainedAxisDisplacementMode = XRGeneralGrabTransformer.ConstrainedAxisDisplacementMode.ObjectRelativeWithLockedWorldUp;
        GENGrab.allowTwoHandedRotation = XRGeneralGrabTransformer.TwoHandedRotationMode.FirstHandDirectedTowardsSecondHand;
        GENGrab.allowOneHandedScaling = true;
        GENGrab.allowTwoHandedScaling = true;
        GENGrab.clampScaling = false;
        GENGrab.maximumScaleRatio = 10;
        GENGrab.scaleMultiplier = 1;

        var Interactable = nRGO.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        Interactable.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;
        Interactable.useDynamicAttach = true;

        Interactable.colliders.RemoveAt(0);


        Interactable.selectEntered.AddListener((ctx) => {

            NetworkGrabInteractable.Instance.SelectObject(ctx, nRGO);
            //MainClientUpdater.Instance.AddUpdatable(nRGO);

        });

        Interactable.selectExited.AddListener((ctx) => {

            NetworkGrabInteractable.Instance.DeselectObject(ctx, nRGO);
            //MainClientUpdater.Instance.RemoveUpdatable(nRGO);

        });
        //Interactable.selectEntered.AddListener((ctx) => { testGrabInteraction.SelectObject(ctx); });

        //Interactable.selectExited.AddListener((ctx) => { testGrabInteraction.DeselectObject(ctx); });

    }

    public void SendPrimitiveUpdate(int sID, int primitiveType, float scale = 1, Vector3 primitivePos = default, Vector4 primitiveRot = default)
        {
            var primitiveUpdate = new Primitive(
                (int) NetworkUpdateHandler.Instance.client_id,
                sID,
                (int) primitiveType,
                scale,
                primitivePos,
                primitiveRot
            );

            var primSer = JsonUtility.ToJson(primitiveUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("primitive", primSer);

            komodoMessage.Send();
        }

        public void ReceivePrimitiveUpdate(string stringData, bool receivingCall = false)
        {
            Primitive newData = JsonUtility.FromJson<Primitive>(stringData);


        Debug.Log("GOT NEW PRIMITIVE DATA : " + newData.guid);

            //detect if we should render or notrender it
            if (newData.indentifier == 9)
            {
                if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.guid))
                {
                    NetworkedObjectsManager.Instance.networkedObjectFromEntityId[newData.guid].gameObject.SetActive(true);
                }

                return;
            }
            else if (newData.indentifier == -9)
            {
                if (NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(newData.guid))
                {
                    NetworkedObjectsManager.Instance.networkedObjectFromEntityId[newData.guid].gameObject.SetActive(false);
                }

                return;
            }
            

            PrimitiveType primitiveToInstantiate = PrimitiveType.Sphere;

                switch (newData.indentifier)
                {
                    case 0:
                        primitiveToInstantiate = PrimitiveType.Sphere;
                        break;

                    case 1:
                        primitiveToInstantiate = PrimitiveType.Capsule;
                        break;

                    case 2:
                        primitiveToInstantiate = PrimitiveType.Cylinder;
                        break;

                    case 3:
                        primitiveToInstantiate = PrimitiveType.Cube;
                        break;

                    case 4:
                        primitiveToInstantiate = PrimitiveType.Plane;
                        break;

                    case 5:
                        primitiveToInstantiate = PrimitiveType.Quad;
                        break;
                }

            var primitive = GameObject.CreatePrimitive(primitiveToInstantiate);

            NetworkedGameObject nAGO = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(primitive, customEntityID: newData.guid, modelType: MODEL_TYPE.Primitive);

           // entityManager.AddComponentData(nAGO.entity, new PrimitiveTag { });

            primitive.tag = "Interactable";

            var pos = newData.pos;

            var rot = newData.rot;

            var scale = newData.scaleFactor;

            primitive.transform.position = pos;

            primitive.transform.SetGlobalScale(Vector3.one * scale);

            primitive.transform.rotation = new Quaternion(rot.x, rot.y, rot.z, rot.w);

            primitive.transform.SetParent(primitiveCreationParent.transform, true);


        if (receivingCall)
            GameStateManager.Instance.isAssetImportFinished = true;
    }
}

   
//}
