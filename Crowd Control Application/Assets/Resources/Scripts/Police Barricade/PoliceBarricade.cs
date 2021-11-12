using Unity.Entities;

// The tag for objects that are a police barricade 
[GenerateAuthoringComponent]
public struct PoliceBarricade : IComponentData
{
    public bool northwest;
}

