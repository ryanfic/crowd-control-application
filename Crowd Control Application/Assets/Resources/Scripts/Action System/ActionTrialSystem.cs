using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using crowd_Actions;

public class ActionTrialSystem : ComponentSystem
{
    protected override void OnUpdate(){
        BufferFromEntity<WayPoint> buffers = GetBufferFromEntity<WayPoint>(); // used to access things with waypoint buffer
        Entities.ForEach((Entity crowdEntity, DynamicBuffer<Action> actionBuffer) => {  
            //Entity wayPointHolder = actionBuffer[0].dataHolder; // get the entity that holds the waypoints

            //DynamicBuffer<Action> wayPointBuffer = buffers[wayPointHolder]; // Get the waypoint buffer
            
            /*for(int i = 0; i < wayPointBuffer.Length; i++){ 
                WayPoint wp = wayPointBuffer[i];
                wp.point.x++;
                wayPointBuffer[i] = wp;
                //Do stuff with waypoints
            }*/
        });
    }
}
