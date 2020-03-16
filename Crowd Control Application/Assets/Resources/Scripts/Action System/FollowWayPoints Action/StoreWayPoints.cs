using Unity.Entities;
using Unity.Mathematics;
public struct StoreWayPoints : IComponentData {
    public int id; // the id of the action
    public Entity dataHolder; // where to store the new information
    public int curPointNum; // the position of the point currently being moved towards
}
