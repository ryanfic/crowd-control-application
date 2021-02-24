using Unity.Entities;
using Unity.Collections;

// To Label A Police Line
[GenerateAuthoringComponent]
public struct PoliceUnitName : IComponentData
{
    public FixedString64 String;
}

