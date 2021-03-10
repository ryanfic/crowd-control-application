using Unity.Entities;

// The max rotation speed of the police officer
[GenerateAuthoringComponent]
public struct PoliceOfficerRotationSpeed : IComponentData
{
    public float Value;
}
