using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [SerializeField] private int coins = 1000; // 初始金币数量
    public int Coins => coins;

    public event Action<int> CoinsChanged;
    public event Action<Vector3Int, BuildingData> BuildingUpgraded;
    public event Action<Vector3Int> BuildingDemolished;

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
    public void TryUpgradeBuilding(Vector3Int position)
    {

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
}
