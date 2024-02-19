
using CSCore.XAudio2;
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

    
    //public int currentClientIDSelected;
    public MediaType currentMediaType = MediaType.TEXT;

    public TMP_InputField localInputChat;

    //  public GameObject textboxPrefab;
    public GameObject chatContainer;
    public Transform chatContentParent;

    public int lastMessageid;

    public Scrollbar scrollbar;


    public Button ShareCameraClientButton;


    public WebRTCVideoTexture RemoteClients;

  //  public GameObject shareMediaUI;
    //public Transform videoGridParent;

    //public GameObject videoPrefab;
   
   // public List<Transform> videoTransformList = new List<Transform>();

    public TMP_Text customReceiveText;

    //public GameObject CallReceivePanel;
    public WebRTCReceiveCallReferences CallReceivePanelReferences;

    public VideoFeedManager videoFeedManager;

    Dictionary<int, WebRTCVideoReferences> clientIDToVideoRefsDictionary = new Dictionary<int, WebRTCVideoReferences>();

    public WebRTCVideoTexture webRTCVideoTexture;

    public WebRTCVideoSettings webRTCVideoSettings;

    public Button shareScreenToSelectedClientsButton;
    public Button messageSelectedClientsButton;
    public Button callSelectedClients;
    public void Start()
    {
      
        //ReceivedCall(3);
        videoFeedManager = GetComponent<VideoFeedManager>();

        GlobalMessageManager.Instance.Subscribe("chat", (str) => ReceiveChatUpdate(str));

        //   SetupVideoListeners(fromClientID, videoRefs);

        callSelectedClients.onClick.AddListener(() => MakeCallToSelectedClients());


        webRTCVideoSettings.hangUpButton.onClick.AddListener(() =>
            {
                DisactivateCallPresentation();
              
                SocketIOJSLib.HangUpClient();
            });

        webRTCVideoSettings.micMuteToggle.onValueChanged.AddListener((val) =>
            {
                SocketIOJSLib.SetMicrophone();
                //ToggleMicrophone(val);
            });

        webRTCVideoSettings.showVideoFeedToggle.onValueChanged.AddListener((val) =>
            {
                SocketIOJSLib.SetVideo();
                //ToggleVideo(val);
            });


        webRTCVideoSettings.shareScreenToggle.onValueChanged.AddListener((val) =>
            {
                if (val)
                    SocketIOJSLib.ShareScreen(1);
                else
                    SocketIOJSLib.ShareScreen(0);
            });

        //ReceivedCall(4);
        //ReceivedCall(4);
        //ReceivedCall(2);
        //ReceivedOfferAnswer(2);
        //ReceivedOfferAnswer(2);
        // ReceivedCall(3);
        //SetupVideo(2);

    }


    public void DisactivateCallPresentation()
    {
        webRTCVideoSettings.gameObject.SetActive(false);

        foreach (var item in clientIDToVideoRefsDictionary)
            item.Value.gameObject.SetActive(false);

        foreach (var item in clientIDToToggleDictionary.Keys)
            SetClientConnectedToggleState(item, false);

    }
    public Dictionary<int, Toggle> clientIDToToggleDictionary = new Dictionary<int, Toggle>();
    public Dictionary<int, bool> clientIDToToggleStateDictionary = new Dictionary<int, bool>();


    public void MakeCallToSelectedClients( )
    {

        foreach (KeyValuePair<int,bool> item in clientIDToToggleStateDictionary)
        {
            if (item.Value)
            {
                string name = ClientSpawnManager.Instance.GetPlayerNameFromClientID(item.Key);
                //    Debug.Log($"Calling Client:{ clientID}");
                Debug.Log("GET PLAYER FROM ID : " + name);
                SocketIOJSLib.CallClient(name);

                clientIDToToggleDictionary[item.Key].interactable = false;
            }
        }

    }

    public List<int> clientsAdded = new List<int>();

    public void ReceivedCall(int fromClientID)
    {
        if (!clientsAdded.Contains(fromClientID))
        {
            clientsAdded.Add(fromClientID);
            // return;
        }
        else
        {
            Debug.Log("Returning. Tried Adding same client with id " + fromClientID);
            return;

        }

        CallReceivePanelReferences.clientId = fromClientID;

        string nameOfClient = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);

        customReceiveText.text = $"Receiving Call From: \n {nameOfClient} ";

        CallReceivePanelReferences.gameObject.SetActive(true);
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
            // SetupVideo(fromClientID, true);
            SocketIOJSLib.AnswerClientOffer(ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));
            CallReceivePanelReferences.gameObject.SetActive(false);

        });



    }


    public void ReceiveCallAndAnswer(int fromClientID)
    {
        if (!clientsAdded.Contains(fromClientID))
        {
            clientsAdded.Add(fromClientID);
        }
        else
        {
            Debug.Log("Returning. Tried Adding same client with id " + fromClientID);
            return;

        }

        SocketIOJSLib.AnswerClientOffer(ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));

    }

    public Color selectClient = Color.grey;
    public Color clientInCall = Color.green;
    public void SetClientConnectedToggleState(int clientID, bool isActive )
    {

        if (clientIDToToggleDictionary.ContainsKey(clientID))
        {
            var clientToggle = clientIDToToggleDictionary[clientID];

            clientToggle.interactable = !isActive;
            clientToggle.SetIsOnWithoutNotify(isActive);

            if(isActive)
                clientToggle.graphic.color = clientInCall;
            else
                clientToggle.graphic.color = selectClient;

        }
    }

    public void SetupVideo(int fromClientID, bool receivedClientOffer = false)
    {
        webRTCVideoSettings.gameObject.SetActive(true);

        
        (int videoIndexID, GameObject videoFeedGO) = videoFeedManager.GetOrCreateVideoFeed(fromClientID);

        if (videoFeedGO != null)
        {


            SetClientConnectedToggleState(fromClientID, true);

           //  SetClientForMediaShare(fromClientID);
            SetMediaShareType(2); // call

            var videoRefs = videoFeedGO.GetComponentInChildren<WebRTCVideoReferences>();

            videoRefs.clientID = fromClientID;
            videoRefs.videoIndex = videoIndexID;

            videoFeedGO.SetActive(true);

            ////videoRefs.micMuteToggle.SetIsOnWithoutNotify(true);
            ////videoRefs.shareScreenToggle.SetIsOnWithoutNotify(false);
            ////videoRefs.showVideoFeedToggle.SetIsOnWithoutNotify(true);

            if (clientIDToVideoRefsDictionary.ContainsKey(fromClientID))
                clientIDToVideoRefsDictionary[fromClientID] = videoRefs;
            else
                clientIDToVideoRefsDictionary.Add(fromClientID, videoRefs);

            clientIDToVideoRefsDictionary[fromClientID].clientName.text = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);
            clientIDToVideoRefsDictionary[fromClientID].gameObject.SetActive(true);
            //currentClientIDSelected = fromClientID;


            // SetupVideoListeners(fromClientID, videoRefs);


            webRTCVideoTexture.ProvideWebRTCTexture(videoRefs.videoTexture, ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));
        //    Debug.Log(videoRefs.gameObject.name);


           

            // shareMediaUI.GetComponentInChildren<WebRTCVideoReferences>().clientID = fromClientID;
            //shareMediaUI.SetActive(true);
        }
        else
        {
            Debug.Log("Cannot accept more video feeds, limit reached.");
        }

    }


    public void ReceivedOfferAnswer(int clientID)
    {
        //if (!offerClientsAdded.Contains(clientID))
        //{
        //    offerClientsAdded.Add(clientID);
        //    // return;
        //}
        //else
        //{
        //    Debug.Log("Returning. Received answer for offer with same client with id " + clientID);
        ////    return;

        //}

        Debug.Log("RECEIVEDOFFER FROM ANSWER");
      
            SetupVideo(clientID, false);
    }
    public void EndCall(int clientID)
    {
      //  Debug.Log("ENDED CALL FOR ROOMNAME");
        if (clientIDToVideoRefsDictionary.ContainsKey(clientID))
        {
            clientIDToVideoRefsDictionary[clientID].gameObject.SetActive(false);
            clientIDToVideoRefsDictionary.Remove(clientID);


            if(clientsAdded.Contains(clientID)) 
               clientsAdded.Remove(clientID);
        }

        SetClientConnectedToggleState(clientID, false);

        //foreach (var item in clientIDToToggleDictionary.Keys)
        //{
        //    SetClientConnectedToggleState(item, false);
        //}
        // DisactivateCallPresentation();

        CheckIfAnyConnectionToSetToolsInactive();
        videoFeedManager.RemoveVideoFeed(clientID);
        // clientIDToVideoRefsDictionary


        //if (clientIDToToggleDictionary.ContainsKey(clientID))
        //    clientIDToToggleDictionary[clientID].interactable = true;
    }

    public void RemoveClientConnections(int clientID)
    {
        if(clientID == CallReceivePanelReferences.clientId) {
            CallReceivePanelReferences.gameObject.SetActive(false);
        }

        if (clientIDToVideoRefsDictionary.ContainsKey(clientID))
        {
            if (clientIDToVideoRefsDictionary[clientID].clientID == clientID)
                clientIDToVideoRefsDictionary[clientID].gameObject.SetActive(false);
       
            clientIDToVideoRefsDictionary.Remove(clientID);
        }

       SetClientConnectedToggleState(clientID, false);


        //  DisactivateCallPresentation();

        videoFeedManager.RemoveVideoFeed(clientID);

        //if(clientIDToToggleDictionary.ContainsKey(clientID))
        //clientIDToToggleDictionary[clientID].interactable = true;
        // clientIDToVideoRefsDictionary
      
        
        CheckIfAnyConnectionToSetToolsInactive();
    }

    void CheckIfAnyConnectionToSetToolsInactive()
    {
        bool allInactive = true;

        // Loop over all GameObjects in the dictionary
        foreach (WebRTCVideoReferences v in clientIDToVideoRefsDictionary.Values)
        {
            // If any GameObject is active, set allInactive to false and break the loop
            if (v.gameObject.activeInHierarchy)
            {
                allInactive = false;
                break;
            }
        }

        // If all GameObjects are not active, set webRTCVideoSettings to inactive
        if (allInactive)
        {
            webRTCVideoSettings.gameObject.SetActive(false);

            foreach (var item in clientIDToToggleDictionary.Keys)
            {
                SetClientConnectedToggleState(item, false);
            }


        }
    }

    public Dictionary<GameObject, bool> poolVideoActiveStateDic = new Dictionary<GameObject, bool>();

    //public void SetClientForMediaShare(int clientID)
    //{

    //    clientIDToVideoRefsDictionary[clientID].clientName.text = ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID);
    //    clientIDToVideoRefsDictionary[clientID].gameObject.SetActive(true);
    //   // currentClientIDSelected = clientID;

    //}




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



    //public void TurnOn()
    //{
    //    lastInstantiated.GetComponent<LayoutElement>().flexibleHeight = 1;
    //  //  lastInstantiated.GetComponent<VerticalLayoutGroup>().enabled = true;
    //    //  lastInstantiated.gameObject.SetActive(true);
    //}
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
