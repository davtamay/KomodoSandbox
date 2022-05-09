using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

namespace Komodo.IMPRESS
{
    public class ImpressPlayer : MonoBehaviour
    {
        public TriggerCreatePrimitive triggerCreatePrimitiveLeft;

        public TriggerCreatePrimitive triggerCreatePrimitiveRight;

        public TriggerGroup triggerGroupLeft;

        public TriggerUngroup triggerUngroupRight;

        public TriggerGroup triggerGroupRight;

        public TriggerUngroup triggerUngroupLeft;

        public GameObject cursorGraphic; 
        public void Start()
        {
            cursorGraphic.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            WebXRManager.OnXRChange += onXRChange;
#else
            WebXRManagerEditorSimulator.OnXRChange += onXRChange;
#endif
        }

        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {

            if (state == WebXRState.VR)
            {
                cursorGraphic.SetActive(true);
            }
            else if (state == WebXRState.NORMAL)
            {
                cursorGraphic.SetActive(false);
            }

        }

        //public void Start ()
        //{
        //    if (triggerCreatePrimitiveLeft == null)
        //    {
        //        throw new UnassignedReferenceException("triggerCreatePrimitiveLeft");
        //    }

        //    if (triggerCreatePrimitiveRight == null)
        //    {
        //        throw new UnassignedReferenceException("triggerCreatePrimitiveRight");
        //    }

        //    if (!triggerGroupLeft)
        //    {
        //        throw new UnassignedReferenceException("triggerGroupLeft");
        //    }

        //    if (!triggerUngroupRight)
        //    {
        //        throw new UnassignedReferenceException("triggerUngroupRight");
        //    }

        //    if (!triggerGroupRight)
        //    {
        //        throw new UnassignedReferenceException("triggerGroupRight");
        //    }

        //    if (!triggerUngroupLeft)
        //    {
        //        throw new UnassignedReferenceException("triggerUngroupLeft");
        //    }
        //}
    }
}
