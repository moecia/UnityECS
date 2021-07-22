using Unity.Entities;
using UnityEngine;

public class PrefabEntites : MonoBehaviour, IConvertGameObjectToEntity
{
    public static Entity PrefabEntity;
    public GameObject PrefabGameObject;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        using (var blobAssetStore = new BlobAssetStore())
        {
            var prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                PrefabGameObject,
                GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
            PrefabEntites.PrefabEntity = prefabEntity;
        }
    }
}
