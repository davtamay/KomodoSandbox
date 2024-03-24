//#define TESTING_BEFORE_BUILDING

//using UnityEngine;
//using System.Collections;
//using System.Runtime.InteropServices;
//using UnityEngine.XR;
//using WebXR;
//using CSCore.XAudio2;

//namespace Komodo.Runtime
//{


//    public class KomodoWebXRCamera : MonoBehaviour
//    {
//        public enum CameraID
//        {
//            Main,
//            LeftVR,
//            RightVR,
//            LeftAR,
//            RightAR
//        }

//        public Camera cameraMain, cameraMainEditor, cameraL, cameraR, cameraARL, cameraARR;
//        private WebXRState xrState = WebXRState.NORMAL;
//        private Rect leftRect, rightRect;
//        private int viewsCount = 1;

//        public void Start()
//        {
//            cameraMainEditor.gameObject.SetActive(false);
//        }
//        void OnEnable()
//        {

//#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
//            WebXRManager.OnXRChange += onVRChange;
//#else 
//            WebXRManagerEditorSimulator.OnXRChange += onVRChange;

//#endif

//            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;

//            cameraMain.transform.localPosition = new Vector3(0, 0, 0);
//        }

 
        
//        public void OnDestroy()
//        {
//            WebXRManager.OnXRChange -= onVRChange;
//            WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
//        }

//        public Camera GetCamera(CameraID cameraID)
//        {
//            switch (cameraID)
//            {
//                case CameraID.LeftVR:
//                    return cameraL;
//                case CameraID.RightVR:
//                    return cameraR;
//                case CameraID.LeftAR:
//                    return cameraARL;
//                case CameraID.RightAR:
//                    return cameraARR;
//            }
//#if UNITY_WEBGL && !UNITY_EDITOR
//            return cameraMain;
//#else 
//            return cameraMainEditor;
//#endif
//        }

//        private void onVRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
//        {
//            xrState = state;
//            this.viewsCount = viewsCount;
//            this.leftRect = leftRect;
//            this.rightRect = rightRect;

//            if (xrState == WebXRState.VR)
//            {
//#if UNITY_WEBGL && !UNITY_EDITOR
//                //set complete camera gameobject to false to prevent update calls from freeflight controller
//                cameraMainEditor.gameObject.SetActive(false);
//                cameraMain.gameObject.SetActive(false);
//#else 
//                cameraMainEditor.gameObject.SetActive(true);
//                cameraMain.gameObject.SetActive(false);
//#endif

//                cameraL.enabled = viewsCount > 0;
//                cameraL.rect = leftRect;
//                cameraR.enabled = viewsCount > 1;
//                cameraR.rect = rightRect;

//                cameraARL.enabled = false;
//                cameraARR.enabled = false;
//            }
//            else if (xrState == WebXRState.AR)
//            {
//                cameraMain.gameObject.SetActive(false);

//                cameraL.enabled = false;
//                cameraR.enabled = false;

//                cameraARL.enabled = viewsCount > 0;
//                cameraARL.rect = leftRect;
//                cameraARR.enabled = viewsCount > 1;
//                cameraARR.rect = rightRect;
//            }
//            else if (xrState == WebXRState.NORMAL)
//            {
//                cameraMainEditor.gameObject.SetActive(false);
//                cameraMain.gameObject.SetActive(true);

//                cameraL.enabled = false;
//                cameraR.enabled = false;

//                cameraARL.enabled = false;
//                cameraARR.enabled = false;
//            }
//        }
//private void OnHeadsetUpdate(
//        Matrix4x4 leftProjectionMatrix,
//        Matrix4x4 rightProjectionMatrix,
//        Quaternion leftRotation,
//        Quaternion rightRotation,
//        Vector3 leftPosition,
//        Vector3 rightPosition)
//    {
//      if (xrState == WebXRState.VR)
//      {
//        cameraL.transform.localPosition = leftPosition;
//        cameraL.transform.localRotation = leftRotation;
//        cameraL.projectionMatrix = leftProjectionMatrix;
//        cameraR.transform.localPosition = rightPosition;
//        cameraR.transform.localRotation = rightRotation;
//        cameraR.projectionMatrix = rightProjectionMatrix;
//      }
//      else if (xrState == WebXRState.AR)
//      {
//        cameraARL.transform.localPosition = leftPosition;
//        cameraARL.transform.localRotation = leftRotation;
//        cameraARL.projectionMatrix = leftProjectionMatrix;
//        cameraARR.transform.localPosition = rightPosition;
//        cameraARR.transform.localRotation = rightRotation;
//        cameraARR.projectionMatrix = rightProjectionMatrix;
//      }
//    }
     
//    }
//}









using UnityEngine;

namespace WebXR
{
    [DefaultExecutionOrder(-2019)]
    public class KomodoWebXRCamera : MonoBehaviour
    {
        public enum CameraID
        {
            Main,
            LeftVR,
            RightVR,
            LeftAR,
            RightAR
        }

        private static readonly string mainCameraTag = "MainCamera";
        private static readonly string untaggedTag = "Untagged";

