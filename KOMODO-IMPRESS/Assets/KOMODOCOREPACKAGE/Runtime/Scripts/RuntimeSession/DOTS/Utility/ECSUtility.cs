using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ECSUtility : MonoBehaviour
{
    public static void SetParent(EntityManager dstManager, Entity parent, Entity child, float3 localTranslation, float3 localRotation, float3 localScale)
    {
        //set the child
        if (!dstManager.HasComponent<LocalToWorld>(child))
            dstManager.AddComponentData(child, new LocalToWorld { });

        if (!dstManager.HasComponent<LocalTransform>(child))
            dstManager.AddComponentData(child, new LocalTransform { Position = localTranslation });
        else
            dstManager.SetComponentData(child, new LocalTransform { Position = localTranslation });

        if (!dstManager.HasComponent<LocalTransform>(child))
            dstManager.AddComponentData(child, new LocalTransform { Rotation = quaternion.Euler(localRotation) });
        else
            dstManager.SetComponentData(child, new LocalTransform { Rotation = quaternion.Euler(localRotation) });

        if (!dstManager.HasComponent<LocalTransform>(child))
            dstManager.AddComponentData(child, new LocalTransform { Scale = localScale.x });
        else
            dstManager.SetComponentData(child, new LocalTransform { Scale = localScale.x });

        if (!dstManager.HasComponent<Parent>(child))
            dstManager.AddComponentData(child, new Parent { Value = parent });
        else
            dstManager.SetComponentData(child, new Parent { Value = parent });

        if (!dstManager.HasComponent<Parent>(child))
            dstManager.AddComponentData(child, new Parent());

        //set the parent
        if (!dstManager.HasComponent<LocalToWorld>(parent))
            dstManager.AddComponentData(parent, new LocalToWorld { });

        if (!dstManager.HasComponent<LocalTransform>(parent))
            dstManager.AddComponentData(parent, new LocalTransform { Position = Vector3.one });

        if (!dstManager.HasComponent<LocalTransform>(parent))
            dstManager.AddComponentData(parent, new LocalTransform { Rotation = Quaternion.identity });

        if (!dstManager.HasComponent<LocalTransform>(parent))
            dstManager.AddComponentData(parent, new LocalTransform { Scale = Vector3.one.x });

    }
}
