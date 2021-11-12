using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using crowd_Actions;

// Adds an action to the action queue
public class AddActionSystem : SystemBase {
    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    private EntityQueryDesc addActionQueryDesc;

    private struct AddActionJob: IJobChunk {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        /*[DeallocateOnJobCompletion] public NativeArray<Entity> entityArray;
        [DeallocateOnJobCompletion] public NativeArray<CurrentAction> curActionArray;
        public BufferFromEntity<Action> buffers;*/

        [ReadOnly] public EntityTypeHandle entityType;
        public ComponentTypeHandle<CurrentAction> curActionType;
        public ComponentTypeHandle<AddFollowWayPointsAction> addFollowWPActionType;
        public ComponentTypeHandle<AddGoHomeAction> addGoHomeActionType;
        public ComponentTypeHandle<AddGoToAndWaitAction> addGoToAndWaitActionType;
        public BufferTypeHandle<Action> actionBufferType;
        

        //[DeallocateOnJobCompletion]
        //public NativeArray<ArchetypeChunk> chunks;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex){
            NativeArray<Entity> entityArray = chunk.GetNativeArray(entityType);
            NativeArray<CurrentAction> curActionArray = chunk.GetNativeArray(curActionType);
            BufferAccessor<Action> buffers = chunk.GetBufferAccessor<Action>(actionBufferType);

            for(int i = 0; i < chunk.Count; i++){
                Entity curEntity = entityArray[i];
            
                int newActionPriority = 0;
                float newActionTimeCreated = 0;

                Action toAddAction = new Action { // action to be added, instantiated to generic values
                    id = 0,
                    priority = 0,
                    type = ActionType.No_Action,
                    timeCreated = 0,
                    dataHolder = Entity.Null
                };
                
                //depending on what type of "add action" component the entity has, instantate the values of the new action
                if(chunk.Has<AddFollowWayPointsAction>(addFollowWPActionType)){
                    NativeArray<AddFollowWayPointsAction> newActions = chunk.GetNativeArray(addFollowWPActionType); // obtain the actual data
                    
                    //set up the correct data into the toAddAction
                    toAddAction.id = newActions[i].id;
                    toAddAction.priority = newActions[i].priority;
                    toAddAction.type = ActionType.Follow_WayPoints;
                    toAddAction.timeCreated = newActions[i].timeCreated;
                    toAddAction.dataHolder = newActions[i].dataHolder;

                    newActionPriority = newActions[i].priority;
                    newActionTimeCreated = newActions[i].timeCreated;

                    entityCommandBuffer.RemoveComponent<AddFollowWayPointsAction>(chunkIndex,curEntity); // remove the 'add action' component  
                }
                else if(chunk.Has<AddGoHomeAction>(addGoHomeActionType)){
                    NativeArray<AddGoHomeAction> newActions = chunk.GetNativeArray(addGoHomeActionType);
                    
                    //set up the correct data into the toAddAction
                    toAddAction.id = newActions[i].id;
                    toAddAction.priority = newActions[i].priority;
                    toAddAction.type = ActionType.Go_Home;
                    toAddAction.timeCreated = newActions[i].timeCreated;
                    toAddAction.dataHolder = newActions[i].dataHolder;

                    newActionPriority = newActions[i].priority;
                    newActionTimeCreated = newActions[i].timeCreated;

                    entityCommandBuffer.RemoveComponent<AddGoHomeAction>(chunkIndex,curEntity); // remove the 'add action' component  
                }
                else if(chunk.Has<AddGoToAndWaitAction>(addGoToAndWaitActionType)){
                    NativeArray<AddGoToAndWaitAction> newActions = chunk.GetNativeArray(addGoToAndWaitActionType); // obtain the actual data
                    
                    //set up the correct data into the toAddAction
                    toAddAction.id = newActions[i].id;
                    toAddAction.priority = newActions[i].priority;
                    toAddAction.type = ActionType.Go_And_Wait;
                    toAddAction.timeCreated = newActions[i].timeCreated;
                    toAddAction.dataHolder = newActions[i].dataHolder;

                    newActionPriority = newActions[i].priority;
                    newActionTimeCreated = newActions[i].timeCreated;

                    entityCommandBuffer.RemoveComponent<AddGoToAndWaitAction>(chunkIndex,curEntity); // remove the 'add action' component  
                }

                int actionPos = FindActionPos(buffers[i], newActionPriority, newActionTimeCreated);
                AddAction(chunkIndex, curEntity, buffers[i], actionPos, curActionArray[i], toAddAction);
            }

            
        }
        
