﻿using Unity.Entities;

public struct FetchWayPoints : IComponentData {
    public int id; // the id of the action
    public Entity dataHolder; // the entity with the information to be obtained
}
