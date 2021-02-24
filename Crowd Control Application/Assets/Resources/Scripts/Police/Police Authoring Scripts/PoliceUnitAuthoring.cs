using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;


public class PoliceUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
{
    private enum PoliceLine{
        Front,
        Center,
        Rear
    }
    // With super funtime errors! TODO: FIX LATER!!
    //Errors created when police unit is deleted
    // Errors also because the blob asset store is not deleted (There is a memory leak), but if you delete the asset store then there is the aforementioned error...
    public GameObject PoliceOfficerGO;
    public GameObject PoliceLineGO;
    public GameObject LineHolder;
    
    public float LineSpacing;
    public float LineLength = 1.44f; // how long the police line is, is a function of how many officers there are in a line
    public float LineWidth = 0.24f; // how much space a single officer takes up
    public int OfficersPerLine = 6;
    public float MaxSpeed;
    public float RotationSpeed;
    public string UnitName;

    private BlobAssetStore blobAssetStore;

    /*public void OnDestroy(){
        blobAssetStore.Dispose();
    }*/

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){ 
        //using (BlobAssetStore blobAssetStore = new BlobAssetStore()){ // using causes a lot of errors when it deletes the blobassetstore
            blobAssetStore = new BlobAssetStore();
            dstManager.AddComponent<PoliceUnitComponent>(entity);
            dstManager.AddComponentData<PoliceUnitMaxSpeed>(entity, new PoliceUnitMaxSpeed{
                Value = MaxSpeed
            });
            dstManager.AddComponentData<PoliceUnitRotationSpeed>(entity, new PoliceUnitRotationSpeed{
                Value = RotationSpeed
            });
            dstManager.AddComponentData<PoliceUnitDimensions>(entity, new PoliceUnitDimensions{
                LineSpacing = LineSpacing,
                LineLength = LineLength,
                LineWidth = LineWidth
            });
            Entity policeOfficer = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceOfficerGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity policeLine = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceLineGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity holderEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(LineHolder,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));

            dstManager.SetName(entity, UnitName);
            dstManager.AddComponentData<PoliceUnitName>(entity, new PoliceUnitName{
                String = UnitName
            });
            dstManager.AddComponent<PoliceUnitJustCreated>(entity);

            dstManager.AddBuffer<LinkedEntityGroup>(entity);

            //Set up the front line
            Entity line1 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Front, 1);

            //Set up the center line
            Entity line2 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Center,2);

            //Set up the rear line
            Entity line3 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Rear,3);

            dstManager.DestroyEntity(policeOfficer);
            dstManager.DestroyEntity(policeLine);
            dstManager.DestroyEntity(holderEntity);
        //}
    }

    //Create a police line
    //Also creates police officers for the police line
    private Entity CreatePoliceLine(Entity linePrefab, Entity officerPrefab, Entity holderPrefab, Entity policeUnit, EntityManager dstManager, PoliceLine lineType,int lineNumber){
        Entity policeLine = dstManager.Instantiate(holderPrefab);
        dstManager.SetName(policeLine,"Police Line " + lineNumber);
        dstManager.AddComponentData<Parent>(policeLine, new Parent{Value = policeUnit});
        dstManager.AddComponentData<LocalToParent>(policeLine, new LocalToParent());
        
        switch (lineType){ // add translation & line label based on which line it is in the line (front, center, rear)
            case PoliceLine.Center:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,0f)});
                dstManager.AddComponent<CenterPoliceLineComponent>(policeLine);
                break;
            case PoliceLine.Front:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,LineSpacing)});
                dstManager.AddComponent<FrontPoliceLineComponent>(policeLine);
                break;
            case PoliceLine.Rear:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,-LineSpacing)});
                dstManager.AddComponent<RearPoliceLineComponent>(policeLine);
                break;
        }
        dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(policeLine); // add the police line to the list of entities linked to the police unit
        // Set up a blocker that acts as the physical body that the police line collides with other objects with
        Entity blocker = dstManager.Instantiate(linePrefab);
        dstManager.SetName(blocker,"Police Line " + lineNumber + " Blocker");
        dstManager.AddComponentData<Parent>(blocker, new Parent{Value = policeLine});
        dstManager.AddComponentData<LocalToParent>(blocker, new LocalToParent());
        dstManager.AddComponentData<Translation>(blocker, new Translation{Value = new float3(0f,0f,0f)});
        dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(blocker);
        CreatePoliceOfficers(officerPrefab, policeUnit, policeLine, dstManager);
        return policeLine;
    }

    // Creates a number of police officers
    // Creates as many officers as defined in OfficersPerLine
    private void CreatePoliceOfficers(Entity officerPrefab, Entity policeUnit, Entity policeLine, EntityManager dstManager){
        for(int i = 0; i < OfficersPerLine; i++){
            CreatePoliceOfficer(officerPrefab, policeLine, policeUnit, dstManager, i);
        }
    }

    // Creates a police officer
    // It is assumed that the width of a single police officer is the same as the length (or depth)
    private void CreatePoliceOfficer(Entity officerPrefab, Entity policeLine, Entity policeUnit, EntityManager dstManager, int officerNum){
        Entity policeOfficer = dstManager.Instantiate(officerPrefab);
        dstManager.SetName(policeOfficer,"Police Officer " + (officerNum+1));
        dstManager.AddComponentData<Parent>(policeOfficer, new Parent{Value = policeLine});
        dstManager.AddComponentData<LocalToParent>(policeOfficer, new LocalToParent());
        float offset;
        if(OfficersPerLine%2 == 1){ // if there are an odd number of police officers
            offset = -(OfficersPerLine/2)*LineWidth + officerNum*LineWidth;            
        }
        else{ // if there are an even number of police officers
            offset = (-(OfficersPerLine/2)+0.5f)*LineWidth + officerNum*LineWidth;      
        }
        //Debug.Log("Offset: " + offset);
        dstManager.SetComponentData<Translation>(policeOfficer, new Translation{Value = new float3(offset,0f,0f)});
        dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(policeOfficer);
    }   

}

