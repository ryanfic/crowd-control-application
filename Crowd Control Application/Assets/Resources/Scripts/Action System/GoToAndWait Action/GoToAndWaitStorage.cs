using Unity.Entities;
using Unity.Mathematics;

public struct GoToAndWaitStorage : IComponentData {
    public int id; // the id of the action
    public float timeWaited;
    public float timeToWait;
    public float3 position; // the position currently being moved towards (the wait point position)
}
