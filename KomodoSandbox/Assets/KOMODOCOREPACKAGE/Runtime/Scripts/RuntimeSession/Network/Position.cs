using UnityEngine;

//namespace Komodo.Runtime
//{
    [System.Serializable]
    public struct Position
    {
        public int clientId;
        public int guid;
        public int entityType;
        public float scale;
        public Quaternion rot;
        public Vector3 pos;

        public Position(int clientId, int entityId, int entityType, float scaleFactor, Quaternion rot, Vector3 pos)
        {
            this.clientId = clientId;
            this.guid = entityId;
            this.entityType = entityType;
            this.scale = scaleFactor;
            this.rot = rot;
            this.pos = pos;
        }
    }
//}