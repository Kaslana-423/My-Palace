using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingData : MonoBehaviour
{
    public string buildingName;
    public List<int> coinCost; // 建筑成本
    public int level = 0; // 当前建筑等级
    public BuildingData(string name)
    {
        if (name == "民居")
            coinCost = new List<int>(4) { 50, 100, 150, 200 }; // 示例成本，实际可以根据需要调整
        else if (name == "官府")
            coinCost = new List<int>(4) { 100, 200, 300, 400 }; // 示例成本，实际可以根据需要调整
        buildingName = name;
        level = 0;
    }

}