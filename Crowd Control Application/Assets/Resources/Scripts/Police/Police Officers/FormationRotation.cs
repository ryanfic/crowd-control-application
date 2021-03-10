using Unity.Entities;
using Unity.Mathematics;

// The rotation that the police officer must move to to be "in formation"
[GenerateAuthoringComponent]
public struct FormationRotation : IComponentData
{
    public quaternion Value;
}
