using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 路点管理器，使用单例模式，方便 NPC 随时访问
public class WaypointManager : MonoBehaviour
{
    // 单例实例
    public static WaypointManager Instance { get; private set; }

    [Header("设置")]
    [Tooltip("将场景中的路点 Transform 拖到这里，或者在 Awake 中自动抓取")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("调试显示")]
    public Color gizmoColor = Color.cyan; // 路点显示的颜色
    public float gizmoRadius = 0.3f;      // 路点球体的大小

    private void Awake()
    {
        // 实现单例模式：确保场景中只有一个管理器
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        // 如果列表为空，尝试自动查找当前物体下的所有子物体作为路点
        if (waypoints.Count == 0)
        {
            foreach (Transform child in transform)
            {
                waypoints.Add(child);
            }
        }
    }

    // 获取一个随机路点
    public Transform GetRandomWaypoint()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogWarning("WaypointManager: 没有可用的路点！");
            return null;
        }

        int randomIndex = Random.Range(0, waypoints.Count);
        return waypoints[randomIndex];
    }

    // 可视化辅助线绘制
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        if (waypoints != null && waypoints.Count > 0)
        {
            foreach (Transform point in waypoints)
            {
                if (point != null) DrawWaypoint(point.position);
            }
        }
        else
        {
            // 还没运行时，直接画子物体位置
            foreach (Transform child in transform)
            {
                DrawWaypoint(child.position);
            }
        }
    }

    private void DrawWaypoint(Vector3 position)
    {
        Gizmos.DrawSphere(position, gizmoRadius);
        Gizmos.DrawWireSphere(position, gizmoRadius + 0.1f);
    }
}