using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("基础资源")]
    [SerializeField] private int coins = 100;        // 铜钱
    [SerializeField] private int materials = 50;     // 建材
    [SerializeField] private int population = 50;    // 人力/人口

    [Header("全局状态")]
    [SerializeField] private int prosperity = 0;     // 繁荣度
    [SerializeField] private int personAnger = 0;    // 民怨值 (对应之前的 satisfaction)
    [SerializeField] private int rounds = 1;         // 当前轮数

    // 公开属性访问器
    public int Coins => coins;
    public int Materials => materials;
    public int Population => population;
    public int Prosperity => prosperity;
    public int PersonAnger => personAnger;
    public int Satisfaction => personAnger; // 兼容旧分支的别名
    public int Rounds => rounds;

    // 事件通知
    public event Action<int> CoinsChanged;
    public event Action<int> MaterialsChanged;
    public event Action<int> PopulationChanged;
    public event Action<int> ProsperityChanged;
    public event Action<int> PersonAngerChanged;
    public event Action<int> SatisfactionChanged; // 兼容旧分支的事件
    public event Action<int> RoundsChanged;

    // 建筑数据存储 (保留旧分支定义的字典)
    // public Dictionary<Vector3Int, BuildingData> buildingDataDict = new Dictionary<Vector3Int, BuildingData>();

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

    // --- 核心业务接口：原子扣费 (支持三资源同时扣除) ---
    public bool TrySpendResources(int costCoin, int costPop, int costMat)
    {
        if (coins < costCoin || population < costPop || materials < costMat) return false;

        SetCoins(coins - costCoin);
        SetPopulation(population - costPop);
        SetMaterials(materials - costMat);
        return true;
    }

    // --- 金币逻辑 ---
    public void AddCoins(int amount) { if (amount > 0) SetCoins(coins + amount); }
    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coins < amount) return false;
        SetCoins(coins - amount);
        return true;
    }

    public void SetCoins(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (coins == clamped) return;
        coins = clamped;
        CoinsChanged?.Invoke(coins);
    }

    // --- 建材逻辑 ---
    public void AddMaterials(int amount) { if (amount > 0) SetMaterials(materials + amount); }
    public bool TrySpendMaterials(int amount)
    {
        if (amount <= 0) return true;
        if (materials < amount) return false;
        SetMaterials(materials - amount);
        return true;
    }

    public void SetMaterials(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (materials == clamped) return;
        materials = clamped;
        MaterialsChanged?.Invoke(materials);
    }

    // --- 人口逻辑 ---
    public void AddPopulation(int amount) { if (amount > 0) SetPopulation(population + amount); }
    public bool TrySpendPopulation(int amount)
    {
        if (amount <= 0) return true;
        if (population < amount) return false;
        SetPopulation(population - amount);
        return true;
    }
    public void SetPopulation(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (population == clamped) return;
        population = clamped;
        PopulationChanged?.Invoke(population);
    }

    // --- 满意度/民怨逻辑 (双向兼容) ---
    public void AddPersonAnger(int amount) { if (amount > 0) SetPersonAnger(personAnger + amount); }
    public bool TrySpendPersonAnger(int amount)
    {
        if (amount <= 0) return true;
        if (personAnger < amount) return false;
        SetPersonAnger(personAnger - amount);
        return true;
    }
    public void SetSatisfaction(int value) => SetPersonAnger(value); // 旧接口映射

    public void SetPersonAnger(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (personAnger == clamped) return;
        personAnger = clamped;
        PersonAngerChanged?.Invoke(personAnger);
        SatisfactionChanged?.Invoke(personAnger); // 同步触发旧事件
    }

    // --- 繁荣度逻辑 ---
    public void AddProsperity(int amount) { if (amount > 0) SetProsperity(prosperity + amount); }
    public bool TrySpendProsperity(int amount)
    {
        if (amount <= 0) return true;
        if (prosperity < amount) return false;
        SetProsperity(prosperity - amount);
        return true;
    }
    public void SetProsperity(int value)
    {
        int clamped = Mathf.Max(0, value);
        if (prosperity == clamped) return;
        prosperity = clamped;
        ProsperityChanged?.Invoke(prosperity);
    }
    public bool TryExchangeProsperity(int coin, int pop)
    {
        if (coin < 0 || pop < 0) return false;
        if (coins >= coin && population >= pop)
        {
            SetCoins(coins - coin);
            SetPopulation(population - pop);
            AddProsperity(coin + pop);
            return true;
        }
        return false;
    }

    // --- 轮数逻辑 ---
    public void SetRounds(int value)
    {
        int clamped = Mathf.Max(1, value);
        if (rounds == clamped) return;
        rounds = clamped;
        RoundsChanged?.Invoke(rounds);
    }
}