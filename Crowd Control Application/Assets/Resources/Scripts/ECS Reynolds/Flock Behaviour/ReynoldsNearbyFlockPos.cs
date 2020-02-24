using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(10)]
public struct ReynoldsNearbyFlockPos : IBufferElementData{
    public float3 Value;
} 

