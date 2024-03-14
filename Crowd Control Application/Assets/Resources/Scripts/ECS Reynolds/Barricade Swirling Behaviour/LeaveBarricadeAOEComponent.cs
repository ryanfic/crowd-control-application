using Unity.Entities;

// A label that signifies that an Entity is leaving the barricade's AOE
[GenerateAuthoringComponent]
public struct LeaveBarricadeAOEComponent : IComponentData
{
    public double lastTriggerCollision;
}
