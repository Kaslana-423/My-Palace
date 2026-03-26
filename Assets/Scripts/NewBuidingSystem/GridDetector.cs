using UnityEngine;
using UnityEngine.Tilemaps;

public class GridDetector : MonoBehaviour
{
    public Grid mapGrid;
    public Tilemap validAreaTilemap; // 在 Inspector 里把画了合法区域的 Tilemap 拖进来
    public GameObject previewPrefab;

    // OI 经典操作：加个 500 的偏移量，把 [-500, 500] 的世界网格映射到 [0, 1000] 的数组下标
    private const int OFFSET = 500;
    private const int MAX_SIZE = 1000;

    //[cite_start]// 0: 不可造 (含非凸多边形缺角处 [cite: 10])
    // 1: 空闲且可造
    // 2: 已有建筑占据
    public int[,] gridState = new int[MAX_SIZE, MAX_SIZE];

    private SpriteRenderer previewRenderer;
    private Vector3Int lastCellPos = new Vector3Int(-9999, -9999, 0);

    [Header("模式控制")]
    public bool isBuildMode = false;
    public GameObject buildPanelUI; // 临时拖一个 UI 面板进来测试
    [Header("渲染控制")]
    // 直接拿 Renderer 组件，不要拿 Tilemap，方便 O(1) 控制开关
    public TilemapRenderer validAreaRenderer;
    private void Start()
    {
        if (previewPrefab != null)
        {
            previewRenderer = previewPrefab.GetComponent<SpriteRenderer>();
        }

        InitClustersFromTilemap();
    }

    private void InitClustersFromTilemap()
    {
        // 直接获取 Tilemap 上所有画过砖块的包围盒
        BoundsInt bounds = validAreaTilemap.cellBounds;

        // 遍历包围盒，时间复杂度 O(W * H)，只在 Start 跑一次，毫无压力
        foreach (var pos in bounds.allPositionsWithin)
        {
            //[cite_start]// 如果这个格子里有 Tile，说明属于那 6 个簇的可用区块 [cite: 9]
            if (validAreaTilemap.HasTile(pos))
            {
                int arrayX = pos.x + OFFSET;
                int arrayY = pos.y + OFFSET;

                // 防越界保护
                if (arrayX >= 0 && arrayX < MAX_SIZE && arrayY >= 0 && arrayY < MAX_SIZE)
                {
                    gridState[arrayX, arrayY] = 1;
                }
            }
        }

        // 核心：把表现层关掉！
        // 玩家不需要看到你用来做逻辑判定的丑陋色块
        if (validAreaTilemap.GetComponent<TilemapRenderer>() != null)
        {
            validAreaTilemap.GetComponent<TilemapRenderer>().enabled = false;
        }

        Debug.Log("底层网格数据初始化完毕，空间映射已建立。");
    }

    private void Update()
    {
        // 监听 Tab 键切换建造模式 (后续写 UI 按钮也可以直接调 ToggleBuildMode)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleBuildMode();
        }

