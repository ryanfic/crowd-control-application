using Unity.Entities;

// To tell the system that the police unit should change to the three sided box formation
[GenerateAuthoringComponent]
public struct ToCordonFormComponent : IComponentData
{
    public float LineSpacing;
    public float LineWidth;
}
