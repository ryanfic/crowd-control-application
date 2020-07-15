using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

public class PoliceUnitMovementSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float deltaTime = Time.DeltaTime;

        JobHandle jobHandle = Entities
            .WithAll<PoliceUnitComponent>()
            .ForEach((Entity policeSerial, ref Translation transl)=>{
                float3 result = new float3(1,0,0);
                transl.Value += result * deltaTime; //add movement to the translation            
            }).Schedule(inputDeps);

        return jobHandle;
    }
}

