using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct ReynoldsBehaviourWeights : IComponentData{
    public float maxVelocity; // The maximum speed of the agent
    public float flockWeight;
    public float seekWeight;
    public float fleeWeight;
    
}
