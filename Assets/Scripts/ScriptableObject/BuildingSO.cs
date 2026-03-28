using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "GameData/BuildingData")]
public class BuildingSO : ScriptableObject
{
    [Header("基本信息")]
    public int typeId;
    public string buildingName;
    public int level;
    public bool isCoreBuilding;
    public Vector2Int size = new Vector2Int(1, 1);
    [Header("建造消耗")]
    public int costCoins;
    public int costPopulation;
    public int costMaterial;
    [Header("回合信息")]
    public int spendPopulation;
    public int outputCoins;
    public int outputPopulation;
    public int outputMaterial;
    public int outputProsperity;
    public int outputPersonAnger;
    public int harvestMultiplier;
    public bool isHarvester;

    public GameObject prefab;
    public BuildingSO nextLevelSO;
    [Header("拆除返回")]
    public int refundCoins;
    public int refundPopulation;
    public int refundMaterial;
    [Header("表现层")]
    public Sprite previewSprite; // 专门用来做建造预览虚影的贴图
    public string buildingTitle;
    [TextArea(3, 5)]
    public string buildingIntro;
    public Sprite buildingIcon;
}