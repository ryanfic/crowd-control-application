using Unity.Entities;

[GenerateAuthoringComponent]
[InternalBufferCapacity(5)]
public struct IntBufferElement : IBufferElementData{
    public int Value;
} 
