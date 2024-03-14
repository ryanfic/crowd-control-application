using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FinalDestinationComponent : IComponentData
{
    public float3 destination;
}
