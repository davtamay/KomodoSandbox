using SixLabors.ImageSharp.Formats;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using WebXR;

public class SwitchUIMenuPlacementModality : MonoBehaviour
{
    public Canvas mainUI;
    public Transform handPlacement;

    public bool isStartOnHand;
    private void OnEnable()
    {
        WebXRManager.OnXRChange += OnXRChange;
        OnXRChange(WebXRManager.Instance.XRState,
                    WebXRManager.Instance.ViewsCount,
                    WebXRManager.Instance.ViewsLeftRect,
                    WebXRManager.Instance.ViewsRightRect);
    }
    private void OnDisable()
    {
        WebXRManager.OnXRChange -= OnXRChange;
    }
    public void Start()
    {
        if (isStartOnHand)
        {
            mainUI.transform.SetParent(handPlacement.parent, false);

            RectTransform mainUIRectTransform = mainUI.GetComponent<RectTransform>();
            RectTransform handPlacementTransform = handPlacement.GetComponent<RectTransform>();

            mainUI.renderMode = RenderMode.WorldSpace;
            mainUI.worldCamera = Camera.main;
            mainUIRectTransform.anchoredPosition3D = handPlacementTransform.anchoredPosition3D;
            mainUIRectTransform.sizeDelta = handPlacementTransform.sizeDelta;
            mainUI.transform.localScale = handPlacement.transform.localScale;
        }
    }

    private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
    {
      
        switch (state)
        {
            case WebXRState.NORMAL:
                mainUI.renderMode = RenderMode.ScreenSpaceOverlay;
                break;
            case WebXRState.VR:
                mainUI.renderMode = RenderMode.WorldSpace;

          //     mainUI.GetComponent<RectTransform>() =;

                break;
            case WebXRState.AR:
                mainUI.renderMode = RenderMode.WorldSpace;

                break;



        }
        //        if (m_camera == null)
        //        {
        //            return;
        //        }
        //        switch (state)
        //        {
        //            case WebXRState.NORMAL:
        //                m_camera.clearFlags = m_normalClearFlags;
        //                m_camera.backgroundColor = m_normalBackgroundColor;
        //                m_camera.cullingMask = m_normalCullingMask;
        //                if (m_updateNormalFieldOfView)
        //                {
        //                    m_camera.fieldOfView = m_normalFieldOfView;
        //                    m_camera.ResetProjectionMatrix();
        //                }
        //                if (m_updateNormalLocalPose)
        //                {
        //#if HAS_POSITION_AND_ROTATION
        //            m_transform.SetLocalPositionAndRotation(m_normalLocalPosition, m_normalLocalRotation);
        //#else
        //                    m_transform.localPosition = m_normalLocalPosition;
        //                    m_transform.localRotation = m_normalLocalRotation;
        //#endif
        //                }
        //                break;
        //            case WebXRState.VR:
        //                m_camera.clearFlags = m_vrClearFlags;
        //                m_camera.backgroundColor = m_vrBackgroundColor;
        //                m_camera.cullingMask = m_vrCullingMask;
        //                break;
        //            case WebXRState.AR:
        //                m_camera.clearFlags = m_arClearFlags;
        //                m_camera.backgroundColor = m_arBackgroundColor;
        //                m_camera.cullingMask = m_arCullingMask;
        //                break;
        //        }

    }
}