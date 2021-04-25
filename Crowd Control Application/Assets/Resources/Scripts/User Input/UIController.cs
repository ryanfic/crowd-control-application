using System;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

public class OnLeftClickEventArgs : EventArgs{
    public float3 FromPos;
    public float3 ToPos;
    public JobHandle JobHandle;
}
public class OnRightUpEventArgs : EventArgs{
    public float3 Pos;
}
    
public class UIController : SystemBase {

    public event EventHandler On1Down;
    public event EventHandler On2Down;
    public event EventHandler<OnLeftClickEventArgs> OnLeftMouseClick;
    public event EventHandler<OnRightUpEventArgs> OnRightMouseUp;

    private Camera mainCam;
    private bool leftMousePressed;
    private float3 leftMouseDownPos = float3.zero; // where the left mouse button was pressed down


    protected override void OnCreate(){
        leftMousePressed = false;
        
        //For Testing
        //On1Down += OneDownResponse;
        //On2Down += TwoDownResponse;
        //OnLeftMouseClick += LeftClickResponse;
        //OnRightMouseUp += RightUpResponse;
    }

    protected override void OnStartRunning(){
        mainCam = Camera.main;
    }

    protected override void OnUpdate(){
        bool btn1Down = Input.GetKeyDown(KeyCode.Alpha1);
        bool btn2Down = Input.GetKeyDown(KeyCode.Alpha2);
        bool leftMouseDown = Input.GetMouseButtonDown(0);
        bool leftMouseUp = Input.GetMouseButtonUp(0);
        bool leftMouseHeld = Input.GetMouseButton(0);
        bool rightMouseUp = Input.GetMouseButtonUp(1);

        float rayDistance = 100f;
        
        /*if(leftMousePressed){ // if the left mouse has been pressed, deal with that first
            if(leftMouseUp){ // if the left mouse has been released
                UnityEngine.Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                float3 leftMouseUpPos = float3.zero;
                if(Raycast(ray.origin, ray.direction * rayDistance, ref hit)){ //if something was hit
                    leftMouseUpPos = hit.Position;// use the hit location
                }
                else { // if something was not hit
                    leftMouseUpPos = ray.direction * rayDistance; // use the maximal distance location
                }
                OnLeftMouseClick?.Invoke(this, new OnLeftClickEventArgs{
                    FromPos = leftMouseDownPos,
                    ToPos = leftMouseUpPos,
                    //JobHandle = inputDeps
                });
                leftMousePressed = false;
            }
            else if(!leftMouseHeld){ // if the left mouse release is missed, it will no longer be held
                Debug.Log("ERROR: LEFT MOUSE UP NOT REGISTERED");
                leftMousePressed = false;
            }
        }  
        else { // if the left mouse has not been pressed before, check other buttons
            if(leftMouseDown){ // if the left mouse has been pressed
                UnityEngine.Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                if(Raycast(ray.origin, ray.direction * rayDistance, ref hit)){ // if something was hit
                    leftMouseDownPos = hit.Position; // use the hit location
                }
                else{ // if something was not hit
                    leftMouseDownPos = ray.direction * rayDistance; // use the maximal distance location
                }
                
                leftMousePressed = true;
            }
            else if(rightMouseUp){
                UnityEngine.Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
                Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
                if(Raycast(ray.origin, ray.direction * rayDistance, ref hit)){ // we do care if it hits something
                    OnRightMouseUp?.Invoke(this, new OnRightUpEventArgs{
                        Pos = hit.Position
                    });
                }
                
            }
            else if (btn1Down) {
                On1Down?.Invoke(this, EventArgs.Empty);
            }
            else if(btn2Down){
                On2Down?.Invoke(this, EventArgs.Empty);
            }
        }*/

    }

    private void OneDownResponse(object sender, System.EventArgs e){
        Debug.Log("1 Pressed!");
    }

    private void TwoDownResponse(object sender, System.EventArgs e){
        Debug.Log("2 Pressed!");
    }

    private void LeftClickResponse(object sender, OnLeftClickEventArgs e){
        Debug.Log("Left Click! From "+ e.FromPos + " To " + e.ToPos);
    }
    private void RightUpResponse(object sender, OnRightUpEventArgs e){
        Debug.Log("Right Click! At " + e.Pos);
    }


    //Raycast without caring about what entity you hit
    private bool Raycast(float3 fromPosition, float3 toPosition, ref Unity.Physics.RaycastHit hit){
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>(); //Get the build physics world
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld; //get the collision world
        
        RaycastInput raycastInput = new RaycastInput {
            Start = fromPosition,
            End = toPosition,
            Filter = new CollisionFilter {
                BelongsTo = ~0u, // belongs to all layers
                CollidesWith = ~0u, // collides with all layers
                GroupIndex = 0, // a new group
            }
        };

        if(collisionWorld.CastRay(raycastInput, out hit)){ // use the collision world to cast a ray
            // hit something
            return true;
        }
        else {
            return false;
        }
    }

    //raycast without caring about where collision happened
    private bool Raycast(float3 fromPosition, float3 toPosition, ref Entity e){
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>(); //Get the build physics world
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld; //get the collision world
        
        RaycastInput raycastInput = new RaycastInput {
            Start = fromPosition,
            End = toPosition,
            Filter = new CollisionFilter {
                BelongsTo = ~0u, // belongs to all layers
                CollidesWith = ~0u, // collides with all layers
                GroupIndex = 0, // a new group
            }
        };
        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();

        if(collisionWorld.CastRay(raycastInput, out hit)){ // use the collision world to cast a ray
            // hit something
            e = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            return true;
        }
        else {
            return false;
        }
    }

    // raycast caring about both where and what was hit
    private bool Raycast(float3 fromPosition, float3 toPosition, ref Unity.Physics.RaycastHit hit, ref Entity e){
        BuildPhysicsWorld buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>(); //Get the build physics world
        CollisionWorld collisionWorld = buildPhysicsWorld.PhysicsWorld.CollisionWorld; //get the collision world
        
        RaycastInput raycastInput = new RaycastInput {
            Start = fromPosition,
            End = toPosition,
            Filter = new CollisionFilter {
                BelongsTo = ~0u, // belongs to all layers
                CollidesWith = ~0u, // collides with all layers
                GroupIndex = 0, // a new group
            }
        };

        if(collisionWorld.CastRay(raycastInput, out hit)){ // use the collision world to cast a ray
            // hit something
            e = buildPhysicsWorld.PhysicsWorld.Bodies[hit.RigidBodyIndex].Entity;
            return true;
        }
        else {
            return false;
        }
    }

}

