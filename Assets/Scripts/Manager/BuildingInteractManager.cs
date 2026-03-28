using System;
using UnityEngine;

public class BuildingInteractManager : MonoBehaviour
{
    public static BuildingInteractManager Instance { get; private set; }

    public event Action<BuildingEntity> OnBuildingSelected;
    public event Action<BuildingEntity> OnCoreBuildingSelected;
    public event Action OnSelectionCleared;

    private BuildingEntity currentTarget;
    private GridDetector gridDetector;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        gridDetector = FindObjectOfType<GridDetector>();
    }

    private void Update()
    {
        // === 1. 原有的右键 O(1) 空间寻址逻辑 ===
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            if (gridDetector == null) return;

            Vector3Int cellPos = gridDetector.mapGrid.WorldToCell(mousePos);
            Debug.Log($"<color=orange>[右键寻址]</color> 鼠标世界坐标: {mousePos} -> 算出的查询 Key: {cellPos}");
            if (BuildManager.Instance.buildingGridMap.TryGetValue(cellPos, out BuildingEntity entity))
            {
                currentTarget = entity;
                if (entity.data != null && entity.data.isCoreBuilding)
                {
                    OnCoreBuildingSelected?.Invoke(currentTarget);
                    // Debug.Log("???");
                }
                else if (entity.data != null)
                {
                    OnBuildingSelected?.Invoke(currentTarget);
                }
            }
            else
            {
                ClearSelection();
                Debug.Log("[清除锁定] 点击了空地");
            }
        }

        // === 2. 新增：主程专属键盘盲测后门 ===
        // 只有在 currentTarget 不为空（也就是右键成功锁定了一个建筑）时，才响应按键
        if (currentTarget != null)
        {
            // 按 U 键触发升级
            if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log($"[盲测] 尝试升级当前建筑...");
                RequestUpgrade();
            }

            // 按 X 键或 Delete 键触发拆除 (加个 X 键照顾部分笔记本键盘)
            if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Delete))
            {
                Debug.Log($"[盲测] 尝试拆除当前建筑...");
                RequestDemolish();
            }
        }
    }

    public void ClearSelection()
    {
        currentTarget = null;
        OnSelectionCleared?.Invoke();
    }

    public void RequestUpgrade()
    {
        if (currentTarget == null) return;

        BuildingSO nextData = currentTarget.data.nextLevelSO;
        if (nextData == null) return;

        if (GameManager.Instance.TrySpendResources(nextData.costCoins, nextData.costPopulation, nextData.costMaterial))
        {
            Vector3 pos = currentTarget.transform.position;
            Vector3Int cellPos = new Vector3Int(currentTarget.gridX, currentTarget.gridY, 0);

            BuildManager.Instance.activeBuildings.Remove(currentTarget);
            BuildManager.Instance.UnregisterBuilding(cellPos, currentTarget);
            Destroy(currentTarget.gameObject);

            GameObject clone = Instantiate(nextData.prefab, pos, Quaternion.identity);
            BuildingEntity newEntity = clone.GetComponent<BuildingEntity>();
            newEntity.data = nextData;
            newEntity.gridX = cellPos.x;
            newEntity.gridY = cellPos.y;

            BuildManager.Instance.activeBuildings.Add(newEntity);
            BuildManager.Instance.RegisterBuilding(cellPos, newEntity);

            // ClearSelection();

            currentTarget = newEntity;
            OnBuildingSelected?.Invoke(currentTarget);
        }
    }

    public void RequestDemolish()
    {
        if (currentTarget == null) return;

        if (currentTarget.data != null && currentTarget.data.isCoreBuilding)
        {
            Debug.LogWarning("核心建筑，绝对禁止拆除");
            return;
        }

        GameManager.Instance.AddCoins(currentTarget.data.refundCoins);
        GameManager.Instance.AddPopulation(currentTarget.data.refundPopulation);
        GameManager.Instance.AddMaterials(currentTarget.data.refundMaterial);

        Vector3Int cellPos = new Vector3Int(currentTarget.gridX, currentTarget.gridY, 0);
        int w = currentTarget.data != null ? currentTarget.data.size.x : 1;
        int h = currentTarget.data != null ? currentTarget.data.size.y : 1;

        BuildManager.Instance.activeBuildings.Remove(currentTarget);
        BuildManager.Instance.UnregisterBuilding(cellPos, currentTarget);

        if (gridDetector != null)
        {
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    int arrayX = cellPos.x + i + 500;
                    int arrayY = cellPos.y + j + 500;
                    gridDetector.gridState[arrayX, arrayY] = 1;
                }
            }
        }

        Destroy(currentTarget.gameObject);
        ClearSelection();
    }
}