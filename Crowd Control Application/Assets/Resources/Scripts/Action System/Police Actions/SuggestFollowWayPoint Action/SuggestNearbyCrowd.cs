using Unity.Entities;
using Unity.Mathematics;

[InternalBufferCapacity(10)]
public struct SuggestNearbyCrowd : IBufferElementData{
    public Entity crowdAgent;
} 
