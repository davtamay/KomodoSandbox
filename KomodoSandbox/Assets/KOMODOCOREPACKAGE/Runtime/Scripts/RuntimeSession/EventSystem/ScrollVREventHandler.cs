using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    [RequireComponent(typeof(Slider))]
    public class ScrollVREventHandler : MonoBehaviour, ISliderHover
    {
        private Slider slider;
        private RectTransform rectTrans;
        private Rect rect;

        public bool useScrollRectInsteadOfSlider = false;

        public ScrollRect scrollRect;
        public void Start()
        {
            if (useScrollRectInsteadOfSlider == true) { }  //scrollbar = GetComponent<Scrollbar>(); 
            else slider = GetComponent<Slider>();

            rectTrans = GetComponent<RectTransform>();
        }

        public void OnSliderHover(SliderEventData cursorData)
        {
            //dont adjust slider when input source is not on
            if (!cursorData.isInputActive)
                return;

            //get relative location of the hit from our rectTransform
         var locPos = rectTrans.InverseTransformPoint(cursorData.currentHitLocation);

            //move our reference point above the half point removing negative numbers then divide that with the total width to get ther normalized position
            var posShift = (locPos.x + (rectTrans.rect.width / 2)) / rectTrans.rect.width;



        if (useScrollRectInsteadOfSlider == true)
        {
             posShift = (locPos.y + (rectTrans.rect.height / 2)) / rectTrans.rect.height;

            //float totalWidth = rectTrans.rect.height;// find total width of scroll rect transform.
            //float targetValue = posShift;//100;
            //float targetPercentage = targetValue / totalWidth;

            //scrollRect.verticalNormalizedPosition = targetPercentage;

            scrollRect.verticalNormalizedPosition = posShift;
            //   scrollRect.verticalNormalizedPosition = 1 - (posShift + 0.50f);


        }
        else slider.normalizedValue = posShift;
        }

    }
}