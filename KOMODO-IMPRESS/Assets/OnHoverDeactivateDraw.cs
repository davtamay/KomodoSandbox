using Komodo.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnHoverDeactivateDraw : MonoBehaviour, ICursorHover, IPointerExitHandler
{
    public UnityEvent onHover;
    public UnityEvent onExit;

    public void OnHover(CursorHoverEventData cursorData)
    {
        onHover.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        onExit.Invoke();
    }

  
}
