using UnityEngine;
using Komodo.Runtime;
using UnityEngine.EventSystems;
using Komodo.IMPRESS;

namespace Komodo.Runtime
{
    public class MoveWithHover : MonoBehaviour, IPointerMoveHandler, ICursorHover, IPointerEnterHandler, IPointerExitHandler
    {
        public Transform target;

        public void OnHover (CursorHoverEventData cursorData)
        {
            target.transform.position = cursorData.currentHitLocation;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ColorPickerManager.StartPreviewing();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ColorPickerManager.StopPreviewing();
        }

        //public void OnPointerClick(PointerEventData eventData)
        //{
        //    target.transform.position = eventData.position;
        //}

        public void OnPointerMove(PointerEventData eventData)
        {
            ////   target.transform.position = new Vector3(eventData.position.x, eventData.position.y, eventData.worldPosition.z);
            //   target.transform.position = new Vector3(eventData.worldPosition.x, eventData.worldPosition.y, eventData.worldPosition.z);
            //  target.transform.localPosition = new Vector3(-target.transform.localPosition.x, 0, target.transform.localPosition.y);
            // Convert the world position to local position relative to the target's parent (menu/image)


            //  target.localPosition = new Vector3(eventData.position.x, 0, eventData.position.y); ;


            //Vector3 localPosition = target.InverseTransformPoint(eventData.worldPosition);



            ////// Update the target's local position, setting Y to 0 if your menu is flat on the XZ plane
            //target.localPosition = new Vector3(localPosition.x, 0, localPosition.z);

            target.position = eventData.pointerCurrentRaycast.worldPosition;

        }
    }
}
