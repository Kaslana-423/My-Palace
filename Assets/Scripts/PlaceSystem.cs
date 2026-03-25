using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

public class PlaceSysManager : MonoBehaviour
{
    public Grid grid;               // 场景中的 Grid 组件
    public static PlaceSysManager Instance { get; private set; }
    public GameObject guanfu;
    public GameObject minju;
    public GameObject panel;
    public Tilemap buildingTilemap; // 专门用来放建筑的 Tilemap 层
    public TileBase minjuTile;    // 你在 Tile Palette 里创建好的“建筑” Tile
    public TileBase guanfuTile;   // 另一个建筑 Tile
    private TileBase currentTile;    // 当前选中的建筑 Tile
    private GameObject currentInstance;
    private string currentBuildingName; // 当前选中的建筑名称
    public List<TileBase> MbuildingTiles; // 不同等级的建筑对应不同的 Tile
    public List<TileBase> GbuildingTiles; // 不同等级的建筑对应不同的 Tile
    public event Action<Vector3Int> BuildingUpgraded;
    public event Action<Vector3Int> BuildingDemolished;
    public GameObject ConstructionPanel; // 建造面板
    private Building panelController;
    private bool isPlacing = false; // 是否正在放置建筑
    private Vector3Int selectedBuildingPosition; // 当前选中的建筑坐标
    private bool hasSelectedBuilding = false; // 是否已选中可操作的建筑

    private int frameCount = 0;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        if (panel != null)
        {
            panel.SetActive(false);
        }

        ResolvePanelController();
        if (panelController != null)
        {
            panelController.gameObject.SetActive(false);
        }

        if (minju != null)
        {
            currentInstance = Instantiate(minju);
            currentInstance.SetActive(false); // 初始时隐藏
        }
        else
        {
            Debug.LogWarning("minju 预制体未赋值，初始预览不会创建");
        }

