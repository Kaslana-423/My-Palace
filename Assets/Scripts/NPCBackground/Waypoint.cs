using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("连接的邻居")]
    public List<Waypoint> neighbors = new List<Waypoint>();

    [Header("街道参数")]
    public float radius = 2.0f; 
    public float roadWidth = 2.0f;

    [Header("流量统计")]
    // [新增] 当前有多少个 NPC 正在前往或停留在这个路点
    public int currentOccupancy = 0;

    [Header("调试显示")]
    public Color pointColor = new Color(0, 1, 1, 1f); 
    public Color roadColor = new Color(0, 1, 0, 0.3f);  
    public bool showGizmos = true;

    // [新增] 注册/注销方法
    public void RegisterNPC()
    {
        currentOccupancy++;
    }

    public void UnregisterNPC()
    {
        currentOccupancy--;
        if (currentOccupancy < 0) currentOccupancy = 0;
    }

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
        Vector2 randomCircle = Random.insideUnitCircle * radius;
        return transform.position + new Vector3(randomCircle.x, randomCircle.y, 0);
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // 根据拥挤程度改变颜色：人越多越红，人越少越青
        float crowdFactor = Mathf.Clamp01(currentOccupancy / 5.0f);
        Gizmos.color = Color.Lerp(pointColor, Color.red, crowdFactor);
        
        // [修改] 1. 画路口节点区域（使用空心圆圈，不再遮挡视野）
        Gizmos.DrawWireSphere(transform.position, radius);

        // 2. 画连接道路区域（矩形条带）
        Gizmos.color = roadColor;
        if (neighbors != null)
        {
            foreach (Waypoint neighbor in neighbors)
            {
                if (neighbor != null) DrawRoadSegment(neighbor);
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
            if (neighbor != null && !neighbor.neighbors.Contains(this))
            {
                neighbor.neighbors.Add(this);
                count++;
            }
        }
    }
}