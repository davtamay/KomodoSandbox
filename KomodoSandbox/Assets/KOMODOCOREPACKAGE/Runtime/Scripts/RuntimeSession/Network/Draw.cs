using UnityEngine;

//namespace Komodo.Runtime
//{
    public struct Draw
    {
        // Who is sending the draw update
        public int clientId;

        // An ID unique among all entities (draw strokes, models, model pack subobjects)
        public int guid;

        // The kind of action being performed.
        // See Entity_Type.Line ... Entity_Type.LineNotRender
        public int strokeType;

        // Used for continue line. The visual thickness of the line.
        public float lineWidth;

        // Used for continue line and end line. 
        // The endpoint of the currently drawn line segment.
        public Vector3 pos;

        // Used for continue line.
        // The color of the line segment.
        public Vector4 color;


        public Draw(int clientId, int strokeId, int strokeType, float lineWidth, Vector3 curStrokePos, Vector4 curColor)
        {
            this.clientId = clientId;
            this.guid = strokeId;
            this.strokeType = strokeType;
            this.lineWidth = lineWidth;
            this.pos = curStrokePos;
            this.color = curColor;
        }

        // TODO: Add constructors for ContinueLine, EndLine, ShowLine, HideLIne
    }
//}