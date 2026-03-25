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
    [SerializeField] private int materials = 50;     // 初始建材
    [SerializeField] private int population = 50;    // 初始人口
    [SerializeField] private int satisfaction = 0;   // 初始民怨值
    [SerializeField] private int prosperity = 0;     // 初始繁荣度
    [SerializeField] private int rounds = 1;         // 当前轮数

    // 公开属性访问器
    public int Coins => coins;
    public int Materials => materials; // [新增]
    public int Population => population;
    public int Satisfaction => satisfaction;
    public int Prosperity => prosperity; // [新增]
    public int Rounds => rounds;

    // 事件通知
    public event Action<int> CoinsChanged;
    public event Action<int> MaterialsChanged; // [新增]
    public event Action<int> PopulationChanged;
    public event Action<int> SatisfactionChanged;
    public event Action<int> ProsperityChanged; // [新增]
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

    // --- 金币逻辑 ---
    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        SetCoins(coins + amount);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coins < amount) return false;

        // Debug.Log($"花费金币: {amount}");
        SetCoins(coins - amount);
        return true;
    }

    public void SetCoins(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (coins == clampedValue) return;
        coins = clampedValue;
        CoinsChanged?.Invoke(coins);
    }

    // --- [新增] 建材逻辑 ---
    public void AddMaterials(int amount)
    {
        if (amount <= 0) return;
        SetMaterials(materials + amount);
    }

    public bool TrySpendMaterials(int amount)
    {
        if (amount <= 0) return true;
        if (materials < amount) return false;

        Debug.Log($"花费建材: {amount}");
        SetMaterials(materials - amount);
        return true;
    }

    public void SetMaterials(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (materials == clampedValue) return;
        materials = clampedValue;
        MaterialsChanged?.Invoke(materials);
    }

    // --- 人口逻辑 ---
    public void SetPopulation(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (population == clampedValue) return;
        population = clampedValue;
        PopulationChanged?.Invoke(population);
    }

    // --- 满意度逻辑 ---
    public void SetSatisfaction(int value)
    {
        if (satisfaction == value) return;
        satisfaction = value;
        SatisfactionChanged?.Invoke(satisfaction);
    }

    // --- [新增] 繁荣度逻辑 ---
    public void AddProsperity(int amount)
    {
        SetProsperity(prosperity + amount);
    }

    public void SetProsperity(int value)
    {
        int clampedValue = Mathf.Max(0, value); // 繁荣度通常不为负
        if (prosperity == clampedValue) return;
        prosperity = clampedValue;
        ProsperityChanged?.Invoke(prosperity);
    }

    // --- 轮数逻辑 ---
    public void SetRounds(int value)
    {
        int clampedValue = Mathf.Max(1, value);
        if (rounds == clampedValue) return;
        rounds = clampedValue;
        RoundsChanged?.Invoke(rounds);
    }
}
