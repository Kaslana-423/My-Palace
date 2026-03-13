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
    public TextMeshProUGUI levelText; // 显示等级的文本

    void Start()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
        if (demolishButton != null)
        {
            demolishButton.onClick.AddListener(OnDemolishClicked);
        }
    }

    private void OnUpgradeClicked()
    {

    }

    private void OnDemolishClicked()
    {
        throw new NotImplementedException();
    }
}
