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
    public float minIdleTime = 0.5f; // 缩短一点发呆时间，让街区更热闹
    public float maxIdleTime = 2.0f;

    private NPCState currentState = NPCState.Idle;
    
    // 逻辑上的目标路点（用于获取下一个邻居）
    private Waypoint currentWaypoint; 
    private Waypoint targetWaypoint;  
    
    // [核心修改] 实际移动的物理坐标目标
    private Vector3 currentDestination; 
    
    private float idleTimer;

    // [重要] 改用 OnEnable，因为配合对象池使用时，Start 只会执行一次
    // 每次 SetActive(true) 都会触发 OnEnable
    private void OnEnable()
    {
        InitializeNPC();
    }

    private void InitializeNPC()
    {
        targetWaypoint = null;
        currentState = NPCState.Idle;
        
        // 刚出生时，先搞清楚自己属于哪个路点管辖
        FindNearestWaypointAsStart();
        
        // 出生后稍微发呆一下，或者立刻开始走
        EnterIdleState();
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

    private void FindNearestWaypointAsStart()
    {
        // 性能优化：优先使用 Manager 的快速查找，没有才用 FindObjectsOfType
        if (WaypointManager.Instance != null)
        {
            Transform nearest = WaypointManager.Instance.GetNearestWaypoint(transform.position);
            if (nearest != null)
            {
                currentWaypoint = nearest.GetComponent<Waypoint>();
            }
        }
        else
        {
            // 备用方案（较慢）
            Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
            if (allWaypoints.Length > 0)
            {
                currentWaypoint = allWaypoints.OrderBy(w => Vector3.Distance(transform.position, w.transform.position)).First();
            }
        }
    }

    // --- 状态流转 ---

    private void EnterIdleState()
    {
        currentState = NPCState.Idle;
        idleTimer = Random.Range(minIdleTime, maxIdleTime);
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

        // 1. 获取下一个逻辑路点
        Waypoint nextPoint = currentWaypoint.GetRandomNeighbor();

        if (nextPoint != null)
        {
            targetWaypoint = nextPoint;
            
            // [核心修改] 决定具体的移动坐标！
            // 不再是去圆心，而是去路口范围内的一个随机点
            currentDestination = nextPoint.GetRandomPositionInNode();
            
            // 锁定 Z 轴，防止 NPC 乱飘
            currentDestination.z = transform.position.z;

            currentState = NPCState.Walk;
        }
        else
        {
            // 死胡同，继续发呆
            EnterIdleState(); 
        }
    }

    private void HandleWalk()
    {
        if (targetWaypoint == null)
        {
            EnterIdleState();
            return;
        }

        // [核心修改] 移动向缓存的坐标，而不是 transform.position
        transform.position = Vector3.MoveTowards(transform.position, currentDestination, moveSpeed * Time.deltaTime);

        // 检查到达状态
        if (Vector3.Distance(transform.position, currentDestination) < 0.1f)
        {
            // 到达目标
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;
            
            // 到达后发呆一会
            EnterIdleState();
        }
    }
}