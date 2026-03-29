using UnityEngine;
using UnityEngine.EventSystems;
public class PalaceManager : MonoBehaviour
{
    public static PalaceManager Instance { get; private set; }

    public BuildingEntity[] palaceStages;
    public int currentStageIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < palaceStages.Length; i++)
        {
            if (palaceStages[i] != null)
            {
                palaceStages[i].gameObject.SetActive(i == 0);
            }
        }
    }

    public bool TryUpgradeCoreBuilding(BuildingEntity currentCore)
    {
        if (currentStageIndex >= palaceStages.Length - 1) return false;

        BuildingSO nextData = currentCore.data.nextLevelSO;
        if (nextData == null) return false;

        if (GameManager.Instance.Prosperity < nextData.requiredProsperity)
        {
            Debug.LogWarning($"[拦截] 繁荣度未达标！需要: {nextData.requiredProsperity}, 当前: {GameManager.Instance.Prosperity}");
            return false;
        }

        if (GameManager.Instance.TrySpendResources(nextData.costCoins, nextData.costPopulation, nextData.costMaterial))
        {
            Vector3Int cellPos = new Vector3Int(currentCore.gridX, currentCore.gridY, 0);

            BuildManager.Instance.activeBuildings.Remove(currentCore);

            palaceStages[currentStageIndex].gameObject.SetActive(false);
            currentStageIndex++;
            BuildingEntity nextCore = palaceStages[currentStageIndex];
            nextCore.gameObject.SetActive(true);

            nextCore.gridX = currentCore.gridX;
            nextCore.gridY = currentCore.gridY;
            nextCore.data = nextData;

            BuildManager.Instance.activeBuildings.Add(nextCore);
            BuildManager.Instance.RegisterBuilding(cellPos, nextCore);

            return true;
        }
        return false;
    }
}