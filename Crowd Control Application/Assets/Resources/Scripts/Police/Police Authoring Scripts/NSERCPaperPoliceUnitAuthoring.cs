using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;




// An authoring script to create a police unit, used in the NSERC paper simulations



public class NSERCPaperPoliceUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
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
    public float OfficerLength = 0.24f; // how long the police officer is front to back
    public float OfficerWidth = 0.24f; // how much space a single officer takes up (shoulder width)
    public float OfficerSpacing;
    public int OfficersPerLine = 6;
    public float UnitMaxSpeed = 2;
    public float UnitRotationSpeed = 2;
    public string UnitName;
    public float OfficerMaxSpeed = 2;
    public float OfficerRotationSpeed = 2;
    public bool Is3SidedBox = false;
    public bool IsWedge = false;
    public float WedgeAngle = 60f;

    private BlobAssetStore blobAssetStore;

    /*public void OnDestroy(){
        blobAssetStore.Dispose();
    }*/

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){ 
        //using (BlobAssetStore blobAssetStore = new BlobAssetStore()){ // using causes a lot of errors when it deletes the blobassetstore
            blobAssetStore = new BlobAssetStore();
            dstManager.AddComponent<PoliceUnitComponent>(entity);
            dstManager.AddComponentData<PoliceUnitMaxSpeed>(entity, new PoliceUnitMaxSpeed{
                Value = UnitMaxSpeed
            });
            dstManager.AddComponentData<PoliceUnitRotationSpeed>(entity, new PoliceUnitRotationSpeed{
                Value = UnitRotationSpeed
            });
            dstManager.AddComponentData<PoliceUnitDimensions>(entity, new PoliceUnitDimensions{
                LineSpacing = LineSpacing, // the amount of space between the centerpoints of each police line (when police lines are in a row)
                OfficerLength = OfficerLength, // How long an officer is from front to back
                OfficerWidth = OfficerWidth, // how wide a police officer is, similar to the shoulder width of the police officer
                NumOfficersInLine1 = OfficersPerLine, // the number of officers in the first line
                NumOfficersInLine2 = OfficersPerLine, // the number of officers in the second line
                NumOfficersInLine3 = OfficersPerLine, // the number of officers in the third line
            });

            if(Is3SidedBox){ // if the unit starts off as a 3 sided box
                dstManager.AddComponentData<PoliceUnitMovementDestination>(entity, new PoliceUnitMovementDestination{
                    Value = new float3(0f,0f,0f)
                });
            }
            else if(IsWedge){
                dstManager.AddComponentData<PoliceUnitMovementDestination>(entity, new PoliceUnitMovementDestination{
                    Value = new float3(0f,0f,0f)
                });
            }
            else{ // if the unit starts off as a loose cordon
                dstManager.AddComponentData<PoliceUnitMovementDestination>(entity, new PoliceUnitMovementDestination{
                    Value = new float3(0f,0f,0f/*((LineLength+LineWidth)/2-LineSpacing)*/)
                });
            }
            //dstManager.AddComponentData<PoliceUnitGettingIntoFormation>(entity, new PoliceUnitGettingIntoFormation{});


            Entity policeOfficer = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceOfficerGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity policeLine = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceLineGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity holderEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(LineHolder,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));

            //dstManager.SetName(entity, UnitName);
            dstManager.AddComponentData<PoliceUnitName>(entity, new PoliceUnitName{
                String = UnitName
            });
            dstManager.AddComponent<PoliceUnitJustCreated>(entity);

            dstManager.AddBuffer<LinkedEntityGroup>(entity);

            dstManager.AddBuffer<OfficerInPoliceUnit>(entity);

            DynamicBuffer<OfficerInFormation> inForm = dstManager.AddBuffer<OfficerInFormation>(entity);
            //inForm.Clear();

            dstManager.AddComponent<CopyTransformToGameObject>(entity);

            //Set up the front line
            Entity line1 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Front, 0);

            //Set up the center line
            Entity line2 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Center,1);

            //Set up the rear line
            Entity line3 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Rear,2);

            dstManager.DestroyEntity(policeOfficer);
            dstManager.DestroyEntity(policeLine);
            dstManager.DestroyEntity(holderEntity);
        //}
    }

    //Create a police line
    //Also creates police officers for the police line
    private Entity CreatePoliceLine(Entity linePrefab, Entity officerPrefab, Entity holderPrefab, Entity policeUnit, EntityManager dstManager, PoliceLine lineType,int lineNumber){
        Entity policeLine = dstManager.Instantiate(holderPrefab);
        //dstManager.SetName(policeLine,"Police Line " + (lineNumber+1));
        dstManager.AddComponentData<Parent>(policeLine, new Parent{Value = policeUnit});
        dstManager.AddComponentData<LocalToParent>(policeLine, new LocalToParent());
        
        switch (lineType){ // add translation & line label based on which line it is in the line (front, center, rear)
            case PoliceLine.Center:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,0f)});
                dstManager.AddComponent<CenterPoliceLineComponent>(policeLine);
                break;
            case PoliceLine.Front:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,0f)});
                dstManager.AddComponent<FrontPoliceLineComponent>(policeLine);
                break;
            case PoliceLine.Rear:
                dstManager.AddComponentData<Translation>(policeLine, new Translation{Value = new float3(0f,0f,0f)});
                dstManager.AddComponent<RearPoliceLineComponent>(policeLine);
                break;
        }
        dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(policeLine); // add the police line to the list of entities linked to the police unit
        // Set up a blocker that acts as the physical body that the police line collides with other objects with
        //Entity blocker = dstManager.Instantiate(linePrefab);
        //dstManager.SetName(blocker,"Police Line " + lineNumber + " Blocker");
        //dstManager.AddComponentData<Parent>(blocker, new Parent{Value = policeLine});
        //dstManager.AddComponentData<LocalToParent>(blocker, new LocalToParent());
        //dstManager.AddComponentData<Translation>(blocker, new Translation{Value = new float3(0f,0f,0f)});
        //dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(blocker);
        CreatePoliceOfficers(officerPrefab, policeUnit, policeLine, dstManager, lineNumber);
        return policeLine;
    }

    // Creates a number of police officers
    // Creates as many officers as defined in OfficersPerLine
    private void CreatePoliceOfficers(Entity officerPrefab, Entity policeUnit, Entity policeLine, EntityManager dstManager, int lineNumber){
        for(int i = 0; i < OfficersPerLine; i++){
            CreatePoliceOfficer(officerPrefab, policeLine, policeUnit, dstManager, i, lineNumber);
        }
    }

    // Creates a police officer
    // It is assumed that the width of a single police officer is the same as the length (or depth)
    private void CreatePoliceOfficer(Entity officerPrefab, Entity policeLine, Entity policeUnit, EntityManager dstManager, int officerNum, int lineNumber){
        Entity policeOfficer = dstManager.Instantiate(officerPrefab);
        //dstManager.SetName(policeOfficer,"Police Officer " + (officerNum+1));
        dstManager.AddComponentData<Parent>(policeOfficer, new Parent{Value = policeLine});
        dstManager.AddComponentData<LocalToParent>(policeOfficer, new LocalToParent());
        float xLocation;
        float leftside;
        float xOffset;
        
        if(OfficersPerLine%2 == 1){ // if there are an odd number of police officers
            leftside = -((OfficersPerLine/2) * OfficerSpacing + (OfficersPerLine/2) * OfficerWidth);        
        }
        else{ // if there are an even number of police officers
            leftside = -(((OfficersPerLine/2)-0.5f) * OfficerSpacing + (OfficersPerLine/2) * OfficerWidth);      
        }
        xOffset = (officerNum + 0.5f) * OfficerWidth + officerNum * OfficerSpacing;
        xLocation = leftside + xOffset;

        float topPosition = 1.5f * OfficerLength + LineSpacing;
        float zOffset = -(lineNumber * LineSpacing + (0.5f + lineNumber) * OfficerLength);
        float zLocation = topPosition + zOffset;
        //Debug.Log("Offset: " + offset);
        dstManager.SetComponentData<Translation>(policeOfficer, new Translation{Value = new float3(xLocation,0f,zLocation)});
        dstManager.AddComponentData<PoliceOfficerNumber>(policeOfficer, new PoliceOfficerNumber{
            Value = officerNum
        });
        dstManager.AddComponentData<PoliceOfficerPoliceLineNumber>(policeOfficer, new PoliceOfficerPoliceLineNumber{
            Value = lineNumber
        });
        dstManager.AddComponentData<PoliceUnitOfPoliceOfficer>(policeOfficer, new PoliceUnitOfPoliceOfficer{
            Value = policeUnit
        });
        dstManager.AddComponentData<PoliceOfficer>(policeOfficer, new PoliceOfficer{});
        dstManager.AddComponentData<PoliceOfficerMaxSpeed>(policeOfficer, new PoliceOfficerMaxSpeed{
                Value = OfficerMaxSpeed
            });
            dstManager.AddComponentData<PoliceOfficerRotationSpeed>(policeOfficer, new PoliceOfficerRotationSpeed{
                Value = OfficerRotationSpeed
            });


        /*if(Is3SidedBox){ // if the unit starts off as a 3 sided box
            dstManager.AddComponentData<To3SidedBoxFormComponent>(policeOfficer, new To3SidedBoxFormComponent {
                                LineSpacing = LineSpacing,
                                OfficerLength = OfficerLength,
                                OfficerWidth = OfficerWidth, 
                                OfficerSpacing = OfficerSpacing,
                                NumOfficersInLine1 = OfficersPerLine,
                                NumOfficersInLine2 = OfficersPerLine,
                                NumOfficersInLine3 = OfficersPerLine
                            }); // Add component to change to 3 sided box
        }
        else if(IsWedge){
            dstManager.AddComponentData<ToWedgeFormComponent>(policeOfficer, new ToWedgeFormComponent {
                                Angle = WedgeAngle,
                                OfficerLength = OfficerLength,
                                OfficerWidth = OfficerWidth, 
                                NumOfficersInLine1 = OfficersPerLine,
                                NumOfficersInLine2 = OfficersPerLine,
                                NumOfficersInLine3 = OfficersPerLine,
                                MiddleOfficerNum = (OfficersPerLine * 3 / 2),
                                TotalOfficers = OfficersPerLine * 3
                            }); // Add component to change to wedge
        }
        else{ // if the unit starts off as a loose cordon
            dstManager.AddComponentData<ToParallelCordonFormComponent>(policeOfficer, new ToParallelCordonFormComponent{ 
                                LineSpacing = LineSpacing,
                                OfficerLength = OfficerLength,
                                OfficerWidth = OfficerWidth, 
                                OfficerSpacing = 0f,
                                NumOfficersInLine1 = OfficersPerLine,
                                NumOfficersInLine2 = OfficersPerLine,
                                NumOfficersInLine3 = OfficersPerLine
                            }); // Add component to change to cordon
        }*/

        dstManager.GetBuffer<LinkedEntityGroup>(policeUnit).Add(policeOfficer);
        dstManager.GetBuffer<OfficerInPoliceUnit>(policeUnit).Add(new OfficerInPoliceUnit{ // add the officer to the Police Unit's list of Officers
            officer = policeOfficer
        }); 
        dstManager.GetBuffer<OfficerInFormation>(policeUnit).Add(new OfficerInFormation{}); //Assume that the police officer begins in the proper location for the formation the police unit is in
                                                                                            // so add one officer to the Unit's Officer In Formation buffer
    }   

}

