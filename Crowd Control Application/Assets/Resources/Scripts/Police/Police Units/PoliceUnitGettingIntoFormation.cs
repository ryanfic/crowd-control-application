using Unity.Entities;

// A label to signify that at least one police officer from the police unit is not in formation
// should not do any unit movements until every officer is in formation
[GenerateAuthoringComponent]
public struct PoliceUnitGettingIntoFormation : IComponentData
{
}
