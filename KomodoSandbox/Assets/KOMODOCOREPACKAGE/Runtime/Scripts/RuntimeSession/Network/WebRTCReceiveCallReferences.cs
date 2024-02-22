using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WebRTCReceiveCallReferences : MonoBehaviour
{
    public Button rejectCallButton;
    public Button receiveCallButton;
    public int clientId;
    public TMP_Text customReceiveText;


    public void SetupPanel(int fromClientID, string nameOfClient, System.Action<int> onReject, System.Action<int> onAccept)
    {
        clientId = fromClientID;
        customReceiveText.text = $"Receiving Call From: \n{nameOfClient}";

        gameObject.SetActive(true);

        receiveCallButton.onClick.RemoveAllListeners();
        rejectCallButton.onClick.RemoveAllListeners();

        rejectCallButton.onClick.AddListener(() => onReject(fromClientID));
        receiveCallButton.onClick.AddListener(() => onAccept(fromClientID));
    }

    // Method to reset the panel for reuse
    public void ResetPanel()
    {
        gameObject.SetActive(false);
        receiveCallButton.onClick.RemoveAllListeners();
        rejectCallButton.onClick.RemoveAllListeners();
    }
}
