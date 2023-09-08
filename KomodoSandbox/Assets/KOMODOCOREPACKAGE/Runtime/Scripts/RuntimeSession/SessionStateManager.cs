using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Komodo.Utilities;
using System.Linq;
using Newtonsoft.Json;

namespace Komodo.Runtime
{
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
            int desiredEntityId = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(netObject.entity).entityID;

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
                if (candidateEntityState.id == desiredEntityId)
                {
                    return candidateEntityState;
                }
            }

            Debug.LogError($"SessionStateManager: Could not find EntityState that matched netObject with entity ID {desiredEntityId}");

            return new EntityState();
        }

        private NetworkedGameObject GetNetObjectFromEntityState(EntityState entityState)
        {
            try
            {
                return NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityState.id];
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SessionStateManager: Could not find NetObject that matched EntityState's ID {entityState.id}. {e.Message}");
            }

            return null;
        }

        private void ApplyCatchupToExistingNetObjects ()
        {
            foreach (var netObject in ModelImportInitializer.Instance.networkedGameObjects)
            {
                var entityState = GetEntityStateFromNetObject(netObject);

                UIManager.Instance.ProcessNetworkToggleVisibility(entityState.id, entityState.render);

                int interactionType = entityState.locked ? (int)INTERACTIONS.LOCK : (int)INTERACTIONS.UNLOCK;

                ApplyInteraction(new Interaction(
                    sourceEntity_id: -1,
                    targetEntity_id: entityState.id,
                    interactionType: interactionType
                ));
            }
        }

        public void ApplyCatchup()
        {

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

                return;
            }
            //at times gives argument index error
            SceneManagerExtensions.Instance.SelectScene(_state.scene);


            ClientSpawnManager.Instance.AddNewClients(_state.clients, _state.latestClientPositions, _state.latestClientRotations);


            for (int i = 0; i < _state.entities.Length; i++)
            {
                EntityState entityState = _state.entities[i];
                NetworkedGameObject netObject = GetNetObjectFromEntityState(entityState);

                if (netObject == null)
                {
                    Debug.LogWarning($"Could not catch up state for entity with ID {entityState.id}. Skipping.");
                    continue;
                }

                UIManager.Instance.ProcessNetworkToggleVisibility(netObject.thisEntityID, entityState.render);

                int interactionType = entityState.locked ? (int)INTERACTIONS.LOCK : (int)INTERACTIONS.UNLOCK;

                ApplyInteraction(new Interaction(
                    sourceEntity_id: -1,
                    targetEntity_id: entityState.id,
                    interactionType: interactionType
                ));

                ApplyPosition(entityState.latest);
            }
        }

        public void ApplyPosition(Position positionData)
        {
            //if (GameStateManager.IsAlive && !GameStateManager.Instance.isAssetImportFinished)
            //{
            //    return;
            //}

            if (positionData.entityType != (int)Entity_Type.objects && positionData.entityType != (int)Entity_Type.physicsObject)
            {
                ClientSpawnManager.Instance.AddClientIfNeeded(positionData.clientId);
            }

            switch (positionData.entityType)
            {
                case (int) Entity_Type.users_head:

                    ClientSpawnManager.Instance.ApplyPositionToHead(positionData);

                    break;

                case (int) Entity_Type.users_Lhand:

                    ClientSpawnManager.Instance.ApplyPositionToLeftHand(positionData);

                    break;

                case (int) Entity_Type.users_Rhand:

                    ClientSpawnManager.Instance.ApplyPositionToRightHand(positionData);

                    break;

                case (int) Entity_Type.objects:

                    NetworkedObjectsManager.Instance.TryToApplyPosition(positionData);

                    break;

                case (int) Entity_Type.physicsObject:

                    NetworkedPhysicsManager.Instance.ApplyPositionToStart(positionData);

                    break;

                case (int) Entity_Type.physicsEnd:

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
}
