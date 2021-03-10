using Unity.Entities;

// The max rotation speed of the police unit
[GenerateAuthoringComponent]
public struct PoliceUnitRotationSpeed : IComponentData
{
    public float Value;
}


