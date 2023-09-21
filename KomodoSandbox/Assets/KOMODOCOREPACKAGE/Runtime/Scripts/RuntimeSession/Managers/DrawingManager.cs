using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

//namespace Komodo.IMPRESS
//{
    // TODO(Brandon) - rename this to BrushManager
    public class DrawingManager : DrawingInstanceManager
    {
        public PlayerReferences playerRefs;

        private UnityAction _enable;

        private UnityAction _disable;

        void OnValidate ()
        {
            if (playerRefs == null)
            {
                throw new UnassignedReferenceException("playerRefs");
            }
        }
        //new
        public void Awake()
        {
            base.Awake();

            GlobalMessageManager.Instance.Subscribe("drawWidth", (str) => ReceiveImpressDrawUpdate(str));
        }
        public void ReceiveImpressDrawUpdate(string stringData)
        {
            Draw data = JsonUtility.FromJson<Draw>(stringData);

        //if (!IsLineRendererRegistered(data.strokeId))
        //{
        //    StartLineAndRegisterLineRenderer(data);
        //}

        LineRenderer renderer = GetLineRenderer(data.guid);

        renderer.widthMultiplier = data.lineWidth;

        }
        public void SendDrawUpdate2(int id, Entity_Type entityType, float lineWidth = 1, Vector3 curPos = default, Vector4 color = default)
        {
            var drawUpdate = new Draw
            (
                (int)NetworkUpdateHandler.Instance.client_id,
                id,
                (int)entityType,
                lineWidth,
                curPos,
                color
            );

            var serializedUpdate = JsonUtility.ToJson(drawUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("drawWidth", serializedUpdate);

            komodoMessage.Send();
        }
        ///
        void Start ()
        {
            _enable += Enable;

            KomodoEventManager.Instance.StartListening("drawTool.enable", _enable);

            _disable += Disable;

            KomodoEventManager.Instance.StartListening("drawTool.disable", _disable);
        }

        // Our own function. Not to be confused with Unity's OnEnable.
        public void Enable ()
        {
            playerRefs.drawL.gameObject.SetActive(true);

            playerRefs.drawR.gameObject.SetActive(true);
        }

        // Our own function. Not to be confused with Unity's OnDisable.
        public void Disable ()
        {
            playerRefs.drawL.gameObject.SetActive(false);

            playerRefs.drawR.gameObject.SetActive(false);
        }
    }
//}
