using System.Collections;
using System.Collections.Generic;
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

        public void Start()
        {
            clientIDsToLabelGO = ClientSpawnManager.Instance.GetUsernameMenuDisplayDictionary();
        }
        //Dictionary<string, GameObject> clientIDsToLabelGO = new Dictionary<string, GameObject>();


        public void CreateTextFromString(string clientTextLabel, int clientID, bool isLocalClient = false)
        {
            if (isLocalClient)
            {
                if (!clientIDsToLabelGO.ContainsKey(clientID))
                    clientIDsToLabelGO.Add(clientID, mainClientName);


                mainClientName.text = "Logged in as: <b><color=white>" + clientTextLabel + "</color></b>";
               // mainClientName.color = Color.blue;

                return;
            }


            if (!clientIDsToLabelGO.ContainsKey(clientID))
            {
                //wait to create text until position is situated
                var newObj = Instantiate(textProfile);


                var newButton = newObj.GetComponent<Button>();
                newButton.onClick.AddListener(() =>
                {

                    shareMediaConnection.SetClientForMediaShare(clientID);


                });


                var newText = newObj.GetComponentInChildren<TMP_Text>(true);

              

                clientIDsToLabelGO.Add(clientID, newText);


              //  clientIDsToLabelGO[clientID] = newText;

                newText.text = clientTextLabel;
                newObj.transform.SetParent(transformToAddTextUnder, false);


                


                //ClientSpawnManager.Instance.AddToUsernameMenuLabelDictionary(clientID, newText);
            }
            else
                Debug.Log("CLIENT LABEL + " + clientTextLabel + " Already exist");
        }

        //public void CreateTextFromString(string clientTextLabel, int clientID)
        //{
        //    var clientIDsToLabelGO = ClientSpawnManager.Instance.GetUsernameMenuDisplayDictionary(); 

        //    if (!clientIDsToLabelGO.ContainsKey(clientTextLabel))
        //    {
        //        //wait to create text until position is situated
        //        var newObj = Instantiate(textProfile);

        //        clientIDsToLabelGO.Add(clientTextLabel, newObj);

        //        var newText = newObj.GetComponentInChildren<Text>(true);


        //        clientIDsToLabelGO[clientTextLabel] = newObj;

        //        newText.text = clientTextLabel;
        //        newObj.transform.SetParent(transformToAddTextUnder, false);


        //        ClientSpawnManager.Instance.AddToUsernameMenuLabelDictionary(clientID, newText);
        //    }
        //    else
        //        Debug.Log("CLIENT LABEL + " + clientTextLabel + " Already exist");
        //}

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

        //public async void DeleteClientID_Await(string clientID)
        //{
        //    if (clientIDsToLabelGO.ContainsKey(clientID))
        //    {
        //        while (clientIDsToLabelGO[clientID] == null)
        //            await Task.Delay(1);

        //        Destroy(clientIDsToLabelGO[clientID]);
        //        clientIDsToLabelGO.Remove(clientID);

        //    }
        //    else
        //        Debug.Log("Client Does not exist");

        //}

    }
//}
