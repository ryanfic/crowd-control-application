using Unity.Entities;

//A list of police officers that are in the police unit
[InternalBufferCapacity(18)]
public struct OfficerInPoliceUnit : IBufferElementData {
    public Entity officer;

}

