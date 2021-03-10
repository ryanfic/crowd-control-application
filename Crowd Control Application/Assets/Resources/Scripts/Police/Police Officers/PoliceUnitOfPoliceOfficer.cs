using Unity.Entities;

// The Police Unit that the Police Officer belongs to
[GenerateAuthoringComponent]
public struct PoliceUnitOfPoliceOfficer : IComponentData
{
    public Entity Value;
}