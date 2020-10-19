using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.IO;

// A system for counting (and outputting) the crowd agents in the simulation
public class CrowdAreaCountingSystem : JobComponentSystem {
    private NativeArray<int> count;
    private static float lastCountTime;

    public void OnStartRunning(){
        lastCountTime = (float)Time.ElapsedTime;
    }
    public void OnDestroy(){
        count.Dispose();
    }
    
    private struct CountJob : IJobForEach<CrowdAreaCounter> {
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Translation> targetArray;
        public NativeArray<int> count;

        public void Execute([ReadOnly] ref CrowdAreaCounter counter){
            
            for(int i = 0; i<targetArray.Length; i++){
                Translation trans = targetArray[i];
                if(trans.Value.x >= counter.minX && trans.Value.x <= counter.maxX
                    && trans.Value.z >= counter.minZ && trans.Value.z <= counter.maxZ
                ){ //check if translation is within the area
                    count[0]++;
                }
            }
        }
    }

    private struct OutputCountJob : IJobForEach<CrowdAreaCounter> {
        public float time;
        [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<int> count;

        public void Execute([ReadOnly]ref CrowdAreaCounter counter){
            //if((time - counter.lastCount) > counter.frequency){ // if the time since the last count is greater than the count frequency
                //Debug.Log("COUNT: " + crowdNumber + " AT " + time);// count
                
                //counter.lastCount = time;
                StreamWriter sw = new StreamWriter("crowdflowdata.txt",true);
                string toadd = time + "," + count[0];
                sw.WriteLine(toadd);
                sw.Close();
            //}
        }
    }
    protected override JobHandle OnUpdate(JobHandle inputDeps){
        float time = (float)Time.ElapsedTime;
        float frequency = 1f;
        //Debug.Log("Freq "+frequency+" diff " +(time - lastCountTime));
        if((time - lastCountTime)>frequency){
            lastCountTime = time;
            int[] countArray = {0};
            count = new NativeArray<int>(countArray, Allocator.TempJob);

            EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<Crowd>(),ComponentType.ReadOnly<Translation>());
            NativeArray<Translation> crowdTranslationArray = query.ToComponentDataArray<Translation>(Allocator.TempJob);

            JobHandle countJobHandle = new CountJob{ // creates the counting job
                targetArray = crowdTranslationArray,
                count = count
            }.Schedule(this,inputDeps);
            
            JobHandle outputJobHandle = new OutputCountJob{
                time = time,
                count = count
            }.Schedule(this,countJobHandle);


            return outputJobHandle;
        }
        else{
            return inputDeps;
        }
        
    }
}