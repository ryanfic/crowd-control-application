﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct HasReynoldsSeekTargetPos : IComponentData{
    public float3 targetPos;
}