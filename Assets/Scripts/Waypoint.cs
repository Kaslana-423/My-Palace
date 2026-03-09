using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("连接的邻居")]
    public List<Waypoint> neighbors = new List<Waypoint>();

    [Header("街道参数")]
    [Tooltip("路口节点的半径")]
    public float radius = 2.0f; 
    [Tooltip("连接道路的宽度")]
    public float roadWidth = 2.0f;

    [Header("调试显示")]
    public Color pointColor = new Color(0, 1, 1, 0.5f); // 半透明青色
    public Color roadColor = new Color(0, 1, 0, 0.3f);  // 半透明绿色
    public bool showGizmos = true;

    // 获取随机邻居
    public Waypoint GetRandomNeighbor()
    {
        if (neighbors == null || neighbors.Count == 0) return null;
        return neighbors[Random.Range(0, neighbors.Count)];
    }

    /// <summary>
    /// 获取路口范围内的一个随机点
    /// </summary>
    public Vector3 GetRandomPositionInNode()
    {
        // insideUnitCircle 返回半径为1的圆内随机点
        // 乘以 radius 将其扩大到设定的路口大小
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 1. 画路口节点区域（实心圆盘）
        Gizmos.color = pointColor;
        Gizmos.DrawSphere(transform.position, radius);

        // 2. 画连接道路区域（矩形条带）
        Gizmos.color = roadColor;
        if (neighbors != null)
        {
            foreach (Waypoint neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    DrawRoadSegment(neighbor);
                }
            }
        }
    }

    // 画一条有宽度的路（仅用于可视化，不参与逻辑）
    private void DrawRoadSegment(Waypoint neighbor)
    {
        Vector3 direction = (neighbor.transform.position - transform.position).normalized;
        Vector3 perpendicular = Vector3.Cross(direction, Vector3.forward).normalized * (roadWidth * 0.5f);

        Vector3 p1 = transform.position + perpendicular;
        Vector3 p2 = transform.position - perpendicular;
        Vector3 p3 = neighbor.transform.position - perpendicular;
        Vector3 p4 = neighbor.transform.position + perpendicular;

        // 画四条边
        Gizmos.DrawLine(p1, p2); // 起点宽
        Gizmos.DrawLine(p3, p4); // 终点宽
        Gizmos.DrawLine(p1, p4); // 侧边1
        Gizmos.DrawLine(p2, p3); // 侧边2
        
        // 中间画一条之前的绿线，保持连接关系可见
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, neighbor.transform.position);
        Gizmos.color = roadColor; // 恢复颜色
    }

    // --- 自动双向连接工具 ---
    [ContextMenu("Make Links Bidirectional (双向连接)")]
    public void MakeLinksBidirectional()
    {
        int count = 0;
        foreach (Waypoint neighbor in neighbors)
        {
            if (neighbor != null)
            {
                if (!neighbor.neighbors.Contains(this))
                {
                    neighbor.neighbors.Add(this);
                    count++;
                }
            }
        }
        Debug.Log($"操作完成，新增了 {count} 条反向连接。");
    }
}