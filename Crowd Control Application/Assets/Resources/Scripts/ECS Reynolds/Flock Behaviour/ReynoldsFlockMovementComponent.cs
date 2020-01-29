using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ReynoldsFlockMovement : IComponentData{
    public float3 movement;
}
