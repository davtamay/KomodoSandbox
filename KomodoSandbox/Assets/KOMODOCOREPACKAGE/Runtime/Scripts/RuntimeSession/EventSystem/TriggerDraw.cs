﻿using System;
using UnityEngine;
/* using Unity.Entities;
 */
//namespace Komodo.Runtime
//{

    [RequireComponent(typeof(LineRenderer))]
    public class TriggerDraw : MonoBehaviour
    {
        public ControlerAndHandTracker controlerAndHandTracker;

        private Transform thisTransform;

        private LineRenderer lineRenderer;

        public float distanceThreshold = 0.5f;

        public float timeToCheckNewStrokeIndex;

        private float timePass;

        private int curLineIndex = 0;

        private int strokeID = 0;

        private int strokeIndex;

        [Header("IDENTIFY INTERACTION ID, PLACE A UNIQUE NUMBER EXCEPT 0")]
        public int handID;

        //to disable drawing during color picker selection through unity events;
        [HideInInspector] public bool isSelectingColorPicker;

       // private EntityManager entityManager;

        //to disable drawing during erasing funcionality
        private bool isEraserOn = false;

        public void Set_DRAW_UPDATE(bool active)
        {
            isEraserOn = active;
        }
    Guid myGUID;
        public virtual void Start()
        {
          //  entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            lineRenderer = GetComponent<LineRenderer>();

       // myGUID = Guid.NewGuid();
        strokeID = Guid.NewGuid().GetHashCode();  //myGUID.GetHashCode();// + strokeIndex;
        //set initial stroke id
      //  strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

            thisTransform = transform;

            if(handID == 1)
            {
            thisTransform = controlerAndHandTracker.ReturnCurrentActiveInputTransform_LEFT();
            }
            else if(handID == 2)
            {
            thisTransform = controlerAndHandTracker.ReturnCurrentActiveInputTransform_Right();
            }
        }

        public void Update()
        {
            if (lineRenderer == null || thisTransform == null || isEraserOn || isSelectingColorPicker)
                return;


        thisTransform = transform;

        if (handID == 1)
        {
            thisTransform = controlerAndHandTracker.ReturnCurrentActiveInputTransform_LEFT();
        }
        else if (handID == 2)
        {
            thisTransform = controlerAndHandTracker.ReturnCurrentActiveInputTransform_Right();
        }




        timePass += Time.deltaTime;

            if (timeToCheckNewStrokeIndex < timePass)
            {
                timePass = 0.0f;

                float curDistance = 0;

                if (lineRenderer.positionCount == 0)
                {
                    ++lineRenderer.positionCount;

                    lineRenderer.SetPosition(0, thisTransform.position);

                    curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(0));
                }
                else
                {
                    curDistance = Vector3.Distance(thisTransform.position, lineRenderer.GetPosition(curLineIndex));
                }

                if (curDistance > distanceThreshold)
                {
                    //update visuals per stroke 
                    //offset: 5000 + clientid + child render count 

                    DrawingInstanceManager.Instance.SendDrawUpdate(
                        strokeID,
                        Entity_Type.Line,
                        lineRenderer.widthMultiplier,
                        lineRenderer.GetPosition(curLineIndex),
                        new Vector4
                        (
                            lineRenderer.startColor.r,
                            lineRenderer.startColor.g,
                            lineRenderer.startColor.b,
                            lineRenderer.startColor.a
                        )
                    );


    //            var data =
    //  JsonUtility.ToJson(
    //      new Draw(
    //          1,
    //    strokeID + 100,
    //    (int)Entity_Type.Line,
    //    1,
    //    lineRenderer.GetPosition(lineRenderer.positionCount - 1),
    //    new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
    //    )
    //);
    //            DrawingInstanceManager.Instance.ReceiveDrawUpdate(data);





                ++lineRenderer.positionCount;

                    curLineIndex++;

                    lineRenderer.SetPosition(curLineIndex, thisTransform.position);
                }
            }
        }

        //THIS IS WHERE FUNCTIONS ARE INVOKED (ON RELEASE OF TRIGGER BUTTON WHICH DEACTIVATES PARENT OBJECT
        public virtual void OnDisable()
        {
            //get rid of uncompleted stroke saved up locations 
            if (lineRenderer.positionCount == 1)
            {
                lineRenderer.positionCount = 0;
            }

            if (lineRenderer == null || lineRenderer.positionCount <= 1)
            {
                return;
            }

            //make strokeID identical based on left or right hand add an offset *100 strokeID * 10000
            //ALL STROKE IDS HAVE TO BE UNIQUE TO REFERENCE THROGH THE NETWORK
        //    if (NetworkUpdateHandler.IsAlive)
        //    {
        //    //  Guid myGUID = Guid.NewGuid();
        //    //    strokeID = myGUID.GetHashCode() + strokeIndex;
        //    strokeID = Guid.NewGuid().GetHashCode(); 

        //    //  strokeID = handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;

        //    // TODO: refactor the multiple instances of this 
        //    // calculation into one method.

        //    // TODO: evaluate reduced human readability if there
        //    // are 100 or more registered clients.
        //    // Consider client 111. Then the equation would be 
        //    // 1000000 + 100000 + 1110000 + 1234
        //    // = 2210000 + 891234
        //    // = 3101234
        //    // The apparent hand is 3 and apparent client ID is 10, 
        //    // but the actual hand is 1 and the actual client ID is 
        //    // 111.
        //}
        //    else
        //    {
        //        return;
        //    }

            DrawingInstanceManager.Instance.InitializeFinishedLineFromOwnClient(strokeID, lineRenderer, true);

            curLineIndex = 0;

          //  strokeIndex++;

            if (NetworkUpdateHandler.IsAlive)
            {
               strokeID = Guid.NewGuid().GetHashCode(); // strokeIndex;// handID * 1000000 + 100000 + NetworkUpdateHandler.Instance.client_id * 10000 + strokeIndex;
            }

            lineRenderer.positionCount = 0;
        }
    }
//}