using Unity.Entities;
using Unity.Jobs;

// Test using Buffers from a job
public class TestBufferJobSystem : JobComponentSystem
{
    public struct BufferJob : IJobForEachWithEntity_EB<IntBufferElement>{ // you use IJobForEachWithEntity_EB for dealing with buffers
                                                                            // it is worth noting that you pass in the type of what is stored in the buffer here
        public void Execute(Entity entity, int index, DynamicBuffer<IntBufferElement> dynamicBuffer) { // The execute function takes in a dynamic buffer (Not what is in the buffer)
            /*for(int i = 0; i < dynamicBuffer.Length; i++){ //go through everything in the buffer and increment
                IntBufferElement intBufferEle = dynamicBuffer[i];
                intBufferEle.Value++;
                dynamicBuffer[i] = intBufferEle;
            }*/
        }
    } 

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        return new BufferJob().Schedule(this, inputDeps);
    }
}
