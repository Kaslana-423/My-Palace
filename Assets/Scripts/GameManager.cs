using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [SerializeField] private int coins = 100; // 初始金币数量
    [SerializeField] private int population = 50; // 初始人口数量
    [SerializeField] private int personAnger = 0; // 初始民怨值
    [SerializeField] private int rounds = 1; // 当前轮数
    public int Coins => coins;
    public int Population => population;
    public int PersonAnger => personAnger;
    public int Rounds => rounds;
    public event Action<int> CoinsChanged;
    public event Action<int> PopulationChanged;
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
        if (amount <= 0)
        {
            return;
        }
        SetCoins(coins + amount);
    }

    public bool TrySpendCoins(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (coins < amount)
        {
            return false;
        }
        Debug.Log($"花费金币: {amount}");
        SetCoins(coins - amount);
        return true;
    }

    public void SetCoins(int value)
    {
        int clampedValue = Mathf.Max(0, value);
        if (coins == clampedValue)
        {
            return;
        }
        Debug.Log($"设置金币: {clampedValue}");
        coins = clampedValue;
        CoinsChanged?.Invoke(coins);
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

    public void SetPersonAnger(int value)
    {
        int clampedValue = Mathf.Max(0, value);
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
