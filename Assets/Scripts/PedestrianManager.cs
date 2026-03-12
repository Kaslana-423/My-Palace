using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 1. 定义一个配置结构，显示在面板上
[System.Serializable]
public struct NPCWeight
{
    public string name;         // 备注名字（方便你看）
    public GameObject prefab;   // NPC 预制体
    [Range(0, 100)]
    public int weight;          // 出现权重（比如 50 代表概率很高，1 代表很低）
}

public class PedestrianManager : MonoBehaviour
{
    [Header("NPC 图鉴与权重")]
    // 2. 用列表替代原来的单个 prefab
    public List<NPCWeight> npcTypes = new List<NPCWeight>();
    
    [Header("对象池设置")]
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

    // 3. 修改初始化逻辑：按权重填充池子
    void InitializePool()
    {
        pool = new List<GameObject>();
        GameObject poolRoot = new GameObject("NPC_Pool_Root");

        // 计算总权重
        int totalWeight = 0;
        foreach (var npc in npcTypes)
        {
            totalWeight += npc.weight;
        }

        if (totalWeight <= 0)
        {
            Debug.LogError("NPC 总权重不能为 0！请检查 PedestrianManager 配置。");
            return;
        }

        // 既然池子大小是固定的 (poolSize)，我们需要按比例分配数量
        // 比如 poolSize=50，A权重80，B权重20。
        // A 的数量 = 50 * (80/100) = 40 个
        // B 的数量 = 50 * (20/100) = 10 个
        
        int currentCount = 0;

        foreach (var npc in npcTypes)
        {
            // 计算这种 NPC 该生成多少个
            float ratio = (float)npc.weight / totalWeight;
            int countToSpawn = Mathf.RoundToInt(poolSize * ratio);

            for (int i = 0; i < countToSpawn; i++)
            {
                if (currentCount >= poolSize) break; // 防止计算误差导致溢出

                GameObject obj = Instantiate(npc.prefab, poolRoot.transform);
                obj.SetActive(false); 
                pool.Add(obj);
                currentCount++;
            }
        }

        // 如果因为四舍五入导致数量不够 poolSize，用权重最大的那个补齐
        while (currentCount < poolSize && npcTypes.Count > 0)
        {
            GameObject obj = Instantiate(npcTypes[0].prefab, poolRoot.transform);
            obj.SetActive(false);
            pool.Add(obj);
            currentCount++;
        }
        
        // 4.极其重要：打乱池子！
        // 否则前40个全是A，后10个全是B。导致游戏刚开始全是A，后期才是B。
        ShuffleList(pool);
    }

    // Fisher-Yates 洗牌算法
    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
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
            for (int i = 0; i < 5; i++)
            {
                Waypoint startNode = validRoadStarts[Random.Range(0, validRoadStarts.Count)];
                Waypoint endNode = startNode.GetRandomNeighbor();
                
                if (endNode != null)
                {
                    Vector3 p = GetRandomPointOnRoad(startNode, endNode);
                    if (!innerRect.Contains(p) && outerRect.Contains(p))
                    {
                        spawnPos = p;
                        readyToSpawn = true;
                        break; 
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

            if (insideOuter && !insideInner)
            {
                validWaypoints.Add(wp); 
                if (wp.neighbors.Count > 0) validRoadStarts.Add(wp); 
            }
            else if (insideInner)
            {
                if (wp.neighbors.Count > 0)
                {
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

    // 5. 修改获取逻辑：其实这里不需要改，因为池子已经随机化了，按顺序取就行
    GameObject GetInactiveNPC()
    {
        for (int i = 0; i < pool.Count; i++) 
        {
            if (!pool[i].activeSelf) 
            {
                return pool[i];
            }
        }
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