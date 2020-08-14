using Unity.Entities;
using Unity.Mathematics;

// The location that the police unit wants to move to
[GenerateAuthoringComponent]
public struct PoliceUnitMovementDestination : IComponentData
{
    public float3 Value;
}
