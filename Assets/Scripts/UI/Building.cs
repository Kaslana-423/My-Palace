using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Building : MonoBehaviour
{
    public Button upgradeButton; // 升级按钮
    public Button demolishButton; // 拆除按钮
    public Image buildingImage; // 显示建筑图标的 Image 组件
    public List<Sprite> mingju; // 民居等级图标列表
    public List<Sprite> guanfu; // 官府等级图标列表
    public TextMeshProUGUI coinCostText; // 显示升级成本的文本
    public TextMeshProUGUI buildingNameText; // 显示建筑名称的文本
    public TextMeshProUGUI demolishRefundText; // 显示拆除返还金币的文本

    private void OnEnable()
    {
        if (PlaceSysManager.Instance != null)
        {
            PlaceSysManager.Instance.BuildingUpgraded += RefreshUI;
        }
    }

    private void OnDisable()
    {
        if (PlaceSysManager.Instance != null)
        {
            PlaceSysManager.Instance.BuildingUpgraded -= RefreshUI;
        }
    }

    public void RefreshUI(Vector3Int position)
    {
        if (!GameManager.HasInstance)
        {
            return;
        }

        if (!GameManager.Instance.buildingDataDict.TryGetValue(position, out BuildingData data))
        {
            return;
        }

        buildingNameText.text = data.buildingName + "-lv." + (data.level + 1).ToString();
        if (data.buildingName == "民居")
        {
            if (data.level >= 0 && data.level < mingju.Count)
            {
                SetBuildingSpriteNativeSize(mingju[data.level]);
            }
        }
        else if (data.buildingName == "官府")
        {
            if (data.level >= 0 && data.level < guanfu.Count)
            {
                SetBuildingSpriteNativeSize(guanfu[data.level]);
            }
        }
        if (data.level < data.coinCost.Count)
        {
            coinCostText.text = $"消耗: {data.coinCost[data.level]}";
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeButton.interactable = false; // 禁用升级按钮
        }
        demolishRefundText.text = $"拆除返还: {data.coinCost[Math.Max(0, data.level - 1)] / 2}"; // 返还上一级成本的一半
    }

    private void SetBuildingSpriteNativeSize(Sprite sprite)
    {
        if (buildingImage == null || sprite == null)
        {
            return;
        }

        buildingImage.sprite = sprite;
        buildingImage.SetNativeSize();
    }

}
