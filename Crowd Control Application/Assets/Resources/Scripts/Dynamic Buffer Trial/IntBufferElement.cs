using Unity.Entities;

[InternalBufferCapacity(5)]
public struct IntBufferElement : IBufferElementData{
    public int Value;
} 
