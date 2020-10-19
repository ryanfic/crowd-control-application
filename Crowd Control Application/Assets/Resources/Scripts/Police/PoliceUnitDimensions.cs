using Unity.Entities;

// A component to keep track of the physical dimensions of a police unit
[GenerateAuthoringComponent]
public struct PoliceUnitDimensions : IComponentData
{
    public float LineSpacing; // the amount of space between the centerpoints of each police line
    public float LineLength; // how long the police line is, is a function of how many police officers there are in the line
    public float LineWidth; // how wide the police line is, equivalent to the space an officer takes up front to back
}


