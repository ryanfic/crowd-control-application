using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(10)]
public struct ReynoldsNearbyFlockVel : IBufferElementData{
    public float3 Value;
} 
