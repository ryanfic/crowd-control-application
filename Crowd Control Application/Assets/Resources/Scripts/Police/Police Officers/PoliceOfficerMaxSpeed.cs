using Unity.Entities;

// The max speed of the police officer
[GenerateAuthoringComponent]
public struct PoliceOfficerMaxSpeed : IComponentData
{
    public float Value;
}
