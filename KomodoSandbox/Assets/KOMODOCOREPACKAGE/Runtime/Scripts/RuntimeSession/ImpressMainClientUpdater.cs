using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Komodo.Utilities;
using Komodo.Runtime;
using Unity.Entities;
//using Komodo.IMPRESS;

[System.Serializable]
public struct IMPRESSPosition
{
    public int clientId;
    public int guid;
    public int entityType;
    public Vector3 scaleFactor;
    public Quaternion rot;
    public Vector3 pos;

    public IMPRESSPosition(int clientId, int entityId, int entityType, Vector3 scaleFactor, Quaternion rot, Vector3 pos)
    {
        this.clientId = clientId;
        this.guid = entityId;
        this.entityType = entityType;
        this.scaleFactor = scaleFactor;
        this.rot = rot;
        this.pos = pos;
    }
}
public class ImpressMainClientUpdater : SingletonComponent<ImpressMainClientUpdater>, IUpdatable
{
    public static ImpressMainClientUpdater Instance
    {
        get { return ((ImpressMainClientUpdater)_Instance); }
        set { _Instance = value; }
    }

    private EntityManager entityManager;
    public NetworkedGameObject grabbedGroup;


    public void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        GlobalMessageManager.Instance.Subscribe("impressSync", (data) => _DeserializeAndProcessSyncData(data));
    }

    public void AddUpdatable(Group netObject)
    {
        Debug.Log("group objects "+netObject.netObjectList.Count);
        foreach (NetworkedGameObject item in netObject.netObjectList)// netObject.netObjectList)
        {
            Debug.Log($"{item.name}", item.gameObject);
            MainClientUpdater.Instance.AddUpdatable(item);
        }
        //grabbedGroup = netObject;


        //GameStateManager.Instance.RegisterUpdatableObject(this);

    }

    public void RemoveUpdatable(Group netObject)
    {

        foreach (NetworkedGameObject item in netObject.netObjectList)
        {
            MainClientUpdater.Instance.RemoveUpdatable(item);
        }

        //if (GameStateManager.IsAlive)
        //{
        //    GameStateManager.Instance.DeRegisterUpdatableObject(this);
        //}

        //grabbedGroup = null;




    }
    public void OnUpdate(float realTime)
    {
       //if(grabbedGroup != null)
       // {
       //     SendSyncNetObject(grabbedGroup);
       // }
    }

    //public void SendSyncNetObject(NetworkedGameObject eContainer)
    //{
    //    //List<NetworkedGameObject> netObjInGroup = new List<NetworkedGameObject>();

    //    //foreach (Transform item in eContainer.transform)
    //    //{
    //    //    if(TryGetComponent(out NetworkedGameObject ngo))
    //    //    {
    //    //        netObjInGroup.Add(ngo);


    //    //    }
    //    //}



    //    // var entityData = entityManager.GetComponentData<NetworkEntityIdentificationComponentData>(eContainer.entity);

    //    IMPRESSPosition position = new IMPRESSPosition
    //    {
    //        clientId = entityData.clientID,



    //        entityId = eContainer.thisEntityID,//entityData.entityID,

    //        entityType = (int)entityData.current_Entity_Type,

    //        rot = eContainer.transform.rotation,

    //        pos = eContainer.transform.position,

    //        //since using parenting for objects, we need to translate local to global scalling when having it in your hand, when releasing we need to return such objects scalling from global to local scale
    //        scaleFactor = new Vector3(eContainer.transform.lossyScale.x, eContainer.transform.lossyScale.y, eContainer.transform.lossyScale.z)
    //    };

    //    SendSyncPoseMessage(position);
    //}
    public void SendSyncPoseMessage(IMPRESSPosition pos)
    {
        var posString = JsonUtility.ToJson(pos);

        var message = new KomodoMessage("impressSync", posString);

        message.Send();
    }


    public void _DeserializeAndProcessSyncData(string data)
    {
        var pos = JsonUtility.FromJson<IMPRESSPosition>(data);

        if (!SessionStateManager.IsAlive)
        {
            Debug.LogError("Tried to deserialize and process sync data, but SessionStateManager was not alive.");

            return;
        }

        ApplyPosition(pos);
    }

    public void ApplyPosition(IMPRESSPosition positionData)
    {
        //if (GameStateManager.IsAlive && !GameStateManager.Instance.isAssetImportFinished)
        //{
        //    return;
        //}

        if (positionData.entityType != (int)Entity_Type.objects && positionData.entityType != (int)Entity_Type.physicsObject)
        {
            ClientSpawnManager.Instance.AddClientIfNeeded(positionData.clientId);
        }

        TryToApplyPosition(positionData);

        //switch (positionData.entityType)
        //{
        //    case (int)Entity_Type.users_head:

        //        ClientSpawnManager.Instance.ApplyPositionToHead(positionData);

        //        break;

        //    case (int)Entity_Type.users_Lhand:

        //        ClientSpawnManager.Instance.ApplyPositionToLeftHand(positionData);

        //        break;

        //    case (int)Entity_Type.users_Rhand:

        //        ClientSpawnManager.Instance.ApplyPositionToRightHand(positionData);

        //        break;

        //    case (int)Entity_Type.objects:

        //        NetworkedObjectsManager.Instance.TryToApplyPosition(positionData);

        //        break;

        //    case (int)Entity_Type.physicsObject:

        //        NetworkedPhysicsManager.Instance.ApplyPositionToStart(positionData);

        //        break;

        //    case (int)Entity_Type.physicsEnd:

        //        NetworkedPhysicsManager.Instance.ApplyPositionToEnd(positionData);

        //        break;
        //}
    }


    public bool TryToApplyPosition(IMPRESSPosition positionData)
    {
        if (positionData.entityType != (int)Entity_Type.objects)
        {
            return false;
        }

        int entityId = positionData.guid;

        if (!NetworkedObjectsManager.Instance.networkedObjectFromEntityId.ContainsKey(entityId))
        {
            Debug.LogWarning("Entity ID : " + positionData.guid + "not found in Dictionary dropping object movement packet");

            return false;
        }

        Transform netObjTransform = NetworkedObjectsManager.Instance.networkedObjectFromEntityId[entityId].transform;

        if (!netObjTransform)
        {
            Debug.LogError($"TryToApplyPosition: NetObj with entityID {entityId} had no Transform component");

            return false;
        }

        netObjTransform.position = positionData.pos;

        netObjTransform.rotation = positionData.rot;

        UnityExtensionMethods.SetGlobalScale(netObjTransform, positionData.scaleFactor);

        return true;
    }


}
