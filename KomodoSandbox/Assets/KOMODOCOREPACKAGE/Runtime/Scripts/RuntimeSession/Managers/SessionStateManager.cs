using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Komodo.Utilities;
using System.Linq;
using Newtonsoft.Json;
using System;

//public struct EntityGUID
//{
//    public Dictionary<int, int> guidsInSceneDictionary = new Dictionary<int, int>();
//    public List<int> guid;
//    public List<int> modelType;
//}

//using System.Runtime.CompilerS
//ervices;
//using Komodo.


//namespace Komodo.Runtime
//{
public class SessionStateManager : SingletonComponent<SessionStateManager>
{
    public static SessionStateManager Instance
    {
        get { return (SessionStateManager)_Instance; }

        set { _Instance = value; }
    }

    private EntityManager entityManager;

    private SessionState _state;

    public void Awake()
    {
        var forceAlive = Instance;

        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        //string jsonData = "{\"1\":\"Alice\",\"2\":\"Bob\",\"3\":\"Charlie\"}";
        //Dictionary<string, string> clientDataDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

        //foreach (var item in clientDataDict)
        //{
        //    Debug.Log(item.Value);

        //}
    }

    public void ReceiveGUIDsFromServer(Dictionary<int,int> eGUID)
    {
        foreach (KeyValuePair<int,int> item in eGUID)
        {
            //drawing reach for indexdb or send request to server
            if(item.Value == 3)
            {
                string guid = item.Key.ToString();//$"\"{item.Key}\"";// $"{item.Key}" 


                string value = StorageJSLib.getItem(guid);

                //Get Item does not give value immidetly... how to check for a null result to request instead


                //StorageJSLib.keys();

                //Debug.Log("GUID : " + item.Key + " --- GUID STRING : " + guid);
                //Debug.Log(value);


                //if (!string.IsNullOrEmpty(value))
                //{
                //    Debug.Log("Getting Draw data from indexdb cache: " + item.Value);
                //    var drawData = JsonUtility.FromJson<DrawEntityState>(value);

                //    for (int e = 0; e < drawData.posArray.Length - 1; e++)
                //    {
                //        var drawModel = new Draw(1, item.Key, (int)Entity_Type.Line, drawData.lineWidth, drawData.posArray[e], drawData.color);
                //        DrawingInstanceManager.Instance.ReceiveDrawUpdate(JsonUtility.ToJson(drawModel));
                //    }

                //    //do ending to create the model to grab
                //    var drawModelEnd = new Draw(1, item.Key, (int)Entity_Type.LineEnd, drawData.lineWidth, drawData.posArray[drawData.posArray.Length - 1], drawData.color);
                //    DrawingInstanceManager.Instance.ReceiveDrawUpdate(JsonUtility.ToJson(drawModelEnd));
                //}
                //else
                //{
                //    //Debug.Log("Requesting Draw data from server: " + item.Value);
                //    SocketIOJSLib.RequestDrawStrokeFromServer(item.Key);
                //}
            }
           
        }
        //for (int i = 0; i < eGUID.guid.Count; i++)
        //{
        //    Debug.Log("GUIDS : " + eGUID.guid[i] + "Model_Type:" + eGUID.modelType);
        //}
        //foreach (var item in eGUID.guid)
        //{

        //}

    }


    public void SetSessionState(SessionState state)
    {
        _state = state;
    }

    public SessionState GetSessionState()
    {
        return _state;
    }

    public bool IsReady()
    {
        return (_state != null);
    }

    private EntityState GetEntityStateFromNetObject(NetworkedGameObject netObject)
    {
        // int desiredEntityId = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.entity).entityID;

        //foreach (var candidateEntityState in _state.entities)
        //{
        //    if (candidateEntityState.id == desiredEntityId)
        //    {
        //        return candidateEntityState;
        //    }
        //}
        for (int i = 0; i < _state.entities.Length; i++)
        {


            EntityState candidateEntityState = _state.entities[i];
            if (candidateEntityState.guid == netObject.thisEntityID)//desiredEntityId)
            {
                return candidateEntityState;
            }
        }

        Debug.LogError($"SessionStateManager: Could not find EntityState that matched netObject with entity ID {netObject.thisEntityID}");

        return new EntityState();
    }

    private NetworkedGameObject GetNetObjectFromEntityState(EntityState entityState)
    {
        try
        {
            // return NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityState.id];
            return NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityState.guid];
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SessionStateManager: Could not find NetObject that matched EntityState's ID {entityState.guid}. {e.Message}");
        }

