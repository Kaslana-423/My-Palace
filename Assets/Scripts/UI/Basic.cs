using TMPro;
using UnityEngine;

public class Basic : MonoBehaviour
{
    [Header("资源 UI 节点 (在面板里手动拖拽，严禁改名导致断线)")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI materialText;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI prosperityText;
    public TextMeshProUGUI personAngerText;
    public TextMeshProUGUI roundsText;

    // 用 Start 保证绝对在 GameManager 的 Awake 之后执行，干掉竞态条件
    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[Basic UI] 找不到 GameManager 单例！");
            return;
        }

        // 1. 订阅事件
        GameManager.Instance.CoinsChanged += RefreshCoin;
        GameManager.Instance.MaterialsChanged += RefreshMaterial;
        GameManager.Instance.PopulationChanged += RefreshPopulation;
        GameManager.Instance.ProsperityChanged += RefreshProsperity;
        GameManager.Instance.PersonAngerChanged += RefreshAnger;
        GameManager.Instance.RoundsChanged += RefreshRounds;

        // 2. 初始主动拉取一次数据，防止漏掉初始值
        RefreshCoin(GameManager.Instance.Coins);
        RefreshMaterial(GameManager.Instance.Materials);
        RefreshPopulation(GameManager.Instance.Population);
        RefreshProsperity(GameManager.Instance.Prosperity);
        RefreshAnger(GameManager.Instance.PersonAnger);
        RefreshRounds(GameManager.Instance.Rounds);
    }

    // 严防内存泄漏
    private void OnDestroy()
    {
        if (GameManager.Instance == null) return;
        GameManager.Instance.CoinsChanged -= RefreshCoin;
        GameManager.Instance.MaterialsChanged -= RefreshMaterial;
        GameManager.Instance.PopulationChanged -= RefreshPopulation;
        GameManager.Instance.ProsperityChanged -= RefreshProsperity;
        GameManager.Instance.PersonAngerChanged -= RefreshAnger;
        GameManager.Instance.RoundsChanged -= RefreshRounds;
    }

    private void RefreshCoin(int val) { if (coinText != null) coinText.text = val.ToString(); }
    private void RefreshMaterial(int val) { if (materialText != null) materialText.text = val.ToString(); }
    private void RefreshPopulation(int val) { if (populationText != null) populationText.text = val.ToString(); }
    private void RefreshProsperity(int val) { if (prosperityText != null) prosperityText.text = val.ToString(); }
    private void RefreshAnger(int val) { if (personAngerText != null) personAngerText.text = $"{val}%"; }
    private void RefreshRounds(int val) { if (roundsText != null) roundsText.text = $"第 {val} 轮"; }
}