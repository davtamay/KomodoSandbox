using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
using System;
using Komodo.Utilities;

//namespace Komodo.Runtime
//{
    public class DrawingInstanceManager : SingletonComponent<DrawingInstanceManager>
    {
        public static DrawingInstanceManager Instance
        {
            get { return (DrawingInstanceManager) _Instance; }

            set { _Instance = value; }
        }

        public Transform lineRendererContainerPrefab;

        public EntityManager entityManager;

        [HideInInspector] public Transform userStrokeParent;

        [HideInInspector] public Transform externalStrokeParent;

        private Dictionary<int, LineRenderer> lineRendererFromId = new Dictionary<int, LineRenderer>();

    public void Awake()
    {
        //used to set our managers alive state to true to detect if it exist within scene
        var initManager = Instance;

        //TODO -- warn if we are not attached to a GameObject

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //parent for our stored lines
        userStrokeParent = new GameObject("UserStrokeParent").transform;

        externalStrokeParent = new GameObject("ExternalClientStrokeParent").transform;

        userStrokeParent.SetParent(transform);

        externalStrokeParent.SetParent(transform);

        GlobalMessageManager.Instance.Subscribe("draw", (str) => ReceiveDrawUpdate(str));

        //string data1 = JsonUtility.ToJson(new Draw(0, 222, (int)Entity_Type.Line, 1.0f, Vector3.zero, Vector4.one));
        //string data2 = JsonUtility.ToJson(new Draw(0, 222, (int)Entity_Type.Line, 1.0f, Vector3.one, Vector4.one));
        //string data3 = JsonUtility.ToJson(new Draw(0, 222, (int)Entity_Type.LineEnd, 1.0f, Vector3.one * 2, Vector4.one));


        //ReceiveDrawUpdate(data1);
        //ReceiveDrawUpdate(data2);
        //ReceiveDrawUpdate(data3);
    }

        public void InitializeFinishedLineFromOwnClient(int strokeID, LineRenderer lineRenderer, bool doSendNetworkUpdate)
        {
            //set correct pivot point when scaling object by grabbing
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));

            if (lineRendererContainerPrefab == null)
            {
                throw new System.Exception("Line Renderer Container Prefab is not assigned in DrawingInstanceManager");
            }

            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;

            lineRendCopy.name = "LineR:" + strokeID;

            //Create a reference to use in network
            NetworkedGameObject nAGO = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(pivot, customEntityID: strokeID, modelType: MODEL_TYPE.Drawing);

        // Make own client's draw strokes grabbable
         pivot.tag = TagList.interactable;
       // pivot.tag = TagList.drawing;

        // entityManager.AddComponentData(nAGO.Entity, new DrawingTag { });

        var bColl = pivot.GetComponent<BoxCollider>();

            LineRenderer copiedLR = lineRendCopy.GetComponent<LineRenderer>();

            var color = lineRenderer.startColor;

            copiedLR.startColor = color;

            copiedLR.endColor = color;

            copiedLR.widthMultiplier = lineRenderer.widthMultiplier;

            Bounds newBounds = new Bounds(lineRenderer.GetPosition(0), Vector3.one * 0.01f);

            copiedLR.positionCount = 0;

            for (int i = 0; i < lineRenderer.positionCount; i++)
            {
                copiedLR.positionCount++;

                copiedLR.SetPosition(i, lineRenderer.GetPosition(i));

                newBounds.Encapsulate(new Bounds(lineRenderer.GetPosition(i), Vector3.one * 0.01f));
            }

            pivot.transform.position = newBounds.center;

            bColl.center = lineRendCopy.transform.position;

            bColl.size = newBounds.size;

            lineRendCopy.transform.SetParent(pivot.transform, true);

        if (doSendNetworkUpdate)
        {
            SendDrawUpdate(
                strokeID,
                Entity_Type.LineEnd,
                copiedLR.widthMultiplier,
                lineRenderer.GetPosition(lineRenderer.positionCount - 1),
                new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
            );

          

          //  var data =
          //  JsonUtility.ToJson(
          //      new Draw(
          //          1,
          //    strokeID +100,
          //    (int)Entity_Type.LineEnd,
          //    1,
          //    lineRenderer.GetPosition(lineRenderer.positionCount - 1),
          //    new Vector4(lineRenderer.startColor.r, lineRenderer.startColor.g, lineRenderer.startColor.b, lineRenderer.startColor.a)
          //    )
          //);
          //  ReceiveDrawUpdate(data);
        }

        pivot.transform.SetParent(userStrokeParent, true);

            if (UndoRedoManager.IsAlive)
            {
                //save undoing process for ourselves and others
                UndoRedoManager.Instance.savedStrokeActions.Push(() =>
                {
                    pivot.SetActive(false);

                //    SendDrawUpdate(strokeID, Entity_Type.LineNotRender);
                });
            }
        }

        public void InitializeFinishedLineFromOtherClient(int strokeID, LineRenderer renderer)
        {
            
            GameObject pivot = new GameObject("LineRender:" + strokeID, typeof(BoxCollider));

        NetworkedGameObject netObject = NetworkedObjectsManager.Instance.CreateNetworkedGameObject(pivot, customEntityID: strokeID, modelType: MODEL_TYPE.Drawing); //strokeID, strokeID, true);

        // Make other clients' draw strokes grabbable
        pivot.tag = TagList.interactable;

            //tag created drawing object will be useful in the future for having items with multiple tags
          //  entityManager.AddComponentData(netObject.entity, new DrawingTag { });

            var collider = pivot.GetComponent<BoxCollider>();

            Bounds newBounds = new Bounds(renderer.GetPosition(0), Vector3.one * 0.01f);

            for (int i = 0; i < renderer.positionCount; i++)
            {
                newBounds.Encapsulate(new Bounds(renderer.GetPosition(i), Vector3.one * 0.01f));
            }

            pivot.transform.position = newBounds.center;

            collider.center = renderer.transform.position;

            collider.size = newBounds.size;

            renderer.transform.SetParent(pivot.transform, true);

            pivot.transform.SetParent(externalStrokeParent, true);


        Debug.Log("MADE NEW DRAW :" + strokeID);
             GameStateManager.Instance.isAssetImportFinished = true;
        }

        public void SendDrawUpdate(int id, Entity_Type entityType, float lineWidth = 1, Vector3 curPos = default, Vector4 color = default)
        {
            var drawUpdate = new Draw
            (
                (int) NetworkUpdateHandler.Instance.client_id,
                id,
                (int) entityType,
                lineWidth,
                curPos,
                color
            );

            var serializedUpdate = JsonUtility.ToJson(drawUpdate);

            KomodoMessage komodoMessage = new KomodoMessage("draw", serializedUpdate);

            komodoMessage.Send();
        }

        protected bool IsLineRendererRegistered (int id)
        {
            return lineRendererFromId.ContainsKey(id);
        }

        protected void RegisterLineRenderer (int id, LineRenderer renderer)
        {
            lineRendererFromId.Add(id, renderer);
        }

        protected LineRenderer GetLineRenderer (int id)
        {
            return lineRendererFromId[id];
        }

        protected void UnregisterLineRenderer (int id)
        {
            lineRendererFromId.Remove(id);
        }

        protected LineRenderer CreateLineRendererContainer (Draw data)
        {
            GameObject lineRendCopy = Instantiate(lineRendererContainerPrefab).gameObject;

            lineRendCopy.name = "LineR:" + data.guid;

            lineRendCopy.transform.SetParent(externalStrokeParent, true);

            return lineRendCopy.GetComponent<LineRenderer>();
        }

        protected void ContinueLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.guid))
            {
                Debug.LogWarning($"Line renderer {data.guid} will not be started or continued, because it was never registered.");

                return;
            }

            LineRenderer renderer = GetLineRenderer(data.guid);

            var brushColor = new Vector4(data.color.x, data.color.y, data.color.z, data.color.w);

            renderer.startColor = brushColor;

            renderer.endColor = brushColor;

            renderer.widthMultiplier = data.lineWidth;

            ++renderer.positionCount;

            renderer.SetPosition(renderer.positionCount - 1, data.pos);
        }

        protected void EndLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.guid))
            {
                Debug.LogWarning($"Line renderer {data.guid} will not be ended, because it was never registered.");

                return;
            }

            LineRenderer renderer = GetLineRenderer(data.guid);

            renderer.positionCount += 1;

            renderer.SetPosition(renderer.positionCount - 1, data.pos);

            InitializeFinishedLineFromOtherClient(data.guid, renderer);
        }

        protected void DeleteLine (Draw data)
        {
            if (!IsLineRendererRegistered(data.guid))
            {
                Debug.LogWarning($"Line renderer {data.guid} will not be deleted, because it was never registered.");

                return;
            }

            bool success = NetworkedObjectsManager.Instance.DestroyAndUnregisterEntity(data.guid);

            if (!success)
            {
                Debug.LogWarning($"Could not delete line {data.guid}'s networked object.");

                return;
            }

            UnregisterLineRenderer(data.guid);
        }

        protected void ShowLine (Draw data)
        {
            bool success = NetworkedObjectsManager.Instance.ShowEntity(data.guid);

            if (!success)
            {
                Debug.LogWarning($"Could not show line {data.guid}.");
            }
        }

        protected void HideLine (Draw data)
        {
            bool success = NetworkedObjectsManager.Instance.HideEntity(data.guid);

            if (!success)
            {
                Debug.LogWarning($"Could not hide line {data.guid}.");
            }
        }

        protected void StartLineAndRegisterLineRenderer (Draw data)
        {
            LineRenderer currentLineRenderer = CreateLineRendererContainer(data);

            currentLineRenderer.positionCount = 0;

            RegisterLineRenderer(data.guid, currentLineRenderer);
        }

        public void ReceiveDrawUpdate (string stringData)
        {
            Draw data = JsonUtility.FromJson<Draw>(stringData);

            if (!IsLineRendererRegistered(data.guid))
            {
                StartLineAndRegisterLineRenderer(data);
            }

            switch (data.strokeType)
            {
                // Continues a Line
                case (int) Entity_Type.Line:
                {
                  //  Debug.Log("REVEIVED LINE " + data.guid);
                    ContinueLine(data);

                    break;
                }

                case (int) Entity_Type.LineEnd:
                {
                  //  Debug.Log("REVEIVED LINE END " + data.guid);
                    EndLine(data);

                    break;
                }

                case (int) Entity_Type.LineDelete:
                {
                    DeleteLine(data);

                    break;
                }

                case (int) Entity_Type.LineRender:
                {
                    ShowLine(data);

                    break;
                }

                case (int) Entity_Type.LineNotRender:
                {
                    HideLine(data);

                    break;
                }
            }
        }
    }
//}
