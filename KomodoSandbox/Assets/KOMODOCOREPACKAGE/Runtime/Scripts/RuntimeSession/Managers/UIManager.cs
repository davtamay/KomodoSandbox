using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;
using Komodo.Utilities;
using TMPro;
using System.Reflection;

public struct ToggleState
{
    public int clientID;
    public int guid;
    public bool state;
}

//namespace Komodo.Runtime
//{
   
    public class UIManager : SingletonComponent<UIManager>
    {
        public static UIManager Instance
        {
            get { return ((UIManager)_Instance); }
            set { _Instance = value; }
        }

        [Header("Player Menu")]
        
        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu")]
        public GameObject settingsMenu;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> HeightCalibration")]
        public GameObject heightCalibration;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> CalibrateHeightButton")]
        public GameObject calibrationButtons;

        [Tooltip("Hierarchy: KomodoMenu -> Panels -> SettingsMenu -> NotCalibrating -> ManuallyAdjustHeight")]
        public GameObject manuallyAdjustHeight;

        [Tooltip("Hierarchy: The create tab in player menu")]
        public GameObject createTab;

        [Tooltip("Hierarchy: The Instructor menu button in the settings tab")]
        public GameObject instructorMenuButton;
        
        public GameObject menuPrefab;

        [ShowOnly]
        public GameObject menu;

        [ShowOnly]
        public MainUIReferences menuReferences;

        [ShowOnly]
        public CanvasGroup menuCanvasGroup;

        [ShowOnly]
        public Canvas menuCanvas;

        [ShowOnly]
        private RectTransform menuTransform;

        [ShowOnly]
        public HoverCursor hoverCursor;

        private bool _isRightHanded;

        private bool _isMenuVisible;

        //Default values to use to move menu for XR hands 
        public Vector3 eitherHandRectScale = new Vector3(0.001f, 0.001f, 0.001f);

        public Vector3 leftHandedMenuRectRotation = new Vector3(-30, 180, 180);

        public Vector3 leftHandedMenuRectPosition;

        public GameObject leftHandedMenuAnchor;

        public Vector3 rightHandedMenuRectRotation = new Vector3(-30, 180, 180);

        public Vector3 rightHandMenuRectPosition;

        public GameObject rightHandedMenuAnchor;

        [Header("Initial Loading Process UI")]

        public CanvasGroup initialLoadingCanvas;

        public Text initialLoadingCanvasProgressText;

        [ShowOnly]
        public bool isModelButtonListReady;

        [ShowOnly]
        public bool isSceneButtonListReady;

        [HideInInspector]
        public ChildTextCreateOnCall clientTagSetup;

        //References for displaying user name tags and speechtotext text
        private List<Text> clientUser_Names_UITextReference_list = new List<Text>();

        private List<Text> clientUser_SpeechToText_UITextReference_list = new List<Text>();

        [HideInInspector]
        public Dictionary<int, VisibilityToggle> modelVisibilityToggleList = new Dictionary<int, VisibilityToggle>();

        [HideInInspector]
        public Dictionary<int, LockToggle> modelLockToggleList = new Dictionary<int, LockToggle>();

        [HideInInspector]
        public Dictionary<int, int> entityIDtoButtonIDDictionary = new Dictionary<int, int>();
        public Dictionary<int, int> buttonIDtoEntityIDDictionary = new Dictionary<int, int>();

        [HideInInspector]
        public TMP_Text sessionAndBuildName;

        private ToggleExpandability menuExpandability;

        [Header("UI Cursor for Menu")]

        [ShowOnly]
        public GameObject cursor;

        [ShowOnly]
        public GameObject cursorGraphic;

        private Image cursorImage;


    public GameObject mainMenu;
    public GameObject localVideo;
    public GameObject remoteVideos;
   // public GameObject shareMenu;

        public void Awake()
        {
            // instantiates this singleton in case it doesn't exist yet.
            var uiManager = Instance;

            //clientManager = ClientSpawnManager.Instance;

            if (menuPrefab == null)
            {
                throw new System.Exception("You must set a menuPrefab");
            }
        }

        public void Start () {


            GlobalMessageManager.Instance.Subscribe("render", (s) => {

                var data = JsonUtility.FromJson<ToggleState>(s);

                ProcessNetworkToggleVisibility(data.guid, data.state);
                
            });

            GlobalMessageManager.Instance.Subscribe("lock", (s) => {

                var data = JsonUtility.FromJson<ToggleState>(s);

                ProcessNetworkToggleLock(data.guid, data.state);

            });


            //  GlobalMessageManager.Instance.Subscribe("visibility")
            menu = GameObject.FindWithTag(TagList.menuUI);

            // create a menu if there isn't one already
            if (menu == null) 
            {
                Debug.LogWarning("Couldn't find an object tagged MenuUI in the scene, so creating one now");

                menu = Instantiate(menuPrefab);
            }

            hoverCursor = menu.GetComponentInChildren<HoverCursor>(true);
            //TODO -- fix this, because right now Start is not guaranteed to execute after the menu prefab has instantiated its components.

            if (hoverCursor == null) {
                Debug.LogWarning("You must have a HoverCursor component");
            }

            if (hoverCursor.cursorGraphic == null)
            { 
                Debug.LogWarning("HoverCursor component does not have a cursorGraphic property");
            }

            cursor = hoverCursor.cursorGraphic.transform.parent.gameObject; //TODO -- is there a shorter way to say this?

            cursorGraphic = hoverCursor.cursorGraphic.gameObject;

            menuCanvas = menu.GetComponentInChildren<Canvas>(true);

            if (menuCanvas == null)
            {
                throw new System.Exception("You must have a Canvas component");
            }

            menuCanvasGroup = menu.GetComponentInChildren<CanvasGroup>(true);

            if (menuCanvasGroup == null)
            {
                throw new System.Exception("You must have a CanvasGroup component");
            }

            menuTransform = menuCanvas.GetComponent<RectTransform>();
           
            clientTagSetup = menu.GetComponent<ChildTextCreateOnCall>();

            sessionAndBuildName = menu.GetComponent<MainUIReferences>().sessionAndBuildText;

            if (menuTransform == null) 
            {
                throw new Exception("selection canvas must have a RectTransform component");
            }

            if (rightHandedMenuAnchor == null)
            {
                throw new System.Exception("You must set a right-handed menu anchor");
            }

            if (leftHandedMenuAnchor == null)
            {
                throw new System.Exception("You must set a left-handed menu anchor");
            }
            
            menu.transform.SetParent(leftHandedMenuAnchor.transform);
            
            if (!sessionAndBuildName)
            {
                Debug.LogWarning("sessionAndBuildName was null. Proceeding anyways.");
            }
            
            menuExpandability = menuCanvas.GetComponent<ToggleExpandability>();
    
            if (menuExpandability == null)
            {
                Debug.LogError("No ToggleExpandability component found", this);
            }

            cursorImage = menuCanvas.GetComponent<Image>();
    
            if (cursorImage == null) 
            {
                Debug.LogError("No Image component found on UI ", this);
            }

            DisplaySessionDetails();
        }

        public void EnableCursor ()
        {
            //use ghost cursor on the menu in XR mode
            cursorImage.enabled = true;
        }

        public void DisableCursor ()
        {
            //use ghost cursor on the menu in XR mode
            cursorImage.enabled = false;
        }

        private void DisplaySessionDetails ()
        {
            sessionAndBuildName.text = "In Session: " + NetworkUpdateHandler.Instance.sessionName;

         //   sessionAndBuildName.text += Environment.NewLine +  NetworkUpdateHandler.Instance.buildName;
        }

        public bool GetCursorActiveState() 
        { 
            return cursorGraphic.activeInHierarchy;
        }

        public void ConvertMenuToAlwaysExpanded ()
        {
            menuExpandability.ConvertToAlwaysExpanded();
        }

        public void ConvertMenuToExpandable (bool isExpanded)
        {
            menuExpandability.ConvertToExpandable(isExpanded);
        }

        public void ToggleModelVisibility (int index, bool doShow)
        {
            //If buttons are setup but not the model we have to not invoke this if not found.

            GameObject gObject = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index).gameObject;

            if(gObject != null) 
            gObject.SetActive(doShow);

         
        }

        public void SendMenuVisibilityUpdate(bool visibility)
        {
           
            if (visibility) 
            {

                NetworkUpdateHandler.Instance.SendSyncInteractionMessage (new Interaction 
                {

                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                interactionType = (int)INTERACTIONS.SHOW_MENU,

                targetEntity_id = 0,
                });
            } else {
                NetworkUpdateHandler.Instance.SendSyncInteractionMessage (new Interaction 
                {

                sourceEntity_id = NetworkUpdateHandler.Instance.client_id,

                interactionType = (int)INTERACTIONS.HIDE_MENU,

                targetEntity_id = 0,
                });
            }
        }

        public void SendVisibilityUpdate (int index, bool doShow)
        {

            var nObject = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index);
            if (!nObject)
            {
                Debug.LogError("no NetworkedGameObject found on index in ClientSpawnManager.cs");
                return;
            }

            //GameObject gObject = nObject.gameObject;
           

            NetworkedGameObject netObject = nObject.gameObject.GetComponent<NetworkedGameObject>();

            if (!netObject)
            {
                Debug.LogError("no NetworkedGameObject component found on currentObj in ClientSpawnManager.cs");
                return;
            }
             var entityID =   buttonIDtoEntityIDDictionary[index];
 
            //  int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.entity).entityID;

            ToggleState ts = new ToggleState
            {
                clientID = NetworkUpdateHandler.Instance.client_id,
                guid = entityID,//netObject.thisEntityID,//entityID,
                state = doShow,

            };

           new KomodoMessage("render",JsonUtility.ToJson(ts)).Send();

            //if (doShow)
            //{
            //    NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            //    {
            //        sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
            //        targetEntity_id = entityID,
            //        interactionType = (int) INTERACTIONS.SHOW,
            //    });
            //}
            //else
            //{
            //    NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            //    {
            //        sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
            //        targetEntity_id = entityID,
            //        interactionType = (int) INTERACTIONS.HIDE,
            //    });
            //}

        }
        public void SendLockUpdate(int index,  bool doLock)
        {
            Debug.Log("TOGGLEING + " + index);
            var nObject = NetworkedObjectsManager.Instance.GetNetworkedGameObject(index);

           

            if (!nObject)
            {
                Debug.LogError("no NetworkedGameObject found on index in ClientSpawnManager.cs");
                return;
            }

            GameObject gObject = nObject.gameObject;

            NetworkedGameObject netObject = gObject.GetComponent<NetworkedGameObject>();

            if (!netObject)
            {
                Debug.LogError("no NetworkedGameObject component found on currentObj in ClientSpawnManager.cs");
                return;
            }
            Debug.Log("LOCKING" + nObject.name);
            //thisEntityID is 0 on the receiving end for some reason...

            //netObject.thisEntityID
            //return;
            // int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.entity).entityID;
            
            var entityID = buttonIDtoEntityIDDictionary[index];


            ToggleState ts = new ToggleState
            {
                clientID = NetworkUpdateHandler.Instance.client_id,
                guid = entityID,//netObject.thisEntityID,//netObject.thisEntityID,//entityID,
                state = doLock,

            };



            new KomodoMessage("lock", JsonUtility.ToJson(ts)).Send();
            
            
            
            // int lockState = 0;

            // //SETUP and send network lockstate
            // if (doLock)
            // {
            //     lockState = (int)INTERACTIONS.LOCK;
            // }
            // else
            // {
            //     lockState = (int)INTERACTIONS.UNLOCK;
            // }

            // int entityID = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(NetworkedObjectsManager.Instance.GetNetworkedSubObjectList(this.index)[0].Entity).entityID;

            // NetworkUpdateHandler.Instance.SendSyncInteractionMessage(new Interaction
            // {
            //     sourceEntity_id = NetworkUpdateHandler.Instance.client_id,
            //     targetEntity_id = entityID,
            //     interactionType = lockState,
            // });
        }

        /* TODO: implement these two functions. Right now they don't work because ProcessNetworkToggleVisibility expects an entityID, not an index.
        [ContextMenu("Test Process Network Show Model 0")]
        public void TestProcessNetworkShow()
        {
            ProcessNetworkToggleVisibility(0, true);
        }

        [ContextMenu("Test Process Network Hide Model 0")]
        public void TestProcessNetworkHide()
        {
            ProcessNetworkToggleVisibility(0, false);
        }
        */

        /// <summary>
        /// Show or hide a model via a network update
        /// </summary>
        /// <param name="entityID"></param>
        /// <param name="doShow"></param>
        public void ProcessNetworkToggleVisibility(int index, bool doShow)
        {
            Debug.Log($"Button Index: {index} ");
       //     Debug.Log($"EntityID : {buttonIDtoEntityIDDictionary[index]} ");

            if (!entityIDtoButtonIDDictionary.ContainsKey(index))
            {

                Debug.Log($"ENTITYID {index} TO BUTTON ID DICTIONARY DOES NOT CONTATIN BUTTON ID ");
                return;
            }


            var buttonID = entityIDtoButtonIDDictionary[index];


            if (buttonID > modelVisibilityToggleList.Count || !modelVisibilityToggleList[buttonID])
            {
                Debug.LogError($"Tried to change state of model lock button, but there was none with index {buttonID}");

                return;
            }

            modelVisibilityToggleList[buttonID].ProcessNetworkToggle(doShow);
        }


        public void ProcessNetworkToggleLock (int index, bool doLock)
        {
            Debug.Log("received LOCK : " + index);

            if (!entityIDtoButtonIDDictionary.ContainsKey(index))
            {

                Debug.Log($"ENTITYID {index} TO BUTTON ID DICTIONARY DOES NOT CONTATIN BUTTON ID ");
                return;
            }


            var buttonID = UIManager.Instance.entityIDtoButtonIDDictionary[index];

            //  index =  NetworkedObjectsManager.Instance.networkedObjectFromEntityId[index];

            //   Debug.Log("After received LOCK : " + index);
            if (buttonID > modelLockToggleList.Count || !modelLockToggleList[buttonID])
            {
                Debug.LogError($"Tried to change state of model lock button, but there was none with index {index}");

                return;
            }

            modelLockToggleList[buttonID].ProcessNetworkToggle(doLock, buttonID);
        }

      

        public void ToggleMenuVisibility(bool activeState)
        {
            if (menuCanvasGroup == null) {
                Debug.LogWarning("Tried to toggle visibility for menuCanvasGroup, but it was null. Skipping.");

                return;
            }

            if (activeState)
            {
                menuCanvasGroup.alpha = 1;

                menuCanvasGroup.blocksRaycasts = true;

                SendMenuVisibilityUpdate(activeState);
            }
            else
            {
                menuCanvasGroup.alpha = 0;

                menuCanvasGroup.blocksRaycasts = false;

                SendMenuVisibilityUpdate(activeState);
            }
        }

        public void ToggleLeftHandedMenu ()
        {
            if (_isRightHanded && _isMenuVisible)
            {
                SetLeftHandedMenu();

                return;
            }

            if (_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);

                SetLeftHandedMenu();

                return;
            }

            if (!_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);

                return;
            }

            // the menu is already left-handed and already visible.
            ToggleMenuVisibility(false);
        }

        public void ToggleRightHandedMenu ()
        {
            if (!_isRightHanded && _isMenuVisible)
            {
                SetRightHandedMenu();
                return;
            }

            if (!_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);
                SetRightHandedMenu();
                return;
            }

            if (_isRightHanded && !_isMenuVisible)
            {
                ToggleMenuVisibility(true);
                return;
            }

            // the menu is already right-handed and already visible.
            ToggleMenuVisibility(false);
        }

        public void SetLeftHandedMenu() {
       
        mainMenu.transform.SetSiblingIndex(2);
        localVideo.transform.SetSiblingIndex(1);
        remoteVideos.transform.SetSiblingIndex(0);


        SetHandednessAndPlaceMenu(false);
        }

        public void SetRightHandedMenu() {
      
        remoteVideos.transform.SetSiblingIndex(2);
        localVideo.transform.SetSiblingIndex(1);
        mainMenu.transform.SetSiblingIndex(0);


        //mainMenu.transform.SetSiblingIndex(1);
            SetHandednessAndPlaceMenu(true);
        }

        public void SetHandednessAndPlaceMenu(bool isRightHanded) {
            SetMenuHandedness(isRightHanded);

            PlaceMenuOnCurrentHand();
        }

        public void SetMenuHandedness (bool isRightHanded) {
            _isRightHanded = isRightHanded;
        }

        public void PlaceMenuOnCurrentHand () {
            if (menuCanvas == null) {
                Debug.LogWarning("Could not find menu canvas. Proceeding anyways.");
            }

            Camera leftHandEventCamera = null;

            Camera rightHandEventCamera = null;

            if (EventSystemManager.IsAlive)
            {
                 leftHandEventCamera = EventSystemManager.Instance.inputSource_LeftHand.eventCamera;

                 rightHandEventCamera = EventSystemManager.Instance.inputSource_RighttHand.eventCamera;
            }

            menuTransform.localScale = eitherHandRectScale;

            //enables menu selection laser
            if (_isRightHanded)
            {
                menu.transform.SetParent(rightHandedMenuAnchor.transform);

                menuTransform.localRotation = Quaternion.Euler(rightHandedMenuRectRotation);

                menuTransform.anchoredPosition3D = rightHandMenuRectPosition;

                menuCanvas.worldCamera = rightHandEventCamera;
            }
            else
            {
                menu.transform.SetParent(leftHandedMenuAnchor.transform);

                menuTransform.localRotation = Quaternion.Euler(leftHandedMenuRectRotation);

                menuTransform.anchoredPosition3D = leftHandedMenuRectPosition;

                menuCanvas.worldCamera = leftHandEventCamera;
            }

            menuTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 500); //TODO this might have to go after renderMode changes

            menuCanvas.renderMode = RenderMode.WorldSpace;
        }

        public bool IsReady ()
        {
            //check managers that we are using for our session
            if (!SceneManagerExtensions.IsAlive && !ModelImportInitializer.IsAlive) {
                return true;
            }

            if (SceneManagerExtensions.IsAlive && !ModelImportInitializer.IsAlive) {
                return isSceneButtonListReady;
            }

            if (!SceneManagerExtensions.IsAlive && ModelImportInitializer.IsAlive) {
                return isModelButtonListReady;
            }

            if (SceneManagerExtensions.IsAlive && ModelImportInitializer.IsAlive)
            {
                return isModelButtonListReady && isSceneButtonListReady;
            }

            return false;
        }

        /// <summary> 
        /// This function will enable Create and Height Calibration Panels for VR view.
        /// </summary>
        public void HeightCalibrationButtonsSettings(bool state) 
        {
            heightCalibration.gameObject.SetActive(state);
            calibrationButtons.gameObject.SetActive(state);
            manuallyAdjustHeight.gameObject.SetActive(state);
        }

        public void EnableCreateMenu(bool state)
        {
            createTab.gameObject.SetActive(state);
        }

        public void EnableInstructorMenuButton(bool state)
        {
            instructorMenuButton.gameObject.SetActive(state);
        }

        public void EnableIgnoreLayoutForVRmode(bool state) 
        {
            LayoutElement RecenterButton = settingsMenu.transform.Find("NotCalibrating").transform.Find("RecenterButton").GetComponent<LayoutElement>();

            LayoutElement settingsMenuTitle = settingsMenu.transform.Find("Text").GetComponent<LayoutElement>();

            RecenterButton.ignoreLayout = state;
            settingsMenuTitle.ignoreLayout = state;
        }

        public void SwitchMenuToDesktopMode() 
        {
            DisableCursor();

            //TODO: One of the above actually does the job. Which is it?
            menuCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        //    ConvertMenuToExpandable(false);

          //  HeightCalibrationButtonsSettings(false);

            //EnableCreateMenu(false);

          //  HeightCalibrationButtonsSettings(false);

            EnableInstructorMenuButton(true);

            createTab.GetComponent<TabButton>().onTabDeselected.Invoke();

           // LayoutRebuilder.ForceRebuildLayoutImmediate(settingsMenu.GetComponent<RectTransform>());

        }
    }
//}