        [SerializeField]
        private Camera cameraMain = null, cameraMainEditor = null, cameraL = null, cameraR = null, cameraARL = null, cameraARR = null;

        [SerializeField]
        private bool m_updateNormalFieldOfView = true;

        [SerializeField]
        private bool m_useNormalFieldOfViewFromAwake = true;

        [SerializeField]
        private float m_normalFieldOfView = 60f;

        [SerializeField]
        private bool m_updateNormalLocalPose = true;

        [SerializeField]
        private bool m_useNormalPoseFromAwake = true;

        [SerializeField]
        private Vector3 m_normalLocalPosition = Vector3.zero;

        [SerializeField]
        private Quaternion m_normalLocalRotation = Quaternion.identity;

        public bool UpdateNormalFieldOfView
        {
            get { return m_updateNormalFieldOfView; }
            set { m_updateNormalFieldOfView = value; }
        }

        public bool UseNormalFieldOfViewFromAwake
        {
            get { return m_useNormalFieldOfViewFromAwake; }
            set { m_useNormalFieldOfViewFromAwake = value; }
        }

        public float NormalFieldOfView
        {
            get { return m_normalFieldOfView; }
            set { m_normalFieldOfView = value; }
        }

        public bool UpdateNormalLocalPose
        {
            get { return m_updateNormalLocalPose; }
            set { m_updateNormalLocalPose = value; }
        }

        public bool UseNormalPoseFromAwake
        {
            get { return m_useNormalPoseFromAwake; }
            set { m_useNormalPoseFromAwake = value; }
        }

        public Vector3 NormalLocalPosition
        {
            get { return m_normalLocalPosition; }
            set { m_normalLocalPosition = value; }
        }

        public Quaternion NormalLocalRotation
        {
            get { return m_normalLocalRotation; }
            set { m_normalLocalRotation = value; }
        }



        private WebXRState xrState = WebXRState.NORMAL;
        private Rect leftRect, rightRect;

        private int viewsCount = 1;

        private Transform m_transform;

        [SerializeField]
        private bool updateCameraTag = false;

        private void Awake()
        {
            if (cameraMain == null)
            {
                return;
            }
            m_transform = cameraMain.transform;
            if (m_useNormalFieldOfViewFromAwake)
            {
                m_normalFieldOfView = cameraMain.fieldOfView;
            }
            if (m_useNormalPoseFromAwake)
            {
                m_normalLocalPosition = m_transform.localPosition;
                m_normalLocalRotation = m_transform.localRotation;
            }
        }

        //new
        //public void Start()
        //{
        //    cameraMainEditor.gameObject.SetActive(false);
        //}

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
            WebXRManager.OnViewsDistanceChange += OnViewsDistanceChange;
            OnXRChange(WebXRManager.Instance.XRState,
                        WebXRManager.Instance.ViewsCount,
                        WebXRManager.Instance.ViewsLeftRect,
                        WebXRManager.Instance.ViewsRightRect);


            //NEW
//#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
//                       WebXRManager.OnXRChange += OnXRChange;
//#else
//            WebXRManagerEditorSimulator.OnXRChange += OnXRChange;

//#endif

//                      WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;

//                       cameraMain.transform.localPosition = new Vector3(0, 0, 0);
            //        }
        }


        //NEW
        //public void OnDestroy()
        //{
        //    WebXRManager.OnXRChange -= OnXRChange;
        //    WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
        //}

//        public Camera GetCamera(CameraID cameraID)
//        {
//            switch (cameraID)
//            {
//                case CameraID.LeftVR:
//                    return cameraL;
//                case CameraID.RightVR:
//                    return cameraR;
//                case CameraID.LeftAR:
//                    return cameraARL;
//                case CameraID.RightAR:
//                    return cameraARR;
//            }
//#if UNITY_WEBGL && !UNITY_EDITOR
//                    return cameraMain;
//#else
//            return cameraMainEditor;
//#endif
//        }


        //NEW
        private void onVRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            this.viewsCount = viewsCount;
            this.leftRect = leftRect;
            this.rightRect = rightRect;