        private int FindActionPos(DynamicBuffer<Action> actions, int newActionPriority, float newActionTimeCreated){
            int pos = 0; // where the action should be added
             // find the index where the action should be added
            while(pos < actions.Length && newActionPriority <= actions[pos].priority){
                if(newActionPriority == actions[pos].priority){ // if the priorities are the same
                    //compare the times
                    if(newActionTimeCreated >= actions[pos].timeCreated){ // if the current action time is greater than the other action's time, this action should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(newActionPriority < actions[pos].priority){ // if this action's priority is smaller than the other action's priority, this action should go later
                    pos++;
                }
                else
                    break;
            }     
            return pos;
        }
        private void AddAction(int chunkIndex, Entity entity, DynamicBuffer<Action> actions, int posToAdd, CurrentAction current, Action toAdd){
            if(posToAdd == 0){ // if the action was added at the start of the buffer
                //Debug.Log("Added to start!");
                if(current.type == ActionType.No_Action)
                    entityCommandBuffer.AddComponent<ChangeAction>(chunkIndex,entity, new ChangeAction{}); // tell the system that the current action should be changed
            }
            else{ //
                //Debug.Log("Added after start");
            }
            // Add the action at the correct position
            actions.Insert(posToAdd, toAdd);
        }
    }

    /*private struct AddFollowWayPointsActionJob : IJobForEachWithEntity_EBCC<Action,AddFollowWayPointsAction,CurrentAction> {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref AddFollowWayPointsAction toAdd, ref CurrentAction current){
            int pos = 0; // where the action should be added
             // find the index where the action should be added
            while(pos < actions.Length && toAdd.priority <= actions[pos].priority){
                if(toAdd.priority == actions[pos].priority){ // if the priorities are the same
                    //compare the times
                    if(toAdd.timeCreated >= actions[pos].timeCreated){ // if the current action time is greater than the other action's time, this action should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(toAdd.priority < actions[pos].priority){ // if this action's priority is smaller than the other action's priority, this action should go later
                    pos++;
                }
                else
                    break;
            }
            if(pos == 0){ // if the action was added at the start of the buffer
                Debug.Log("Added to start!");
                if(current.type == ActionType.No_Action)
                    entityCommandBuffer.AddComponent<ChangeAction>(index,entity, new ChangeAction{}); // tell the system that the current action should be changed
            }
            else{ //
                Debug.Log("Added after start");
            }
            // Add the action at the correct position
            actions.Insert( 
                    pos,
                    new Action {
                        id = toAdd.id,
                        priority = toAdd.priority,
                        type = ActionType.Follow_WayPoints,
                        timeCreated = toAdd.timeCreated,
                        dataHolder = toAdd.dataHolder
                    });
            
            entityCommandBuffer.RemoveComponent<AddFollowWayPointsAction>(index,entity); // remove this component
        }
        
    }

    private struct AddGoHomeActionJob : IJobForEachWithEntity_EBCC<Action,AddGoHomeAction,CurrentAction> {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job

        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref AddGoHomeAction toAdd, ref CurrentAction current){
            int pos = 0; // where the action should be added
             // find the index where the action should be added
            while(pos < actions.Length && toAdd.priority <= actions[pos].priority){
                if(toAdd.priority == actions[pos].priority){ // if the priorities are the same
                    //compare the times
                    if(toAdd.timeCreated >= actions[pos].timeCreated){ // if the current action time is greater than the other action's time, this action should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(toAdd.priority < actions[pos].priority){ // if this action's priority is smaller than the other action's priority, this action should go later
                    pos++;
                }
                else
                    break;
            }
            if(pos == 0){ // if the action was added at the start of the buffer
                Debug.Log("Added to start!");
                if(current.type == ActionType.No_Action)
                    entityCommandBuffer.AddComponent<ChangeAction>(index,entity, new ChangeAction{}); // tell the system that the current action should be changed
            }
            else{ //
                Debug.Log("Added after start");
            }
            // Add the action at the correct position
            actions.Insert( 
                    pos,
                    new Action {
                        id = toAdd.id,
                        priority = toAdd.priority,
                        type = ActionType.Go_Home,
                        timeCreated = toAdd.timeCreated,
                        dataHolder = toAdd.dataHolder
                    });
            
            entityCommandBuffer.RemoveComponent<AddGoHomeAction>(index,entity); // remove this component
        }
        
    }

    private struct AddGoToAndWaitActionJob : IJobForEachWithEntity_EBCC<Action,AddGoToAndWaitAction,CurrentAction> {
        public EntityCommandBuffer.ParallelWriter entityCommandBuffer; //Entity command buffer to allow adding/removing components inside the job
        public void Execute(Entity entity, int index, DynamicBuffer<Action> actions, ref AddGoToAndWaitAction toAdd, ref CurrentAction current){
            int pos = 0; // where the action should be added
             // find the index where the action should be added
            while(pos < actions.Length && toAdd.priority <= actions[pos].priority){
                if(toAdd.priority == actions[pos].priority){ // if the priorities are the same
                    //compare the times
                    if(toAdd.timeCreated >= actions[pos].timeCreated){ // if the current action time is greater than the other action's time, this action should go later
                        pos++;
                    }
                    else 
                        break;
                }
                else if(toAdd.priority < actions[pos].priority){ // if this action's priority is smaller than the other action's priority, this action should go later
                    pos++;
                }
                else
                    break;
            }
            if(pos == 0){ // if the action was added at the start of the buffer
                Debug.Log("Added to start!");
                if(current.type == ActionType.No_Action)
                    entityCommandBuffer.AddComponent<ChangeAction>(index,entity, new ChangeAction{}); // tell the system that the current action should be changed
            }
            else{ //
                Debug.Log("Added after start");
            }
            // Add the action at the correct position
            actions.Insert( 
                    pos,
                    new Action {
                        id = toAdd.id,
                        priority = toAdd.priority,
                        type = ActionType.Go_And_Wait,
                        timeCreated = toAdd.timeCreated,
                        dataHolder = toAdd.dataHolder
                    });
            
            entityCommandBuffer.RemoveComponent<AddGoToAndWaitAction>(index,entity); // remove this component
        }
        
    }*/


