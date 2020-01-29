using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct ReynoldsFlockBehaviour : IComponentData{
    public float AvoidanceRadius;
    public float AvoidanceWeight;
    public float CohesionRadius;
    public float CohesionWeight;
    //public float AlignmentRadius;
    //public float AlignmentWeight;
}