        return null;
    }

    private void ApplyCatchupToExistingNetObjects()
    {
        foreach (var netObject in ModelImportInitializer.Instance.networkedGameObjects)
        {
            var entityState = GetEntityStateFromNetObject(netObject);

            UIManager.Instance.ProcessNetworkToggleVisibility(entityState.guid, entityState.render);

            int interactionType = entityState.locked ? (int)INTERACTIONS.LOCK : (int)INTERACTIONS.UNLOCK;

            ApplyInteraction(new Interaction(
                sourceEntity_id: -1,
                targetEntity_id: entityState.guid,
                interactionType: interactionType
            ));
        }
    }

    public IEnumerator ApplyCatchup()
    {
        yield return new WaitUntil(() => !GameStateManager.Instance.isApplyingCatchup);

        GameStateManager.Instance.isApplyingCatchup = true;

        //if (!UIManager.IsAlive)
        //{
        //    Debug.LogWarning("Tried to process session state for lock and visibility, but there was no UIManager.");
        //}

        if (!SceneManagerExtensions.IsAlive)
        {
            Debug.LogWarning("Tried to process session state for scene, but there was no SceneManagerExtensions.");
        }

        if (!ClientSpawnManager.IsAlive)
        {
            Debug.LogWarning("Tried to process session state for clients, but there was no ClientSpawnManager.");
        }

        if (_state == null)
        {
            Debug.LogWarning("SessionStateManager: state was null. State catch-up will not be applied.");

            yield break;
        }
        //at times gives argument index error
        SceneManagerExtensions.Instance.SelectScene(_state.scene);


        ClientSpawnManager.Instance.AddNewClients(_state.clients, _state.latestClientPositions, _state.latestClientRotations);

        bool skipRestOfCode = false;

        for (int i = 0; i < _state.entities.Length; i++)
        {
            EntityState entityState = _state.entities[i];

            if (!NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(_state.entities[i].guid))
            {
               
              

                switch (entityState.modelType)
                {
                    //url 
                    case 1:

                        GameStateManager.Instance.isAssetImportFinished = false;

                        var newModel = new ModelData { modelType = 1, guid = entityState.guid, modelURL = entityState.url, pos = entityState.latest.pos, rot = entityState.latest.rot, scale = entityState.latest.scale };

                        Debug.Log("SCALE FACTOR: " + entityState.latest.scale);

                        if (!string.IsNullOrEmpty(entityState.url))
                            InstantiateAssetCards.Instance.InstantiateAssetFromData(JsonUtility.ToJson(newModel));
                        else
                        {
                            //this is for child net objects deriving from parent url reference.
                            skipRestOfCode = true;
                            GameStateManager.Instance.isAssetImportFinished = true;
                            ApplyPosition(entityState.latest);
                        }

                        break;

                    //primitive
                    case 2:
                        skipRestOfCode = true;
                        GameStateManager.Instance.isAssetImportFinished = false;

                        //Debug.Log("Received primitive state " +  entityState.guid);
                        // GameStateManager.Instance.isAssetImportFinished = false;
                        var primitiveModel = new Primitive { modelType = 2, guid = entityState.guid, indentifier = entityState.indentifier, scaleFactor = entityState.latest.scale, pos = entityState.latest.pos, rot = new Vector4(entityState.latest.rot.x, entityState.latest.rot.y, entityState.latest.rot.z, entityState.latest.rot.w )};
                        CreatePrimitiveManager.Instance.ReceivePrimitiveUpdate(JsonUtility.ToJson(primitiveModel), true);

                        break;

                    case 3:

                        

                        //GameStateManager.Instance.isAssetImportFinished = false;

                        ////make sure that the user completed the line before spawning one.
                        //// Line = 10, LineEnd = 11
                        //if (entityState.drawEntity.strokeType == 11)
                        //{
                        //    Debug.Log("REVEIVED ENTITY STATE OF 11 " + entityState.guid);

                        //    //do all before end
                        //    for (int e = 0; e < entityState.drawEntity.posArray.Length - 1; e++)
                        //    {
                        //        var drawModel = new Draw(1, entityState.guid, (int)Entity_Type.Line, entityState.drawEntity.lineWidth, entityState.drawEntity.posArray[e], entityState.drawEntity.color);
                        //        DrawingInstanceManager.Instance.ReceiveDrawUpdate(JsonUtility.ToJson(drawModel));
                        //    }

                        //    //do ending to create the model to grab
                        //    var drawModelEnd = new Draw(1, entityState.guid, (int)Entity_Type.LineEnd, entityState.drawEntity.lineWidth, entityState.drawEntity.posArray[entityState.drawEntity.posArray.Length - 1], entityState.drawEntity.color);
                        //    DrawingInstanceManager.Instance.ReceiveDrawUpdate(JsonUtility.ToJson(drawModelEnd));

                        //}
                        //else
                            skipRestOfCode = true;


                        break;


                }





            }
            

            yield return new WaitUntil(() => GameStateManager.Instance.isAssetImportFinished);

            if (skipRestOfCode)
                 continue;

            //it is not waiting


            NetworkedGameObject netObject = GetNetObjectFromEntityState(entityState);

            if (netObject == null)
            {
                Debug.LogWarning($"Could not catch up state for entity with ID {entityState.guid}. Skipping.");
                continue;
            }

            // UIManager.Instance.ProcessNetworkToggleVisibility(netObject.thisEntityID, entityState.render);


            UIManager.Instance.ProcessNetworkToggleVisibility(entityState.guid, entityState.render);

            UIManager.Instance.ProcessNetworkToggleLock(entityState.guid, entityState.locked);


            //int interactionType = entityState.locked ? (int)INTERACTIONS.LOCK : (int)INTERACTIONS.UNLOCK;

            //ApplyInteraction(new Interaction(
            //    sourceEntity_id: -1,
            //    targetEntity_id: entityState.id,
            //    interactionType: interactionType
            //));

            ApplyPosition(entityState.latest);
        }
        GameStateManager.Instance.isApplyingCatchup = false;
    }

    public void ApplyPosition(Position positionData)
    {
        //if (GameStateManager.IsAlive && !GameStateManager.Instance.isAssetImportFinished)
        //{
        //    return;
        //}
      //  Debug.Log("ENTITY_TYPE: " + positionData.entityType);

        if (positionData.entityType != (int)Entity_Type.objects && positionData.entityType != (int)Entity_Type.physicsObject)
        {
            ClientSpawnManager.Instance.AddClientIfNeeded(positionData.clientId);
        }

        switch (positionData.entityType)
        {
            case (int)Entity_Type.users_head:

                ClientSpawnManager.Instance.ApplyPositionToHead(positionData);

                break;

            case (int)Entity_Type.users_Lhand:

                ClientSpawnManager.Instance.ApplyPositionToLeftHand(positionData);

                break;

            case (int)Entity_Type.users_Rhand:

                ClientSpawnManager.Instance.ApplyPositionToRightHand(positionData);

                break;

            case (int)Entity_Type.objects:

                
                NetworkedObjectsManager.Instance.TryToApplyPosition(positionData);

                break;

            case (int)Entity_Type.physicsObject:

                NetworkedPhysicsManager.Instance.ApplyPositionToStart(positionData);

                break;

            case (int)Entity_Type.physicsEnd:

                NetworkedPhysicsManager.Instance.ApplyPositionToEnd(positionData);

                break;
        }
    }

    public void ApplyNamesForClientsInSession(string data)
    {
        //Dictionary<int, string> clientDataDict = JsonUtility.FromJson<Dictionary<int, string>>(data);
        Dictionary<int, string> clientDataDict = JsonConvert.DeserializeObject<Dictionary<int, string>>(data);




        Debug.Log($"Received COUNT: {clientDataDict.Count} ");
        foreach (var item in clientDataDict)
        {
            SpeechToTextSnippet snippet;
            snippet.target = item.Key;
            snippet.text = item.Value;
            snippet.stringType = (int)STRINGTYPE.CLIENT_NAME;

            Debug.Log($"Received INFO: {item.Key} ");



            //update main list for client;
            if (NetworkUpdateHandler.Instance.clientIDToName.ContainsKey(item.Key))
                NetworkUpdateHandler.Instance.clientIDToName[item.Key] = item.Value;
            else
                NetworkUpdateHandler.Instance.clientIDToName.Add(item.Key, item.Value);


            ClientSpawnManager.Instance.ProcessSpeechToTextSnippet(snippet);
        }
        //var data = JsonUtility.FromJson<OtherClientInfo>(clientNames);

        //     var deserializedData = JsonUtility.FromJson<SpeechToText>(data);


    }

    public void ApplyInteraction(Interaction interactionData)
    {
        NetworkedObjectsManager.Instance.ApplyInteraction(interactionData);
    }
}
//}
