using System;
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

        public TMP_InputField localInputChat;

      //  public GameObject textboxPrefab;
        public GameObject chatContainer;
        public Transform chatContentParent;

        public int lastMessageid;

        public Scrollbar scrollbar;

      //  GameObject vlg;
        public void Start()
        {
            GlobalMessageManager.Instance.Subscribe("chat", (str) => ReceiveChatUpdate(str));
            //   vlg = chatContainer.gam;
        }

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
           // textboxPrefab
         //   localInputChat.text;



        }

        Transform lastInstantiated;
        public void SendTextToSession()
        {
            if (string.IsNullOrEmpty(localInputChat.text))
                return;

          //  lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = false;



            var sendText = new SendText
            {
                client_id = NetworkUpdateHandler.Instance.client_id,
                session_id = NetworkUpdateHandler.Instance.session_id,
                text = localInputChat.text,
                type = (int)STRINGTYPE.SPEECH_TO_TEXT,
            };

            localInputChat.text = "";

            //KomodoMessage newMessage;
            var serializedUpdate = JsonUtility.ToJson(sendText);

            KomodoMessage komodoMessage = new KomodoMessage("chat", serializedUpdate, -1);

            komodoMessage.Send();

          //  Debug.Log("SENDING CHAT");

            //    Invoke("TurnOn", 0.1f);
        }


        public void ReceiveChatUpdate(string data)
        {
            Debug.Log("Receive CHAT");

            var deserializedData = JsonUtility.FromJson<SendText>(data);
            SpeechToTextSnippet snippet;
            snippet.target = deserializedData.client_id;
            snippet.text = deserializedData.text;
            snippet.stringType = (int)STRINGTYPE.SPEECH_TO_TEXT;
            ClientSpawnManager.Instance.ProcessSpeechToTextSnippet(snippet);

            string clientName = NetworkUpdateHandler.Instance.GetPlayerNameFromClientID(deserializedData.client_id);
            CreateNewText(clientName, deserializedData.text);

        }
        public void CreateNewText(string clientName, string text)
        {
            lastInstantiated = Instantiate(chatContainer, chatContentParent).transform;
          //  lastInstantiated.SetAsFirstSibling();

            TMP_Text chatText = lastInstantiated.GetComponentInChildren<TMP_Text>();

            chatText.text = $"<b><color=blue>{clientName} : </color></b>" + text;

            lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = false;

           // scrollbar.value = 1;

            Invoke("TurnOn", 0.25f);

        }



        public void TurnOn()
        {
            lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = true;
            //  lastInstantiated.gameObject.SetActive(true);
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
