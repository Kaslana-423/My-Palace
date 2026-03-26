using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NPCWeight
{
    public string name;
    public GameObject prefab;
    [Range(0, 100)]
    public int weight;
}

public class PedestrianManager : MonoBehaviour
{
    public static PedestrianManager Instance { get; private set; }

    [Header("NPC 配置")]
    public List<NPCWeight> npcTypes = new List<NPCWeight>();
    public GameObject npcPrefab; // 备用默认

    [Header("总量控制")]
    [Tooltip("地图上总共生成多少个 NPC")]
    public int totalNPCCount = 50; 

    [Header("分布偏好")]
    [Tooltip("多少概率生成在道路连线上？(0=全在路口, 1=全在路上)")]
    [Range(0f, 1f)]
    public float spawnOnRoadChance = 0.8f; 

    // 缓存所有路点
    private List<Waypoint> allWaypointsValues = new List<Waypoint>();
    // 存储所有生成的 NPC 实例
    private List<GameObject> allNPCs = new List<GameObject>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 1. 获取地图上所有路点
        if (WaypointManager.Instance != null)
        {
            foreach (Transform t in WaypointManager.Instance.waypoints)
            {
                if (t != null)
                {
                    Waypoint wp = t.GetComponent<Waypoint>();
                    if (wp != null) allWaypointsValues.Add(wp);
                }
            }
        }

        if (allWaypointsValues.Count == 0)
        {
            Debug.LogError("PedestrianManager: 场景中没有路点！无法生成 NPC。");
            return;
        }

        // 2. 一次性生成所有 NPC
        SpawnAllNPCs();
    }

    void SpawnAllNPCs()
    {
        // 计算权重总和
        int totalWeight = 0;
        foreach (var npc in npcTypes) totalWeight += npc.weight;

        // 生成策略：
        // 我们可以先建立一个“生成队列”，里面按权重配比放好了要生成的 Prefab
        List<GameObject> spawnQueue = new List<GameObject>();

        if (totalWeight > 0)
        {
            foreach (var npc in npcTypes)
            {
                float ratio = (float)npc.weight / totalWeight;
                int count = Mathf.RoundToInt(totalNPCCount * ratio);
                for (int i = 0; i < count; i++) spawnQueue.Add(npc.prefab);
            }
        }
        else if (npcPrefab != null)
        {
            for (int i = 0; i < totalNPCCount; i++) spawnQueue.Add(npcPrefab);
        }

        // 补齐数量 (防止四舍五入少了)
        while (spawnQueue.Count < totalNPCCount && npcTypes.Count > 0)
        {
            spawnQueue.Add(npcTypes[0].prefab);
        }

        // 3. 开始实例化
        GameObject root = new GameObject("NPC_Root");
        
        foreach (GameObject prefabToSpawn in spawnQueue)
        {
            if (prefabToSpawn == null) continue;

            // --- 寻找随机出生点 ---
            Vector3 spawnPos = GetRandomLocationOnMap();

            GameObject npcObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity, root.transform);
            
            // 确保 Z 轴正确
            Vector3 p = npcObj.transform.position;
            p.z = 0;
            npcObj.transform.position = p;

            allNPCs.Add(npcObj);
        }

        Debug.Log($"PedestrianManager: 已生成 {allNPCs.Count} 个 NPC，开始全图游走。");
    }

    // 获取地图上任意一个合法的行走位置 (路口或道路)
    Vector3 GetRandomLocationOnMap()
    {
        // 1. 随机选一个锚点
        Waypoint wp = allWaypointsValues[Random.Range(0, allWaypointsValues.Count)];

        // 2. 决定：生在路口里？还是生在连出去的路上？
        bool trySpawnOnRoad = (wp.neighbors.Count > 0) && (Random.value < spawnOnRoadChance);

        if (trySpawnOnRoad)
        {
            // 随机选一条连出去的路
            Waypoint neighbor = wp.neighbors[Random.Range(0, wp.neighbors.Count)];
            return GetRandomPointOnRoad(wp, neighbor);
        }
        else
        {
            // 生成在路口节点内
            return wp.GetRandomPositionInNode();
        }
    }

    Vector3 GetRandomPointOnRoad(Waypoint start, Waypoint end)
    {
        float t = Random.value;
        Vector3 pointOnLine = Vector3.Lerp(start.transform.position, end.transform.position, t);

        Vector3 dir = (end.transform.position - start.transform.position).normalized;
        // 2D 垂直向量 (-y, x)
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);

        float avgWidth = (start.roadWidth + end.roadWidth) * 0.5f;
        float offset = Random.Range(-avgWidth * 0.5f, avgWidth * 0.5f);

        return pointOnLine + perpendicular * offset;
    }
}