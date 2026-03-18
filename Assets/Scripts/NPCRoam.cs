using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq; // 用于排序找最近初始点

public class NPCRoam : MonoBehaviour
{
    private enum NPCState
    {
        Idle, // 发呆
        Walk  // 移动
    }

    [Header("移动参数")]
    public float moveSpeed = 2.0f;
    public float minIdleTime = 0.5f;
    public float maxIdleTime = 2.0f;
    
    // [新增] 渲染层级基数
    [Header("渲染设置")]
    [Tooltip("排序层级基数，防止Y轴为正时层级过低被地面遮挡。建议设为 5000 或更大。")]
    public int sortingOrderBase = 5000; 

    private NPCState currentState = NPCState.Idle;
    
    // 逻辑上的目标路点（用于获取下一个邻居）
    private Waypoint currentWaypoint;
    private Waypoint targetWaypoint;

    // 实际移动的物理坐标目标
    private Vector3 currentDestination;

    private float idleTimer;

    // [新增] 动画组件引用
    private Animator animator;
    // [新增] 渲染组件引用，用于排序
    private SpriteRenderer spriteRenderer;

    // 缓存 Animator 参数的哈希值，能微弱提升性能
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int DirXHash = Animator.StringToHash("DirX");
    private static readonly int DirYHash = Animator.StringToHash("DirY");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        // 获取 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // 每次 SetActive(true) 都会触发 OnEnable
    private void OnEnable()
    {
        InitializeNPC();
    }

