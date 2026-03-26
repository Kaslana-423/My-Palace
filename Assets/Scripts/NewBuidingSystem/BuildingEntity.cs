using UnityEngine;

public class BuildingEntity : MonoBehaviour
{
    // 在生成这个建筑时，把对应的 SO 塞给它
    public BuildingSO data;

    // 如果地图支持升级，记录当前坐标，方便查表
    public int gridX;
    public int gridY;
}