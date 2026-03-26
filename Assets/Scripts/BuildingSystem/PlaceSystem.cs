using System.Collections.Generic;
using System.Data.Common;
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
    private bool isPlacing = false; // 是否正在放置建筑
    private bool isChecking = false; // 是否正在检查升级
    private Vector3Int selectedBuildingPosition; // 当前选中的建筑坐标
    private bool hasSelectedBuilding = false; // 是否已选中可操作的建筑

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        panel.SetActive(false);
        currentInstance = Instantiate(minju);
        currentInstance.SetActive(false); // 初始时隐藏
        currentBuildingName = "民居";
        currentTile = minjuTile;
    }
    void Update()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 2. 转换为格子坐标
        Vector3Int cellPos = buildingTilemap.WorldToCell(mouseWorldPos);
        if (Input.GetKeyDown(KeyCode.Tab)) // 按 Tab 键打开建造面板
        {
            if (panel.activeSelf)
            {
                isPlacing = false;
                panel.SetActive(false);
            }
            isPlacing = !isPlacing;
            ConstructionPanel.SetActive(isPlacing);

            if (currentInstance != null)
                currentInstance.SetActive(isPlacing);
        }

        bool isPointerOverUI = IsPointerOverUI();
        if (!isPointerOverUI)
        {
            selectedBuildingPosition = cellPos; // 更新当前选中坐标
            ManageResources(cellPos);
        }

        if (currentInstance == null || !isPlacing) return;
        // 3. 应用位置
        currentInstance.transform.position = buildingTilemap.GetCellCenterWorld(cellPos);


        if (Input.GetKeyDown(KeyCode.Alpha1)) // 按 1 键切换到民居
        {
            Destroy(currentInstance); // 销毁当前的预览
            currentInstance = Instantiate(minju); // 重新生成民居副本
            currentTile = minjuTile;
            currentBuildingName = "民居";
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // 按 2 键切换到官府
        {
            Destroy(currentInstance); // 销毁当前的预览
            currentInstance = Instantiate(guanfu); // 重新生成官府副本
            currentTile = guanfuTile;
            currentBuildingName = "官府";
        }
        // 1. 获取鼠标世界坐标


        if (!isPointerOverUI && Input.GetMouseButtonDown(0)) // 点击左键建造
        {// 1. 获取鼠标屏幕坐标
            // 2. 检查逻辑：这里是否已经有建筑了？
            if (!buildingTilemap.HasTile(cellPos))
            {
                // 3. 执行建造
                PlaceBuilding(cellPos);
            }
            else
            {
                Debug.Log("这里已经有东西啦！");
            }
        }

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
        // 调用 API 在指定位置放置 Tile
        buildingTilemap.SetTile(position, currentTile);
        BuildingData data = new BuildingData(currentBuildingName);
        GameManager.Instance.buildingDataDict[position] = data; // 同步数据
        Debug.Log($"在 {position} 建造了 {currentTile.name}");
    }

    void ManageResources(Vector3Int position)
    {
        if (Input.GetMouseButtonDown(1) && !isPlacing) // 右键查看建筑信息/升级/拆除
        {

            panel.gameObject.transform.position = Input.mousePosition;
            if (buildingTilemap.HasTile(position))
            {
                selectedBuildingPosition = position;
                hasSelectedBuilding = true;
                panel.SetActive(!isChecking);
                isChecking = !isChecking;
                BuildingUpgraded?.Invoke(position);
            }
            else
            {
                panel.SetActive(false);
                isChecking = false;
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

        if (buildingTilemap.HasTile(position))
        {
            buildingTilemap.SetTile(position, null); // 移除 Tile
            GameManager.Instance.buildingDataDict.Remove(position); // 同步移除数据
            if (hasSelectedBuilding && selectedBuildingPosition == position)
            {
                hasSelectedBuilding = false;
                panel.SetActive(false);
                isChecking = false;
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
                // 3. 防御性检查：检查 coinCost 列表是否存在
                if (data.coinCost == null)
                {
                    Debug.LogError("你的 BuildingData 类里面的 coinCost 列表没有实例化！(忘记 new List<int>() 了)");
                    return;
                }

                // --- 下面是你原本正常的逻辑 ---
                if (data.buildingName == "民居")
                {
                    if (data.level < data.coinCost.Count - 1)
                    {
                        int nextLevel = data.level + 1;
                        int upgradeCost = data.coinCost[nextLevel];

                        if (!GameManager.Instance.TrySpendCoins(upgradeCost))
                        {
                            Debug.Log($"金币不足，升级民居需要 {upgradeCost} 金币，当前只有 {GameManager.Instance.Coins} 金币");
                            return;
                        }
                        data.level = nextLevel;
                        buildingTilemap.SetTile(position, MbuildingTiles[data.level]);
                        Debug.Log($"这是一个民居，等级 {data.level}，升级花费 {upgradeCost} 金币");
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
                        int upgradeCost = data.coinCost[nextLevel];
                        if (!GameManager.Instance.TrySpendCoins(upgradeCost))
                        {
                            Debug.Log($"金币不足，升级官府需要 {upgradeCost} 金币，当前只有 {GameManager.Instance.Coins} 金币");
                            return;
                        }
                        data.level = nextLevel;
                        buildingTilemap.SetTile(position, GbuildingTiles[data.level]);
                        Debug.Log($"这是一个官府，等级 {data.level}，升级花费 {upgradeCost} 金币");
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
}