using Unity.Entities;

public struct RemoveAction : IComponentData {
    public int id; // the id of the action to be removed
}
