using Unity.Entities;
using Unity.Mathematics;

public enum Season{
    Winter,
    Summer,
    Spring,
    Fall
}

public struct Neato{
    public int thing;
}

[InternalBufferCapacity(5)]
public struct PriorityElement : IBufferElementData{
    public int priority;
    public int message;
    public float timeAdded;

    public Season season;
    public Neato neato;
    public Entity WPHolder;
    
} 

public struct WayPoint : IBufferElementData{
    public float3 point;
}

