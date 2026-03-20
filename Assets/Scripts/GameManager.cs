using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("核心资源")]
    [SerializeField] private int coins = 100;        // 初始金币 (铜钱)
    [SerializeField] private int population = 50;    // 初始人口
    [SerializeField] private int satisfaction = 0;   // 初始满意度 (正数满意, 负数不满意)
    [SerializeField] private int rounds = 1;         // 当前轮数

    // 公开属性访问器
    public int Coins => coins;
    public int Population => population;
    public int Satisfaction => satisfaction;
    public int Rounds => rounds;

    // 事件通知
    public event Action<int> CoinsChanged;
    public event Action<int> PopulationChanged;
    public event Action<int> SatisfactionChanged; // 原 PersonAngerChanged
    public event Action<int> RoundsChanged;

    public Dictionary<Vector3Int, BuildingData> buildingDataDict = new Dictionary<Vector3Int, BuildingData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        SetCoins(coins + amount);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coins < amount) return false;

        Debug.Log($"花费金币: {amount}");
        SetCoins(coins - amount);
        return true;
    }

    public void SetCoins(int value)
    {
        int clampedValue = Mathf.Max(0, value); // 金币不能为负
        if (coins == clampedValue) return;

        Debug.Log($"设置金币: {clampedValue}");
        coins = clampedValue;
        CoinsChanged?.Invoke(coins);
    }

    public void SetPopulation(int value)
    {
        int clampedValue = Mathf.Max(0, value); // 人口不能为负
        if (population == clampedValue) return;

        Debug.Log($"设置人口: {clampedValue}");
        population = clampedValue;
        PopulationChanged?.Invoke(population);
    }

    public void SetSatisfaction(int value)
    {
        // 关键修改：
        // 1. 变量名改为 satisfaction
        // 2. 去掉了 Mathf.Max(0, value)，允许负数出现
        // 正数 = 满意，负数 = 愤怒
        if (satisfaction == value) return;

        Debug.Log($"设置满意度: {value}");
        satisfaction = value;
        SatisfactionChanged?.Invoke(satisfaction);
    }

    public void SetRounds(int value)
    {
        int clampedValue = Mathf.Max(1, value);
        if (rounds == clampedValue) return;

        Debug.Log($"设置轮数: {clampedValue}");
        rounds = clampedValue;
        RoundsChanged?.Invoke(rounds);
    }
}
