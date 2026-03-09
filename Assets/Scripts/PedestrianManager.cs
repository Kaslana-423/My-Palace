using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PedestrianManager : MonoBehaviour
{
    [Header("对象池设置")]
    public GameObject npcPrefab;
    public int poolSize = 50;

    [Header("生成规则")]
    public float spawnInterval = 0.2f;

    [Tooltip("生成区域距离摄像机边缘的最近距离")]
    public float minSpawnMargin = 2.0f; 
    [Tooltip("生成区域距离摄像机边缘的最远距离")]
    public float maxSpawnMargin = 8.0f; 
    
    [Tooltip("在道路段上生成的概率 (0~1)")]
    public float spawnOnRoadChance = 0.7f; 
    
    [Header("回收规则")]
    public float despawnMargin = 12.0f;

    private List<GameObject> pool;
    private List<Waypoint> validWaypoints = new List<Waypoint>();
    private List<Waypoint> validRoadStarts = new List<Waypoint>(); 

    private Camera mainCam;
    private Transform camTransform;
    private float timer;

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam != null)
        {
            camTransform = mainCam.transform;
        }
        else
        {
            Debug.LogError("场景中找不到 MainCamera！");
            this.enabled = false;
            return;
        }

        InitializePool();
    }

    void InitializePool()
    {
        pool = new List<GameObject>();
        GameObject poolRoot = new GameObject("NPC_Pool_Root");
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(npcPrefab, poolRoot.transform);
            obj.SetActive(false); 
            pool.Add(obj);
        }
    }

    void Update()
    {
        Rect cameraRect = GetCameraWorldRect();
        Rect despawnRect = ExpandRect(cameraRect, despawnMargin);

        CheckDespawn(despawnRect);

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            TrySpawnOnePedestrian(cameraRect);
            timer = 0f;
        }
    }

    Rect GetCameraWorldRect()
    {
        float height = 2f * mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        Vector3 camPos = camTransform.position;
        return new Rect(camPos.x - width / 2f, camPos.y - height / 2f, width, height);
    }

    Rect ExpandRect(Rect original, float margin)
    {
        return new Rect(
            original.x - margin, 
            original.y - margin, 
            original.width + margin * 2f, 
            original.height + margin * 2f
        );
    }

    // [核心修改 2] 在生成时进行多次尝试，剔除掉那些落在屏幕内的“坏点”
    void TrySpawnOnePedestrian(Rect cameraRect)
    {
        GameObject npc = GetInactiveNPC();
        if (npc == null) return; 
        if (WaypointManager.Instance == null) return;

        Rect innerRect = ExpandRect(cameraRect, minSpawnMargin);
        Rect outerRect = ExpandRect(cameraRect, maxSpawnMargin);

        CollectValidSpawnAreas(innerRect, outerRect);

        bool hasNodes = validWaypoints.Count > 0;
        bool hasRoads = validRoadStarts.Count > 0;

        Vector3 spawnPos = Vector3.zero;
        bool readyToSpawn = false;

        // --- 尝试道路生成 ---
        if (hasRoads && Random.value < spawnOnRoadChance)
        {
            // 给5次机会。虽然 startNode 可能在屏幕内，但路是长的
            // 我们随机取点，如果取到了屏幕内的点，就重试，直到取到屏幕外的点
            for (int i = 0; i < 5; i++)
            {
                Waypoint startNode = validRoadStarts[Random.Range(0, validRoadStarts.Count)];
                Waypoint endNode = startNode.GetRandomNeighbor();
                
                if (endNode != null)
                {
                    Vector3 p = GetRandomPointOnRoad(startNode, endNode);
                    // 必须满足：在内框外（看不到的地方），且在外框内（别太远）
                    // contains 返回 true 表示在框内，我们要 !inner.Contains
                    if (!innerRect.Contains(p) && outerRect.Contains(p))
                    {
                        spawnPos = p;
                        readyToSpawn = true;
                        break; // 找到了！退出循环
                    }
                }
            }
        }
        
        // --- 尝试路口生成 ---
        if (!readyToSpawn && hasNodes)
        {
            Waypoint targetNode = validWaypoints[Random.Range(0, validWaypoints.Count)];
            spawnPos = targetNode.GetRandomPositionInNode();
            readyToSpawn = true;
        }

        if (readyToSpawn)
        {
            spawnPos.z = 0;
            npc.transform.position = spawnPos;
            npc.SetActive(true);
        }
    }

    // [核心修改 1] 放宽筛选条件
    void CollectValidSpawnAreas(Rect inner, Rect outer)
    {
        validWaypoints.Clear();
        validRoadStarts.Clear();

        foreach (Transform t in WaypointManager.Instance.waypoints)
        {
            Waypoint wp = t.GetComponent<Waypoint>(); 
            if (wp == null) continue;

            Vector2 pos = t.position; 

            bool insideOuter = outer.Contains(pos);
            bool insideInner = inner.Contains(pos);

            // 情况 A：点正好在生成带上 -> 完美，直接加
            if (insideOuter && !insideInner)
            {
                validWaypoints.Add(wp); // 可以作为点生成
                if (wp.neighbors.Count > 0) validRoadStarts.Add(wp); // 也可以作为路起点
            }
            // 情况 B（修复你的Bug）：点在屏幕内（太近），但它连着外面的路
            else if (insideInner)
            {
                // 点生成？不行！还是会突然冒出来。所以不加进 validWaypoints
                
                // 路生成？有可能！检查它的邻居有没有在外面的
                // 只要该点有连线出去，这根线必然穿过生成带
                if (wp.neighbors.Count > 0)
                {
                    // 只要有邻居，我们就把它作为候选路段加入
                    // 具体这个路段的哪一部分在生成带上，交给 TrySpawn 里的随机重试去解决
                    validRoadStarts.Add(wp);
                }
            }
        }
    }

    Vector3 GetRandomPointOnRoad(Waypoint start, Waypoint end)
    {
        float t = Random.value;
        Vector3 pointOnLine = Vector3.Lerp(start.transform.position, end.transform.position, t);

        Vector3 dir = (end.transform.position - start.transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(dir, Vector3.forward).normalized;

        float avgWidth = (start.roadWidth + end.roadWidth) * 0.5f;
        float offsetAmount = Random.Range(-avgWidth * 0.5f, avgWidth * 0.5f);

        return pointOnLine + perpendicular * offsetAmount;
    }

    void CheckDespawn(Rect despawnRect)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].activeSelf)
            {
                Vector2 npcPos = pool[i].transform.position;
                if (!despawnRect.Contains(npcPos))
                {
                    pool[i].SetActive(false); 
                }
            }
        }
    }

    GameObject GetInactiveNPC()
    {
        for (int i = 0; i < pool.Count; i++) if (!pool[i].activeSelf) return pool[i];
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        if (Camera.main == null) return;
        
        float height = 2f * Camera.main.orthographicSize;
        float width = height * Camera.main.aspect;
        Vector3 camPos = Camera.main.transform.position;
        camPos.z = 0;
        
        Vector3 size = new Vector3(width, height, 0);
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(camPos, size);

        Gizmos.color = Color.green;
        Vector3 sizeMin = new Vector3(width + minSpawnMargin * 2, height + minSpawnMargin * 2, 0);
        Gizmos.DrawWireCube(camPos, sizeMin);

        Vector3 sizeMax = new Vector3(width + maxSpawnMargin * 2, height + maxSpawnMargin * 2, 0);
        Gizmos.DrawWireCube(camPos, sizeMax);

        Gizmos.color = Color.red;
        Vector3 sizeDespawn = new Vector3(width + despawnMargin * 2, height + despawnMargin * 2, 0);
        Gizmos.DrawWireCube(camPos, sizeDespawn);
    }
}