        currentBuildingName = "民居";
        currentTile = minjuTile;
    }
    void Update()
    {
        if (buildingTilemap == null || Camera.main == null)
        {
            return;
        }

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        // 转换为格子坐标
        Vector3Int cellPos = buildingTilemap.WorldToCell(mouseWorldPos);

        // 【基础诊断：确保 Update 在执行】
        if (frameCount % 60 == 0) // 每60帧打一次，避免日志刷屏
        {
            Debug.Log($"Update 执行中... isPlacing={isPlacing}, cellPos={cellPos}");
        }
        frameCount++;

        // 【优先处理 Tab 键：管理建造模式的开关】
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log($"按下 Tab 键，当前 isPlacing={isPlacing}");
            GameObject panelRoot = GetPanelRoot();
            
            // 如果升级/拆除面板是打开的，关闭它
            if (panelRoot != null && panelRoot.activeSelf)
            {
                panelRoot.SetActive(false);
            }

            // 切换建造模式
            isPlacing = !isPlacing;
            Debug.Log($"建造模式已切换为：{isPlacing}");

            if (ConstructionPanel != null)
            {
                ConstructionPanel.SetActive(isPlacing);
            }
            else
            {
                Debug.LogError("ConstructionPanel 未在 Inspector 里赋值！");
            }

            if (currentInstance != null)
            {
                currentInstance.SetActive(isPlacing);
                Debug.Log($"预览物体可见性设置为：{isPlacing}");
            }
            else
            {
                Debug.LogError("currentInstance 为 null！");
            }
        }

        // 【处理 1/2 键切换建筑类型：在建造模式下执行】
        if (Input.GetKeyDown(KeyCode.Alpha1) && isPlacing)
        {
            Debug.Log("按下 1 键，切换到民居");
            SwitchPreview(minju, minjuTile, "民居", cellPos);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && isPlacing)
        {
            Debug.Log("按下 2 键，切换到官府");
            SwitchPreview(guanfu, guanfuTile, "官府", cellPos);
        }

        // 【临时：用 G 键替代 1 来测试】
        if (Input.GetKeyDown(KeyCode.G) && isPlacing)
        {
            Debug.Log("按下 G 键，发出建造逻辑执行信号");
            SwitchPreview(minju, minjuTile, "民居", cellPos);
        }

        // 【处理右键点击：打开升级/拆除面板】
        bool isPointerOverUI = IsPointerOverUI();
        selectedBuildingPosition = cellPos;
        ManageResources(cellPos);

        // 【建造模式：显示预览并允许放置】
        if (isPlacing && currentInstance != null)
        {
            // 更新预览位置
            currentInstance.transform.position = buildingTilemap.GetCellCenterWorld(cellPos);

            // 左键点击放置建筑
            if (!isPointerOverUI && Input.GetMouseButtonDown(0))
            {
                if (!buildingTilemap.HasTile(cellPos))
                {
                    PlaceBuilding(cellPos);
                }
                else
                {
                    Debug.Log("这里已经有东西啦！");
                }
            }
        }
    }

    private void ResolvePanelController()
    {
        if (panelController != null)
        {
            return;
        }

        if (panel == null)
        {
            return;
        }

        panelController = panel.GetComponent<Building>();
        if (panelController == null)
        {
            panelController = panel.GetComponentInParent<Building>();
        }
        if (panelController == null)
        {
            panelController = panel.GetComponentInChildren<Building>(true);
        }
    }

    private GameObject GetPanelRoot()
    {
        ResolvePanelController();
        if (panelController != null)
        {
            return panelController.gameObject;
        }

        return panel;
    }

    private void SwitchPreview(GameObject prefab, TileBase tile, string buildingName, Vector3Int cellPos)
    {
        if (prefab == null)
        {
            Debug.LogError($"{buildingName} 预制体未赋值");
            return;
        }

        if (currentInstance != null)
        {
            DestroyImmediate(currentInstance);
        }

        currentInstance = Instantiate(prefab, transform);
        if (currentInstance == null)
        {
            Debug.LogError($"Instantiate({buildingName}) 返回 null！");
            return;
        }

        currentInstance.SetActive(true);
        currentInstance.transform.position = buildingTilemap.GetCellCenterWorld(cellPos);
        currentTile = tile;
        currentBuildingName = buildingName;
        Debug.Log($"{buildingName}预览已显示");
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        return EventSystem.current.IsPointerOverGameObject();
    }

    void PlaceBuilding(Vector3Int position)
    {
        if (buildingTilemap == null || currentTile == null)
        {
            return;
        }

        if (GameManager.Instance == null || GameManager.Instance.buildingDataDict == null)
        {
            Debug.LogError("GameManager 或建筑数据字典未初始化，无法建造");
            return;
        }

        // 调用 API 在指定位置放置 Tile
        buildingTilemap.SetTile(position, currentTile);
        BuildingData data = new BuildingData(currentBuildingName);
        GameManager.Instance.buildingDataDict[position] = data; // 同步数据
        Debug.Log($"在 {position} 建造了 {currentTile.name}");
    }

    void ManageResources(Vector3Int position)
    {
        if (Input.GetMouseButtonDown(1)) // 右键查看建筑信息/升级/拆除
        {
            // 右键优先用于查看建筑：如果还在建造模式，先退出建造模式。
            if (isPlacing)
            {
                isPlacing = false;
                if (ConstructionPanel != null)
                {
                    ConstructionPanel.SetActive(false);
                }

                if (currentInstance != null)
                {
                    currentInstance.SetActive(false);
                }
            }

            if (panel == null || buildingTilemap == null)
            {
                return;
            }

            ResolvePanelController();
            GameObject panelRoot = GetPanelRoot();
            if (panelRoot == null)
            {
                return;
            }

            if (buildingTilemap.HasTile(position))
            {
                selectedBuildingPosition = position;
                hasSelectedBuilding = true;
                Debug.Log($"[PlaceSystem] 选中建筑坐标: {position}");
                TileBase clickedTile = buildingTilemap.GetTile(position);
                BuildingData displayData = CreateDisplayDataFromTile(clickedTile);
                if (displayData != null)
                {
                    Debug.Log($"[PlaceSystem] Tile推断数据 buildingName={displayData.buildingName}, level={displayData.level}");
                    if (GameManager.Instance != null && GameManager.Instance.buildingDataDict != null)
                    {
                        GameManager.Instance.buildingDataDict[position] = displayData;
                    }
                }
                else
                {
                    Debug.LogWarning($"[PlaceSystem] 无法从 Tile 推断坐标 {position} 的建筑类型，请检查 minjuTile/guanfuTile 以及等级Tile列表配置");
                }

                // 右键点到建筑时始终显示并刷新当前建筑，避免“只开关不刷新”导致看到旧数据。
                if (!panelRoot.activeSelf)
                {
                    panelRoot.SetActive(true);
                }

                if (panelController != null)
                {
                    panelController.OpenPanel(position);
                    if (displayData != null)
                    {
                        panelController.RefreshUI(displayData);
                    }
                }
                else
                {
                    Debug.LogError("未找到 Building 组件，请检查是否挂在 Panel 或其子节点上");
                }
            }
            else
            {
                panelRoot.SetActive(false);
                hasSelectedBuilding = false;
                Debug.Log("该位置没有建筑，无法打开升级面板");
            }
        }
    }

    public void UpgradeSelectedBuilding()
    {
        if (!hasSelectedBuilding)
        {
            Debug.Log("请先右键选中一个建筑，再点击升级按钮");
            return;
        }
        UpgradeBuilding(selectedBuildingPosition);
    }

    public void DestroySelectedBuilding()
    {
        if (!hasSelectedBuilding)
        {
            Debug.Log("请先右键选中一个建筑，再点击拆除按钮");
            return;
        }
        DestroyBuilding(selectedBuildingPosition);
    }

    void DestroyBuilding(Vector3Int position)
    {
        if (buildingTilemap == null)
        {
            return;
        }

        if (buildingTilemap.HasTile(position))
        {
            if (GameManager.Instance != null && GameManager.Instance.buildingDataDict.TryGetValue(position, out BuildingData data))
            {
                int refundLevel = Math.Max(0, data.level);
                ResourceAmount refund = GetDemolishRefund(data, refundLevel);
                if (!refund.IsZero)
                {
                    GameManager.Instance.Add(refund);
                }
            }

            buildingTilemap.SetTile(position, null); // 移除 Tile
            if (GameManager.Instance != null && GameManager.Instance.buildingDataDict != null)
            {
                GameManager.Instance.buildingDataDict.Remove(position); // 同步移除数据
            }

            if (hasSelectedBuilding && selectedBuildingPosition == position)
            {
                hasSelectedBuilding = false;
                GameObject panelRoot = GetPanelRoot();
                if (panelRoot != null)
                {
                    panelRoot.SetActive(false);
                }
            }
            BuildingDemolished?.Invoke(position);
            Debug.Log($"拆除了 {position} 的建筑");
        }
        else
        {
            Debug.Log("当前选中位置没有建筑可拆除");
        }
    }

    public void UpgradeBuilding(Vector3Int position)
    {
        if (buildingTilemap.HasTile(position))
        {
            // 1. 防御性检查：GameManager 和字典是否正常？
            if (GameManager.Instance == null || GameManager.Instance.buildingDataDict == null)
            {
                Debug.LogError("GameManager 或字典未初始化！");
                return;
            }

            // 2. 安全读取字典：TryGetValue 避免字典里没数据时报错
            if (GameManager.Instance.buildingDataDict.TryGetValue(position, out BuildingData data))
            {
                // 3. 防御性检查：检查三种成本列表是否存在
                if (data.coinCost == null || data.manpowerCost == null || data.materialCost == null)
                {
                    Debug.LogError("BuildingData 的资源成本列表未初始化！请检查 coinCost/manpowerCost/materialCost");
                    return;
                }

                // --- 下面是你原本正常的逻辑 ---
                if (data.buildingName == "民居")
                {
                    if (data.level < data.coinCost.Count - 1)
                    {
                        int nextLevel = data.level + 1;
                        ResourceAmount upgradeCost = GetUpgradeCost(data, nextLevel);

                        if (!GameManager.Instance.TrySpend(upgradeCost))
                        {
                            Debug.Log($"资源不足，升级民居需要 铜钱:{upgradeCost.coin} 人力:{upgradeCost.manpower} 材料:{upgradeCost.material}，当前资源为 铜钱:{GameManager.Instance.Coins} 人力:{GameManager.Instance.Manpower} 材料:{GameManager.Instance.Materials}");
                            return;
                        }
                        data.level = nextLevel;
                        buildingTilemap.SetTile(position, MbuildingTiles[data.level]);
                        Debug.Log($"这是一个民居，等级 {data.level}，升级花费 铜钱:{upgradeCost.coin} 人力:{upgradeCost.manpower} 材料:{upgradeCost.material}");
                        BuildingUpgraded?.Invoke(position);
                    }
                    else
                    {
                        Debug.Log($"这是一个民居，等级 {data.level}，已经满级了！");
                    }
                }
                else if (data.buildingName == "官府")
                {
                    if (data.level < data.coinCost.Count - 1)
                    {
                        int nextLevel = data.level + 1;
                        ResourceAmount upgradeCost = GetUpgradeCost(data, nextLevel);
                        if (!GameManager.Instance.TrySpend(upgradeCost))
                        {
                            Debug.Log($"资源不足，升级官府需要 铜钱:{upgradeCost.coin} 人力:{upgradeCost.manpower} 材料:{upgradeCost.material}，当前资源为 铜钱:{GameManager.Instance.Coins} 人力:{GameManager.Instance.Manpower} 材料:{GameManager.Instance.Materials}");
                            return;
                        }
                        data.level = nextLevel;
                        buildingTilemap.SetTile(position, GbuildingTiles[data.level]);
                        Debug.Log($"这是一个官府，等级 {data.level}，升级花费 铜钱:{upgradeCost.coin} 人力:{upgradeCost.manpower} 材料:{upgradeCost.material}");
                        BuildingUpgraded?.Invoke(position);
                    }
                    else
                    {
                        Debug.Log($"这是一个官府，等级 {data.level}，已经满级了！");
                    }
                }
            }
            else
            {
                // 如果 Tilemap 上有图片，但字典里没数据，触发这里的提示
                Debug.LogWarning($"在 {position} 发现建筑贴图，但 GameManager 字典里没有它的数据！(是不是你在编辑器里手动刷的？)");
            }
        }
    }

    private ResourceAmount GetUpgradeCost(BuildingData data, int levelIndex)
    {
        return new ResourceAmount(
            GetCostByLevel(data.coinCost, levelIndex),
            GetCostByLevel(data.manpowerCost, levelIndex),
            GetCostByLevel(data.materialCost, levelIndex)
        );
    }

    private ResourceAmount GetDemolishRefund(BuildingData data, int levelIndex)
    {
        return new ResourceAmount(
            Mathf.RoundToInt(GetCostByLevel(data.coinCost, levelIndex) * 0.75f),
            Mathf.RoundToInt(GetCostByLevel(data.manpowerCost, levelIndex) * 0.75f),
            Mathf.RoundToInt(GetCostByLevel(data.materialCost, levelIndex) * 0.75f)
        );
    }

    private int GetCostByLevel(List<int> costList, int levelIndex)
    {
        if (costList == null || levelIndex < 0 || levelIndex >= costList.Count)
        {
            return 0;
        }

        return costList[levelIndex];
    }

    private BuildingData CreateDisplayDataFromTile(TileBase tile)
    {
        string inferredName = InferBuildingNameByTile(tile);
        if (string.IsNullOrEmpty(inferredName))
        {
            return null;
        }

        int inferredLevel = InferBuildingLevelByTile(tile, inferredName);
        return new BuildingData(inferredName, inferredLevel);
    }

    private string InferBuildingNameByTile(TileBase tile)
    {
        if (tile == null)
        {
            return string.Empty;
        }

        if (tile == minjuTile || (MbuildingTiles != null && MbuildingTiles.Contains(tile)))
        {
            return "民居";
        }

        if (tile == guanfuTile || (GbuildingTiles != null && GbuildingTiles.Contains(tile)))
        {
            return "官府";
        }

        return string.Empty;
    }

    private int InferBuildingLevelByTile(TileBase tile, string buildingName)
    {
        if (tile == null)
        {
            return 0;
        }

        if (buildingName == "民居")
        {
            if (MbuildingTiles != null)
            {
                int idx = MbuildingTiles.IndexOf(tile);
                if (idx >= 0)
                {
                    return idx;
                }
            }

            if (tile == minjuTile)
            {
                return 0;
            }
        }

        if (buildingName == "官府")
        {
            if (GbuildingTiles != null)
            {
                int idx = GbuildingTiles.IndexOf(tile);
                if (idx >= 0)
                {
                    return idx;
                }
            }

            if (tile == guanfuTile)
            {
                return 0;
            }
        }

        return 0;
    }
}