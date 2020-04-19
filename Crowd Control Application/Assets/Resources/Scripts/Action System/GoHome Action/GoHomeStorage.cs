using Unity.Entities;
using Unity.Mathematics;

public struct GoHomeStorage : IComponentData {
    public int id; // the id of the action
    public float3 homePoint; // the position currently being moved towards (the home point position)
}

