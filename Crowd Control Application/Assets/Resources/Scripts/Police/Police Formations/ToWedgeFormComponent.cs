using Unity.Entities;

// To tell the system that the police unit should change to the wedge formation
[GenerateAuthoringComponent]
public struct ToWedgeFormComponent : IComponentData
{
    public float Angle; // The distance from the back of an officer in the front line to the front of an officer in the center line
    public float OfficerLength; // the distance from the back to the front of an officer
    public float OfficerWidth; // the distance from the left side to the right side of an officer (similar to shoulder width)
    public int NumOfficersInLine1; // the number of officers in the first line
    public int NumOfficersInLine2; // the number of officers in the second line
    public int NumOfficersInLine3; // the number of officers in the third line

    public int MiddleOfficerNum; // the number of the middle officer (the lead officer)

    public int TotalOfficers; // the number of officers in the police unit
}
