using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // 单例，方便网格检测脚本 O(1) 访问
    public static BuildManager Instance { get; private set; }

    [Header("建筑数据表 (把所有 SO 拖进这里)")]
    public List<BuildingSO> buildingDataAssets;

    // 运行时查表用的字典 (类似 OI 里的 hash map)
    private Dictionary<int, BuildingSO> bDict = new Dictionary<int, BuildingSO>();

    [Header("运行时存活建筑 (供明天轮次结算查表用)")]
    public List<BuildingEntity> activeBuildings = new List<BuildingEntity>();
    [Header("当前选中的建筑笔刷")]
    public int currentBuildingId = 1;
    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 初始化字典，把 Inspector 里拖进来的 SO 映射好，保证后续 O(1) 寻址
        foreach (var data in buildingDataAssets)
        {
            if (!bDict.ContainsKey(data.typeId))
            {
                bDict.Add(data.typeId, data);
            }
        }
    }

    // 仅做资源校验，不扣除。供 UI 鼠标悬浮变绿/红使用
    public bool HasEnoughResources(int typeId)
    {
        if (!bDict.ContainsKey(typeId)) return false;
        var req = bDict[typeId];

        // [cite_start]// 向 GameManager 查询铜钱、人力、建材是否足够 [cite: 15, 30]
        return GameManager.Instance.Coins >= req.costCoins && GameManager.Instance.Population >= req.costPopulation && GameManager.Instance.Materials >= req.costMaterial;
    }

    // [cite_start]// 执行真实建造：扣资源 + 生成实体 + 依赖注入 [cite: 30]
    public bool ExecuteBuild(int typeId, Vector3 worldPos)
    {
        if (!bDict.ContainsKey(typeId)) return false;
        var req = bDict[typeId];

        // GameManager 的 TrySpendResources 是原子的，三项全够才会扣除并返回 true
        if (GameManager.Instance.TrySpendResources(req.costCoins, req.costPopulation, req.costMaterial))
        {
            // 1. 生成空壳预制体
            GameObject clone = Instantiate(req.prefab, worldPos, Quaternion.identity);

            // 2. 核心操作：运行时依赖注入！把 SO 的指针赋给实例
            BuildingEntity entity = clone.GetComponent<BuildingEntity>();
            if (entity != null)
            {
                entity.data = req; // 灵魂注入
                activeBuildings.Add(entity); // 登记造册，明天的回合管理器直接遍历这个 List
            }
            else
            {
                Debug.LogError($"你的 Prefab {clone.name} 忘了挂 BuildingEntity 脚本了！");
            }

            Debug.Log($"建造成功！消耗 钱:{req.costCoins} 人:{req.costPopulation} 材:{req.costMaterial}");
            return true;
        }

        Debug.Log("资源不足，建造拦截。");
        return false;
    }
    // 直接在按钮的 OnClick 事件里绑定这个方法，传个 1 或者 2 进来就行。
    public void SetCurrentBuilding(int typeId)
    {
        if (bDict.ContainsKey(typeId))
        {
            currentBuildingId = typeId;
            Debug.Log($"[UI层通信] 已切换建造笔刷，当前选中 ID: {currentBuildingId}");
        }
        else
        {
            Debug.Log($"[警告] 试图切换到不存在的建筑 ID: {typeId}，请检查 SO 数据是否配置！");
        }
    }
    // 获取当前选中的建筑数据，时间复杂度 O(1)
    public BuildingSO GetCurrentBuildingData()
    {
        if (bDict.ContainsKey(currentBuildingId))
        {
            return bDict[currentBuildingId];
        }
        return null;
    }
}