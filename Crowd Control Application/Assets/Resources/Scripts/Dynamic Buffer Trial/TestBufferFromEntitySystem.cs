using Unity.Entities;

public class TestBufferFromEntitySystem : ComponentSystem
{
    protected override void OnUpdate(){
        Entities.WithAll<Tag_Bob>().ForEach((Entity bobEntity) => {  // Cycle through all Entities with Tag_Bob component
            BufferFromEntity<IntBufferElement> intBufferFromEntity = GetBufferFromEntity<IntBufferElement>(); // used to access things with IntBufferElement Buffer

            Entity aliceEntity = Entity.Null; // Initially set Alice reference to Null

            // Find an Alice Entity
            Entities.WithAll<Tag_Alice>().ForEach((Entity aliceEntityTmp) => { 
                aliceEntity = aliceEntityTmp;
            });

            DynamicBuffer<IntBufferElement> aliceDynamicBuffer = intBufferFromEntity[aliceEntity]; // Get Alice's buffer using the BufferFromEntity and the Alice Entity reference
            
            for(int i = 0; i < aliceDynamicBuffer.Length; i++){ //go through everything in the buffer and increment
                IntBufferElement intBufferEle = aliceDynamicBuffer[i];
                intBufferEle.Value++;
                aliceDynamicBuffer[i] = intBufferEle;
            }
        });
    }
}
