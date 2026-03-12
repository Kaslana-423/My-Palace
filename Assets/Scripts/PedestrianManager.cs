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
    public GameObject npcPrefab;

    [Header("流量控制")]
    public int targetDensity = 30;
    public int minEmergencyCount = 5; 

    [Header("生成范围")]
    public float spawnMargin = 5.0f;
    public float despawnMargin = 15.0f;
    
    // [新增] 道路生成概率控制
    [Header("分布偏好")]
    [Tooltip("多少概率生成在道路连线上？(0=全在路口, 1=全在路上)")]
    [Range(0f, 1f)]
    public float spawnOnRoadChance = 0.8f; // 建议设高一点，因为道路面积通常比路口大

    private List<GameObject> pool;
    private Camera mainCam;
    private Transform camTransform;
    private List<Waypoint> allWaypointsValues = new List<Waypoint>();
    private Vector3 lastCamPos;
    private Vector3 camVelocity;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;
        if (mainCam == null)
        {
            this.enabled = false;
            return;
        }
        camTransform = mainCam.transform;
        lastCamPos = camTransform.position;

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

        InitializePool();
    }

    void InitializePool()
    {
        pool = new List<GameObject>();
        GameObject poolRoot = new GameObject("NPC_Pool_Root");
        int poolSize = targetDensity + 15;

        int totalWeight = 0;
        foreach (var npc in npcTypes) totalWeight += npc.weight;
        if (totalWeight <= 0 && npcPrefab != null)
        {
            for(int i=0; i<poolSize; i++) CreateObj(npcPrefab, poolRoot);
            return;
        }

        int currentCount = 0;
        foreach (var npc in npcTypes)
        {
            float ratio = (float)npc.weight / totalWeight;
            int count = Mathf.RoundToInt(poolSize * ratio);
            for (int i = 0; i < count; i++)
            {
                if (currentCount >= poolSize) break;
                CreateObj(npc.prefab, poolRoot);
                currentCount++;
            }
        }
        while (currentCount < poolSize && npcTypes.Count > 0)
        {
            CreateObj(npcTypes[0].prefab, poolRoot);
            currentCount++;
        }
        
        for (int i = 0; i < pool.Count; i++)
        {
            var temp = pool[i];
            int r = Random.Range(i, pool.Count);
            pool[i] = pool[r];
            pool[r] = temp;
        }
    }

    void CreateObj(GameObject prefab, GameObject root)
    {
        GameObject obj = Instantiate(prefab, root.transform);
        obj.SetActive(false);
        pool.Add(obj);
    }

    void Update()
    {
        if (mainCam == null) return;

        Vector3 currentCamPos = camTransform.position;
        camVelocity = (currentCamPos - lastCamPos) / Time.deltaTime;
        lastCamPos = currentCamPos;

        Rect cameraRect = GetCameraWorldRect(0f);
        Rect spawnRect = GetCameraWorldRect(spawnMargin);
        Rect despawnRect = GetCameraWorldRect(despawnMargin);

        int activeCount = 0;
        int activeOnScreenCount = 0;

        for (int i = 0; i < pool.Count; i++)
        {
            GameObject npc = pool[i];
            if (npc.activeSelf)
            {
                Vector2 pos = npc.transform.position;
                if (!despawnRect.Contains(pos))
                {
                    npc.SetActive(false);
                }
                else
                {
                    activeCount++;
                    if (cameraRect.Contains(pos)) activeOnScreenCount++;
                }
            }
        }

        if (activeCount < targetDensity)
        {
            int spawnBudget = 2; 
            bool isEmergency = activeOnScreenCount < minEmergencyCount;
            if (isEmergency) spawnBudget = 5;

            for (int k = 0; k < spawnBudget; k++)
            {
                if (activeCount >= targetDensity) break;

                Vector3? spawnPos = FindBestSpawnPosition(cameraRect, spawnRect, isEmergency);
                
                if (spawnPos.HasValue)
                {
                    SpawnNPC(spawnPos.Value);
                    activeCount++;
                    if (cameraRect.Contains(spawnPos.Value)) activeOnScreenCount++;
                }
            }
        }
    }

    // [核心修改] 寻找最佳生成点：同时支持路口和道路
    Vector3? FindBestSpawnPosition(Rect cameraRect, Rect spawnRect, bool allowScreenSpawn)
    {
        // 候选点列表（直接存算好的坐标 Vector3，不再存 Waypoint）
        List<Vector3> candidates = new List<Vector3>();
        
        // 随机采样 15 次
        for(int i=0; i<15; i++)
        {
            if (allWaypointsValues.Count == 0) break;
            
            // 1. 随机选一个锚点
            Waypoint wp = allWaypointsValues[Random.Range(0, allWaypointsValues.Count)];
            
            // 2. 决定：生在路口里？还是生在连出去的路上？
            // 如果它有邻居，且随机数命中了道路概率 -> 尝试生成在道路上
            bool trySpawnOnRoad = (wp.neighbors.Count > 0) && (Random.value < spawnOnRoadChance);

            Vector3 potentialPos;

            if (trySpawnOnRoad)
            {
                // 随机选一条连出去的路
                Waypoint neighbor = wp.neighbors[Random.Range(0, wp.neighbors.Count)];
                // 计算道路上的随机点
                potentialPos = GetRandomPointOnRoad(wp, neighbor);
            }
            else
            {
                // 生成在路口节点内
                potentialPos = wp.GetRandomPositionInNode();
            }

            // 3. 验证这个具体的点 (PotentialPos) 是否合格
            // 不再验证 wp.transform.position，必须验证算出来的那个确切坐标

            // A. 必须在生成圈内
            if (!spawnRect.Contains(potentialPos)) continue;

            // B. 视野判断
            bool onScreen = cameraRect.Contains(potentialPos);
            if (onScreen && !allowScreenSpawn) continue;

            // C. 方向偏置 (Move Bias)
            if (camVelocity.sqrMagnitude > 0.1f)
            {
                Vector2 dirToWP = (Vector2)potentialPos - (Vector2)camTransform.position;
                if (Vector2.Dot(dirToWP.normalized, camVelocity.normalized) < -0.2f)
                {
                    if (!allowScreenSpawn) continue;
                }
            }

            // 如果通过了所有考验，加入候选
            candidates.Add(potentialPos);
        }

        if (candidates.Count == 0) return null;

        // 从合法的候选点里随一个返回
        return candidates[Random.Range(0, candidates.Count)];
    }

    // [新增] 计算道路连线上的随机点（带宽度偏移）
    Vector3 GetRandomPointOnRoad(Waypoint start, Waypoint end)
    {
        // 1. 在连线上随机取进度 t (0~1)
        float t = Random.value;
        Vector3 pointOnLine = Vector3.Lerp(start.transform.position, end.transform.position, t);

        // 2. 计算垂直方向（2D平面上，(x, y) 的垂直向量是 (-y, x)）
        Vector3 dir = (end.transform.position - start.transform.position).normalized;
        Vector3 perpendicular = new Vector3(-dir.y, dir.x, 0);

        // 3. 计算随机偏移量
        // 取两端路宽的平均值
        float avgWidth = (start.roadWidth + end.roadWidth) * 0.5f;
        float offset = Random.Range(-avgWidth * 0.5f, avgWidth * 0.5f);

        return pointOnLine + perpendicular * offset;
    }

    void SpawnNPC(Vector3 pos)
    {
        GameObject npc = GetInactiveNPC();
        if (npc != null)
        {
            pos.z = 0;
            npc.transform.position = pos;
            npc.SetActive(true);
        }
    }

    GameObject GetInactiveNPC()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeSelf) return pool[i];
        }
        return null; 
    }

    Rect GetCameraWorldRect(float margin)
    {
        float height = 2f * mainCam.orthographicSize;
        float width = height * mainCam.aspect;
        Vector3 pos = camTransform.position;
        return new Rect(pos.x - width / 2f - margin, pos.y - height / 2f - margin, width + margin * 2f, height + margin * 2f);
    }

    private void OnDrawGizmos()
    {
        if (Camera.main == null) return;
        
        Rect camRect = GetCameraWorldRect(0);
        Rect spawnRect = GetCameraWorldRect(spawnMargin);
        Rect despawnRect = GetCameraWorldRect(despawnMargin);

        Gizmos.color = Color.white;
        DrawRect(camRect);
        Gizmos.color = Color.yellow;
        DrawRect(spawnRect);
        Gizmos.color = Color.red;
        DrawRect(despawnRect);
    }

    void DrawRect(Rect r)
    {
        Vector3 p1 = new Vector3(r.x, r.y, 0);
        Vector3 p2 = new Vector3(r.x + r.width, r.y, 0);
        Vector3 p3 = new Vector3(r.x + r.width, r.y + r.height, 0);
        Vector3 p4 = new Vector3(r.x, r.y + r.height, 0);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}