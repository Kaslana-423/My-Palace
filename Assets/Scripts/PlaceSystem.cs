using UnityEngine;
using UnityEngine.Tilemaps;

public class PlaceSystem : MonoBehaviour
{
    public Grid grid;               // 场景中的 Grid 组件
    public GameObject guanfu;
    public GameObject minju;
    public Tilemap buildingTilemap; // 专门用来放建筑的 Tilemap 层
    public TileBase minjuTile;    // 你在 Tile Palette 里创建好的“建筑” Tile
    public TileBase guanfuTile;   // 另一个建筑 Tile
    private TileBase currentTile;    // 当前选中的建筑 Tile
    private GameObject currentInstance;

    public GameObject ConstructionPanel; // 建造面板
    private bool isPlacing = false; // 是否正在放置建筑

    void Awake()
    {
        currentInstance = Instantiate(minju); // 默认放置民居
        Instantiate(guanfu);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) // 按 B 键打开建造面板
        {
            isPlacing = !isPlacing;
            ConstructionPanel.SetActive(isPlacing);
            currentInstance.SetActive(isPlacing); // 只有在放置模式下才显示预览建筑
            currentTile = minjuTile; // 默认选中民居
        }
        if (currentInstance == null || !isPlacing) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) // 按 1 键切换到民居
        {
            Destroy(currentInstance);
            currentInstance = Instantiate(minju);
            currentTile = minjuTile;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) // 按 2 键切换到官府
        {
            Destroy(currentInstance);
            currentInstance = Instantiate(guanfu);
            currentTile = guanfuTile;
        }
        // 1. 获取鼠标世界坐标
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        // 2. 转换为格子坐标
        Vector3Int cellPos = buildingTilemap.WorldToCell(mouseWorldPos);
        // 3. 应用位置
        currentInstance.transform.position = buildingTilemap.GetCellCenterWorld(cellPos);

        if (Input.GetMouseButtonDown(0)) // 点击左键建造
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

    void PlaceBuilding(Vector3Int position)
    {
        // 调用 API 在指定位置放置 Tile
        buildingTilemap.SetTile(position, currentTile);
        Debug.Log($"在 {position} 建造了 {currentTile.name}！");
    }
}