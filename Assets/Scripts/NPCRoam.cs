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
    public float minIdleTime = 1.0f;
    public float maxIdleTime = 3.0f;

    private NPCState currentState = NPCState.Idle;
    private Waypoint currentWaypoint; // 当前所在的路点（或者上一个经过的路点）
    private Waypoint targetWaypoint;  // 正在前往的路点
    private float idleTimer;

    private void Start()
    {
        // 初始化：先找到场景里离自己最近的一个路点，假装自己刚从那里过来
        FindNearestWaypointAsStart();
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
        // 在场景中找所有的 Waypoint 组件（注意效率，仅在 Start 用一次）
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        
        if (allWaypoints.Length > 0)
        {
            // 使用 Linq 简便地按距离排序取最近的一个
            currentWaypoint = allWaypoints.OrderBy(w => Vector3.Distance(transform.position, w.transform.position)).First();
            // 瞬间吸附过去，或者只是将其标记为当前点均可，这里只标记逻辑归属
        }
        else
        {
            Debug.LogError("场景里没有 Waypoint 组件！无法初始化 NPC。");
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
            FindNearestWaypointAsStart(); // 尝试救援
            if (currentWaypoint == null) return;
        }

        // 核心改动：不再向 Manager 要随机点，而是向当前点要邻居
        Waypoint nextPoint = currentWaypoint.GetRandomNeighbor();

        if (nextPoint != null)
        {
            targetWaypoint = nextPoint;
            currentState = NPCState.Walk;
        }
        else
        {
            // 如果当前点没有邻居（死胡同），继续呆着或者报错
            // Debug.LogWarning($"路点 {currentWaypoint.name} 没有连接邻居！");
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

        Vector3 targetPos = targetWaypoint.transform.position;
        targetPos.z = transform.position.z; // 保持 Z 轴一致

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            // 到达目标
            // 更新逻辑状态：我现在到达了 target，它变成了我的 current
            currentWaypoint = targetWaypoint;
            targetWaypoint = null;
            
            EnterIdleState();
        }
    }
}