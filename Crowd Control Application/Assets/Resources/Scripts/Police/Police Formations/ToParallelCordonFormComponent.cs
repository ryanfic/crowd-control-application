using Unity.Entities;

// To tell the system that the police unit should change to the three sided box formation
[GenerateAuthoringComponent]
public struct ToParallelCordonFormComponent : IComponentData
{
    public float LineSpacing; // The distance from the back of an officer in the front line to the front of an officer in the center line
    public float OfficerLength; // the distance from the back to the front of an officer
    public float OfficerWidth; // the distance from the left side to the right side of an officer (similar to shoulder width)
    public float OfficerSpacing; // The space between officers within a line
    public int NumOfficersInLine1; // the number of officers in the first line
    public int NumOfficersInLine2; // the number of officers in the second line
    public int NumOfficersInLine3; // the number of officers in the third line
}
