
using Komodo.Utilities;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//namespace Komodo.Runtime
//{
public enum MediaType { TEXT = 0, AUDIO = 1, VIDEO = 2, SHARE_SCREEN = 3 }
public class ShareMediaConnection : SingletonComponent<ShareMediaConnection>
{
    public static ShareMediaConnection Instance
    {
        get { return ((ShareMediaConnection)_Instance); }
        set { _Instance = value; }
    }

    
    public int currentClientIDSelected;
    public MediaType currentMediaType = MediaType.TEXT;

    public TMP_InputField localInputChat;

    //  public GameObject textboxPrefab;
    public GameObject chatContainer;
    public Transform chatContentParent;

    public int lastMessageid;

    public Scrollbar scrollbar;


    public Button ShareCameraClientButton;


    public WebRTCVideoTexture RemoteClients;

    public GameObject shareMediaUI;
    public Transform videoGridParent;

    public GameObject videoPrefab;
   
    public List<Transform> videoTransformList = new List<Transform>();

    public TMP_Text customReceiveText;

    //public GameObject CallReceivePanel;
    public WebRTCReceiveCallReferences CallReceivePanelReferences;
    //public void ToggleMicrophone(bool value)
    //{
    //    SetMicrophone();
    //}

    //public void ToggleVideo(bool value)
    //{
    //    SetVideo();
    //}

    //public void ShareScreenToggle(bool value)
    //{
    //    if(value)
    //        ShareScreen(1);
    //    else
    //        ShareScreen(0);
    //}

    //public void Hangup()
    //{
    //    HangUpClient();
    //}


    public void Start()
    {
        //var sendText = new SendText
        //{
        //    client_id = NetworkUpdateHandler.Instance.client_id,
        //    session_id = NetworkUpdateHandler.Instance.session_id,
        //    text = "csjdjsadjasdDAsdsadsadsad",
        //    type = (int)STRINGTYPE.SPEECH_TO_TEXT,
        //};
        //ReceiveChatUpdate(JsonUtility.ToJson(sendText));

        //sendText.text = "sdsadsadasdsadSAsd";
        //ReceiveChatUpdate(JsonUtility.ToJson(sendText));





        GlobalMessageManager.Instance.Subscribe("chat", (str) => ReceiveChatUpdate(str));

        foreach (var item in videoTransformList)
        {
            var references = item.GetComponentInChildren<WebRTCVideoReferences>(true);
            references.hangUpButton.onClick.AddListener(() =>
            {
                shareMediaUI.gameObject.SetActive(false);
                SocketIOJSLib.HangUpClient();
            
            });

            references.micMuteToggle.onValueChanged.AddListener((val) =>
            {
                SocketIOJSLib.SetMicrophone();
                //ToggleMicrophone(val);
            });

            references.showVideoFeedToggle.onValueChanged.AddListener((val) =>
            {
                SocketIOJSLib.SetVideo();
                //ToggleVideo(val);
            });

            references.shareScreenToggle.onValueChanged.AddListener((val) =>
            {
                if(val)
                 SocketIOJSLib.ShareScreen(1);
                else
                 SocketIOJSLib.ShareScreen(0);
            });

            
        }

      //  ShareCameraClientButton.onClick.AddListener(delegate { InvokeMediaShare(); });
    }
    
    public void ReceivedCall(int fromClientID)
    {
        CallReceivePanelReferences.clientId = fromClientID;

        string nameOfClient = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);

        customReceiveText.text = $"Receiving Call From: \n {nameOfClient} ";


        CallReceivePanelReferences.gameObject.SetActive(true);

       // var buttons = CallReceivePanelReferences.receiveCallButton//gameObject.GetComponentsInChildren<Button>(true);

        CallReceivePanelReferences.receiveCallButton.onClick.RemoveAllListeners();
        CallReceivePanelReferences.rejectCallButton.onClick.RemoveAllListeners();

        //reject call
        CallReceivePanelReferences.rejectCallButton.onClick.AddListener(() =>
        {
            SocketIOJSLib.RejectClientOffer(fromClientID);
            CallReceivePanelReferences.gameObject.SetActive(false);
        });


