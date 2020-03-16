using Unity.Entities;
using Unity.Mathematics;

/*public enum ActionTypes{
    Follow_Waypoints,
    Go_Home
}*/

public struct Neato{
    public int thing;
}

[InternalBufferCapacity(5)]
public struct PriorityElement : IBufferElementData{
    public int priority;
    public int message;
    public float timeAdded;

    public Entity WPHolder;
    
} 

/*public struct WayPoint : IBufferElementData{
    public float3 point;
}
*/
