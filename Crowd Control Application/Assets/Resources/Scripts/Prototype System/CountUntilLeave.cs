using Unity.Entities;

[GenerateAuthoringComponent]
public struct CountUntilLeave : IComponentData{
    public float timeWaited;
    public float timeUntilLeave;
}
