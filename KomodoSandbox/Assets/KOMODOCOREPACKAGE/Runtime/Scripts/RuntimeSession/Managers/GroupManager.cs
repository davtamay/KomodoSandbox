using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;
using JetBrains.Annotations;
using UnityEngine.XR.Interaction.Toolkit.Transformers;


//namespace Komodo.IMPRESS
//{
public struct GroupProperties
    {
        public int groupID;
        public int entityID;
        public bool isAdding;
    }

    public enum GroupColor
    {
        RED,
        BLUE,

    }

    public class GroupManager : SingletonComponent<GroupManager>
    {
        public static GroupManager Instance
        {
            get { return (GroupManager)_Instance; }
            set { _Instance = value; }
        }

        private BoxCollider currentRootCollider;

        private UnityAction _showGroups;

        private UnityAction _hideGroups;

        private UnityAction _enableGrouping;

        private UnityAction _disableGrouping;

        private UnityAction _enableUngrouping;

        private UnityAction _disableUngrouping;

        private UnityAction _selectRed;

        private UnityAction _selectBlue;

        private Group currentGroup;

        private int _currentGroupType;

        public ImpressControllerInteraction leftControllerInteraction;

        public ImpressControllerInteraction rightControllerInteraction;

        public ImpressPlayer player;

        public GameObject groupBoundingBox;

        public Dictionary<int, Group> groups = new Dictionary<int, Group>();

        public Dictionary<int, Group> clientIDToGroup = new Dictionary<int, Group>();
        public void Awake()
        {
            //used to set our managers alive state to true to detect if it exist within scene
            var initManager = Instance;

            //register our message with the funcion that will be receiving updates from others
            GlobalMessageManager.Instance.Subscribe("group", (str) => ReceiveGroupUpdate(str));
        }

        public void Start()
        {
            if (!leftControllerInteraction)
            {
                throw new UnassignedReferenceException("LeftControllerInteraction");
            }

            if (!rightControllerInteraction)
            {
                throw new UnassignedReferenceException("RightControllerInteraction");
            }

            if (!player)
            {
                throw new UnassignedReferenceException("player");
            }

            if (!groupBoundingBox)
            {
                throw new UnassignedReferenceException("groupBoundingBox");
            }

            _showGroups += ShowGroups;

            ImpressEventManager.StartListening("groupTool.showGroups", _showGroups);

            _hideGroups += HideGroups;

            ImpressEventManager.StartListening("groupTool.hideGroups", _hideGroups);

            _enableGrouping += _EnableGrouping;

            ImpressEventManager.StartListening("groupTool.enableGrouping", _enableGrouping);

            _disableGrouping += _DisableGrouping;

            ImpressEventManager.StartListening("groupTool.disableGrouping", _disableGrouping);

            _enableUngrouping += _EnableUngrouping;

            ImpressEventManager.StartListening("groupTool.enableUngrouping", _enableUngrouping);

            _disableUngrouping += _DisableUngrouping;

            ImpressEventManager.StartListening("groupTool.disableUngrouping", _disableUngrouping);

            _selectRed += _SelectRed;

            ImpressEventManager.StartListening("groupTool.selectRed", _selectRed);

            _selectBlue += _SelectBlue;

            ImpressEventManager.StartListening("groupTool.selectBlue", _selectBlue);
        }

        private void _EnableGrouping()
        {
            player.triggerGroupLeft.gameObject.SetActive(true);

            player.triggerGroupRight.gameObject.SetActive(true);
        }

        private void _DisableGrouping()
        {
            player.triggerGroupLeft.gameObject.SetActive(false);

            player.triggerGroupRight.gameObject.SetActive(false);
        }

        private void _EnableUngrouping()
        {
            player.triggerUngroupLeft.gameObject.SetActive(true);

            player.triggerUngroupRight.gameObject.SetActive(true);
        }

        private void _DisableUngrouping()
        {
            player.triggerUngroupLeft.gameObject.SetActive(false);

            player.triggerUngroupRight.gameObject.SetActive(false);
        }

        /// <summary>
        /// Show and hide bounding boxes, which represent groups.
        /// </summary>
        public void ShowGroups()
        {
            //foreach (var item in groups.Values)
            //{
            //    item.GetComponent<MeshRenderer>().enabled = true;
            //}
        }

        public void HideGroups()
        {
            //foreach (var item in groups.Values)
            //{
            //    item.transform.GetComponent<BoxCollider>().enabled = true;

            //    foreach (Transform colItems in item.transform.GetChild(0))
            //    {
            //        colItems.GetComponent<BoxCollider>().enabled = false;
            //    }

            //    item.GetComponent<MeshRenderer>().enabled = false;
            //}
        }

        private void _SelectRed()
        {
            this._currentGroupType = (int)GroupColor.RED;
        }

        private void _SelectBlue()
        {
            this._currentGroupType = (int)GroupColor.BLUE;
        }

        private Group _CreateNewGroup(int groupID)
        {
            currentGroup = Instantiate(groupBoundingBox).AddComponent<Group>();


        SetupXRToolkitGrabbable(currentGroup);

            //make net component
            //  NetworkedObjectsManager.Instance.CreateNetworkedGameObject(currentGroup.gameObject);
            //   currentGroup.gameObject.tag = "group";

        //make child of parent to contain our grouped objects
            currentGroup.groupsParent = new GameObject("Linker Parent").transform;

            var mat = currentGroup.GetComponent<MeshRenderer>().material;

            Color color = Color.red;

            if (groupID == (int)GroupColor.RED)
            {
                color = Color.red;
            }
            else if (groupID == (int)GroupColor.BLUE)
            {
                color = Color.red;
            }
            else if (groupID == 2)
            {
                color = Color.red;
            }
            else if (groupID == 3)
            {
                color = Color.red;
            }

            color.a = 0.39f;

            mat.SetColor("_Color", color);

            currentGroup.GetComponent<MeshRenderer>().material = mat;

            //add new item to our dictionary and collection
            groups.Add(groupID, currentGroup);

            currentGroup.groups = new List<Collider>();

        return currentGroup;
        }

    UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable currentGroupInteractable;

    public LineRenderingScaleToWidthAdjustment lineRenderingScaleToWidthAdjustment;
    public void SetupXRToolkitGrabbable(Group nRGO)
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

        currentGroupInteractable = nRGO.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        currentGroupInteractable.selectMode = UnityEngine.XR.Interaction.Toolkit.Interactables.InteractableSelectMode.Multiple;
        currentGroupInteractable.useDynamicAttach = true;
        //var Interactable = nRGO.gameObject.AddComponent<XRGrabInteractable>();
        //Interactable.selectMode = InteractableSelectMode.Multiple;
        //Interactable.useDynamicAttach = true;

        currentGroupInteractable.selectEntered.AddListener((ctx) => { lineRenderingScaleToWidthAdjustment.SelectObject(ctx); });

        currentGroupInteractable.selectExited.AddListener((ctx) => { lineRenderingScaleToWidthAdjustment.DeselectObject(ctx); });

    }

    private void _UpdateGroupBounds()
        {
            //eliminate all objects that are not visible (erase/undo sideeffect)
            for (int i = 0; i < currentGroup.groups.Count; i += 1)
            {
                var _collider = currentGroup.groups[i];

                if (!_collider.gameObject.activeInHierarchy)
                {
                    currentGroup.groups.Remove(_collider);
                }
            }


            //remove our children before we create a new bounding box for our parent containing the group
            currentGroup.transform.DetachChildren();

            currentGroup.groupsParent.transform.DetachChildren();

            //establish new bounding box
            var newBound = new Bounds(currentGroup.groups[0].transform.position, Vector3.one * 0.02f);

            for (int i = 0; i < currentGroup.groups.Count; i += 1)
            {
                var _collider = currentGroup.groups[i];

                //turn it on to get bounds info 
                _collider.enabled = true;

                //set new collider bounds
                newBound.Encapsulate(new Bounds(_collider.transform.position, _collider.bounds.size));

                _collider.enabled = false;
            }

            //set fields for our new created bounding box
            currentGroup.transform.position = newBound.center;

            currentGroup.transform.SetGlobalScale(newBound.size);

            currentGroup.transform.rotation = Quaternion.identity;

            currentGroup.groupsParent.rotation = Quaternion.identity;

            //recreate our collider to be consistent with the new render bounds
            if (currentGroup.TryGetComponent(out BoxCollider boxCollider))
            {
            currentGroupInteractable.colliders.Remove(boxCollider);
            currentGroupInteractable.enabled = false;

            Destroy(boxCollider);
            }
       

        //  if (!currentGroup.TryGetComponent(out BoxCollider bC))
        currentRootCollider = currentGroup.gameObject.AddComponent<BoxCollider>();

        currentGroupInteractable.colliders.Add(currentRootCollider);


        currentGroupInteractable.enabled = true;

        //currentGroupInteractable.


        //add children again
        foreach (var item in currentGroup.groups)
            {
                item.transform.SetParent(currentGroup.groupsParent, true);
            }

            //add our collection parent to our bounding box parent
            currentGroup.groupsParent.SetParent(currentGroup.transform, true);
        }



        /// <summary>
        /// Add an object to a specific group
        /// </summary>
        /// <param name="collider"> The collider of the adding elemen</param>
        /// <param name="otherClientID">customID should not be included or should be -1 to use the userID instead for identifying what group to add to</param>
        public void AddToGroup(Collider collider, int otherClientID = -1)
        {
            if (!collider.CompareTag("Interactable"))
            {
                return;
            }
        
            

            foreach (KeyValuePair<int, Group> candidateGroup in groups)
            {
                //we do not want to add the constructing boxes to itself
                if (candidateGroup.Value.gameObject.GetInstanceID() == collider.gameObject.GetInstanceID())
                {
                    return;
                }
            }

            // check if this is a call from the user or an external client
            int groupID = NetworkUpdateHandler.Instance.client_id;//GroupIDFromClientIDOrGroupType(otherClientID);


        Group currentGroup;
            if (!groups.ContainsKey(groupID))
            {
            currentGroup = _CreateNewGroup(groupID);
            }

            currentGroup = groups[groupID];

         //   Debug.Log("UNDERGROUP : " + currentGroup.groups.Count);

            if (currentGroup.groups.Contains(collider))
            {
                return; // if the group already has this collider, stop.
            }

            leftControllerInteraction.EndGrab();
            rightControllerInteraction.EndGrab();

            currentGroup.groups.Add(collider);

            _UpdateGroupBounds();

            _DropGroupedObject(collider);



        //new
        if (!clientIDToGroup.ContainsKey(groupID))
        {
            clientIDToGroup.Add(groupID, currentGroup );


            //Debug.Log("ADDING NEW COLLIDER");
        }
       
            clientIDToGroup[groupID].netObjectList.Add(collider.GetComponent<NetworkedGameObject>());

        }

        // Tells the controller to stop holding the object represented by collider.
        private void _DropGroupedObject(Collider collider)
        {
            leftControllerInteraction.EndGrab();
            rightControllerInteraction.EndGrab();
            /* TODO FIX THIS TO WORK WITH KOMODOCORE v0.5.4

              leftControllerInteraction.EndGrab();
                rightControllerInteraction.EndGrab();
            //only set our grab object to the group object when it is a different object than its own to avoid the object reparenting itself
            if (leftControllerInteraction.thisHandTransform != null
                && leftControllerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID())
            {
                leftControllerInteraction.Drop();
            }

            if (rightControllerInteraction.currentTransform != null
                && rightControllerInteraction.currentTransform.GetInstanceID() == collider.transform.GetInstanceID())
            {
                rightControllerInteraction.Drop();
            }
            */

        }

        //public void SendGroupUpdate(int _entityID, int _groupID, bool isAdding)
        //{
        //    KomodoMessage km = new KomodoMessage("group", JsonUtility.ToJson(
        //        new GroupProperties
        //        {
        //            entityID = _entityID,
        //            groupID = _groupID,
        //            isAdding = isAdding
        //        })
        //    );

        //    km.Send();
        //}

        public void RemoveFromGroup(Collider collider)
        {
            //if (!collider.CompareTag("Interactable"))
            //    return;



            //to check if this is a call from the client or external clients
            int groupID = NetworkUpdateHandler.Instance.client_id;//_currentGroupType;

            //disable main collider of the linkedgroup to access its child group item colliders
            if (collider.TryGetComponent(out Group lg))
            {

                currentGroup = lg;

                //disable main collider and enable the child elements to remove from main
                lg.transform.GetComponent<BoxCollider>().enabled = false;

                lg.meshRenderer = lg.GetComponent<MeshRenderer>();

                StartCoroutine(ReenableParentColliderOutsideRenderBounds(lg.meshRenderer));

                foreach (Transform colItems in lg.transform.GetChild(0))
                {
                    colItems.GetComponent<BoxCollider>().enabled = true;
                }

                return;
            }

            //if (currentGroup && currentGroup.groupsParent)
            //    if (!collider.transform.IsChildOf(currentGroup.groupsParent))
            //    {
            //        return;
            //    }


            //if we dont drop our grouped object from our hand when we remove a collider we get issues with collider loosing its children while still visible
            leftControllerInteraction.EndGrab();
            rightControllerInteraction.EndGrab();

            collider.enabled = true;

            if (!groups.ContainsKey(groupID))
            {
                return;
            }

            currentGroup = groups[groupID];


            //handle the items that we touch
            if (collider.transform.IsChildOf(currentGroup.groupsParent))
            {
                collider.transform.parent = null;

            }

            Debug.Log("UNDERGROUP : " + currentGroup.groups.Count);

            if (currentGroup.groups.Count > 2)
            {
                //  UpdateGroup(currentGroup);


                currentGroup.groups.Remove(collider);
                UpdateGroup(currentGroup);

            }
            else
            {


                //new
                if (ImpressStretchManager.Instance.firstObjectGrabbed)
                    if (ImpressStretchManager.Instance.firstObjectGrabbed.TryGetComponent(out BoxCollider col))
                    {
                        col.enabled = true;
                    }

                if (ImpressStretchManager.Instance.secondObjectGrabbed)
                    if (ImpressStretchManager.Instance.secondObjectGrabbed.TryGetComponent(out BoxCollider col))
                    {
                        col.enabled = true;
                    }
                foreach (var item in currentGroup.groupsParent)
                {
                    if (TryGetComponent(out BoxCollider col))
                        col.enabled = true;
                }
                //

                //leftControllerInteraction.EndGrab();
                //rightControllerInteraction.EndGrab();


                UpdateGroup(currentGroup);


                currentGroup.groupsParent.DetachChildren();


                currentGroup.groups.Remove(collider);
                groups.Remove(groupID);


                foreach (var item in currentGroup.groups)
                {
                    item.enabled = true;
                }




                Destroy(currentGroup.groupsParent.gameObject);
                Destroy(currentGroup.gameObject);
            }


            Debug.Log("GROUP LENGTH COUNT : " + currentGroup.groups.Count);


            if (clientIDToGroup.ContainsKey(groupID))
            {

                if (clientIDToGroup[groupID].netObjectList.Contains(collider.GetComponent<NetworkedGameObject>()))
                    clientIDToGroup[groupID].netObjectList.Remove(collider.GetComponent<NetworkedGameObject>());
            }


            //   _UpdateGroupBounds();
        }



        public void ExitGroup(Group lg)
        {
            if (lg)
            {
                lg.transform.GetComponent<BoxCollider>().enabled = true;

                foreach (Transform colItems in lg.transform.GetChild(0))
                {
                    colItems.GetComponent<Collider>().enabled = false;
                }
            }
        }

        public IEnumerator ReenableParentColliderOutsideRenderBounds(MeshRenderer lgRend)
        {
            yield return new WaitUntil(() =>
            {
                if (lgRend == null)
                {
                    return true;
                }

                if (!lgRend.bounds.Contains(leftControllerInteraction.transform.position) &&
                    !lgRend.bounds.Contains(rightControllerInteraction.transform.position))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            });

            if (lgRend == null)
            {
                yield break;
            }

            lgRend.transform.GetComponent<BoxCollider>().enabled = true;

            foreach (Transform colItems in lgRend.transform.GetChild(0))
            {
                colItems.GetComponent<Collider>().enabled = false;
            }
        }

        public void ReceiveGroupUpdate(string message)
        {
            GroupProperties data = JsonUtility.FromJson<GroupProperties>(message);

            // TODO FIX THIS TO WORK WITH KOMODOCORE v0.5.4
            if (data.isAdding)
            {
                //if (!clientIDToGroup.ContainsKey(data.groupID))
                //    clientIDToGroup.Add(data.groupID, new Group { netObjectList = new List<NetworkedGameObject>() { NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID] } });
                //else
                //    clientIDToGroup[data.groupID].netObjectList.Add(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID]);


                //AddToGroup(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);
            }
            else
            {
                //if (clientIDToGroup.ContainsKey(data.groupID))
                //{
                //    if (clientIDToGroup[data.groupID].netObjectList.Contains(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID]))
                //        clientIDToGroup[data.groupID].netObjectList.Remove(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID]);
                //}
                //     clientIDToGroup.Add(data.groupID, new Group { netObjectList = new List<NetworkedGameObject>() { NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID] } });
                //else
                //    clientIDToGroup[data.groupID].netObjectList.Add(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID]);

                // RemoveFromGroup_EXTERNAL(NetworkedObjectsManager.Instance.networkedObjectFromEntityId[data.entityID].GetComponent<Collider>(), data.groupID);
            }

        }

        public void UpdateGroup(Group linkParent)
        {

            //eliminate all objects that are not visible (erase/undo sideeffect)
            for (int i = 0; i < currentGroup.groups.Count; i += 1)
            {
                var _collider = currentGroup.groups[i];

                if (!_collider.gameObject.activeInHierarchy)
                {
                    currentGroup.groups.Remove(_collider);
                }
            }


            List<Transform> childList = new List<Transform>();

            var rootParent = linkParent.transform;

            var parentOfCollection = linkParent.groupsParent;

            Bounds newBound = default;

            if (linkParent.groups.Count != 0)
            {
                newBound = new Bounds(linkParent.groups[0].transform.position, Vector3.one * 0.02f);// new Bounds();
            }
            else
            {
                return;
            }

            for (int i = 0; i < rootParent.GetChild(0).childCount; i++)
            {
                childList.Add(parentOfCollection.GetChild(i));

                var col = parentOfCollection.GetChild(i).GetComponent<Collider>();

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));
            }

            rootParent.transform.DetachChildren();

            parentOfCollection.transform.DetachChildren();

            rootParent.position = newBound.center;

            rootParent.SetGlobalScale(newBound.size);

            rootParent.rotation = Quaternion.identity;

            parentOfCollection.rotation = Quaternion.identity;



            BoxCollider[] bcs = rootParent.GetComponents<BoxCollider>();

            for (int i = 0; i < bcs.Length; i++)
            {
                Destroy(bcs[i]);
            }


            //if (rootParent.TryGetComponent(out Collider boxCollider))
            //{
            //    Destroy(boxCollider);
            //}


            rootParent.gameObject.AddComponent<BoxCollider>();

            foreach (var item in childList)
            {
                item.transform.SetParent(parentOfCollection.transform, true);
            }

            parentOfCollection.transform.SetParent(rootParent.transform, true);
        }

        public void UpdateGroupExternal(Group linkParent)
        {
            List<Transform> childList = new List<Transform>();

            var rootParent = linkParent.transform;

            var parentOfCollection = linkParent.groupsParent;

            Bounds newBound = default;

            if (linkParent.groups.Count != 0)
            {
                newBound = new Bounds(linkParent.groups[0].transform.position, Vector3.one * 0.02f);// new Bounds();
            }
            else
            {
                return;
            }

            for (int i = 0; i < rootParent.GetChild(0).childCount; i++)
            {
                childList.Add(parentOfCollection.GetChild(i));

                var col = parentOfCollection.GetChild(i).GetComponent<Collider>();

                //set new collider bounds
                newBound.Encapsulate(new Bounds(col.transform.position, col.bounds.size));
            }

            rootParent.transform.DetachChildren();

            parentOfCollection.transform.DetachChildren();

            rootParent.position = newBound.center;

            rootParent.SetGlobalScale(newBound.size);

            rootParent.rotation = Quaternion.identity;

            parentOfCollection.rotation = Quaternion.identity;

            if (rootParent.TryGetComponent(out Collider boxCollider))
            {
                Destroy(boxCollider);
            }

            rootParent.gameObject.AddComponent<BoxCollider>();


            parentOfCollection.transform.SetParent(rootParent.transform, true);
            parentOfCollection.rotation = Quaternion.identity;

            foreach (var item in childList)
            {

                item.transform.SetParent(parentOfCollection.transform, true);
                // item.SetGlobalScale(item.localScale.x * Vector3.one);
            }



            //parentOfCollection.transform.SetParent(rootParent.transform, true);
        }
    }
//}




