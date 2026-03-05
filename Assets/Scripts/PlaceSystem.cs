using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceSystem : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject buildingPrefab; // 需要放置的建筑预制体

    void Update()
    {


        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // 确保z轴为0
        Vector2Int gridPos = gridManager.WorldToIsoGrid(worldPos);
        Vector3 placePos = gridManager.IsoGridToWorld(gridPos.x, gridPos.y);

        buildingPrefab.transform.position = placePos; // 将建筑预制体移动到放置位置

    }
}
