using Unity.Entities;

namespace Komodo.Runtime
{
    public struct NetworkEntityIdentificationComponentData : IComponentData
    {
        public int clientID;
        public int entityID;
        public int sessionID;


        public Entity_Type current_Entity_Type;
    }
}
