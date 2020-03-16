using Unity.Entities;

public enum ActionType {
    Follow_WayPoints,
    Go_Home
}

[InternalBufferCapacity(5)]
public struct Action : IBufferElementData {
    public int id;

    public int priority;
    public ActionType type;
    public float timeCreated;

    public Entity dataHolder;

}
