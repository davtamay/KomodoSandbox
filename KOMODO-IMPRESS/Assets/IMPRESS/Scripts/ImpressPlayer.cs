using Komodo.Runtime;
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


        [Header("EventSystem References")]
        public TriggerEventInputSource triggerEventInputSourceL;
        public TriggerEventInputSource triggerEventInputSourceR;


        public IEnumerator Start()
        {
            cursorGraphic.SetActive(false);

#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
            WebXRManager.OnXRChange += onXRChange;
#else
            WebXRManagerEditorSimulator.OnXRChange += onXRChange;

            yield return new WaitForEndOfFrame();
            EventSystemManager.Instance.SetToDesktop();
#endif
        }
        public WebXRState currentState;
        private void onXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            currentState = state;

            if (state == WebXRState.VR)
            {
              //  UIManager.Instance.ToggleMenuVisibility(false);
                cursorGraphic.SetActive(true);
            }
            else if (state == WebXRState.NORMAL)
            {
                //UIManager.Instance.SEt
               UIManager.Instance.ToggleMenuVisibility(true);
                cursorGraphic.SetActive(false);
            }

        }

        public void LeftUIMenuOn()
        {
            UIManager.Instance.SetRightHandedMenu();
            UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceR, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceL);
            }

        }

        public void LeftUIMenuOff()
        {
            if (currentState != WebXRState.NORMAL)
            {
                UIManager.Instance.SetRightHandedMenu();
                UIManager.Instance.ToggleMenuVisibility(false);

            }
        }

        public void RightUIMenuOn()
        {
            UIManager.Instance.SetLeftHandedMenu();
            UIManager.Instance.ToggleMenuVisibility(true);

            //switch event inputs if switching hands so the cursor can reapear with the alternate hand
            if (EventSystemManager.IsAlive)
            {
                EventSystemManager.Instance.xrStandaloneInput.RegisterInputSource(triggerEventInputSourceL, true);
                EventSystemManager.Instance.RemoveInputSourceWithoutClick(triggerEventInputSourceR);
            }


        }

        public void RightUIMenuOff()
        {
            if (currentState != WebXRState.NORMAL)
            {
                UIManager.Instance.SetLeftHandedMenu();
                UIManager.Instance.ToggleMenuVisibility(false);
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
