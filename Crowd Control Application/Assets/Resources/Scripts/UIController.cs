using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public class UIController : JobComponentSystem {

    public event EventHandler On1Down;
    public event EventHandler On2Down;

    public struct UIControllerComponent :IComponentData {
        public bool Enabled;
    }

    private DOTSEvents_NextFrame<Btn1DownEvent> btn1Events;
    private DOTSEvents_NextFrame<Btn2DownEvent> btn2Events;

    public struct Btn1DownEvent : IComponentData {
        public double Value; 
    }

    public struct Btn2DownEvent : IComponentData {
        public double Value; 
    }

    protected override void OnCreate(){
        btn1Events = new DOTSEvents_NextFrame<Btn1DownEvent>(World);
        btn2Events = new DOTSEvents_NextFrame<Btn2DownEvent>(World);
        Entity controller = EntityManager.CreateEntity();
        EntityManager.AddComponentData<UIControllerComponent>(controller, new UIControllerComponent{
            Enabled = true
        });

        //For Testing
        On1Down += OneDownResponse;
        On2Down += TwoDownResponse;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps){
        bool btn1Down = Input.GetKeyDown(KeyCode.Alpha1);
        bool btn2Down = Input.GetKeyDown(KeyCode.Alpha2);

        DOTSEvents_NextFrame<Btn1DownEvent>.EventTrigger btn1EventTrigger = btn1Events.GetEventTrigger();
        DOTSEvents_NextFrame<Btn2DownEvent>.EventTrigger btn2EventTrigger = btn2Events.GetEventTrigger();

        JobHandle jobHandle = Entities.ForEach((int entityInQueryIndex, ref UIControllerComponent controller) =>{
            if (btn1Down) {
                btn1EventTrigger.TriggerEvent(entityInQueryIndex);
            }
            else if(btn2Down){
                btn2EventTrigger.TriggerEvent(entityInQueryIndex);
            }
        }).Schedule(inputDeps);

        btn1Events.CaptureEvents(jobHandle, (Btn1DownEvent oneEvent) =>{
            On1Down?.Invoke(this, EventArgs.Empty);
        });
        btn2Events.CaptureEvents(jobHandle, (Btn2DownEvent twoEvent) =>{
            On2Down?.Invoke(this, EventArgs.Empty);
        });

        return jobHandle;
    }

    private void OneDownResponse(object sender, System.EventArgs e){
        Debug.Log("1 Pressed!");
    }

    private void TwoDownResponse(object sender, System.EventArgs e){
        Debug.Log("2 Pressed!");
    }

}
