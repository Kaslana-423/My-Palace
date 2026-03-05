using UnityEngine;

public class GridManager : MonoBehaviour
{
    public float tileWidth = 1.28f;  // 对应像素 128
    public float tileHeight = 0.64f; // 对应像素 64
    public int width = 36;
    public int height = 36;

    // 获取点击位置对应的网格坐标
    public Vector2Int WorldToIsoGrid(Vector3 worldPos)
    {
        // 这里的 worldPos 是鼠标点击的 Unity 世界坐标
        float x = worldPos.x / tileWidth;
        float y = worldPos.y / tileHeight;

        int gridX = Mathf.FloorToInt(x - y);
        int gridY = Mathf.FloorToInt(x + y);

        return new Vector2Int(gridX, gridY);
    }

    // 获取网格中心的世界坐标（用于放置建筑吸附）
    public Vector3 IsoGridToWorld(int x, int y)
    {
        float worldX = (x + y) * (tileWidth / 2f);
        float worldY = (y - x) * (tileHeight / 2f);
        return new Vector3(worldX, worldY, 0);
    }

    /// <summary>
    /// Update is called every frame, if the MonoBehaviour is enabled.
    /// </summary>
    private void Update()
    {

    }
}