                    if (xrState == WebXRState.VR)
                    {
        #if UNITY_WEBGL && !UNITY_EDITOR
                        //set complete camera gameobject to false to prevent update calls from freeflight controller
                        cameraMainEditor.gameObject.SetActive(false);
                        cameraMain.gameObject.SetActive(false);
        #else 
                        cameraMainEditor.gameObject.SetActive(true);
                        cameraMain.gameObject.SetActive(false);
        #endif

                        cameraL.enabled = viewsCount > 0;
                        cameraL.rect = leftRect;
                        cameraR.enabled = viewsCount > 1;
                        cameraR.rect = rightRect;

                        cameraARL.enabled = false;
                        cameraARR.enabled = false;
                    }
                    else if (xrState == WebXRState.AR)
                    {
                        cameraMain.gameObject.SetActive(false);

                        cameraL.enabled = false;
                        cameraR.enabled = false;

                        cameraARL.enabled = viewsCount > 0;
                        cameraARL.rect = leftRect;
                        cameraARR.enabled = viewsCount > 1;
                        cameraARR.rect = rightRect;
                    }
                    else if (xrState == WebXRState.NORMAL)
{
    cameraMainEditor.gameObject.SetActive(false);
    cameraMain.gameObject.SetActive(true);

    cameraL.enabled = false;
    cameraR.enabled = false;

    cameraARL.enabled = false;
    cameraARR.enabled = false;
}
                }

        // void OnEnable()
        //        {

        //#if UNITY_WEBGL && !UNITY_EDITOR || TESTING_BEFORE_BUILDING
        //            WebXRManager.OnXRChange += onVRChange;
        //#else 
        //            WebXRManagerEditorSimulator.OnXRChange += onVRChange;

        //#endif

        //            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;

        //            cameraMain.transform.localPosition = new Vector3(0, 0, 0);
        //        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
            WebXRManager.OnViewsDistanceChange -= OnViewsDistanceChange;
        }

        private void SwitchXRState()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    cameraMain.enabled = false;
                    cameraL.enabled = false;
                    cameraR.enabled = false;
                    cameraARL.enabled = viewsCount > 0;
                    cameraARL.rect = leftRect;
                    cameraARR.enabled = viewsCount > 1;
                    cameraARR.rect = rightRect;
                    if (updateCameraTag)
                    {
                        cameraMain.tag = untaggedTag;
                        cameraL.tag = untaggedTag;
                        cameraARL.tag = mainCameraTag;
                    }
                    break;
                case WebXRState.VR:
                    cameraMain.enabled = false;
                    cameraL.enabled = viewsCount > 0;
                    cameraL.rect = leftRect;
                    cameraR.enabled = viewsCount > 1;
                    cameraR.rect = rightRect;
                    cameraARL.enabled = false;
                    cameraARR.enabled = false;
                    if (updateCameraTag)
                    {
                        cameraMain.tag = untaggedTag;
                        cameraL.tag = mainCameraTag;
                        cameraARL.tag = untaggedTag;
                    }
                    break;
                case WebXRState.NORMAL:
                    cameraMain.enabled = true;
                    cameraL.enabled = false;
                    cameraR.enabled = false;
                    cameraARL.enabled = false;
                    cameraARR.enabled = false;
                    if (updateCameraTag)
                    {
                        cameraMain.tag = mainCameraTag;
                        cameraL.tag = untaggedTag;
                        cameraARL.tag = untaggedTag;
                    }
                    if (m_updateNormalFieldOfView)
                    {
                        cameraMain.fieldOfView = m_normalFieldOfView;
                        cameraMain.ResetProjectionMatrix();
                    }
                    if (m_updateNormalLocalPose)
                    {
#if HAS_POSITION_AND_ROTATION
            m_transform.SetLocalPositionAndRotation(m_normalLocalPosition, m_normalLocalRotation);
#else
                        m_transform.localPosition = m_normalLocalPosition;
                        m_transform.localRotation = m_normalLocalRotation;
#endif
                    }
                    break;
            }
        }

        public Quaternion GetLocalRotation()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    return cameraARL.transform.localRotation;
                case WebXRState.VR:
                    return cameraL.transform.localRotation;
            }
            return cameraMain.transform.localRotation;
        }

        public Vector3 GetLocalPosition()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    if (viewsCount > 1)
                    {
                        return (cameraARL.transform.localPosition + cameraARR.transform.localPosition) * 0.5f;
                    }
                    return cameraARL.transform.localPosition;
                case WebXRState.VR:
                    return (cameraL.transform.localPosition + cameraR.transform.localPosition) * 0.5f;
            }
            return cameraMain.transform.localPosition;
        }

        public Camera GetCamera(CameraID cameraID)
        {
            switch (cameraID)
            {
                case CameraID.LeftVR:
                    return cameraL;
                case CameraID.RightVR:
                    return cameraR;
                case CameraID.LeftAR:
                    return cameraARL;
                case CameraID.RightAR:
                    return cameraARR;
            }
#if UNITY_WEBGL && !UNITY_EDITOR
                    return cameraMain;
#else
            return cameraMainEditor;
#endif




         //   return cameraMain;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            this.viewsCount = viewsCount;
            this.leftRect = leftRect;
            this.rightRect = rightRect;
            SwitchXRState();
        }

        private void OnHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix)
        {
            if (xrState == WebXRState.VR)
            {
                cameraL.projectionMatrix = leftProjectionMatrix;
                cameraR.projectionMatrix = rightProjectionMatrix;
            }
            else if (xrState == WebXRState.AR)
            {
                cameraARL.projectionMatrix = leftProjectionMatrix;
                cameraARR.projectionMatrix = rightProjectionMatrix;
            }
            if (viewsCount == 1)
            {
                cameraMain.projectionMatrix = leftProjectionMatrix;
            }
        }

        private void OnViewsDistanceChange(float distance)
        {
            float halfDistance = distance * 0.5f;
            var left = new Vector3(-halfDistance, 0f, 0f);
            var right = new Vector3(halfDistance, 0f, 0f);
            cameraL.transform.localPosition = left;
            cameraR.transform.localPosition = right;
            cameraARL.transform.localPosition = left;
            cameraARR.transform.localPosition = right;
        }
    }
}
