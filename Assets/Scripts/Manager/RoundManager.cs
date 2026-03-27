using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    private readonly int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private readonly int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ExecuteNextRound();
        }
    }

    public void ExecuteNextRound()
    {
        if (!GameManager.HasInstance || BuildManager.Instance == null) return;

        int roundCoins = 0;
        int roundMaterials = 0;
        int roundPopulation = 0;
        int roundProsperity = 0;
        int roundAnger = 0;

        foreach (var entity in BuildManager.Instance.activeBuildings)
        {
            if (entity == null || entity.data == null) continue;

            BuildingSO data = entity.data;

            roundCoins += data.outputCoins;
            roundMaterials += data.outputMaterial;
            roundPopulation += data.outputPopulation;
            roundProsperity += data.outputProsperity;
            roundAnger += data.outputPersonAnger;

            if (data.isHarvester)
            {
                int sumLevel = 0;
                for (int i = 0; i < 8; i++)
                {
                    Vector3Int neighborPos = new Vector3Int(entity.gridX + dx[i], entity.gridY + dy[i], 0);

                    if (BuildManager.Instance.buildingGridMap.TryGetValue(neighborPos, out BuildingEntity neighbor))
                    {
                        if (neighbor.data != null && !neighbor.data.isHarvester)
                        {
                            sumLevel += neighbor.data.level;
                        }
                    }
                }
                roundCoins += sumLevel * data.harvestMultiplier;
            }
        }

        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ProcessGlobalYields(ref roundCoins, ref roundMaterials, ref roundPopulation, ref roundProsperity, ref roundAnger);
        }

        GameManager.Instance.AddCoins(roundCoins);
        GameManager.Instance.AddMaterials(roundMaterials);
        GameManager.Instance.AddPopulation(roundPopulation);
        GameManager.Instance.AddProsperity(roundProsperity);
        GameManager.Instance.AddPersonAnger(roundAnger);

        int nextRound = GameManager.Instance.Rounds + 1;
        GameManager.Instance.SetRounds(nextRound);
    }
}