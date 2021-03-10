using Unity.Entities;

// A component to keep track of the physical dimensions of a police unit
[GenerateAuthoringComponent]
public struct PoliceUnitDimensions : IComponentData
{
    public float LineSpacing; // the amount of space between the centerpoints of each police line (when police lines are in a row)
    public float OfficerLength; // How long an officer is from front to back
    public float OfficerWidth; // how wide a police officer is, similar to the shoulder width of the police officer
    public int NumOfficersInLine1; // the number of officers in the first line
    public int NumOfficersInLine2; // the number of officers in the second line
    public int NumOfficersInLine3; // the number of officers in the third line
}


