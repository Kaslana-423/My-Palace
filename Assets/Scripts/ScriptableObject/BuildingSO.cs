using UnityEngine;

[CreateAssetMenu(fileName = "NewBuildingData", menuName = "GameData/BuildingData")]
public class BuildingSO : ScriptableObject
{
    public int typeId;
    public string buildingName;
    public int level;

    public int costCoins;
    public int costPopulation;
    public int costMaterial;

    public int spendPopulation;
    public int outputPopulation;
    public int harvestMultiplier;
    public int outputPersonAnger;

    public GameObject prefab;
    public BuildingSO nextLevelSO;
    public int refundCoins;
    public int refundPopulation;
    public int refundMaterial;
    [Header("表现层")]
    public Sprite previewSprite; // 专门用来做建造预览虚影的贴图
}