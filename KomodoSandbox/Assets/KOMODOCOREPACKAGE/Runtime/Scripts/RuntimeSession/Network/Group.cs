using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;
using Komodo.Runtime;

//namespace Komodo.IMPRESS
//{
    public class Group : MonoBehaviour
    {
        public List<Collider> groups;
       // public List<Transform> parentsForColliders;

        public Transform groupsParent;

        public GameObject currentGroupBoundingBox;

        public MeshRenderer meshRenderer;

        public List<NetworkedGameObject> netObjectList = new List<NetworkedGameObject>();
    }
//}