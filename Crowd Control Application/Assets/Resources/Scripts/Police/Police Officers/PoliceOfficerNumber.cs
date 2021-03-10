using Unity.Entities;

// The number of the police officer within a Police Line
[GenerateAuthoringComponent]
public struct PoliceOfficerNumber : IComponentData
{
    public int Value;
}

