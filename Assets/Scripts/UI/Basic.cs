using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Basic : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public TextMeshProUGUI coinText;         // 显示金币
    public TextMeshProUGUI materialsText;    // 显示建材
    public TextMeshProUGUI populationText;   // 显示人口
    public TextMeshProUGUI satisfactionText; // 显示满意度
    public TextMeshProUGUI prosperityText;   // 显示繁荣度
    public TextMeshProUGUI roundsText;       // 显示轮数

    private void Start()
    {
        if (GameManager.HasInstance)
        {
            // 订阅事件
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            GameManager.Instance.MaterialsChanged += OnMaterialsChanged; // [新增]
            GameManager.Instance.PopulationChanged += OnPopulationChanged;
            GameManager.Instance.SatisfactionChanged += OnSatisfactionChanged;
            GameManager.Instance.ProsperityChanged += OnProsperityChanged; // [新增]
            GameManager.Instance.RoundsChanged += OnRoundsChanged;

            // 初始化显示
            RefreshCoinDisplay(GameManager.Instance.Coins);
            RefreshMaterialsDisplay(GameManager.Instance.Materials); // [新增]
            RefreshPopulationDisplay(GameManager.Instance.Population);
            RefreshSatisfactionDisplay(GameManager.Instance.Satisfaction);
            RefreshProsperityDisplay(GameManager.Instance.Prosperity); // [新增]
            RefreshRoundsDisplay(GameManager.Instance.Rounds);
        }
    }

    private void OnDisable()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.MaterialsChanged -= OnMaterialsChanged; // [新增]
            GameManager.Instance.PopulationChanged -= OnPopulationChanged;
            GameManager.Instance.SatisfactionChanged -= OnSatisfactionChanged;
            GameManager.Instance.ProsperityChanged -= OnProsperityChanged; // [新增]
            GameManager.Instance.RoundsChanged -= OnRoundsChanged;
        }
    }

    // --- 回调函数 ---
    private void OnCoinsChanged(int value) => RefreshCoinDisplay(value);
    private void OnMaterialsChanged(int value) => RefreshMaterialsDisplay(value); // [新增]
    private void OnPopulationChanged(int value) => RefreshPopulationDisplay(value);
    private void OnSatisfactionChanged(int value) => RefreshSatisfactionDisplay(value);
    private void OnProsperityChanged(int value) => RefreshProsperityDisplay(value); // [新增]
    private void OnRoundsChanged(int value) => RefreshRoundsDisplay(value);

    // --- 刷新显示 ---
    private void RefreshCoinDisplay(int value)
    {
        if (coinText != null) coinText.text = value.ToString();
    }

    // [新增]
    private void RefreshMaterialsDisplay(int value)
    {
        if (materialsText != null) materialsText.text = value.ToString();
    }

    private void RefreshPopulationDisplay(int value)
    {
        if (populationText != null) populationText.text = value.ToString();
    }

    private void RefreshSatisfactionDisplay(int value)
    {
        if (satisfactionText != null)
        {
            satisfactionText.text = value.ToString();
            // satisfactionText.color = value < 0 ? Color.red : Color.white;
        }
    }

    // [新增]
    private void RefreshProsperityDisplay(int value)
    {
        if (prosperityText != null) prosperityText.text = value.ToString();
    }

    private void RefreshRoundsDisplay(int value)
    {
        if (roundsText != null) roundsText.text = "第 " + value.ToString() + " 轮";
    }
}
