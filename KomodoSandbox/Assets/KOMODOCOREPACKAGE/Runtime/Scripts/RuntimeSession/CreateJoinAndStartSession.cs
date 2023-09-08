using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace Komodo.Runtime
{
    [Serializable]
    public struct SessionInfo
    {
        public int id;
        public string name;
        public string date;
    }
    public struct SessionChangeInfo
    {
        public int id;
        public int oldSession_Id;
        public int session_id;
    }
    public struct ClientInfo
    {
        public int id;
        public int session_id;
        public string name;

    }
    public struct OtherClientInfo
    {
        public int id;
        public int session_id;
        public string name;
    }


    public class CreateJoinAndStartSession : MonoBehaviour
    {
        public GameObject prefabSessionButton;
        public Transform sessionButtonContentPanel;
        public TMPro.TMP_InputField sessionNameInputField;

        public Button startSessionButton;

        public GameObject loginPanel;
        public GameObject sessionPanel;

        public TMP_InputField clientName;
        public TMP_InputField clientPassword;


      
      

        public void ExitAuthentication()
        {
            gameObject.SetActive(false);
        }

        public void RequestClientID()
        {
            ClientInfo clientInfo = new ClientInfo();
            clientInfo.name = clientName.text;

            clientInfo.session_id = NetworkUpdateHandler.Instance.session_id;

            string info = JsonUtility.ToJson(clientInfo);

#if UNITY_WEBGL && !UNITY_EDITOR

            SocketIOJSLib.RequestClientIdFromServer();
#endif

        }

        public void ProvideServerWithClientInfo()
        {
            ClientInfo clientInfo = new ClientInfo();
            clientInfo.name = clientName.text;
            clientInfo.id = NetworkUpdateHandler.Instance.client_id;
            clientInfo.session_id = NetworkUpdateHandler.Instance.session_id;

            string info = JsonUtility.ToJson(clientInfo);
#if UNITY_WEBGL && !UNITY_EDITOR
            SocketIOJSLib.ProvideClientDataToServer(info);
#endif
        }


        public List<int> allSessions = new List<int>();

        public static int selectedSession = 1;
        public void SelectSession(int session)
        {
           
//#if UNITY_WEBGL && !UNITY_EDITOR
//            SocketIOJSLib.SetSessionId(session);
//#endif
            selectedSession = session;

            GoToNewSession();
            ///  NetworkUpdateHandler.Instance.session_id = session;
        }
      
        TMP_Text[] textList;
        public List<Toggle> toggleList = new List<Toggle>();
        bool firstCall = false;
        public void ServerCreate(int sessiontestID, string name = "", string date = "", bool fromServer = false) {

          //  Debug.Log("RECEIVED SESSION NUMBER:" + sessiontestID);

            if (!allSessions.Contains(sessiontestID))
                allSessions.Add(sessiontestID);
            else
                return;


            var sessionButton = Instantiate(prefabSessionButton, sessionButtonContentPanel);
            
            
            sessionButton.name = sessiontestID.ToString();


             textList = sessionButton.GetComponentsInChildren<TMPro.TMP_Text>(true);


            NetworkUpdateHandler.Instance.sessionsToCountTextDictionary.Add(sessiontestID, textList[2]);


            if (fromServer)
            {
                textList[0].text = name;
                textList[1].text = "Created at : " + date;

            }
            else
            {
                textList[0].text = sessionNameInputField.text;
                textList[1].text = "Created at : " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss");
                sessionNameInputField.text = "";

            }
           

            var toggle = sessionButton.GetComponent<Toggle>();

            toggleList.Add(toggle);


            toggle.onValueChanged.AddListener((isON) =>
            {

                if (toggle.isOn)
                {

                    SelectSession(sessiontestID);

                    foreach (var item in toggleList)
                    {
                        item.interactable = true;
                    }


                    toggle.interactable = false;

                    Debug.Log("session selected : " + sessiontestID);
                }

            }); 

            if (!firstCall)
            {
                firstCall = true;
                toggle.isOn = true;
                textList[0].text = "Lobby_Room";
            }
            //  SelectSession(sessiontestID);



            toggle.group = sessionButtonContentPanel.GetComponent<ToggleGroup>();

            toggle.group.allowSwitchOff = false;

         //   toggle.group.allowSwitchOff = true



            startSessionButton.interactable = true;

        }

        public  void CreateNewSession()
        {


            SessionInfo sessionInfo = new SessionInfo();
            sessionInfo.name = sessionNameInputField.text;
            

         //   sessionInfo.date = textList[1].text;
         //this one with the back slash does not work in unity
            string info =  JsonUtility.ToJson(sessionInfo);
          //  Debug.Log(info);

#if UNITY_WEBGL && !UNITY_EDITOR
            SocketIOJSLib.RequestSessionIdFromServer(info);
#endif

        }

        public void GoToNewSession()
        {
            //NetworkUpdateHandler.Instance.wasSessionChanged= true;
            SocketIOAdapter.Instance.LeaveSyncSession();
            SocketIOAdapter.Instance.LeaveChatSession();
            //LeaveSyncSession();

            //LeaveChatSession();

            SessionChangeInfo schangeInfo;
            schangeInfo.id = NetworkUpdateHandler.Instance.client_id;
            schangeInfo.oldSession_Id = NetworkUpdateHandler.Instance.session_id;


            schangeInfo.session_id = selectedSession;
            string data = JsonUtility.ToJson(schangeInfo);


          //  NetworkUpdateHandler.Instance.session_id = selectedSession;


#if UNITY_WEBGL && !UNITY_EDITOR
            SocketIOJSLib.RequestToEnteredNewSession(data);
            
#endif
            //ClientSpawnManager.Instance.RemoveAllClients();



#if UNITY_WEBGL && !UNITY_EDITOR
           // SocketIOJSLib.SendStateCatchUpRequest();
            //SocketIOJSLib.RequestClientNames(selectedSession);
#endif
        }



    }
}