/*
public class NSERCPaperPoliceUnitAuthoring : MonoBehaviour, IConvertGameObjectToEntity 
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
    public float OfficerLength = 0.24f; // how long the police officer is front to back
    public float OfficerWidth = 0.24f; // how much space a single officer takes up (shoulder width)
    public int OfficersPerLine = 6;
    public float MaxSpeed;
    public float RotationSpeed;
    public string UnitName;
    public bool Is3SidedBox = false;

    private BlobAssetStore blobAssetStore;



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
                LineSpacing = LineSpacing, // the amount of space between the centerpoints of each police line (when police lines are in a row)
                OfficerLength = OfficerLength, // How long an officer is from front to back
                OfficerWidth = OfficerWidth, // how wide a police officer is, similar to the shoulder width of the police officer
                NumOfficersInLine1 = OfficersPerLine, // the number of officers in the first line
                NumOfficersInLine2 = OfficersPerLine, // the number of officers in the second line
                NumOfficersInLine3 = OfficersPerLine, // the number of officers in the third line
            });
            if(Is3SidedBox){ // if the unit starts off as a 3 sided box
                dstManager.AddComponentData<PoliceUnitMovementDestination>(entity, new PoliceUnitMovementDestination{
                    Value = new float3(0f,0f,0f)
                });
            }
            else{ // if the unit starts off as a loose cordon
                dstManager.AddComponentData<PoliceUnitMovementDestination>(entity, new PoliceUnitMovementDestination{
                    Value = new float3(0f,0f,((LineLength+LineWidth)/2-LineSpacing))
                });
            }
            Entity policeOfficer = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceOfficerGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity policeLine = GameObjectConversionUtility.ConvertGameObjectHierarchy(PoliceLineGO,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            Entity holderEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(LineHolder,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));

            //dstManager.SetName(entity,"Police Unit " + UnitName);

            //Set up the front line
            Entity line1 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Front, 1);

            //Set up the center line
            Entity line2 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Center,2);

            //Set up the rear line
            Entity line3 = CreatePoliceLine(policeLine, policeOfficer, holderEntity, entity, dstManager, PoliceLine.Rear,3);

            /*if(Is3SidedBox){
                dstManager.AddComponentData<To3SidedBoxFormComponent>(line1, new To3SidedBoxFormComponent{ // add a component to make the unit change to a 3 sided box
                    LineSpacing = LineSpacing,
                    LineLength = LineLength,
                    LineWidth = LineWidth
                });
                dstManager.AddComponentData<To3SidedBoxFormComponent>(line2, new To3SidedBoxFormComponent{ // add a component to make the unit change to a 3 sided box
                    LineSpacing = LineSpacing,
                    LineLength = LineLength,
                    LineWidth = LineWidth
                });
                dstManager.AddComponentData<To3SidedBoxFormComponent>(line3, new To3SidedBoxFormComponent{ // add a component to make the unit change to a 3 sided box
                    LineSpacing = LineSpacing,
                    LineLength = LineLength,
                    LineWidth = LineWidth
                });
            }*/
