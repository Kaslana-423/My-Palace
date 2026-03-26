using System;
using UnityEngine;

public enum ResourceType
{
    Coin,
    Manpower,
    Material
}

[Serializable]
public struct ResourceAmount
{
    public int coin;
    public int manpower;
    public int material;

    public ResourceAmount(int coin, int manpower, int material)
    {
        this.coin = Mathf.Max(0, coin);
        this.manpower = Mathf.Max(0, manpower);
        this.material = Mathf.Max(0, material);
    }

    public static ResourceAmount Zero => new ResourceAmount(0, 0, 0);

    public bool IsZero => coin <= 0 && manpower <= 0 && material <= 0;
}

public interface IResourceProvider
{
    event Action<ResourceType, int> ResourceChanged;

    int GetResource(ResourceType type);
    bool CanAfford(ResourceAmount cost);
    bool TrySpend(ResourceAmount cost);
    void Add(ResourceAmount amount);
}
