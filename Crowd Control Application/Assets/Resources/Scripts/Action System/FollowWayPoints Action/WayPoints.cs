using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(5)]
public struct WayPoint : IBufferElementData{
    public float3 value;
}

