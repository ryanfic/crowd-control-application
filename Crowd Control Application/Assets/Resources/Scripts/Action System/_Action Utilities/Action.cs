using Unity.Entities;
namespace crowd_Actions{
    public enum ActionType {
        Follow_WayPoints,
        Go_Home,
        No_Action
    }

    [InternalBufferCapacity(5)]
    public struct Action : IBufferElementData {
        public int id;

        public int priority;
        public ActionType type;
        public float timeCreated;

        public Entity dataHolder;

    }
}

