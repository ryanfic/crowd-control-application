using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ReynoldsMovementValues : IComponentData{
    public float3 flockMovement;
    public float3 seekMovement;
    public float3 fleeMovement;
    
}