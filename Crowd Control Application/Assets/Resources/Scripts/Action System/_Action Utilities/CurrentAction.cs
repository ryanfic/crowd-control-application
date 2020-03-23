using Unity.Entities;

public struct CurrentAction : IComponentData {
    public int id; // the id of the action currently being performed
    public ActionType type; // the action type of the action currently being performed
    
}
