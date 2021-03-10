using Unity.Entities;

// The max speed of the police unit
[GenerateAuthoringComponent]
public struct PoliceUnitMaxSpeed : IComponentData
{
    public float Value;
}
