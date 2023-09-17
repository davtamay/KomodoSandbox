using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Komodo.Runtime
{
    public static class SocketIOJSLib
    {
        public static int SUCCESS = 0;

        public static int FAILURE = 1;

        // import callable js functions
        // socket.io with webgl
        // https://www.gamedev.net/articles/programming/networking-and-multiplayer/integrating-socketio-with-unity-5-webgl-r4365/
        [DllImport("__Internal")]
        public static extern string SetSocketIOAdapterName(string name);

        [DllImport("__Internal")]
        public static extern int SetSyncEventListeners();

        [DllImport("__Internal")]
        public static extern int OpenSyncConnection();

        [DllImport("__Internal")]
        public static extern int OpenChatConnection();

        [DllImport("__Internal")]
        public static extern int JoinSyncSession(int session_id);

        [DllImport("__Internal")]
        public static extern int JoinChatSession(int session_id);

        [DllImport("__Internal")]
        public static extern int SendStateCatchUpRequest();

        [DllImport("__Internal")]
        public static extern int SetChatEventListeners();

        //[DllImport("__Internal")]
        //public static extern int GetClientIdFromBrowser();

        //[DllImport("__Internal")]
        //public static extern int GetSessionIdFromBrowser();

        [DllImport("__Internal")]
        public static extern int GetIsTeacherFlagFromBrowser();





        //[DllImport("__Internal")]
        //public static extern void ListenForPageUnload(int client_id);

        [DllImport("__Internal")]
        public static extern void RequestClientNames(int session_id);


        //[DllImport("__Internal")]
        //public static extern void SetSessionId(int session_id);



        [DllImport("__Internal")]
        public static extern void RequestClientIdFromServer();




        [DllImport("__Internal")]
        public static extern void RequestUUIDFromServer();





        [DllImport("__Internal")]
        public static extern void ProvideClientDataToServer(string clientData);
        //ProvideClientDataToServer

        //set listener to get the request info
        [DllImport("__Internal")]
        public static extern void ListenForClientIdFromServer();




        //send an emit to request info

        [DllImport("__Internal")]
        public static extern void RequestSessionIdFromServer(string sessionInfo);

        //set listener to get the request info
        //[DllImport("__Internal")]
        //public static extern void ListenForSessionIdFromServer();


        [DllImport("__Internal")]
        public static extern void RequestAllSessionIdsFromServer();

        //[DllImport("__Internal")]
        //public static extern void ListenForSessionIdsFromServer();

        [DllImport("__Internal")]
        public static extern void RequestLobbySessionFromServer();


        [DllImport("__Internal")]
        public static extern void RequestToEnteredNewSession(string sessionInfo);

        //[DllImport("__Internal")]
        //public static extern void ReceiveClientInSessionNames(string names);

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceivePosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendPosition(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SocketIOSendInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitSocketIOReceiveInteraction(int[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void InitReceiveDraw(float[] array, int size);

        // [DllImport("__Internal")]
        // private static extern void SendDraw(float[] array, int size);

        [DllImport("__Internal")]
        public static extern int EnableVRButton();

        [DllImport("__Internal")]
        public static extern string GetSessionDetails();

        // TODO(rob): move this to GlobalMessageManager.cs
        //sendTo = -1 (To all), 0 (To all Except sender), clientID (to target clientID)
        [DllImport("__Internal")]
        public static extern void BrowserEmitMessage(string type, string message, int sendTo = 0);

        [DllImport("__Internal")]
        public static extern int LeaveSyncSession();

        [DllImport("__Internal")]
        public static extern int LeaveChatSession();

        [DllImport("__Internal")]
        public static extern int CloseSyncConnection();

        [DllImport("__Internal")]
        public static extern int CloseChatConnection();
    }
}