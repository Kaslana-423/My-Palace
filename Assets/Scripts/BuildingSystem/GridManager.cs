using UnityEngine;

public class GridManager : MonoBehaviour
{
    // 对应你的像素 128x64，保持 2:1 比例
    public float tileWidth = 2.12f;
    public float tileHeight = 1f;

    // 获取点击位置对应的网格坐标 (世界坐标 -> 整数格索引)
    public Vector2Int WorldToIsoGrid(Vector3 worldPos)
    {
        // 核心公式：等距旋转与缩放的逆运算
        // 将点击的坐标按比例还原到正方形网格空间
        float x = worldPos.x / (tileWidth / 2f);
        float y = worldPos.y / (tileHeight / 2f);

        // 旋转 45 度并取整
        int gridX = Mathf.FloorToInt((x - y) / 2f);
        int gridY = Mathf.FloorToInt((x + y) / 2f);

        return new Vector2Int(gridX, gridY);
    }

    // 获取网格中心的世界坐标（用于建筑吸附）
    public Vector3 IsoGridToWorld(int x, int y)
    {
        // 这里的 +0.5f 是为了吸附到格子的“中心”，而不是格子的“左顶点”
        float centerX = x;
        float centerY = y;

        float worldX = (centerX + centerY) * (tileWidth / 2f);
        float worldY = (centerY - centerX) * (tileHeight / 2f);

        return new Vector3(worldX, worldY, 0);
    }

    private void Update()
    {

    }
}