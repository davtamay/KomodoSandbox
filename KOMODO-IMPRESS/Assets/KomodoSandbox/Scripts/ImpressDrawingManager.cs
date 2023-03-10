using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Komodo.Runtime;

// TODO(Brandon) - rename this to BrushManager
public class ImpressDrawingManager : DrawingInstanceManager
{

    //public void Awake()
    //{
    //    GlobalMessageManager.Instance.Subscribe("drawWidth", (str) => ReceiveImpressDrawUpdate(str));
    //}
    //public void ReceiveImpressDrawUpdate(string stringData)
    //{
    //    Draw data = JsonUtility.FromJson<Draw>(stringData);

    //    //if (!IsLineRendererRegistered(data.strokeId))
    //    //{
    //    //    StartLineAndRegisterLineRenderer(data);
    //    //}

    //    LineRenderer renderer = GetLineRenderer(data.strokeId);

    //    renderer.widthMultiplier = data.lineWidth;

    //}
    //public void SendDrawUpdate2(int id, Entity_Type entityType, float lineWidth = 1, Vector3 curPos = default, Vector4 color = default)
    //{
    //    var drawUpdate = new Draw
    //    (
    //        (int)NetworkUpdateHandler.Instance.client_id,
    //        id,
    //        (int)entityType,
    //        lineWidth,
    //        curPos,
    //        color
    //    );

    //    var serializedUpdate = JsonUtility.ToJson(drawUpdate);

    //    KomodoMessage komodoMessage = new KomodoMessage("drawWidth", serializedUpdate);

    //    komodoMessage.Send();
    //}


}

