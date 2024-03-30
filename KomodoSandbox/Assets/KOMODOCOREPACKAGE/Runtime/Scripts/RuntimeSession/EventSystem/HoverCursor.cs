using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    public GameObject cursorGraphic;
    private Image cursorImage;
    public Color hoverColor;
    private Color originalColor;
    [Header("GameObjects to deactivate and activate when selecting in UI")]
    public GameObject[] objectsToDeactivateOnHover;

    void Awake()
    {
        // Ensure the cursorGraphic and its Image component are assigned and valid
        if (!cursorGraphic)
        {
            throw new Exception("Cursor GameObject is not set in the inspector.");
        }
        cursorImage = GetComponent<Image>();
        if (!cursorImage)
        {
            throw new Exception("Missing Image component on this GameObject.");
        }
        originalColor = cursorImage.color; // Set originalColor based on the initial Image color
    }

    void Start()
    {
        ShowCursor(false); // Optionally, set false if cursor should not be shown initially.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer entered on UI element.");
        ShowCursor(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Pointer exited from UI element.");
        ShowCursor(false);
    }

    private void ShowCursor(bool show)
    {
        cursorImage.color = show ? hoverColor : originalColor;
        cursorGraphic.transform.parent.gameObject.SetActive(show);
        // Optionally, activate/deactivate additional GameObjects
        foreach (var obj in objectsToDeactivateOnHover)
        {
            obj.SetActive(!show);
        }
    }

    public void OnDisable()
    {
        // Ensure the cursor reverts to its original state if the UI element is disabled
        ShowCursor(false);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        // Update cursor position and ensure it's active when moving over UI elements
        if (eventData.pointerCurrentRaycast.gameObject != null) // Check if the raycast hits a UI element
        {
            cursorGraphic.transform.position = eventData.pointerCurrentRaycast.worldPosition;
            ShowCursor(true); // Show the cursor when over UI elements, adjust if behavior differs
        }else
            ShowCursor(false);
    }
}
