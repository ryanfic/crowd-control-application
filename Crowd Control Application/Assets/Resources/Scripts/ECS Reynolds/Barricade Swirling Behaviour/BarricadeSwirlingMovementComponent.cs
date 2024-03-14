using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BarricadeSwirlingMovementComponent : IComponentData
{
    public float totalRadius;
    public float halfRadius;
    public float3 aoeCenter;
    public double lastTriggerCollision;
}
