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
    private void Start()
    {
        BuildingEntity[] prePlacedBuildings = FindObjectsOfType<BuildingEntity>();
        GridDetector detector = FindObjectOfType<GridDetector>(); // 确保你能拿到网格组件

        foreach (var entity in prePlacedBuildings)
        {
            if (!activeBuildings.Contains(entity))
            {
                Debug.Log(entity.name);
                activeBuildings.Add(entity);

                // 绝对不要相信你在 Inspector 里手敲的数字！
                // 直接拿皇宫真实的 Transform 世界坐标，逆向算出它在网格里的精准 Cell 坐标
                Vector3Int exactPos = Vector3Int.zero;

                if (detector != null && detector.mapGrid != null)
                {
                    exactPos = detector.mapGrid.WorldToCell(entity.transform.position);
                    // 顺手把正确的坐标回写给实体，防止以后别的逻辑取用时读到脏数据
                    entity.gridX = exactPos.x;
                    entity.gridY = exactPos.y;
                }
                else
                {
                    Debug.LogError("找不到 GridDetector，皇宫坐标初始化失败！");
                }

                RegisterBuilding(exactPos, entity);
                Debug.Log($"<color=green>[初始化装载]</color> 皇宫世界坐标: {entity.transform.position} -> 被强行映射到了网格 Key: {exactPos}");
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

            if (EventManager.Instance != null)
            {
                EventManager.Instance.CheckActionTrigger();
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

    public void RegisterBuilding(Vector3Int startPos, BuildingEntity entity)
    {
        int width = entity.data != null ? entity.data.size.x : 1;
        int height = entity.data != null ? entity.data.size.y : 1;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3Int pos = new Vector3Int(startPos.x + i, startPos.y + j, 0);
                buildingGridMap[pos] = entity;
            }
        }
    }

    public void UnregisterBuilding(Vector3Int startPos, BuildingEntity entity)
    {
        int width = entity.data != null ? entity.data.size.x : 1;
        int height = entity.data != null ? entity.data.size.y : 1;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector3Int pos = new Vector3Int(startPos.x + i, startPos.y + j, 0);
                if (buildingGridMap.ContainsKey(pos))
                {
                    buildingGridMap.Remove(pos);
                }
            }
        }
    }
}