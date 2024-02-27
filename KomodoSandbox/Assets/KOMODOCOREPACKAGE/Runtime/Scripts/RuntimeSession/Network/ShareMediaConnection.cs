
using CSCore.XAudio2;
using JetBrains.Annotations;
using Komodo.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static ShareMediaConnection;

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


    public Transform receiveCallParentPanel;
    public GameObject receiveCallPrefab;
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

                foreach (int id in clientsWithVideo)
                    webRTCVideoTexture.RemoveTexture(ClientSpawnManager.Instance.GetPlayerNameFromClientID(id));

                clientsWithVideo.Clear();

                clientsCalled.Clear();


                webRTCVideoSettings.transform.parent.gameObject.SetActive(false);




                if (isLocalClientStreaming)
                {
                    webRTCVideoTexture.RemoveTexture("localVideo");
                    localVideoReferences.gameObject.SetActive(false);

                    localVideoReferences.clientName.text = "<color=black>" + ClientSpawnManager.Instance.GetPlayerNameFromClientID(NetworkUpdateHandler.Instance.client_id) + "</color>";

                    isLocalClientStreaming = false;
                }

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



        // Assuming you've already parsed the JSON into deviceList
        //  PopulateDeviceDropdowns();


        // Add listener for the video input dropdown
        videoInputDropdown.onValueChanged.AddListener(delegate
        {
            DropdownValueChanged(videoInputDropdown, 1);
        });

        // Add listener for the audio input dropdown
        audioInputDropdown.onValueChanged.AddListener(delegate
        {

            DropdownValueChanged(audioInputDropdown, 2);
        });

        audioOutputDropdown.onValueChanged.AddListener(delegate
        {

            DropdownValueChanged(audioOutputDropdown, 3);
        });


    }




    public void DisactivateCallPresentation()
    {
        //if (isLocalClientStreaming)
        //{
        //    webRTCVideoTexture.RemoveTexture("localVideo");
        //    localVideoReferences.gameObject.SetActive(false);
        //      webRTCVideoSettings.gameObject.SetActive(false);

        //    isLocalClientStreaming = false;
        //}



        foreach (var item in clientIDToVideoRefsDictionary)
            item.Value.gameObject.SetActive(false);

        foreach (var item in clientIDToToggleDictionary.Keys)
            SetClientConnectedToggleState(item, false);


        // isLocalClientStreaming = false;

    }
    public Dictionary<int, Toggle> clientIDToToggleDictionary = new Dictionary<int, Toggle>();
    public Dictionary<int, bool> clientIDToToggleStateDictionary = new Dictionary<int, bool>();


    public List<int> clientsCalled = new List<int>();

    public void MakeCallToSelectedClients()
    {
        if (!isLocalClientStreaming)
        {

            localVideoReferences.gameObject.SetActive(true);
            localVideoReferences.clientName.text = "<color=black>" + ClientSpawnManager.Instance.GetPlayerNameFromClientID(NetworkUpdateHandler.Instance.client_id) + "</color>";


            //  (int videoIndexID, GameObject videoFeedGO) = videoFeedManager.GetOrCreateVideoFeed(fromClientID);
            webRTCVideoTexture.ProvideWebRTCTexture(localVideoReferences.videoTexture, "localVideo");




            isLocalClientStreaming = true;



            webRTCVideoSettings.transform.parent.gameObject.SetActive(true);
        }

        foreach (KeyValuePair<int, bool> item in clientIDToToggleStateDictionary)
        {
            if (item.Value)
            {
                if (clientsCalled.Contains(item.Key))
                    continue;

                clientsCalled.Add(item.Key);

                string name = ClientSpawnManager.Instance.GetPlayerNameFromClientID(item.Key);
                //    Debug.Log($"Calling Client:{ clientID}");
                Debug.Log("GET PLAYER FROM ID : " + name);
                SocketIOJSLib.CallClient(name);

                clientIDToToggleDictionary[item.Key].interactable = false;
            }
        }

    }


    public List<int> clientsWithVideo = new List<int>();


    private List<WebRTCReceiveCallReferences> pool = new List<WebRTCReceiveCallReferences>();


    Dictionary<int, WebRTCReceiveCallReferences> callsAvailableDictionary = new Dictionary<int, WebRTCReceiveCallReferences>();

    // Method to handle received calls
    public void ReceivedCall(int fromClientID)
    {



        string nameOfClient = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);



        var panel = GetPanel();
        

        if (callsAvailableDictionary.ContainsKey(fromClientID))
        {
            ResetAndReturnPanel(fromClientID);
            callsAvailableDictionary[fromClientID] = panel;
        }
        else
        {
            callsAvailableDictionary.Add(fromClientID, panel);
        }

        panel.SetupPanel(fromClientID, nameOfClient, RejectCall, AcceptCall);

    }

    private void RejectCall(int clientId)
    {
        SocketIOJSLib.RejectClientOffer(clientId);
        ResetAndReturnPanel(clientId);
    }

    private void AcceptCall(int clientId)
    {

        SocketIOJSLib.AnswerClientOffer(ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientId));
        SetupVideo(clientId); // Assuming SetupVideo is a method defined elsewhere
        ResetAndReturnPanel(clientId);
    }

    public void CallFailed(int clientId)
    {
        if (clientsCalled.Contains(clientId))
            clientsCalled.Remove(clientId);

        SetClientConnectedToggleState(clientId, false);
    }
    private void ResetAndReturnPanel(int clientId)
    {
        var panel = pool.Find(p => p.clientId == clientId);

        if (panel != null)
        {
            panel.ResetPanel();
            //    clientsAdded.Remove(clientId);
        }
    }


    // Method to get a panel from the pool
    public WebRTCReceiveCallReferences GetPanel()
    {
        foreach (var panel in pool)
        {
            if (!panel.gameObject.activeInHierarchy)
            {
                return panel;
            }
        }

        // If no inactive panel is found, create a new one
        var newPanelInstance = Instantiate(receiveCallPrefab, receiveCallParentPanel).GetComponent<WebRTCReceiveCallReferences>();
        pool.Add(newPanelInstance);
        return newPanelInstance;
    }





    public void ReceiveCallAndAnswer(int fromClientID)
    {

        if (callsAvailableDictionary.ContainsKey(fromClientID))
        {
            ResetAndReturnPanel(fromClientID);
            callsAvailableDictionary.Remove(fromClientID);
        }
        //else
        //{
        //    callsAvailableDictionary.Add(fromClientID, panel);
        //}

        SocketIOJSLib.AnswerClientOffer(ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));
        SetupVideo(fromClientID);

    }

    public Color selectClient = Color.grey;
    public Color clientInCall = Color.green;

    public Color clientRejectColor = Color.red;
    public void SetClientConnectedToggleState(int clientID, bool isActive)
    {

        if (clientIDToToggleDictionary.ContainsKey(clientID))
        {
            var clientToggle = clientIDToToggleDictionary[clientID];

            clientToggle.interactable = !isActive;
            clientToggle.SetIsOnWithoutNotify(isActive);

            if (isActive)
                clientToggle.graphic.color = clientInCall;
            else
                clientToggle.graphic.color = selectClient;

        }
    }


    bool isLocalClientStreaming = false;

    public WebRTCVideoReferences localVideoReferences;
    public void SetupVideo(int fromClientID)
    {

        if (!clientsWithVideo.Contains(fromClientID))
            clientsWithVideo.Add(fromClientID);
        else
            Debug.Log("Setting up client already in setup video list");


        if (!isLocalClientStreaming)
        {

            localVideoReferences.gameObject.SetActive(true);
            //  (int videoIndexID, GameObject videoFeedGO) = videoFeedManager.GetOrCreateVideoFeed(fromClientID);
            webRTCVideoTexture.ProvideWebRTCTexture(localVideoReferences.videoTexture, "localVideo");

            localVideoReferences.clientName.text = "<color=black>" + ClientSpawnManager.Instance.GetPlayerNameFromClientID(NetworkUpdateHandler.Instance.client_id) + "</color>";

            isLocalClientStreaming = true;
        }
        webRTCVideoSettings.transform.parent.gameObject.SetActive(true);


        Debug.Log("****************received setup video from :" + fromClientID);


        (int videoIndexID, GameObject videoFeedGO) = videoFeedManager.GetOrCreateVideoFeed(fromClientID);

        if (videoFeedGO != null)
        {


            SetClientConnectedToggleState(fromClientID, true);

            //  SetClientForMediaShare(fromClientID);
            SetMediaShareType(2); // call

            var videoRefs = videoFeedGO.GetComponentInChildren<WebRTCVideoReferences>(true);

            videoRefs.clientID = fromClientID;
            videoRefs.videoIndex = videoIndexID;

            videoFeedGO.SetActive(true);


            if (clientIDToVideoRefsDictionary.ContainsKey(fromClientID))
                clientIDToVideoRefsDictionary[fromClientID] = videoRefs;
            else
                clientIDToVideoRefsDictionary.Add(fromClientID, videoRefs);

            clientIDToVideoRefsDictionary[fromClientID].clientName.text = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);
            clientIDToVideoRefsDictionary[fromClientID].gameObject.SetActive(true);
            //currentClientIDSelected = fromClientID;


            // SetupVideoListeners(fromClientID, videoRefs);
            // videoRefs.videoTexture.texture =


            webRTCVideoTexture.ProvideWebRTCTexture(videoRefs.videoTexture, ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));

            //   webRTCVideoTexture.ProvideWebRTCTexture(videoRefs.videoTexture, ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID));
            //  Debug.Log("CHECKING FOR NULL REFERENCE ISSUE WHEN MAKING CALL ABOVE MAYBE PROVIDE");




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
        Debug.Log("RECEIVEDOFFER FROM ANSWER");

        SetupVideo(clientID);
    }
    public void EndCall(int clientID)
    {
        //  Debug.Log("ENDED CALL FOR ROOMNAME");
        if (clientIDToVideoRefsDictionary.ContainsKey(clientID))
        {
            clientIDToVideoRefsDictionary[clientID].gameObject.SetActive(false);
            clientIDToVideoRefsDictionary.Remove(clientID);


            if (clientsWithVideo.Contains(clientID))
                clientsWithVideo.Remove(clientID);
        }

        SetClientConnectedToggleState(clientID, false);



        CheckIfAnyConnectionToSetToolsInactive();
        videoFeedManager.RemoveVideoFeed(clientID);
        // clientIDToVideoRefsDictionary


        webRTCVideoTexture.RemoveTexture(ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID));

        if (clientsCalled.Contains(clientID))
            clientsCalled.Remove(clientID);


        //  if(clientsWithVideo.Count <= )
        //if (clientIDToToggleDictionary.ContainsKey(clientID))
        //    clientIDToToggleDictionary[clientID].interactable = true;
    }
    public void EmptyRoom() { }


    public void CallRejected(int clientID)
    {

        Debug.Log(clientID + "rejected the call");

        if (clientIDToToggleDictionary.ContainsKey(clientID))
        {
            var clientToggle = clientIDToToggleDictionary[clientID];

            clientToggle.graphic.color = clientRejectColor;
        }


        StartCoroutine(SetClientCallStatusBackToNormalAfterTime(clientID));



        if (clientsCalled.Contains(clientID))
            clientsCalled.Remove(clientID);

    }
    public IEnumerator SetClientCallStatusBackToNormalAfterTime(int clientID)
    {
        yield return new WaitForSecondsRealtime(4);

        SetClientConnectedToggleState(clientID, false);
    }

    public void RemoveClientConnections(int clientID)
    {
        if (clientID == CallReceivePanelReferences.clientId)
        {
            CallReceivePanelReferences.gameObject.SetActive(false);
        }

        if (clientIDToVideoRefsDictionary.ContainsKey(clientID))
        {
            if (clientIDToVideoRefsDictionary[clientID].clientID == clientID)
                clientIDToVideoRefsDictionary[clientID].gameObject.SetActive(false);

            clientIDToVideoRefsDictionary.Remove(clientID);
        }

        SetClientConnectedToggleState(clientID, false);

        videoFeedManager.RemoveVideoFeed(clientID);

        CheckIfAnyConnectionToSetToolsInactive();



        webRTCVideoTexture.RemoveTexture(ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID));
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
            //   webRTCVideoSettings.gameObject.SetActive(false);

            foreach (var item in clientIDToToggleDictionary.Keys)
            {
                SetClientConnectedToggleState(item, false);
            }


        }
    }

    public Dictionary<GameObject, bool> poolVideoActiveStateDic = new Dictionary<GameObject, bool>();



    public void CreatePrivateChat(string from, string to)
    {
        //instantiate prefab
        //get the text and add the "to" name

        //reuse the same rect view.
        //pool the quantity of generated chat boxes.
        //fill it in. old get new. whats a good e



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











    [System.Serializable]
    public class DeviceInfo
    {
        public string deviceId;
        public string kind;
        public string label;
    }

    [System.Serializable]
    public class DeviceList
    {
        public DeviceInfo[] devices;
    }


    public TMP_Dropdown videoInputDropdown;
    public TMP_Dropdown audioInputDropdown;
    public TMP_Dropdown audioOutputDropdown;
    public void SetDeviceList(string deviseListJSON)
    {
        // Parse the JSON string into DeviceList
        DeviceList deviceList = JsonUtility.FromJson<DeviceList>(deviseListJSON);

        PopulateDeviceDropdowns(deviceList);

        SetDropdownToSavedValue(videoInputDropdown, savedVideoKey);
        SetDropdownToSavedValue(audioInputDropdown, savedAudioKey);
        SetDropdownToSavedValue(audioOutputDropdown, savedAudioOutputKey);

    }

    public string savedVideoKey = "videoDevice";
    public string savedAudioKey = "audioInputDevice";
    public string savedAudioOutputKey = "audioOutputDevice";
    // Dropdown change event handler
    void DropdownValueChanged(TMP_Dropdown dropdown, int type)
    {
        string label = dropdown.options[dropdown.value].text; // Assuming the option text contains the device ID

        string deviceId = labelToDeviceID[label]; //(device.label, device.deviceId);

        // Call JavaScript function based on the type of device
        switch (type)
        {
            case 1:
#if UNITY_WEBGL && !UNITY_EDITOR
           SocketIOJSLib.ChangeVideoDevice(deviceId);
#endif
                PlayerPrefs.SetString(savedVideoKey, deviceId);


                break;

            case 2:
#if UNITY_WEBGL && !UNITY_EDITOR
          SocketIOJSLib.ChangeAudioDevice(deviceId);
#endif

                PlayerPrefs.SetString(savedAudioKey, deviceId);

                break;

            case 3:

#if UNITY_WEBGL && !UNITY_EDITOR
                SocketIOJSLib.ChangeAudioOutputDevice(deviceId);
#endif
                PlayerPrefs.SetString(savedAudioOutputKey, deviceId);
                break;

        }
    }

    Dictionary<string, string> labelToDeviceID = new Dictionary<string, string>();
    void PopulateDeviceDropdowns(DeviceList deviceList)
    {
        videoInputDropdown.ClearOptions();
        audioInputDropdown.ClearOptions();

        List<string> videoOptions = new List<string>();
        List<string> audioInputOptions = new List<string>();
        List<string> audioOutputOptions = new List<string>();

        foreach (DeviceInfo device in deviceList.devices)
        {
            if (!labelToDeviceID.ContainsKey(device.label))
                labelToDeviceID.Add(device.label, device.deviceId);

            // Example logic to categorize devices (you might need additional info in your JSON for this)
            if (device.kind == "videoinput")//device.label.Contains("Camera"))
            {
                videoOptions.Add(device.label);
            }
            else if (device.kind == "audioinput")//device.label.Contains("Microphone"))
            {
                audioInputOptions.Add(device.label);
            }
            else if (device.kind == "audiooutput")//device.label.Contains("Microphone"))
            {
                audioOutputOptions.Add(device.label);
            }
        }

        videoInputDropdown.AddOptions(videoOptions);
        audioInputDropdown.AddOptions(audioInputOptions);
        audioOutputDropdown.AddOptions(audioOutputOptions);

        //if (PlayerPrefs.HasKey(savedVideoKey))
        //    videoInputDropdown.onValueChanged.Invoke((v) => { 


        //    });


        //    DropdownValueChanged(videoInputDropdown, 1);

        //if(PlayerPrefs.HasKey())

    }

    void SetDropdownToSavedValue(TMP_Dropdown dropdown, string playerPrefsKey)
    {
        if (PlayerPrefs.HasKey(playerPrefsKey))
        {
            string savedDeviceId = PlayerPrefs.GetString(playerPrefsKey);
            // Find the index of the option that corresponds to the saved device ID
            int index = dropdown.options.FindIndex(option => labelToDeviceID[option.text] == savedDeviceId);
            if (index != -1)
            { // Check if the device was found
                dropdown.value = index;
                dropdown.RefreshShownValue();
            }
        }
    }

}
//}
