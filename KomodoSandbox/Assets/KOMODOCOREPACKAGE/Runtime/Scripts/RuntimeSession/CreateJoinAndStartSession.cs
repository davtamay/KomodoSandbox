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
    public struct SessionInfoArray
    {
        public List<SessionInfo> sessionInfos;

    }

    public class CreateJoinAndStartSession : MonoBehaviour
    {
        public GameObject prefabSessionButton;
        public Transform sessionButtonContentPanel;
        public TMPro.TMP_InputField sessionNameInputField;

        public Button startSessionButton;


        public void Awake()
        {

#if UNITY_WEBGL && !UNITY_EDITOR
            SocketIOJSLib.OpenSyncConnection();
            SocketIOJSLib.OpenChatConnection();

            SocketIOAdapter.Instance.SetName();
          
            
            SocketIOJSLib.ListenForSessionIdFromServer();
            SocketIOJSLib.ListenForSessionIdsFromServer();

#endif
        }
        public void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
        SocketIOJSLib.RequestAllSessionIdsFromServer();




      //  SocketIOJSLib.ListenForSessionIdFromServer();
#endif
        }
      
        public List<int> allSessions = new List<int>();

        public void SelectSession(int session)
        {

            NetworkUpdateHandler.Instance.session_id = session;
            // Debug.Log(session);
           // Debug.Log("SESSION NUMBER SET : " + session);
        }
        //public int currentSelectedSession;

        //public void CreateNewSession()
        //{
        //    CreateNewSession();
        //    //StartCoroutine();
        //}
        TMP_Text[] textList;
        public void ServerCreate(int sessiontestID, string name = "", string date = "", bool fromServer = false) {

          //  Debug.Log("RECEIVED SESSION NUMBER:" + sessiontestID);

            if (!allSessions.Contains(sessiontestID))
                allSessions.Add(sessiontestID);
            else
                return;


            var sessionButton = Instantiate(prefabSessionButton, sessionButtonContentPanel);
            
            
            sessionButton.name = sessiontestID.ToString();


             textList = sessionButton.GetComponentsInChildren<TMPro.TMP_Text>();


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
            toggle.isOn = true;

            toggle.onValueChanged.AddListener((isON) =>
            {

                if (toggle.isOn)
                {
                    SelectSession(sessiontestID);
                }

            }); ;

            SelectSession(sessiontestID);



            toggle.group = sessionButtonContentPanel.GetComponent<ToggleGroup>();





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
    }
}