using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BarricadeAOEComponent : IComponentData
{
    public float totalRadius;
    public float halfRadius;
    public float3 aoeCenter;
}

