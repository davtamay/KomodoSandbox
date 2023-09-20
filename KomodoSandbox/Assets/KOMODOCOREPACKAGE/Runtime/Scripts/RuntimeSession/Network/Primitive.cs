using UnityEngine;

public struct Primitive
{
    public int modelType;
    public int clientId;
    public int guid;
    public int indentifier;
    public float scaleFactor;
    public Vector3 pos;
    public Vector4 rot;

    public Primitive(int clientId, int primitiveId, int primitiveType, float scale, Vector3 primitivePos, Vector4 primitiveRot)
    {
        this.modelType = 2;

        this.clientId = clientId;

        this.guid = primitiveId;

        this.indentifier = primitiveType;

        this.scaleFactor = scale;

        this.pos = primitivePos;

        this.rot = primitiveRot;
    }
}