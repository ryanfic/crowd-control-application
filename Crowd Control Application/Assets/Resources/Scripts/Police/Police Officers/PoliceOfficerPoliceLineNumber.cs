using Unity.Entities;

// The number of the Police Line that the Police Officer belongs to
[GenerateAuthoringComponent]
public struct PoliceOfficerPoliceLineNumber : IComponentData
{
    public int Value;
}
