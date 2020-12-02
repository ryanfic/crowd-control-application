using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct StationaryQuadrantEntity : IComponentData{ // Quadrant System only works with entities with this component
    //empty component works fine, but we can add info too
    public TypeEnum typeEnum;

    public enum TypeEnum{
        Intersection,
        StopSign
    }
}