/*
            dstManager.DestroyEntity(policeOfficer);
            dstManager.DestroyEntity(policeLine);
            dstManager.DestroyEntity(holderEntity);
        //}
    }

    //Create a police line
    //Also creates police officers for the police line
    private Entity CreatePoliceLine(Entity linePrefab, Entity officerPrefab, Entity holderPrefab, Entity policeUnit, EntityManager dstManager, PoliceLine lineType,int lineNumber){
        Entity policeLine = dstManager.Instantiate(holderPrefab);
        //dstManager.SetName(policeLine,"Police Line " + lineNumber);
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
        // Set up a blocker that acts as the physical body that the police line collides with other objects with
        Entity blocker = dstManager.Instantiate(linePrefab);
        //dstManager.SetName(blocker,"Police Line " + lineNumber + " Blocker");
        dstManager.AddComponentData<Parent>(blocker, new Parent{Value = policeLine});
        dstManager.AddComponentData<LocalToParent>(blocker, new LocalToParent());
        dstManager.AddComponentData<Translation>(blocker, new Translation{Value = new float3(0f,0f,0f)});
        CreatePoliceOfficers(officerPrefab, policeLine, dstManager);
        return policeLine;
    }

    // Creates a number of police officers
    // Creates as many officers as defined in OfficersPerLine
    private void CreatePoliceOfficers(Entity officerPrefab, Entity policeLine, EntityManager dstManager){
        for(int i = 0; i < OfficersPerLine; i++){
            CreatePoliceOfficer(officerPrefab, policeLine, dstManager, i);
        }
    }

    // Creates a police officer
    // It is assumed that the width of a single police officer is the same as the length (or depth)
    private void CreatePoliceOfficer(Entity officerPrefab, Entity policeLine, EntityManager dstManager, int officerNum){
        Entity policeOfficer = dstManager.Instantiate(officerPrefab);
        //dstManager.SetName(policeOfficer,"Police Officer " + (officerNum+1));
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
    }   

}*/