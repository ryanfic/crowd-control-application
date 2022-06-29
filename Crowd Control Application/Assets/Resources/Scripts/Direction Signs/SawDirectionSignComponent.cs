using Unity.Entities;

// The tag for objects that are triggers for when the agent has seen a direction sign 
[GenerateAuthoringComponent]
public struct SawDirectionSign : IComponentData
{
    public bool seen;
}