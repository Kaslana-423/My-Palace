using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildingData
{
    public string buildingName;
    public List<int> coinCost; // 铜钱成本
    public List<int> manpowerCost; // 人力成本
    public List<int> materialCost; // 材料成本

    public int level = 0; // 当前建筑等级

    public BuildingData(string name, int initialLevel = 0)
    {
        if (name == "民居")
        {
            // 1-4级建造/升级消耗：20铜钱；50铜钱40人力；150铜钱150人力5材料；500铜钱500人力20材料
            coinCost = new List<int>(4) { 20, 50, 150, 500 };
            manpowerCost = new List<int>(4) { 12, 40, 150, 500 };
            materialCost = new List<int>(4) { 0, 0, 5, 20 };
        }
        else if (name == "官府")
        {
            // 1-4级建造/升级消耗：30人力；120铜钱120人力；450铜钱450人力10材料；1500铜钱1500人力40材料
            coinCost = new List<int>(4) { 0, 120, 450, 1500 };
            manpowerCost = new List<int>(4) { 30, 120, 450, 1500 };
            materialCost = new List<int>(4) { 0, 0, 10, 40 };
        }
        else
        {
            coinCost = new List<int>();
            manpowerCost = new List<int>();
            materialCost = new List<int>();
        }

        buildingName = name;
        if (coinCost != null && coinCost.Count > 0)
        {
            level = Mathf.Clamp(initialLevel, 0, coinCost.Count - 1);
        }
        else
        {
            level = 0;
        }
    }

}