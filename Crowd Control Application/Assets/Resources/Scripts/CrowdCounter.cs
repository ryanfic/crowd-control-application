using Unity.Entities;

[GenerateAuthoringComponent]
public struct CrowdCounter : IComponentData{
    public float frequency;
    public float lastCount;

}

