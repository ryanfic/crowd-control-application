using Unity.Entities;

public class TestBufferSystem : ComponentSystem{
    protected override void OnUpdate(){
        /*Entities.ForEach((DynamicBuffer<IntBufferElement> dynamicBuffer) =>{ // pass in the buffer
            for(int i = 0; i < dynamicBuffer.Length; i++){ //go through everything in the buffer and increment
                IntBufferElement intBufferEle = dynamicBuffer[i];
                intBufferEle.Value++;
                dynamicBuffer[i] = intBufferEle;
            }
        });*/
    }
}