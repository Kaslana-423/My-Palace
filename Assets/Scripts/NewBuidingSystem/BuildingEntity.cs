using UnityEngine;

public class BuildingEntity : MonoBehaviour
{
    // 在生成这个建筑时，把对应的 SO 塞给它
    public BuildingSO data;

    // 如果地图支持升级，记录当前坐标，方便查表
    public int gridX;
    public int gridY;
    // 纯纯的 Editor 渲染流，不会打包进最终游戏，不消耗运行时性能
    private void OnDrawGizmosSelected()
    {
        // 如果连数据图纸都没挂，直接踢掉防报错
        if (data == null) return;

        // 临时拿一下场景里的网格基准 (Gizmos 里 FindObject 不在乎那点常数开销)
        Grid mapGrid = FindObjectOfType<Grid>();
        if (mapGrid == null) return;

        // 拿到当前 Transform 算出来的绝对左下角锚点
        Vector3Int baseCellPos = mapGrid.WorldToCell(transform.position);
        baseCellPos.z = 0; // 锁死 Z 轴防污染

        int width = data.size.x;
        int height = data.size.y;

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // 亮橙色边框
        Color fillColor = new Color(1f, 0.5f, 0f, 0.3f); // 半透明填充

        // O(W * H) 遍历这栋建筑应该占据的所有格子
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // 算出每一个占位格的网格坐标
                Vector3Int currentCell = new Vector3Int(baseCellPos.x + i, baseCellPos.y + j, 0);

                // 将网格坐标转换回世界坐标的中心点，用于精准画框
                Vector3 cellCenterWorld = mapGrid.GetCellCenterWorld(currentCell);
                Vector3 cellSize = mapGrid.cellSize;

                // 画一个半透明实心方块
                Gizmos.color = fillColor;
                Gizmos.DrawCube(cellCenterWorld, cellSize * 0.95f); // 稍微缩水一点，留点缝隙好观察

                // 画一圈高亮线框
                Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
                Gizmos.DrawWireCube(cellCenterWorld, cellSize);
            }
        }
    }
}