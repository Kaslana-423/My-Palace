using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; 

public class NPCRoam : MonoBehaviour
{
    private enum NPCState
    {
        Idle, // 发呆
        Walk  // 移动
    }

    [Header("移动参数")]
    public float baseMoveSpeed = 0.2f; 
    private float actualMoveSpeed;     
    
    public float minIdleTime = 0.0f;   
    public float maxIdleTime = 1.0f;

    [Header("鸟群算法 - 分离力")]
    [Tooltip("如果太近，互相推开的半径")]
    public float separationRadius = 0.8f; 
    [Tooltip("推开的力度")]
    public float separationForce = 1.5f;
    [Tooltip("检测这一层（只检测其他NPC）")]
    public LayerMask npcLayer; 

    [Header("渲染设置")]
    public int sortingOrderBase = 5000; 

    private NPCState currentState = NPCState.Idle;
    
    private Waypoint previousWaypoint; 
    private Waypoint currentWaypoint;  // 逻辑上归属的路点
    private Waypoint targetWaypoint;   // 正在前往的路点
    
    private Vector3 currentDestination;
    private float idleTimer;
    
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int DirXHash = Animator.StringToHash("DirX");
    private static readonly int DirYHash = Animator.StringToHash("DirY");
    
    // 缓存碰撞检测数组，避免GC
    private Collider2D[] nearbyNPCs = new Collider2D[10]; 

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        actualMoveSpeed = baseMoveSpeed * Random.Range(0.85f, 1.15f);
        InitializeNPC();
    }

    private void InitializeNPC()
    {
        FindNearestWaypointAsStart();
        
        if (currentWaypoint == null) return;

        // 注册拥挤度
        currentWaypoint.RegisterNPC();

        previousWaypoint = null;
        targetWaypoint = currentWaypoint; 
        
        // 初始移动目标
        currentDestination = currentWaypoint.GetRandomPositionInNode();
        currentDestination.z = 0; 

        EnterWalkState(false); 
    }

    private void OnDisable()
    {
        // 退出时记得注销
        if (targetWaypoint != null) targetWaypoint.UnregisterNPC();
        else if (currentWaypoint != null) currentWaypoint.UnregisterNPC();

        if (animator != null) animator.SetBool(IsWalkingHash, false);
    }

    private void Update()
    {
        switch (currentState)
        {
            case NPCState.Idle:
                HandleIdle();
                break;
            case NPCState.Walk:
                HandleWalk();
                break;
        }
    }

    private void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = (int)(-transform.position.y * 100) + sortingOrderBase;
        }
    }

    private void FindNearestWaypointAsStart()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.waypoints.Count == 0) return;

        // 简化的最近点查找
        currentWaypoint = WaypointManager.Instance.waypoints
            .OrderBy(w => Vector3.Distance(transform.position, w.position))
            .FirstOrDefault()?.GetComponent<Waypoint>();
    }

    private void EnterIdleState()
    {
        currentState = NPCState.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        if (animator != null) animator.SetBool(IsWalkingHash, false);
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        
        // 发呆的时候也要应用分离力，防止叠在一起
        ApplySeparationForce();

        if (idleTimer <= 0)
        {
            EnterWalkState(true);
        }
    }

    private void EnterWalkState(bool findNewDestination)
    {
        if (currentWaypoint == null) return;

        if (findNewDestination)
        {
            // [宏观均匀] 智能选择人少的路口
            Waypoint nextPoint = SelectNextWaypointBalanced();

            if (nextPoint != null)
            {
                // 注销旧的
                if (targetWaypoint != null) targetWaypoint.UnregisterNPC();
                else if (currentWaypoint != null) currentWaypoint.UnregisterNPC();

                // 更新引用
                previousWaypoint = currentWaypoint; 
                currentWaypoint = null; // 在路上时，暂时不归属任何点，或者归属 target
                targetWaypoint = nextPoint;         
                
                // 注册新的
                targetWaypoint.RegisterNPC();

                currentDestination = nextPoint.GetRandomPositionInNode();
                currentDestination.z = 0;
            }
            else
            {
                EnterIdleState(); // 死胡同
                return;
            }
        }

        currentState = NPCState.Walk;
        if (animator != null) animator.SetBool(IsWalkingHash, true);
    }

    // [核心算法 1] 负载均衡选点
    private Waypoint SelectNextWaypointBalanced()
    {
        if (currentWaypoint == null || currentWaypoint.neighbors == null) return null;
        var neighbors = currentWaypoint.neighbors;
        if (neighbors.Count == 0) return null;

        // 只有一条路，没得选
        if (neighbors.Count == 1) return neighbors[0];

        // 排除掉头路 (除非是死胡同)
        List<Waypoint> candidates = new List<Waypoint>(neighbors);
        if (previousWaypoint != null && candidates.Contains(previousWaypoint))
        {
            candidates.Remove(previousWaypoint);
        }
        
        // 如果剔除完没路了（比如到了只有两条路的尽头），那只能掉头
        if (candidates.Count == 0) return previousWaypoint;

        // --- 核心：加权随机 ---
        // 拥挤度越低，权重越高
        float totalWeight = 0f;
        float[] weights = new float[candidates.Count];

        for (int i = 0; i < candidates.Count; i++)
        {
            // 权重公式： 1 / (当前人数 + 1)
            // 人数 0 -> 权重 1.0
            // 人数 4 -> 权重 0.2
            // 这样人少的地方会有极大的概率被选中，但人多的地方也有一点点机会
            float w = 1.0f / (candidates[i].currentOccupancy + 1.0f);
            
            // 可以稍微加一点随机扰动，防止所有 NPC 在同一帧做出完全一样的决定
            w *= Random.Range(0.9f, 1.1f);

            weights[i] = w;
            totalWeight += w;
        }

        // 轮盘赌算法选择
        float randomValue = Random.Range(0, totalWeight);
        float currentSum = 0;

        for (int i = 0; i < candidates.Count; i++)
        {
            currentSum += weights[i];
            if (randomValue <= currentSum)
            {
                return candidates[i];
            }
        }

        // 兜底返回第一个
        return candidates[0];
    }

    private void HandleWalk()
    {
        if (targetWaypoint == null)
        {
            EnterIdleState();
            return;
        }

        Vector3 moveDir = (currentDestination - transform.position).normalized;

        // [核心算法 2] 叠加分离力 (微观均匀)
        Vector3 separationVec = CalculateSeparationVector();
        
        // 最终合力方向 = 目标方向 + 分离方向
        // normalized 确保速度恒定
        Vector3 finalDir = (moveDir + separationVec).normalized;

        // 处理动画
        if (animator != null)
        {
            animator.SetFloat(DirXHash, finalDir.x);
            animator.SetFloat(DirYHash, finalDir.y);
        }

        // 移动
        transform.position += finalDir * actualMoveSpeed * Time.deltaTime;

        // 检查到达
        if (Vector3.Distance(transform.position, currentDestination) < 0.2f) // 稍微放宽一点，因为分离力可能让你对不准圆心
        {
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;
            EnterIdleState();
        }
    }
    
    // 如果在 Idle 状态也想被推开，调用这个
    private void ApplySeparationForce()
    {
        Vector3 separationVec = CalculateSeparationVector();
        if (separationVec.sqrMagnitude > 0.01f)
        {
            // 发呆时被推开的速度稍微慢一点
            transform.position += separationVec * (actualMoveSpeed * 0.5f) * Time.deltaTime;
        }
    }

    // 计算分离向量 (Boids Separation)
    private Vector3 CalculateSeparationVector()
    {
        Vector3 separation = Vector3.zero;
        int count = Physics2D.OverlapCircleNonAlloc(transform.position, separationRadius, nearbyNPCs);

        for (int i = 0; i < count; i++)
        {
            Collider2D other = nearbyNPCs[i];
            if (other.gameObject == this.gameObject) continue;

            // 只检测 NPC (假设 NPC 都有 NPCRoam 脚本，或者通过 Layer 过滤)
            // 这里你可以通过 LayerMask 优化，避免检测到建筑
            
            Vector3 diff = transform.position - other.transform.position;
            float dist = diff.magnitude;

            if (dist > 0.01f) // 防止除以0
            {
                // 距离越近，推力越大
                float force = (1.0f / dist) * separationForce;
                separation += diff.normalized * force;
            }
        }
        
        // 限制最大推力，防止瞬移
        return Vector3.ClampMagnitude(separation, 2.0f);
    }
}