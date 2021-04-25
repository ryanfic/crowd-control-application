using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using System.IO;
using System;

// A system for counting (and outputting) the crowd agents in the simulation
public class CrowdCountingSystem : SystemBase {

    private bool onCrowdRepelStep;
    //public event EventHandler NoCrowdLeftEvent;

    protected override void OnCreate(){
        //onCrowdRepelStep = false;
    }

    /*private struct CountJob : IJobForEach<CrowdCounter> {
        public float time;
        public int crowdNumber;

        public void Execute(ref CrowdCounter counter){
            if((time - counter.lastCount) > counter.frequency){ // if the time since the last count is greater than the count frequency
                //Debug.Log("COUNT: " + crowdNumber + " AT " + time);// count
                counter.lastCount = time;

                StreamWriter sw = new StreamWriter("crowdflowdata.txt",true);
                string toadd = time + "," + crowdNumber;
                sw.WriteLine(toadd);
                sw.Close();
            }
        }
    }*/
    protected override void OnUpdate(){
        /*if(onCrowdRepelStep){
            EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<Crowd>());
            //Debug.Log("CrowdCount" + query.CalculateEntityCount());
            if(query.CalculateEntityCount() == 0){
                Debug.Log("No Crowd");
                //if(onCrowdRepelStep){
                    NoCrowdLeftEvent?.Invoke(this, EventArgs.Empty);
                    Debug.Log("Invoked the event");
                    onCrowdRepelStep = false;
                //}
            }
        }
        
        /*NativeArray<Entity> array = query.ToEntityArray(Allocator.TempJob); // Get the arrays corresponding to the entities queried
        
        int crowdCount = array.Length;

        array.Dispose();

        CountJob countJob = new CountJob{ // creates the counting job
            time = ((float)Time.ElapsedTime-15),
            crowdNumber = crowdCount*100,
        };
        JobHandle jobHandle = countJob.Schedule(this, inputDeps);

        return jobHandle;*/
    }
    /*public void setOnCrowdRepelStep(){
        Debug.Log("Started Counting");
        onCrowdRepelStep = true;
    }*/
    public int checkCrowdNumber(){
        EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<Crowd>());
            //Debug.Log("CrowdCount" + query.CalculateEntityCount());
        return query.CalculateEntityCount();
    }
}
