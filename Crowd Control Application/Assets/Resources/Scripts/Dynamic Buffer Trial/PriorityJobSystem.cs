using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

public class PriorityJobSystem : ComponentSystem
{
    protected override void OnUpdate(){
        BufferFromEntity<WayPoint> buffers = GetBufferFromEntity<WayPoint>(); // used to access things with waypoint buffer
        Entities.ForEach((Entity crowdEntity, DynamicBuffer<PriorityElement> priBuffer) => {  
            Entity wayPointHolder = priBuffer[0].WPHolder; // get the entity that holds the waypoints

            DynamicBuffer<WayPoint> wayPointBuffer = buffers[wayPointHolder]; // Get the waypoint buffer
            
            for(int i = 0; i < wayPointBuffer.Length; i++){ 
                WayPoint wp = wayPointBuffer[i];
                wp.value.x++;
                wayPointBuffer[i] = wp;
                //Do stuff with waypoints
            }
        });
    }
}
