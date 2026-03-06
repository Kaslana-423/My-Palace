using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour
{
    [Header("连接的邻居（单向）")]
    public List<Waypoint> neighbors = new List<Waypoint>();

    [Header("调试显示")]
    public float gizmoRadius = 0.3f;
    public Color pointColor = Color.cyan;
    public Color lineColor = Color.green;

    public Waypoint GetRandomNeighbor()
    {
        if (neighbors == null || neighbors.Count == 0) return null;
        return neighbors[Random.Range(0, neighbors.Count)];
    }

    // --- 可视化改进：画出方向 ---
    private void OnDrawGizmos()
    {
        Gizmos.color = pointColor;
        Gizmos.DrawSphere(transform.position, gizmoRadius);

        Gizmos.color = lineColor;
        if (neighbors != null)
        {
            foreach (Waypoint neighbor in neighbors)
            {
                if (neighbor != null)
                {
                    // 技巧：只画 90% 的长度，留出空隙
                    // 如果是双向连接，另一头回来的线会补上个空隙
                    // 如果你看到线条断在终点前，说明那是单行道！
                    Vector3 direction = neighbor.transform.position - transform.position;
                    Vector3 endPos = transform.position + direction * 0.9f; 
                    
                    Gizmos.DrawLine(transform.position, endPos);
                    
                    // 可选：画个小箭头头（示意）
                    // Gizmos.DrawWireSphere(endPos, 0.1f); 
                }
            }
        }
    }

    // --- 新功能：右键点击组件，选择 "Make Links Bidirectional" ---
    // 这个功能会自动检查所有邻居，强行把“我”加到“邻居”的列表里，实现双向通行
    [ContextMenu("Make Links Bidirectional (双向连接)")]
    public void MakeLinksBidirectional()
    {
        int count = 0;
        foreach (Waypoint neighbor in neighbors)
        {
            if (neighbor != null)
            {
                // 如果邻居的列表里没有我，就把我加进去
                if (!neighbor.neighbors.Contains(this))
                {
                    neighbor.neighbors.Add(this);
                    Debug.Log($"已修复连接: {neighbor.name} -> {this.name}");
                    count++;
                }
            }
        }
        Debug.Log($"操作完成，新增了 {count} 条反向连接。");
    }
}