using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance { get; private set; }

    public List<BuildingSO> buildingDataAssets;
    public Dictionary<Vector3Int, BuildingEntity> buildingGridMap = new Dictionary<Vector3Int, BuildingEntity>();

    private Dictionary<int, BuildingSO> bDict = new Dictionary<int, BuildingSO>();

    public List<BuildingEntity> activeBuildings = new List<BuildingEntity>();
    public int currentBuildingId = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        foreach (var data in buildingDataAssets)
        {
            if (!bDict.ContainsKey(data.typeId))
            {
                bDict.Add(data.typeId, data);
            }
        }
    }

    public bool HasEnoughResources(int typeId)
    {
        if (!bDict.ContainsKey(typeId)) return false;
        var req = bDict[typeId];

        return GameManager.Instance.Coins >= req.costCoins && GameManager.Instance.Population >= req.costPopulation && GameManager.Instance.Materials >= req.costMaterial;
    }

    public bool ExecuteBuild(int typeId, Vector3 worldPos, Vector3Int cellPos)
    {
        if (!bDict.ContainsKey(typeId)) return false;
        var req = bDict[typeId];

        if (GameManager.Instance.TrySpendResources(req.costCoins, req.costPopulation, req.costMaterial))
        {
            GameObject clone = Instantiate(req.prefab, worldPos, Quaternion.identity);

            BuildingEntity entity = clone.GetComponent<BuildingEntity>();
            if (entity != null)
            {
                entity.data = req;
                entity.gridX = cellPos.x;
                entity.gridY = cellPos.y;

                activeBuildings.Add(entity);
                RegisterBuilding(cellPos, entity);
            }

            return true;
        }

        return false;
    }

    public void SetCurrentBuilding(int typeId)
    {
        if (bDict.ContainsKey(typeId))
        {
            currentBuildingId = typeId;
        }
    }

    public BuildingSO GetCurrentBuildingData()
    {
        if (bDict.ContainsKey(currentBuildingId))
        {
            return bDict[currentBuildingId];
        }
        return null;
    }

    public void RegisterBuilding(Vector3Int pos, BuildingEntity entity)
    {
        buildingGridMap[pos] = entity;
    }

    public void UnregisterBuilding(Vector3Int pos)
    {
        if (buildingGridMap.ContainsKey(pos))
        {
            buildingGridMap.Remove(pos);
        }
    }
}