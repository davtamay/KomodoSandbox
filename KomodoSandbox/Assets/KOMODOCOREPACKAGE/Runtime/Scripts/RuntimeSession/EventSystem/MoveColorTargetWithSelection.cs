using UnityEngine;
using UnityEngine.EventSystems;

namespace Komodo.Runtime
{
    public class MoveColorTargetWithSelection : MonoBehaviour, ICursorHover, IPointerMoveHandler
    {
        public Transform target;
        public Vector3 lastLocalPos;

        public void OnHover(CursorHoverEventData cursorData)
        {
            /// only move when we turn on our lazer that is emiting the event query
            if (cursorData.inputSourceActiveState)
            {
                target.transform.position = cursorData.currentHitLocation;
                lastLocalPos = target.transform.localPosition;
            }
            else
            {
                target.transform.localPosition = lastLocalPos;
            }


        }

        public void OnPointerClick(PointerEventData eventData)
        {
            //if (cursorData.inputSourceActiveState)
            //{
                target.transform.position = eventData.worldPosition;
                lastLocalPos = target.transform.localPosition;
            //}
            //else
            //{
            //    target.transform.localPosition = lastLocalPos;
            //}

        }

        public void OnPointerMove(PointerEventData eventData)
        {
            target.transform.position = eventData.position;
            lastLocalPos = target.transform.localPosition;
        }
    
    }
}
