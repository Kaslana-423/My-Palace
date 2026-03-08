using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "NewBuilding", menuName = "Building/BuildingData")]
public class BuildingData : ScriptableObject
{
    public List<TileBase> buildingTiles; // 不同等级的建筑对应不同的 Tile
    public string buildingName;
    public List<int> coinCost; // 建筑成本

}