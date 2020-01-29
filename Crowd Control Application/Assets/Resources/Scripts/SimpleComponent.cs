using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct SimpleComponent : IComponentData{
    public float3 position;
    public bool isTrue;
}

