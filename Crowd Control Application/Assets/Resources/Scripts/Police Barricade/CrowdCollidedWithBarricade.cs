using Unity.Entities;

// The tag to say a crowd agent has collided with a  police barricade 

public struct CrowdCollidedWithBarricade : IComponentData
{
    public bool northwest;
}
