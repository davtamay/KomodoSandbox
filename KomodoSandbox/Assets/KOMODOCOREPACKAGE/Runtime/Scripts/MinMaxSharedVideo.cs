using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinMaxSharedVideo : MonoBehaviour
{
    public float maxHeight;
    public float minHeight;
    RectTransform rectTransform;
    public List<GameObject> disactivationList = new List<GameObject>();
    public void Start()
    {
         rectTransform = GetComponent<RectTransform>();
        
    }

    public void ToggleMinMax(bool value)
    {
        if(rectTransform == null)
        {
         rectTransform = GetComponent<RectTransform>();

        }
        if (value)
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, maxHeight);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, minHeight);
        }

        foreach (var item in disactivationList)
        {
            item.SetActive(value);
        }

    }
}
