using Unity.Entities;
using Unity.Mathematics;

// The location that the police officer must move to to be "in formation"
[GenerateAuthoringComponent]
public struct FormationLocation : IComponentData
{
    public float3 Value;
}
