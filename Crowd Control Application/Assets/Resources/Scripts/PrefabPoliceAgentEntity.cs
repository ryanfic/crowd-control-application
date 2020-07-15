using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PrefabPoliceAgentEntity : MonoBehaviour, IConvertGameObjectToEntity {
    public static Entity prefabEntity;
    public GameObject prefabGameObject;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem){
        using (BlobAssetStore blobAssetStore = new BlobAssetStore()){
            Entity prefabPoliceAgent = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefabGameObject,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            
            dstManager.SetName(prefabPoliceAgent,"Police Agent Prefab");
            
            PrefabPoliceAgentEntity.prefabEntity = prefabPoliceAgent;
        }
    }
}
