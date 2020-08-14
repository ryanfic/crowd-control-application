using Unity.Entities;

//[GenerateAuthoringComponent]
public struct CrowdAreaCounter : IComponentData{
    //public float frequency;
    //public float lastCount;
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;
}
