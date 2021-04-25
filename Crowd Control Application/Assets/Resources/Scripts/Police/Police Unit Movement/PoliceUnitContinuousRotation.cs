using Unity.Entities;

// A component to tell the system the police unit should rotating (either to the right or left)
[GenerateAuthoringComponent]
public struct PoliceUnitContinuousRotation : IComponentData
{
    public bool RotateLeft;
    public bool WaitingAtAngle;
    public float WaitTime;
    public int LastWaitAngle;
    
}

