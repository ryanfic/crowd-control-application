using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct ReynoldsBehaviourData : IComponentData{
    public float maxVelocity; // The maximum speed of the agent
    public float fleeWeight;
    public float flockWeight;
    public float seekWeight;
    
}
