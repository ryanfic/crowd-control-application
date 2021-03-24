using Unity.Entities;

// To tell the system that the police unit should change to the single line cordon formation
[GenerateAuthoringComponent]
public struct ToSingleCordonFormComponent : IComponentData
{
    public float OfficerWidth; // the distance from the left side to the right side of an officer (similar to shoulder width)
    public float OfficerSpacing; // The space between officers within a line
    public int NumOfficersInLine1; // the number of officers in the first line
    public int NumOfficersInLine2; // the number of officers in the second line
    public int NumOfficersInUnit; // the number of officers in the police unit
}