    private void InitializeNPC()
    {
        // 1. 找到最近的路点（锚点）
        FindNearestWaypointAsStart();
        
        if (currentWaypoint == null) return;

        // [Bug 修复核心]
        // 刚生成的 NPC 物理位置可能在路段中间。
        // 如果直接寻找 Neighbor，会导致 NPC 走斜线切角穿墙。
        // 解决方案：强制将第一个目标设为“最近的路点（锚点）本身”。
        // 效果：NPC 会先沿着道路走回最近的路口，到达路口后再进行正常的寻路逻辑。
        
        targetWaypoint = currentWaypoint; 

        // 设定移动坐标为该路口的随机点
        currentDestination = currentWaypoint.GetRandomPositionInNode();
        currentDestination.z = transform.position.z;

        // 2. 跳过出生后的 Idle，直接进入 Walk 状态去“归位”
        // 这样玩家看到的是 NPC 一出生就在走路，而不是生出来站着发呆
        currentState = NPCState.Walk;
        
        // 记得同步动画状态
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, true);
        }
    }

    // [新增] 在 OnDisable 时重置状态，防止对象池复用时状态错误
    private void OnDisable()
    {
        if (animator != null)
        {
            // 重置为非行走状态
            animator.SetBool(IsWalkingHash, false);
        }
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

    // [新增] 在每一帧渲染前，根据 Y 轴高度动态调整层级
    private void LateUpdate()
    {
        if (spriteRenderer != null)
        {
            // [修改] 加上 sortingOrderBase (5000)
            // 举例：Y = 10 -> -1000 + 5000 = 4000 (大于地面 0，显示正常)
            // 举例：Y = -10 -> 1000 + 5000 = 6000 (挡住上面的 4000，遮挡关系正确)
            spriteRenderer.sortingOrder = (int)(-transform.position.y * 100) + sortingOrderBase;
        }
    }

    private void FindNearestWaypointAsStart()
    {
        if (WaypointManager.Instance == null || WaypointManager.Instance.waypoints.Count == 0) return;

        List<Transform> allWaypoints = WaypointManager.Instance.waypoints;
        Camera cam = Camera.main;
        if (cam == null) return;

        // 准备三个候选列表，优先级递减
        Transform bestTarget = null;
        float minDistance = float.MaxValue;

        // 我们把所有候选点分成三个优先级：
        // 1级：终点就在屏幕里 (OnScreen)
        // 2级：虽然终点在外面，但走过去会穿过屏幕 (Crossing)
        // 3级：完全在屏幕外 (OffScreen)
        
        int bestPriority = -1; // 3=OnScreen, 2=Crossing, 1=OffScrenn

        Vector3 myPos = transform.position;
        // 获取我的视口坐标 (0~1)
        Vector3 myViewportPos = cam.WorldToViewportPoint(myPos); 
        myViewportPos.z = 0; // 忽略深度

        foreach (Transform wp in allWaypoints)
        {
            if (wp == null) continue;

            float dist = Vector3.Distance(myPos, wp.position);
            
            // 视口坐标检查
            Vector3 wpViewportPos = cam.WorldToViewportPoint(wp.position);
            wpViewportPos.z = 0;

            bool isTargetOnScreen = IsPointInViewport(wpViewportPos);
            bool isPathCrossingScreen = false;

            // 如果终点不在屏幕内，我们检测一下路径是否穿过屏幕
            // 简单的采样检测：取路径上的 25%, 50%, 75% 处，看是否有一点在屏幕内
            if (!isTargetOnScreen)
            {
                // 只有当我和目标分别在屏幕两侧（或者我是刚出屏幕及目标在另一头）时，才大概率穿越
                // 这里用简单的插值采样，性能足够且有效
                for (float t = 0.2f; t <= 0.8f; t += 0.2f)
                {
                    Vector3 samplePos = Vector3.Lerp(myViewportPos, wpViewportPos, t);
                    if (IsPointInViewport(samplePos))
                    {
                        isPathCrossingScreen = true;
                        break;
                    }
                }
            }

            // 评级
            int priority = 1; // 默认三级优先级
            if (isTargetOnScreen) priority = 3; // 一级优先级
            else if (isPathCrossingScreen) priority = 2; // 二级优先级

            // 择优逻辑：
            // 1. 如果发现更高优先级的类别，立刻强制切换到该类别，并重置最小距离
            // 2. 如果优先级相同，选距离最近的
            if (priority > bestPriority)
            {
                bestPriority = priority;
                bestTarget = wp;
                minDistance = dist;
            }
            else if (priority == bestPriority)
            {
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestTarget = wp;
                }
            }
        }

        if (bestTarget != null)
        {
            currentWaypoint = bestTarget.GetComponent<Waypoint>();
        }
    }

    // 辅助函数：判断视口坐标是否在屏幕内 (0-1)
    private bool IsPointInViewport(Vector3 vpPos)
    {
        return vpPos.x >= 0 && vpPos.x <= 1 && vpPos.y >= 0 && vpPos.y <= 1;
    }

    // --- 状态流转 ---

    private void EnterIdleState()
    {
        currentState = NPCState.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);

        // [核心逻辑]
        // 1. 告诉 Animator 我们停下了，切换到 Idle 状态树
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, false);

            // 2.【关键】不要把 DirX 和 DirY 归零！
            // Animator 会保留最后一次移动的 DirY 值。
            // 你的 Idle Blend Tree (1D) 会读取这个残留的 DirY：
            // 如果上次是向下走 (DirY ≈ -1)，Idle树就会播放 DownIdle。
            // 如果上次是向上走 (DirY ≈ 1)，Idle树就会播放 UpIdle。
        }
    }

    private void HandleIdle()
    {
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            EnterWalkState();
        }
    }

    private void EnterWalkState()
    {
        if (currentWaypoint == null)
        {
            FindNearestWaypointAsStart();
            if (currentWaypoint == null) return;
        }

        // --- 回归纯随机逻辑 ---
        
        Waypoint nextPoint = null;
        List<Waypoint> neighbors = currentWaypoint.neighbors;

        if (neighbors != null && neighbors.Count > 0)
        {
            // 直接在所有邻居中随机选一个，没有任何偏好
            nextPoint = neighbors[Random.Range(0, neighbors.Count)];
        }
        else
        {
            // 死胡同
            EnterIdleState(); 
            return;
        }

        // --- 目标确认 ---

        if (nextPoint != null)
        {
            targetWaypoint = nextPoint;

            // 决定具体的移动坐标
            currentDestination = nextPoint.GetRandomPositionInNode();
            currentDestination.z = transform.position.z; // 锁定 Z 轴

            if (animator != null) animator.SetBool(IsWalkingHash, true);
            currentState = NPCState.Walk;
        }
    }

    private void HandleWalk()
    {
        if (targetWaypoint == null)
        {
            EnterIdleState();
            return;
        }

        // [新增] 计算并传递方向给 Animator
        if (animator != null)
        {
            Vector3 diff = currentDestination - transform.position;

            // 只有当距离足够大时才更新方向，避免微小抖动
            if (diff.sqrMagnitude > 0.001f)
            {
                Vector3 direction = diff.normalized;

                // 将 Vector3 的 xyz 映射到 Animator 的参数
                animator.SetFloat(DirXHash, direction.x);
                animator.SetFloat(DirYHash, direction.y);
            }
        }

        // 移动向缓存的坐标
        transform.position = Vector3.MoveTowards(transform.position, currentDestination, moveSpeed * Time.deltaTime);

        // 检查到达状态
        if (Vector3.Distance(transform.position, currentDestination) < 0.1f)
        {
            // 到达目标
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;

            EnterIdleState();
        }
    }
}