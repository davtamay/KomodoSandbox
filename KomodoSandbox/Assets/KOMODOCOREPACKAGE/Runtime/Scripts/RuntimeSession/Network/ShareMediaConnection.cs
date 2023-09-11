using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Komodo.Runtime
{
    public enum MediaType {TEXT = 0, AUDIO = 1, VIDEO = 2, SHARE_SCREEN = 3 } 
    public class ShareMediaConnection : MonoBehaviour
    {
        public GameObject shareMediaUI;
        public int currentClientIDSelected;
        public MediaType currentMediaType = MediaType.TEXT;



        public TMP_Text localInputChat;

        public void SetClientForMediaShare(int clientID)
        {
            shareMediaUI.SetActive(true);
            currentClientIDSelected = clientID;

        }
        public void SetMediaShareType(int mediaType)
        {
            currentMediaType =(MediaType) mediaType;


        }

        public void SendTextToClient()
        {
           
         //   localInputChat.text;



        }

        public void SendTextToSession()
        {



        }
        public void SendTextToWorld()
        {



        }


        public void InvokeMediaShare()
        {
            switch(currentMediaType)
            {
                case MediaType.TEXT:
                    break; 
                case MediaType.AUDIO:
                    break; 
                case MediaType.VIDEO:    
                    break;
                case MediaType.SHARE_SCREEN:
                    break;
            }


        }

    }
}
