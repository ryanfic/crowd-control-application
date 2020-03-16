using Unity.Entities;

public struct ChangeAction : IComponentData {
    public int fromId; // the id of the action to be changed from
    public ActionType fromType; // the action type to be changed from
    
}

