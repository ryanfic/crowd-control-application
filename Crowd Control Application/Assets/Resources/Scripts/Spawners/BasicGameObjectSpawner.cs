﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGameObjectSpawner : MonoBehaviour
{
    public GameObject GoTemplate;
    public float cellWidth = 10f;
    public int agentsPerCell = 50;
    public int numAgentsToSpawn = 125;
    // Start is called before the first frame update
    void Start()
    {
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
                    spawnGameObject(posToSpawn); // Spawn the agent
                    numAgSpawn--;
                }
            }
        }
    }
    
    private void spawnGameObject(Vector3 pos)
    {
        Quaternion spawnRotation = Quaternion.identity;
        GameObject gameObj = Instantiate(GoTemplate, pos, spawnRotation);
        
    }

}
