using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebRTCVideoReferences : MonoBehaviour
{
  
    public TMP_Text clientName;
    public int clientID;
    public int videoIndex;
    public RawImage videoTexture;

    public Button maximizeVideo;
    public Button minimizeVideo;

    public RawImage audioTexture;  
    
  //  public 
    public MinMaxSharedVideo minMaxSharedVideo;
    public void Start()
    {
        
        if(maximizeVideo!= null) 
        maximizeVideo.onClick.AddListener(() => {

            if(!minMaxSharedVideo)
                minMaxSharedVideo = GetComponentInParent<MinMaxSharedVideo>();


            if (minMaxSharedVideo != null)
            {


                minMaxSharedVideo.SwitchToVideoMaxSize(this);
                minMaxSharedVideo.SetMaxVideoActiveState(true);
            }
        
        });

        if (minimizeVideo != null)
            minimizeVideo.onClick.AddListener(() => {

               
                //if (minMaxSharedVideo != null)
                  
                  //  minMaxSharedVideo.SetToMin();

            });

    }
}
