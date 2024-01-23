using UnityEngine;
using Komodo.Utilities;

public class InternetConnectivityManager : SingletonComponent<InternetConnectivityManager>
{
    public static InternetConnectivityManager Instance
    {
        get { return (InternetConnectivityManager)_Instance; }

        set { _Instance = value; }
    }

    public event System.Action<bool> OnInternetConnectivityChanged;

    private void Update()
    {
        NetworkReachability reachability = Application.internetReachability;

        // Check if the status has changed
        bool isConnected = reachability != NetworkReachability.NotReachable;

        // Invoke the event if it has changed
        if (isConnected != lastIsConnectedStatus)
        {
            lastIsConnectedStatus = isConnected;
            OnInternetConnectivityChanged?.Invoke(isConnected);
        }
    }

    private bool lastIsConnectedStatus = false;
}
