using UnityEngine;

public class BuildingPanelWaker : MonoBehaviour
{
    public GameObject panelRoot;
    public GameObject CorePanel;

    private void Start()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.OnBuildingSelected += (entity) => panelRoot.SetActive(true);
            BuildingInteractManager.Instance.OnSelectionCleared += () => panelRoot.SetActive(false);
            BuildingInteractManager.Instance.OnCoreBuildingSelected += (entity) => CorePanel.SetActive(true);
            BuildingInteractManager.Instance.OnSelectionCleared += () => CorePanel.SetActive(false);
        }

        if (panelRoot != null) panelRoot.SetActive(false);
        if (CorePanel != null) CorePanel.SetActive(false);
    }
}