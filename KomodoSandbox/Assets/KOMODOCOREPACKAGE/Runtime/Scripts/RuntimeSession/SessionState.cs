

//namespace Komodo.Runtime
//{
    [System.Serializable]
    public class SessionState
    {
        public int[] clients;
        public float[] latestClientPositions;
        public float[] latestClientRotations;
        public EntityState[] entities;
        public int scene;
        public bool isRecording;
    }
//}