        //accept call
        CallReceivePanelReferences.receiveCallButton.onClick.AddListener(() =>
        {

            SocketIOJSLib.AnswerClientOffer(nameOfClient);
            // AcceptCallFromClient(nameOfClient);
            CallReceivePanelReferences.gameObject.SetActive(false);
            SetClientForMediaShare(fromClientID);
            SetMediaShareType(2);// call

            shareMediaUI.GetComponentInChildren<WebRTCVideoReferences>().clientID = fromClientID;
            shareMediaUI.SetActive(true);
        });



    }

    public void ReceivedOfferAnswer(int fromClientID)
    {
        Debug.Log("RECEIVEDOFFER FROM ANSWER");
        CallReceivePanelReferences.gameObject.SetActive(false);
        videoTransformList[0].gameObject.SetActive(true);
    }
    public void EndCall(string roomName)
    {
        Debug.Log("ENDED CALL FOR ROOMNAME");
        videoTransformList[0].gameObject.SetActive(false);
    }

    public void RemoveClientConnections(int clientID)
    {
        if(clientID == CallReceivePanelReferences.clientId) {
            CallReceivePanelReferences.gameObject.SetActive(false);
        }

       if(shareMediaUI.GetComponentInChildren<WebRTCVideoReferences>().clientID == clientID)
        {
            shareMediaUI.SetActive(false);
        }
    }

    public Dictionary<GameObject, bool> poolVideoActiveStateDic = new Dictionary<GameObject, bool>();

    public void SetClientForMediaShare(int clientID)
    {
        shareMediaUI.GetComponentInChildren<WebRTCVideoReferences>().clientName.text = ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID);
        shareMediaUI.SetActive(true);
        currentClientIDSelected = clientID;

    }

    // Method to get the next available or create a new video GameObject
    public GameObject GetOrCreateVideo()
    {
        foreach (var item in poolVideoActiveStateDic)
        {
            // Check for an inactive video and return it if found
            if (!item.Value) // if the video is inactive
            {
                item.Key.SetActive(true); // Activate the video GameObject
                poolVideoActiveStateDic[item.Key] = true; // Update its state in the dictionary
                return item.Key; // Return the now-active video GameObject
            }
        }

        // If no inactive video is found, create a new one
        GameObject newVideo = Instantiate(videoPrefab, videoGridParent);
        poolVideoActiveStateDic.Add(newVideo, true); // Add the new video to the dictionary as active
        return newVideo; // Return the new video GameObject
    }



    public void SetMediaShareType(int mediaType)
    {
        currentMediaType = (MediaType)mediaType;


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

        string clientName = ClientSpawnManager.Instance.GetPlayerNameFromClientID(deserializedData.client_id);
        CreateNewText(clientName, deserializedData.text);

    }
    public void CreateNewText(string clientName, string text)
    {
        lastInstantiated = Instantiate(chatContainer, chatContentParent).transform;
        //  lastInstantiated.SetAsFirstSibling();

        TMP_Text chatText = lastInstantiated.GetComponentInChildren<TMP_Text>();

        chatText.text = $"<b><color=blue>{clientName} : </color></b>" + text;

        //var ver = lastInstantiated.GetComponent<VerticalLayoutGroup>();

        //ver.childForceExpandHeight= false;
        //ver.childForceExpandHeight = true;
        var le = lastInstantiated.GetComponent<LayoutElement>();
        le.preferredHeight = chatText.preferredHeight;

        // lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = false;

        // scrollbar.value = 1;

        //  Invoke("TurnOn", 0.3f);

    }



    public void TurnOn()
    {
        lastInstantiated.GetComponent<LayoutElement>().flexibleHeight = 1;
      //  lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = true;
        //  lastInstantiated.gameObject.SetActive(true);
    }
    public void SendTextToWorld()
    {



    }


    public void InvokeMediaShare()
    {
        switch (currentMediaType)
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
//}
