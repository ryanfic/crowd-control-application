using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ObstacleAvoidanceMovementComponent : IComponentData
{
    public float movementPerRay;
    public int numberOfRays;
    public int visionAngle;
    public float visionLength;
}
