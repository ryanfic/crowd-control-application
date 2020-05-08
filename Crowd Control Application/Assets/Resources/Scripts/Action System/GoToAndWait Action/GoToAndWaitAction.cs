using Unity.Entities;
using Unity.Mathematics;
public struct GoToAndWaitAction : IComponentData {
    public int id; // the id of the action
    public float timeWaited;
    public float timeToWait;
    public float3 position;
}
