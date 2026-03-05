using UnityEngine;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    public string buildingName;
    public Vector2Int size; // 比如皇宫 10x10，民居 2x3
    public GameObject prefab; // 建筑模型
}