        // --- 主程专用硬编码测试热键 ---
        if (isBuildMode)
        {
            bool needRefresh = false;

            // 按数字键 1，强制切到 ID 为 1 的建筑 (民居)
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                BuildManager.Instance.SetCurrentBuilding(5);
                needRefresh = true;
            }
            // 按数字键 2，强制切到 ID 为 2 的建筑 (官府)
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                BuildManager.Instance.SetCurrentBuilding(1);
                needRefresh = true;
            }

            // 如果切了笔刷，哪怕鼠标没动，也要立刻拿着当前的坐标强行刷新一次视觉表现！
            if (needRefresh)
            {
                // 用我们之前缓存的 lastCellPos 直接重绘
                UpdateGridHighlight(lastCellPos);
            }
        }
        // -----------------------------------

        // 状态锁：如果不在建造模式，直接掐断所有网格检测和输入响应
        if (!isBuildMode) return;
        // --- 下面全是你原有的网格映射和查表逻辑 ---
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3Int cellPos = mapGrid.WorldToCell(mouseWorldPos);

        if (cellPos != lastCellPos)
        {
            lastCellPos = cellPos;
            UpdateGridHighlight(cellPos);
        }

        // 只有在建造模式下，左键才算作“建造”
        // ... 前面的输入拦截和网格换算保持不变 ...

        if (Input.GetMouseButtonDown(0))
        {
            // 每次点击时，向 BuildManager 询问现在手里拿的是什么笔刷
            int currentId = BuildManager.Instance.currentBuildingId;

            // 把 currentId 传进去检测能不能造
            int state = CheckGridState(cellPos.x, cellPos.y, currentId);

            if (state == 3) // 3 表示空闲、在可用簇内，且钱/人/建材全够
            {
                Vector3 spawnPos = mapGrid.CellToWorld(cellPos);

                // 把 currentId 传进去执行真建造
                bool buildSuccess = BuildManager.Instance.ExecuteBuild(currentId, spawnPos, cellPos);

                if (buildSuccess)
                {
                    gridState[cellPos.x + OFFSET, cellPos.y + OFFSET] = 2; // 标记占位
                    UpdateGridHighlight(cellPos); // 刷新高亮框状态
                }
            }
        }
    }

    public void ToggleBuildMode()
    {
        isBuildMode = !isBuildMode;
        if (buildPanelUI != null)
        {
            buildPanelUI.SetActive(isBuildMode); // 同步显示/隐藏张皓宇的面板
        }
        // --- 核心控制：直接开关渲染器 ---
        if (validAreaRenderer != null)
        {
            validAreaRenderer.enabled = isBuildMode;
        }
        if (!isBuildMode)
        {
            // 退出模式时，记得把高亮框藏起来，并重置缓存
            if (previewRenderer != null) previewRenderer.enabled = false;
            lastCellPos = new Vector3Int(-9999, -9999, 0);
        }
    }

    // 查表时记得带上 OFFSET
    private int CheckGridState(int x, int y, int bId)
    {
        int arrayX = x + OFFSET;
        int arrayY = y + OFFSET;

        if (arrayX < 0 || arrayX >= MAX_SIZE || arrayY < 0 || arrayY >= MAX_SIZE) return 0; // 越界

        // 如果底层网格不是 1 (空闲)，那就直接返回它的真实状态 (0 或 2)
        if (gridState[arrayX, arrayY] != 1) return gridState[arrayX, arrayY];

        // 走到这里说明格子是空的，向大管家查物价
        if (!BuildManager.Instance.HasEnoughResources(bId)) return 4; // 穷，买不起

        return 3; // 完美合法
    }

    private void UpdateGridHighlight(Vector3Int cellPos)
    {
        if (previewRenderer == null) return;

        // 1. 拿 ID 和 查状态
        int currentId = BuildManager.Instance.currentBuildingId;
        int state = CheckGridState(cellPos.x, cellPos.y, currentId);
        // --- 新增：动态切换建筑虚影的贴图 ---
        BuildingSO data = BuildManager.Instance.GetCurrentBuildingData();
        if (data != null && data.previewSprite != null)
        {
            // O(1) 指针替换，把当前笔刷的图塞给高亮框
            previewRenderer.sprite = data.previewSprite;
        }
        // ------------------------------------

        Vector3 worldPos = mapGrid.CellToWorld(cellPos);
        // 如果是 Isometric 地图，视情况开启下面这行锚点对齐
        // worldPos -= new Vector3(0, mapGrid.cellSize.y / 2f, 0);
        previewPrefab.transform.position = worldPos;

        // 3. 根据状态码驱动 UI 变色
        if (state == 3)
        {
            // 完美合法：标绿
            previewRenderer.enabled = true;
            previewRenderer.color = new Color(0f, 1f, 0f, 0.5f);
        }
        else if (state == 2 || state == 4)
        {
            // 占位(2) 或者 没钱(4)：标红
            previewRenderer.enabled = true;
            previewRenderer.color = new Color(1f, 0f, 0f, 0.5f);
        }
        else
        {
            // 越界或不可建区域(0)：直接隐藏
            previewRenderer.enabled = false;
        }
        // Debug.Log($"[变色测试] 当前鼠标坐标: {cellPos}, 查出的状态码: {state}, Sprite是否为空: {previewRenderer.sprite == null}");
    }
}