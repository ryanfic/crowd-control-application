using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Collections;
using Unity.Jobs;

//namespace ECSBoids{
/*public struct FlockBehaviour : IComponentData{
    public float AvoidanceRadius;
    public float AvoidanceWeight;
    public float CohesionRadius;
    public float CohesionWeight;
    //public float AlignmentRadius;
    //public float AlignmentWeight;
}*/
public class ECSBoidsHandler : MonoBehaviour
{
    [SerializeField] private Material entityMaterial;
    [SerializeField] private Mesh entityMesh;
    [SerializeField] private float cellWidth = 10f;
    [SerializeField] private int agentsPerCell = 50;
    [SerializeField] private int numAgentsToSpawn = 125;

    [SerializeField] private float crowdAvoidanceRadius = 5f;
    [SerializeField] private float crowdAvoidanceWeight = 1f;
    [SerializeField] private float crowdCohesionRadius = 10f;
    [SerializeField] private float crowdCohesionWeight = 1f;

    private static EntityManager eManager;

    

    private void Start() {
        eManager = World.Active.EntityManager;

        Vector3 gridCenter = new Vector3(0,0,0);
        int numCells = (int)Mathf.Ceil(((float)numAgentsToSpawn)/agentsPerCell); // Calculate number of cells needed from the number of agents to spawn and agents per cell
        int gridX = (int)Mathf.Ceil(Mathf.Sqrt(numCells)); // How many cells are in each row of grid
        createGrid(gridCenter, numCells, gridX);
    }
    private void createGrid(Vector3 gridCenter, int numCells, int gridX){
        int agentsLeftToSpawn = numAgentsToSpawn;
        bool isEven = (gridX%2)==0;
        int cellsToLeft = gridX/2;
        float topLeftX;
        if(isEven){
            topLeftX = cellWidth * cellsToLeft;
        }
        else{
            topLeftX = cellWidth * (cellsToLeft+0.5f);
        }
        
        Vector3 toTopLeft = new Vector3 (topLeftX,gridCenter.y,topLeftX);
        Vector3 topLeft = gridCenter - toTopLeft;
        Vector3 cellTopLeft = new Vector3();
        Vector3 offset = new Vector3();
        for(int i = 0; i < gridX; i++){
            for(int j = 0; j < gridX; j++){  
                offset.x = cellWidth * j;
                offset.z = cellWidth * i;
                // Calculate cell center
                cellTopLeft = topLeft + offset;
                if(agentsLeftToSpawn > 0){ // If more agents need to be spawned
                    if(agentsLeftToSpawn > agentsPerCell){ // If the cell can be filled with agents
                        spawnInCell(cellTopLeft,agentsPerCell); // Fill cell with agents
                        agentsLeftToSpawn -= agentsPerCell;
                    }
                    else{ // there the cell cannot be filled with agents
                        spawnInCell(cellTopLeft,agentsLeftToSpawn); // spawn the rest of the agents
                        agentsLeftToSpawn = 0; // no agents left to spawn
                    }
                }
            }
        }
    }
    private void spawnInCell(Vector3 cellTopLeft, int numAgSpawn){
        int agentsPerRow = (int)Mathf.Ceil(Mathf.Sqrt(agentsPerCell));
        Vector3 posToSpawn = new Vector3();
        Vector3 offset = new Vector3();
        for(int i = 0; i < agentsPerRow; i++){
            for(int j = 0; j < agentsPerRow; j++){  
                offset.x = (cellWidth/agentsPerRow) * j;
                offset.z = (cellWidth/agentsPerRow) * i;
                // Calculate spawn location
                posToSpawn = cellTopLeft + offset;
                if(numAgSpawn > 0){ // If more agents need to be spawned
                    spawnCrowdEntity(posToSpawn); // Spawn the agent
                    numAgSpawn--;
                }
            }
        }
    }

    private void spawnCrowdEntity(Vector3 pos)
    {
        Entity en = eManager.CreateEntity( //create the entity
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(RenderMesh),
            //typeof(Scale),
            typeof(ReynoldsFlockBehaviour),
            typeof(ReynoldsFlockMovement),
            typeof(QuadrantEntity)
        );
        SetEntityComponentData(en, new float3(pos.x, pos.y, pos.z), entityMesh, entityMaterial); //set component data
        //eManager.SetComponentData(en, new Scale{ Value = 0.5f}); //set size of entity
        eManager.SetComponentData(en,new ReynoldsFlockBehaviour{AvoidanceRadius = crowdAvoidanceRadius,
                                                        AvoidanceWeight = crowdAvoidanceWeight,
                                                        CohesionRadius = crowdCohesionRadius,
                                                        CohesionWeight = crowdCohesionWeight});
        eManager.SetComponentData(en, new ReynoldsFlockMovement{movement = float3.zero});
        eManager.SetComponentData(en, new QuadrantEntity{ typeEnum = QuadrantEntity.TypeEnum.Crowd}); //set the type of entity
    }
    private void SetEntityComponentData(Entity entity, float3 spawnPosition, Mesh mesh, Material material)
    {
        eManager.SetSharedComponentData<RenderMesh>(entity, //set up the Render mesh of the entity
            new RenderMesh{
                material = material,
                mesh = mesh
            }
        );

        eManager.SetComponentData<Translation>(entity, //set up the position of the entity
            new Translation{
                Value = spawnPosition
            }
        );
    }
}
//}