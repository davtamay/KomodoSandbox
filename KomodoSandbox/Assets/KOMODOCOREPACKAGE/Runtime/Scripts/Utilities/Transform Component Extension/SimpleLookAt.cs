using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Komodo.Utilities
{
    public class SimpleLookAt : MonoBehaviour//,IUpdatable
    {

        //not in main updatable because I want it to be shut off when not in view and not on;


        public Transform lookAtTarget;
        public Transform thisTransform;

        public bool isUp = true;

        public string playspaceTag = "XRCamera";

        public bool isLookAtMainCamera = false;
        void Start()
        {
            if(isLookAtMainCamera)
                lookAtTarget = Camera.main.transform;

            if(lookAtTarget == null)
            {
                gameObject.SetActive(false);
            }
           
            if (thisTransform == null)
                thisTransform = transform;
            //thisTransform = transform;

            //if (lookAtTarget == null)
            //    lookAtTarget = GameObject.FindWithTag(playspaceTag).transform;
        }
        public void Update()
        {
            if (isUp)
                thisTransform.LookAt(lookAtTarget, Vector3.up);
            else
                thisTransform.LookAt(thisTransform.position - (lookAtTarget.position - thisTransform.position), Vector3.up);
        }
    }
}
