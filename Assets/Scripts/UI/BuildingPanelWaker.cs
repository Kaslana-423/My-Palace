using UnityEngine;

public class BuildingPanelWaker : MonoBehaviour
{
    public GameObject panelRoot;

    private void Start()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.OnBuildingSelected += (entity) => panelRoot.SetActive(true);
            BuildingInteractManager.Instance.OnSelectionCleared += () => panelRoot.SetActive(false);
        }

        if (panelRoot != null) panelRoot.SetActive(false);
    }
}