using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinMaxSharedVideo : MonoBehaviour
{
    public float maxHeight;
    public float minHeight;
    RectTransform rectTransform;
    public List<GameObject> disactivationList = new List<GameObject>();


    public GridLayoutGroup gridlayout;


    public WebRTCVideoReferences maxSizeVideoReference;

    public void Start()
    {
         rectTransform = GetComponent<RectTransform>();

        gridlayout = GetComponentInParent<GridLayoutGroup>();
        // var e = new GridLayoutGroupEditor();
        //e.
    }

    public float viewSizeWhenMaxScreenOn;
    public float viewSizeWhenMinScreenOff;
    public LayoutElement viewMaskLayout;
    
    
    GameObject currentVideoReferenceGOMaximize;

    public void SetMaxVideoActiveState(bool active)
    {
        maxSizeVideoReference.gameObject.SetActive(active);

        if (active)
        {
            viewMaskLayout.minHeight = viewSizeWhenMaxScreenOn;
        }else
            viewMaskLayout.minHeight = viewSizeWhenMinScreenOff;

    }

    public void SetSizeBack()
    {

        SetMaxVideoActiveState(false);

        if (currentVideoReferenceGOMaximize)
            currentVideoReferenceGOMaximize.SetActive(true) ;
    }

    public void SetSizeBackAndDeactivateOriginalVideoShare(int clientIDToCheck)
    {
        if (clientIDToCheck != maxSizeVideoReference.clientID)
            return;

        SetMaxVideoActiveState(false);

        if (currentVideoReferenceGOMaximize)
            currentVideoReferenceGOMaximize.SetActive(false);
    }
    public void SwitchToVideoMaxSize(WebRTCVideoReferences webRTCVideoReferences)
    {
        maxSizeVideoReference.clientID = webRTCVideoReferences.clientID;
        maxSizeVideoReference.clientName.text = webRTCVideoReferences.clientName.text;
        maxSizeVideoReference.videoTexture.texture = webRTCVideoReferences.videoTexture.mainTexture;
        maxSizeVideoReference.videoIndex = webRTCVideoReferences.videoIndex;


        if(currentVideoReferenceGOMaximize)
            currentVideoReferenceGOMaximize.SetActive(true);


        currentVideoReferenceGOMaximize = webRTCVideoReferences.gameObject;

        currentVideoReferenceGOMaximize.SetActive(false);
    }

    public void SetToMin()
    {
        ToggleMinMax(false);
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
          //  gridlayout.cellSize = new Vector2(220, maxHeight);
        }
        else
        {
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, minHeight);
          //  gridlayout.cellSize= new Vector2(220, minHeight);
        }

        foreach (var item in disactivationList)
        {
            item.SetActive(value);
        }

    }


}
