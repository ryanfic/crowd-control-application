using Unity.Entities;

[GenerateAuthoringComponent]
public struct SuggestFollowWayPointsAction : IComponentData {
    public int id; // the id of the follow waypoints action to be added to the crowd agent
    public float frequency; // the frequency at which the police agent suggests following the waypoints
    public float lastSuggestionTime; // the time since the last suggestion
    public float radius; // the radius at which the crowd can hear the police agent's suggestion
    public Entity waypointHolder; // the entity that holds the waypoints that the police agent suggests
}

