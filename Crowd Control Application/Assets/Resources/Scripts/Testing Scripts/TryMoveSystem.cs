using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Physics;
using Unity.Transforms;


public class TryMoveSystem : SystemBase {

    private EndSimulationEntityCommandBufferSystem commandBufferSystem; // the command buffer system that runs after everything else

    protected override void OnCreate(){
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }

    protected override void OnUpdate(){

        EntityCommandBuffer.ParallelWriter commandBuffer = commandBufferSystem.CreateCommandBuffer().AsParallelWriter(); // create a command buffer
        /*JobHandle forwardHandle = Entities
            .WithAll<TryMoveForward>() 
            .ForEach((Entity mover, int entityInQueryIndex, ref PhysicsVelocity physicsVelocity)=>{
                physicsVelocity.Linear.x = 3f;
                //commandBuffer.RemoveComponent<TryMoveForward>(entityInQueryIndex, mover);
            }).Schedule(this.Dependency);*/

        JobHandle forwardHandle = Entities
            .WithAll<TryMoveForward>() 
            .ForEach((Entity mover, int entityInQueryIndex, ref Translation transl)=>{
                
                transl = new Translation { Value = new float3(transl.Value.x + 0.05f, transl.Value.y, transl.Value.z) };
                //commandBuffer.RemoveComponent<TryMoveForward>(entityInQueryIndex, mover);
            }).Schedule(this.Dependency);


        commandBufferSystem.AddJobHandleForProducer(forwardHandle);

        this.Dependency = forwardHandle;
    
        
    }



}



