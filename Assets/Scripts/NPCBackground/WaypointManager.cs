using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 路点管理器：只负责提供“所有路点在哪里”，不再负责画图
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [Header("设置")]
    public List<Transform> waypoints = new List<Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // 自动初始化路点列表
        if (waypoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                waypoints.Add(child);
            }
        }
    }

    // 获取随机点（保留用于旧逻辑）
    public Transform GetRandomWaypoint()
    {
        if (waypoints.Count == 0) return null;
        return waypoints[Random.Range(0, waypoints.Count)];
    }

    /// <summary>
    /// [性能优化] 获取距离指定位置最近的路点。
    /// 避免使用 LINQ 以减少运行时垃圾回收（GC）。
    /// </summary>
    public Transform GetNearestWaypoint(Vector3 position)
    {
        if (waypoints.Count == 0) return null;

        Transform nearest = null;
        float minDistSq = float.MaxValue; // 使用距离平方比较，省去开平方根的性能消耗

        foreach (Transform wp in waypoints)
        {
            if (wp == null) continue;
            
            // 计算距离平方
            float distSq = (wp.position - position).sqrMagnitude;
            if (distSq < minDistSq)
            {
                minDistSq = distSq;
                nearest = wp;
            }
        }
        
        return nearest;
    }
}