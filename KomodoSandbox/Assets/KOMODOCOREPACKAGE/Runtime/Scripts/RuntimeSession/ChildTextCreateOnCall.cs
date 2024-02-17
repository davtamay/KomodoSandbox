using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//namespace Komodo.Runtime
//{
public class ChildTextCreateOnCall : MonoBehaviour
{
    public Transform transformToAddTextUnder;
    public GameObject textProfile;

    Dictionary<int, TMP_Text> clientIDsToLabelGO = new Dictionary<int, TMP_Text>();

    public TMP_Text mainClientName;

    public ShareMediaConnection shareMediaConnection;

    //public GameObject CallReceivePanel;
    //public TMP_Text customReceiveText;







    public void Start()
    {
        clientIDsToLabelGO = ClientSpawnManager.Instance.GetUsernameMenuDisplayDictionary();
        //  ReceivedCall(2);

      //  CreateTextFromString("assas", 5, false);


    }

    public void CreateTextFromString(string clientTextLabel, int clientID, bool isLocalClient = false)
    {



        if (isLocalClient)
        {
            if (!clientIDsToLabelGO.ContainsKey(clientID))
                clientIDsToLabelGO.Add(clientID, mainClientName);
            else
                clientIDsToLabelGO[clientID] = mainClientName;


            mainClientName.text = "Logged in as: <b><color=white>" + clientTextLabel + "</color></b>";

            return;
        }


        if (!clientIDsToLabelGO.ContainsKey(clientID))
        {
            //wait to create text until position is situated
            var newObj = Instantiate(textProfile);
          


            //main client button
            //var newButton = newObj.GetComponent<Button>();
            //    newButton.onClick.AddListener(() =>
            //    {
            //        shareMediaConnection.SetClientForMediaShare(clientID);
            //    });

            var newText = newObj.GetComponentInChildren<TMP_Text>(true);

            clientIDsToLabelGO.Add(clientID, newText);

            newText.text = clientTextLabel;


            var references = newObj.GetComponent<ClientConnectionReferences>();
            references.clientID = clientID;

            Toggle toggle = references.selectedToggle();

            toggle.onValueChanged.AddListener((v) => { 
            
                if (shareMediaConnection.clientIDToToggleStateDictionary.ContainsKey(clientID))//TryGetValue(clientID, out var newState))
                {
                    shareMediaConnection.clientIDToToggleStateDictionary[clientID] = v;
                }
                else
                {
                    shareMediaConnection.clientIDToToggleStateDictionary.Add(clientID, v);
                }
            
            });
           
            if (shareMediaConnection.clientIDToToggleDictionary.ContainsKey(clientID))
                shareMediaConnection.clientIDToToggleDictionary[clientID] = toggle;
            else

            shareMediaConnection.clientIDToToggleDictionary.Add(clientID, toggle);
            //references.MakeCallButton.onClick.AddListener(() =>
            //{
            //    string name = ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID);
            ////    Debug.Log($"Calling Client:{ clientID}");
            //    Debug.Log("GET PLAYER FROM ID : " + name);
            //    SocketIOJSLib.CallClient(name);

            //});
            //references.AcceptCallButton.onClick.AddListener(() =>
            //{

            //    SocketIOJSLib.AnswerClientOffer(ClientSpawnManager.Instance.GetPlayerNameFromClientID(clientID));
            //});

            //references.cancelCall.onClick.AddListener(() =>
            //{
            //    SocketIOJSLib.HangUpClient();
            //  //  ShareMediaConnection.Instance.Hangup();

            //});



            newObj.transform.SetParent(transformToAddTextUnder, false);


            //ClientSpawnManager.Instance.AddToUsernameMenuLabelDictionary(clientID, newText);
        }
        else
            Debug.Log("CLIENT LABEL + " + clientTextLabel + " Already exist");
    }

    public void DeleteTextFromString(int clientID)
    {
        DeleteClientID_Await(clientID);
    }

    public async void DeleteClientID_Await(int clientID)
    {
        if (clientIDsToLabelGO.ContainsKey(clientID))
        {
            while (!clientIDsToLabelGO.ContainsKey(clientID)) //clientIDsToLabelGO[clientID] == null)
                await Task.Delay(1);

            //DELETE Button Parent not only test
            Destroy(clientIDsToLabelGO[clientID].transform.parent.gameObject);
            clientIDsToLabelGO.Remove(clientID);

        }
        else
            Debug.Log("Client Does not exist");

    }

    public void ReceivedCall(int fromClientID)
    {
        ShareMediaConnection.Instance.ReceivedCall(fromClientID);

        //string nameOfClient = ClientSpawnManager.Instance.GetPlayerNameFromClientID(fromClientID);

        //customReceiveText.text = $"Receiving Call From: \n {nameOfClient} ";


        //CallReceivePanel.SetActive(true);

        //var buttons = CallReceivePanel.GetComponentsInChildren<Button>(true);

        //buttons[0].onClick.RemoveAllListeners();
        //buttons[1].onClick.RemoveAllListeners();

        ////reject call
        //buttons[0].onClick.AddListener(() =>
        //{
        //    SocketIOJSLib.RejectClientOffer(fromClientID);
        //    CallReceivePanel.SetActive(false);
        //});


        ////accept call
        //buttons[1].onClick.AddListener(() =>
        //{

        //    SocketIOJSLib.AnswerClientOffer(nameOfClient);
        //    // AcceptCallFromClient(nameOfClient);
        //    CallReceivePanel.SetActive(false);
        //    shareMediaConnection.SetClientForMediaShare(fromClientID);
        //    shareMediaConnection.SetMediaShareType(2);// call
        //    shareMediaConnection.shareMediaUI.SetActive(true);
        //});




    }


    //public void ReceivedOfferAnswer(int fromClientID)
    //{
    //    Debug.Log("RECEIVEDOFFER FROM ANSWER");
    //    CallReceivePanel.SetActive(false);
    //    ShareMediaConnection.Instance.videoTransformList[0].gameObject.SetActive(true);
    //}
    //public void EndCall(string roomName)
    //{
    //    Debug.Log("ENDED CALL FOR ROOMNAME");
    //    ShareMediaConnection.Instance.videoTransformList[0].gameObject.SetActive(false);
    //}




}
//}
