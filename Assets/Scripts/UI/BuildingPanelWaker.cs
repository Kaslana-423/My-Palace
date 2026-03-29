using UnityEngine;

public class BuildingPanelWaker : MonoBehaviour
{
    public GameObject panelRoot;
    public GameObject CorePanel;

    private void Start()
    {
        if (BuildingInteractManager.Instance != null)
        {
            // --- 普通民居管线 ---
            BuildingInteractManager.Instance.OnBuildingSelected += (entity) =>
            {
                if (panelRoot != null)
                {
                    panelRoot.SetActive(true);
                    var ui = panelRoot.GetComponentInChildren<BuildingPanelUI>(true);
                    if (ui != null) ui.RefreshData(entity);
                    else Debug.LogError("找不到 BuildingPanelUI 组件，检查是不是没挂载！");
                }
            };

            // --- 皇宫核心管线 ---
            BuildingInteractManager.Instance.OnCoreBuildingSelected += (entity) =>
            {
                if (CorePanel != null)
                {
                    CorePanel.SetActive(true);
                    var ui = CorePanel.GetComponentInChildren<PalacePanelUI>(true);
                    if (ui != null) ui.BindPalaceData(entity);
                    else Debug.LogError("找不到 PalacePanelUI 组件，检查是不是没挂载！");
                }
            };

            // --- 关面板管线 ---
            BuildingInteractManager.Instance.OnSelectionCleared += () =>
            {
                if (panelRoot != null) panelRoot.SetActive(false);
                if (CorePanel != null) CorePanel.SetActive(false);
            };
        }

        if (panelRoot != null) panelRoot.SetActive(false);
        if (CorePanel != null) CorePanel.SetActive(false);
    }
}