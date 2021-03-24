using Unity.Entities;
using Unity.Mathematics;

// A label to signify that a police officer is at the correct location to be in formation
// The label also contains data on how much the police officer should rotate per tick of time
[GenerateAuthoringComponent]
public struct PoliceOfficerAtFormationLocation : IComponentData
{
    public quaternion RotationPerTick;
}