    protected override void OnCreate() {
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        addActionQueryDesc = new EntityQueryDesc{
            All = new ComponentType[]{
                typeof(Action),
                ComponentType.ReadOnly<CurrentAction>()
            },
            Any = new ComponentType[]{
                ComponentType.ReadOnly<AddFollowWayPointsAction>(),
                ComponentType.ReadOnly<AddGoHomeAction>(),
                ComponentType.ReadOnly<AddGoToAndWaitAction>()
            }
        }; // define what we are looking for in the add action job
        base.OnCreate();
    }
    protected override void OnUpdate(){
        EntityQuery addActionQuery = GetEntityQuery(addActionQueryDesc); //Query the entities with the correct components

        AddActionJob addActionJob = new AddActionJob {
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(),
            entityType = GetEntityTypeHandle(),
            curActionType = GetComponentTypeHandle<CurrentAction>(),
            addFollowWPActionType = GetComponentTypeHandle<AddFollowWayPointsAction>(),
            addGoHomeActionType = GetComponentTypeHandle<AddGoHomeAction>(),
            addGoToAndWaitActionType = GetComponentTypeHandle<AddGoToAndWaitAction>(),
            actionBufferType = GetBufferTypeHandle<Action>()
        };

        JobHandle addActionHandle = addActionJob.Schedule(addActionQuery, this.Dependency);
        addActionHandle.Complete();
        //this.Dependency = addActionHandle;
        


        /*BufferFromEntity<Action> actionBuffFromEnt = GetBufferFromEntity<Action>();
        NativeArray<Entity> addActionEntityArray = addActionQuery.ToEntityArray(Allocator.TempJob);//get the array of entities
        NativeArray<CurrentAction> curActionArray = addActionQuery.ToComponentDataArray<CurrentAction>(Allocator.TempJob);*/
        //schedule the job
        
        //commandBufferSystem.AddJobHandleForProducer(followJobHandle);// do the command buffer thing

        //this.Dependency = ;// update the dependencies

       
       
        /*AddFollowWayPointsActionJob addFollowWPJob = new AddFollowWayPointsActionJob{ // creates the "follow waypoints" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter()
        };
        JobHandle followJobHandle = addFollowWPJob.Schedule(this, inputDeps);

        commandBufferSystem.AddJobHandleForProducer(followJobHandle); // tell the system to execute the command buffer after the job has been completed

        AddGoHomeActionJob addGoHomeJob = new AddGoHomeActionJob{ // creates the "Go home" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter()
        };
        JobHandle goJobHandle = addGoHomeJob.Schedule(this, followJobHandle);

        commandBufferSystem.AddJobHandleForProducer(goJobHandle); // tell the system to execute the command buffer after the job has been completed

        AddGoToAndWaitActionJob addGoWaitJob = new AddGoToAndWaitActionJob{ // creates the "Go and wait" job
            entityCommandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter()
        };
        JobHandle goWaitJobHandle = addGoWaitJob.Schedule(this, goJobHandle);

        commandBufferSystem.AddJobHandleForProducer(goWaitJobHandle); // tell the system to execute the command buffer after the job has been completed*/

    }
}
