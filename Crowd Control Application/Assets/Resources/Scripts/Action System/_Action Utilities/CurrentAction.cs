using Unity.Entities;
using crowd_Actions;

public struct CurrentAction : IComponentData {
    public int id; // the id of the action currently being performed
    public ActionType type; // the action type of the action currently being performed
    public Entity dataHolder; // the holder of the information of the current action
}
