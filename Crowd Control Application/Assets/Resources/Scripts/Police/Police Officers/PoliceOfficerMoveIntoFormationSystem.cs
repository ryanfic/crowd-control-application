using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;


// A system that makes the police officer move into the correct translation and rotation to be 'in formation'
public class PoliceOfficerMoveIntoFormationSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc movementQueryDesc;
    private EntityQueryDesc rotationQueryDesc;
    private static readonly float moveTolerance = 0.01f;
    private static readonly float rotTolerance = 3f;
    private static readonly float rotSpeedDegreeValue = 3f;

    // The job that causes the police officer to get into formation in 2 steps:
    // Step 1: move towards the correct location
    private struct MoveToFormationLocation: IJobChunk{
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translType;
        public ComponentTypeHandle<Rotation> rotType;
        [ReadOnly] public ComponentTypeHandle<FormationLocation> formLocType;
        [ReadOnly] public ComponentTypeHandle<FormationRotation> formRotType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerMaxSpeed> speedType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerRotationSpeed> rotSpeedType;


        [ReadOnly] public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> translArray = chunk.GetNativeArray(translType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotType);
            NativeArray<FormationLocation> formLocArray = chunk.GetNativeArray(formLocType);
            NativeArray<FormationRotation> formRotArray = chunk.GetNativeArray(formRotType);
            NativeArray<PoliceOfficerMaxSpeed> speedArray = chunk.GetNativeArray(speedType);
            NativeArray<PoliceOfficerRotationSpeed> rotSpeedArray = chunk.GetNativeArray(rotSpeedType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation transl = translArray[i];
                Rotation rot = rotArray[i];
                FormationLocation formLoc = formLocArray[i];
                FormationRotation formRot = formRotArray[i];
                PoliceOfficerMaxSpeed speed = speedArray[i];
                PoliceOfficerRotationSpeed rotSpeed = rotSpeedArray[i];


                float3 direction = math.normalize((formLoc.Value - transl.Value));
                quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
                float angleToDestination = math.degrees(AngleBetweenQuaternions(rot.Value,turn));

                //Debug.Log("Angle: " + angle);
                if(math.distance(transl.Value,formLoc.Value) > moveTolerance){ // move towards the correct location (Step 1)
                    float3 result = (formLoc.Value - transl.Value) * deltaTime; // the direction of movement
                    if(math.distance(result,float3.zero) > speed.Value){// if the movement is faster than the max speed, cull the movement to match the max speed
                        result = math.normalize(result) * speed.Value;
                    }
                    float3 newTransl = result + transl.Value;
                    translArray[i] = new Translation{Value = newTransl};  
                    rotArray[i] = new Rotation{Value = RotateTowards(rot.Value, transl.Value, formLoc.Value, rotSpeed.Value * deltaTime)};
                }
                else{ // the officer is now in the correct position
                    //find how much the officer should rotate per tick
                    float angle = AngleBetweenQuaternions(rot.Value,formRot.Value);
                    Debug.Log("Angle: " + math.degrees(angle));

                    float percent;
                    if(angle == 0 || angle != angle){
                        percent = 1;
                    } 
                    else {
                        percent = math.radians(rotSpeed.Value * rotSpeedDegreeValue)/angle;
                    }
                    float numberOfTicks = math.ceil(1/percent);
                    percent = 1/numberOfTicks;
                    Debug.Log("Percent: " + percent);
                    if(percent > 1f){
                        percent = 1f;
                    }
                    Debug.Log("Number of ticks: " + (numberOfTicks));
                    quaternion rotPerTick = math.slerp(rot.Value, formRot.Value, percent);
                    quaternion inverse = math.inverse(rot.Value); // find out how much to rotate by to get back to forward from the current rotation
                    rotPerTick = math.mul(rotPerTick, inverse);  // get the relative rotation per tick
                    
                    commandBuffer.AddComponent<PoliceOfficerAtFormationLocation>(chunkIndex,entity, new PoliceOfficerAtFormationLocation{
                        RotationPerTick = rotPerTick
                    }); // signify that the officer is at the correct position
                }    
            }
        }
    }
    // Step 2: rotate to the correct rotation
    private struct RotateToFormationDirection : IJobChunk{
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translType;
        public ComponentTypeHandle<Rotation> rotType;
        [ReadOnly] public ComponentTypeHandle<FormationLocation> formLocType;
        [ReadOnly] public ComponentTypeHandle<FormationRotation> formRotType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitOfPoliceOfficer> unitType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerAtFormationLocation> atLocationType;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> translArray = chunk.GetNativeArray(translType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotType);
            NativeArray<FormationLocation> formLocArray = chunk.GetNativeArray(formLocType);
            NativeArray<FormationRotation> formRotArray = chunk.GetNativeArray(formRotType);
            NativeArray<PoliceUnitOfPoliceOfficer> unitArray = chunk.GetNativeArray(unitType);
            NativeArray<PoliceOfficerAtFormationLocation> atLocationArray = chunk.GetNativeArray(atLocationType);


            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation transl = translArray[i];
                Rotation rot = rotArray[i];
                FormationLocation formLoc = formLocArray[i];
                FormationRotation formRot = formRotArray[i];
                PoliceUnitOfPoliceOfficer policeUnit = unitArray[i];
                PoliceOfficerAtFormationLocation officerRotPerTick = atLocationArray[i];

                float finalAngleDiff = math.degrees(AngleBetweenQuaternions(rot.Value,formRot.Value));

                if(math.distance(transl.Value,formLoc.Value) > moveTolerance){ // if the officer was knocked away from their formation location
                    commandBuffer.RemoveComponent<PoliceOfficerAtFormationLocation>(chunkIndex,entity); //signify that the officer should move back into position
                }
                else if(finalAngleDiff > rotTolerance){ // rotate towards the correct rotation (Step 2)
                    quaternion newRot = math.mul(rot.Value, officerRotPerTick.RotationPerTick);//math.mul(math.mul(officerRotPerTick.RotationPerTick,rot.Value),math.inverse(officerRotPerTick.RotationPerTick));
                    rotArray[i] = new Rotation{Value = newRot};
                }
                else{ // the officer is now in the proper formation
                    commandBuffer.RemoveComponent<PoliceOfficerAtFormationLocation>(chunkIndex,entity); // we no longer need to label the officer as being in formation
                    commandBuffer.RemoveComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity); // signify that the officer is no longer out of formation
                    commandBuffer.AppendToBuffer<OfficerInFormation>(chunkIndex, policeUnit.Value, new OfficerInFormation{}); // signify to the police unit that one more officer is in formation
                }    
            }
        }
    }
    /*private struct OfficerMoveIntoFormationJob : IJobChunk {
        public EntityCommandBuffer.ParallelWriter commandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<Translation> translType;
        public ComponentTypeHandle<Rotation> rotType;
        [ReadOnly] public ComponentTypeHandle<FormationLocation> formLocType;
        [ReadOnly] public ComponentTypeHandle<FormationRotation> formRotType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerMaxSpeed> speedType;
        [ReadOnly] public ComponentTypeHandle<PoliceOfficerRotationSpeed> rotSpeedType;
        [ReadOnly] public ComponentTypeHandle<PoliceUnitOfPoliceOfficer> unitType;


        [ReadOnly] public float deltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<Translation> translArray = chunk.GetNativeArray(translType);
            NativeArray<Rotation> rotArray = chunk.GetNativeArray(rotType);
            NativeArray<FormationLocation> formLocArray = chunk.GetNativeArray(formLocType);
            NativeArray<FormationRotation> formRotArray = chunk.GetNativeArray(formRotType);
            NativeArray<PoliceOfficerMaxSpeed> speedArray = chunk.GetNativeArray(speedType);
            NativeArray<PoliceOfficerRotationSpeed> rotSpeedArray = chunk.GetNativeArray(rotSpeedType);
            NativeArray<PoliceUnitOfPoliceOfficer> unitArray = chunk.GetNativeArray(unitType);

            for(int i = 0; i < chunk.Count; i++){
                Entity entity = entityArray[i];  
                Translation transl = translArray[i];
                Rotation rot = rotArray[i];
                FormationLocation formLoc = formLocArray[i];
                FormationRotation formRot = formRotArray[i];
                PoliceOfficerMaxSpeed speed = speedArray[i];
                PoliceOfficerRotationSpeed rotSpeed = rotSpeedArray[i];
                PoliceUnitOfPoliceOfficer policeUnit = unitArray[i];


                float3 direction = math.normalize((formLoc.Value - transl.Value));
                quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
                float angleToDestination = math.degrees(AngleBetweenQuaternions(rot.Value,turn));
                float finalAngleDiff = math.degrees(AngleBetweenQuaternions(rot.Value,formRot.Value));

                //Debug.Log("Angle: " + angle);
                if(math.distance(transl.Value,formLoc.Value) > moveTolerance){ // move towards the correct location (Step 1)
                    float3 result = (formLoc.Value - transl.Value) * deltaTime; // the direction of movement
                    if(math.distance(result,float3.zero) > speed.Value){// if the movement is faster than the max speed, cull the movement to match the max speed
                        result = math.normalize(result) * speed.Value;
                    }
                    float3 newTransl = result + transl.Value;
                    translArray[i] = new Translation{Value = newTransl};  
                    rotArray[i] = new Rotation{Value = RotateTowards(rot.Value, transl.Value, formLoc.Value, rotSpeed.Value * deltaTime)};
                }
                else if(finalAngleDiff > rotTolerance){ // rotate towards the correct rotation (Step 2)
                    rotArray[i] = new Rotation{Value = RotateTowards(rot.Value, formRot.Value, rotSpeed.Value * deltaTime)};
                }
                else{ // the officer is now in the proper formation
                    commandBuffer.RemoveComponent<PoliceOfficerOutOfFormation>(chunkIndex,entity); // signify that the officer is no longer out of formation
                    commandBuffer.AppendToBuffer<OfficerInFormation>(chunkIndex, policeUnit.Value, new OfficerInFormation{}); // signify to the police unit that one more officer is in formation
                }    
            }
        }
    }*/



    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        movementQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceOfficerOutOfFormation>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<FormationLocation>(),
                ComponentType.ReadOnly<FormationRotation>(),
                ComponentType.ReadOnly<PoliceOfficerMaxSpeed>(),
                ComponentType.ReadOnly<PoliceOfficerRotationSpeed>(),
                ComponentType.ReadOnly<PoliceUnitOfPoliceOfficer>(),
                typeof(Translation),
                typeof(Rotation)
            },
            None = new ComponentType[]{
                ComponentType.ReadOnly<PoliceOfficerAtFormationLocation>()/*,
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<ToParallelCordonFormComponent>(),
                ComponentType.ReadOnly<ToSingleCordonFormComponent>(),
                ComponentType.ReadOnly<ToWedgeFormComponent>()*/
            }
        };

        rotationQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                ComponentType.ReadOnly<PoliceOfficerOutOfFormation>(),
                ComponentType.ReadOnly<PoliceOfficer>(),
                ComponentType.ReadOnly<FormationLocation>(),
                ComponentType.ReadOnly<FormationRotation>(),
                ComponentType.ReadOnly<PoliceOfficerMaxSpeed>(),
                ComponentType.ReadOnly<PoliceOfficerRotationSpeed>(),
                ComponentType.ReadOnly<PoliceUnitOfPoliceOfficer>(),
                ComponentType.ReadOnly<PoliceOfficerAtFormationLocation>(),
                typeof(Translation),
                typeof(Rotation)
            }/*,
            None = new ComponentType[]{
                ComponentType.ReadOnly<To3SidedBoxFormComponent>(),
                ComponentType.ReadOnly<ToParallelCordonFormComponent>(),
                ComponentType.ReadOnly<ToSingleCordonFormComponent>(),
                ComponentType.ReadOnly<ToWedgeFormComponent>()
            }*/
        };

        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery movementQuery = GetEntityQuery(movementQueryDesc); // query the entities

        EntityQuery rotQuery = GetEntityQuery(rotationQueryDesc); // query the entities

        MoveToFormationLocation movementJob = new MoveToFormationLocation{ // creates the movement job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            translType = GetComponentTypeHandle<Translation>(),
            rotType = GetComponentTypeHandle<Rotation>(),
            formLocType = GetComponentTypeHandle<FormationLocation>(true),
            formRotType = GetComponentTypeHandle<FormationRotation>(true),
            speedType = GetComponentTypeHandle<PoliceOfficerMaxSpeed>(true),
            rotSpeedType = GetComponentTypeHandle<PoliceOfficerRotationSpeed>(true),
            deltaTime = Time.DeltaTime
        };
        JobHandle movementJobHandle = movementJob.Schedule(movementQuery, this.Dependency);        

        commandBufferSystem.AddJobHandleForProducer(movementJobHandle); // tell the system to execute the command buffer after the job has been completed

        RotateToFormationDirection rotJob = new RotateToFormationDirection{ // creates the rotation job
            commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType =  GetEntityTypeHandle(),
            translType = GetComponentTypeHandle<Translation>(),
            rotType = GetComponentTypeHandle<Rotation>(),
            formLocType = GetComponentTypeHandle<FormationLocation>(true),
            formRotType = GetComponentTypeHandle<FormationRotation>(true),
            unitType = GetComponentTypeHandle<PoliceUnitOfPoliceOfficer>(true),
            atLocationType = GetComponentTypeHandle<PoliceOfficerAtFormationLocation>(true),
        };
        JobHandle rotJobHandle = rotJob.Schedule(rotQuery, movementJobHandle);        

        commandBufferSystem.AddJobHandleForProducer(rotJobHandle); // tell the system to execute the command buffer after the job has been completed

        this.Dependency = rotJobHandle;
    }



    // don't use a speed greater than 1
    // does not rotate upwards
    private static quaternion RotateTowards(quaternion initRotation, float3 fromPos, float3 toPos, float speed = 1){
        float3 start = new float3(fromPos.x,0,fromPos.z);
        float3 end = new float3(toPos.x,0,toPos.z);
        float3 direction = math.normalize((end - start));
        quaternion turn = quaternion.LookRotationSafe(direction, new float3(0f,1f,0f));
        return math.slerp(initRotation, turn, speed);
    }

    private static quaternion RotateTowards(quaternion initRotation, quaternion finalRotation, float speed = 1){
        return math.slerp(initRotation, finalRotation, speed);
    }


    // angle from q1 to q2
    // angle is always positive
    // angle is in radians (use math.degrees() to get degrees)
    private static float AngleBetweenQuaternions(quaternion q1, quaternion q2){
        quaternion difference = math.mul(q1,math.inverse(q2)); // the angle between q1 and q2
        float angle = 2 * math.acos(difference.value[3]); // angle in rads
        return angle;

    }
}

