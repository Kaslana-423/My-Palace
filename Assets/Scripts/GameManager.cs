using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, IResourceProvider
{
    public static GameManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [SerializeField] private int coins = 100; // 初始金币数量
    [SerializeField] private int manpower = 20; // 初始人力数量
    [SerializeField] private int materials = 30; // 初始材料数量
    [SerializeField] private int population = 50; // 初始人口数量
    [SerializeField] private int prosperity = 0; // 初始繁荣度
    [SerializeField] private int personAnger = 0; // 初始民怨值(百分比数值)
    [SerializeField] private int rounds = 1; // 当前轮数
    public int Coins => coins;
    public int Manpower => manpower;
    public int Materials => materials;
    public int Population => population;
    public int Prosperity => prosperity;
    public int PersonAnger => personAnger;
    public int Rounds => rounds;
    public event Action<int> CoinsChanged;
    public event Action<int> ManpowerChanged;
    public event Action<int> MaterialsChanged;
    public event Action<ResourceType, int> ResourceChanged;
    public event Action<int> PopulationChanged;
    public event Action<int> ProsperityChanged;
    public event Action<int> PersonAngerChanged;
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
        AddResource(ResourceType.Coin, amount);
    }

    public bool TrySpendCoins(int amount)
    {
        return TrySpendResource(ResourceType.Coin, amount);
    }

    public void SetCoins(int value)
    {
        SetResource(ResourceType.Coin, value);
    }

    public void AddManpower(int amount)
    {
        AddResource(ResourceType.Manpower, amount);
    }

    public bool TrySpendManpower(int amount)
    {
        return TrySpendResource(ResourceType.Manpower, amount);
    }

    public void SetManpower(int value)
    {
        SetResource(ResourceType.Manpower, value);
    }

    public void AddMaterials(int amount)
    {
        AddResource(ResourceType.Material, amount);
    }

    public bool TrySpendMaterials(int amount)
    {
        return TrySpendResource(ResourceType.Material, amount);
    }

    public void SetMaterials(int value)
    {
        SetResource(ResourceType.Material, value);
    }

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Coin:
                return coins;
            case ResourceType.Manpower:
                return manpower;
            case ResourceType.Material:
                return materials;
            default:
                return 0;
        }
    }

    public bool CanAfford(ResourceAmount cost)
    {
        return coins >= Mathf.Max(0, cost.coin)
               && manpower >= Mathf.Max(0, cost.manpower)
               && materials >= Mathf.Max(0, cost.material);
    }

    public bool TrySpend(ResourceAmount cost)
    {
        if (!CanAfford(cost))
        {
            return false;
        }

        if (cost.coin > 0)
        {
            SetResource(ResourceType.Coin, coins - cost.coin);
        }
        if (cost.manpower > 0)
        {
            SetResource(ResourceType.Manpower, manpower - cost.manpower);
        }
        if (cost.material > 0)
        {
            SetResource(ResourceType.Material, materials - cost.material);
        }

        return true;
    }

    public void Add(ResourceAmount amount)
    {
        if (amount.coin > 0)
        {
            AddResource(ResourceType.Coin, amount.coin);
        }
        if (amount.manpower > 0)
        {
            AddResource(ResourceType.Manpower, amount.manpower);
        }
        if (amount.material > 0)
        {
            AddResource(ResourceType.Material, amount.material);
        }
    }

    public bool TrySpendResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        int current = GetResource(type);
        if (current < amount)
        {
            return false;
        }

        SetResource(type, current - amount);
        return true;
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetResource(type, GetResource(type) + amount);
    }

    public void SetResource(ResourceType type, int value)
    {
        int clampedValue = Mathf.Max(0, value);

        switch (type)
        {
            case ResourceType.Coin:
                if (coins == clampedValue)
                {
                    return;
                }
                Debug.Log($"设置金币: {clampedValue}");
                coins = clampedValue;
                CoinsChanged?.Invoke(coins);
                ResourceChanged?.Invoke(type, coins);
                break;
            case ResourceType.Manpower:
                if (manpower == clampedValue)
                {
                    return;
                }
                Debug.Log($"设置人力: {clampedValue}");
                manpower = clampedValue;
                ManpowerChanged?.Invoke(manpower);
                ResourceChanged?.Invoke(type, manpower);
                break;
            case ResourceType.Material:
                if (materials == clampedValue)
                {
                    return;
                }
                Debug.Log($"设置材料: {clampedValue}");
                materials = clampedValue;
                MaterialsChanged?.Invoke(materials);
                ResourceChanged?.Invoke(type, materials);
                break;
        }
    }

    public void SetPopulation(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (population == clampedValue)
        {
            return;
        }
        Debug.Log($"设置人口: {clampedValue}");
        population = clampedValue;
        PopulationChanged?.Invoke(population);
    }

    public void SetProsperity(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (prosperity == clampedValue)
        {
            return;
        }
        Debug.Log($"设置繁荣度: {clampedValue}");
        prosperity = clampedValue;
        ProsperityChanged?.Invoke(prosperity);
    }

    public void AddProsperity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }
        SetProsperity(prosperity + amount);
    }

    public void SetPersonAnger(int value)
    {
        int clampedValue = Mathf.Clamp(value, 0, 100);
        if (personAnger == clampedValue)
        {
            return;
        }
        Debug.Log($"设置民怨: {clampedValue}");
        personAnger = clampedValue;
        PersonAngerChanged?.Invoke(personAnger);
    }

    public void SetRounds(int value)
    {
        int clampedValue = Mathf.Max(1, value);
        if (rounds == clampedValue)
        {
            return;
        }
        Debug.Log($"设置轮数: {clampedValue}");
        rounds = clampedValue;
        RoundsChanged?.Invoke(rounds);
    }


}
