using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(5)]
public struct FloatBufferElement : IBufferElementData{
    public float3 Value;
} 
