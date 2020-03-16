﻿using Unity.Entities;

public struct FollowWayPointsStorage : IComponentData {
    public int id; // the id of the action
    public int curPointNum; // the position of the point currently being moved towards
}

