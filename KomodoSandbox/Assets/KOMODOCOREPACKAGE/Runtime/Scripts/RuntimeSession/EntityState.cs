
using UnityEngine;

//namespace Komodo.Runtime
//{
    [System.Serializable]
    public struct EntityState
    {
        public int modelType;
        public int guid;
        public Position latest;
        public bool render;
        public bool locked;

     

    //specific values are below, can we add a custom value struct

        public string url;

    //for primitive id, or for draw type
        public int indentifier;


       public DrawEntityState drawEntity;



}

public struct DrawEntityState{

    // LineEnd = 11
    public int strokeType;
    public float lineWidth;
    public Vector4 color;
    public Vector3[] posArray; 

}
