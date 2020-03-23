using Unity.Entities;

public struct AddFollowWayPointsAction : IComponentData {
    public int id; // the id of the action to be added
    public int priority; // the priority of the action to be added
    public float timeCreated; // the time the action was added (thus created)
    public Entity dataHolder; // the entity that holds the data for the action
}
