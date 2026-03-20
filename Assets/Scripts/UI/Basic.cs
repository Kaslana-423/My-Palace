using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Basic : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public TextMeshProUGUI coinText;         // 显示金币
    public TextMeshProUGUI populationText;   // 显示人口
    public TextMeshProUGUI satisfactionText; // [修改] 显示满意度 (原民怨)
    public TextMeshProUGUI roundsText;       // 显示轮数

    private void Start()
    {
        if (GameManager.HasInstance)
        {
            // 订阅金币
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            RefreshCoinDisplay(GameManager.Instance.Coins);

            // 订阅人口
            GameManager.Instance.PopulationChanged += OnPopulationChanged;
            RefreshPopulationDisplay(GameManager.Instance.Population);

            // [修改] 订阅满意度
            GameManager.Instance.SatisfactionChanged += OnSatisfactionChanged;
            RefreshSatisfactionDisplay(GameManager.Instance.Satisfaction);

            // 订阅轮数
            GameManager.Instance.RoundsChanged += OnRoundsChanged;
            RefreshRoundsDisplay(GameManager.Instance.Rounds);
        }
    }

    private void OnDisable()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.PopulationChanged -= OnPopulationChanged;
            GameManager.Instance.SatisfactionChanged -= OnSatisfactionChanged;
            GameManager.Instance.RoundsChanged -= OnRoundsChanged;
        }
    }

    // --- 回调函数 ---

    private void OnCoinsChanged(int currentCoins)
    {
        RefreshCoinDisplay(currentCoins);
    }

    private void OnPopulationChanged(int currentPopulation)
    {
        RefreshPopulationDisplay(currentPopulation);
    }

    // [修改] 对应变量名变更
    private void OnSatisfactionChanged(int currentSatisfaction)
    {
        RefreshSatisfactionDisplay(currentSatisfaction);
    }

    private void OnRoundsChanged(int currentRounds)
    {
        RefreshRoundsDisplay(currentRounds);
    }

    // --- 刷新显示 ---

    private void RefreshRoundsDisplay(int value)
    {
        if (roundsText != null)
            roundsText.text = "第 " + value.ToString() + " 轮";
    }

    private void RefreshCoinDisplay(int value)
    {
        if (coinText != null)
            coinText.text = value.ToString();
    }

    private void RefreshPopulationDisplay(int value)
    {
        if (populationText != null)
            populationText.text = value.ToString();
    }

    // [修改] 这里可以处理一下正负数的显示颜色，比如负数显红
    private void RefreshSatisfactionDisplay(int value)
    {
        if (satisfactionText != null)
        {
            satisfactionText.text = value.ToString();
            
            // 可选：如果小于0，显示红色；否则显示白色/绿色
            // satisfactionText.color = value < 0 ? Color.red : Color.white;
        }
            
    }
}
