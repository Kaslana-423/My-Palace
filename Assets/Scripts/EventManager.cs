using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("事件配置")]
    public List<GameEvent> eventPool = new List<GameEvent>();

    [Header("模块引用")]
    [SerializeField] private EventUIManager uiManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<EventUIManager>();
        }
    }

    private void Update()
    {
        // 方便测试：按 T 键触发
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerRandomEvent();
        }
    }

    /// <summary>
    /// 触发一个随机事件
    /// </summary>
    [ContextMenu("Test Random Event")]
    public void TriggerRandomEvent()
    {
        if (eventPool == null || eventPool.Count == 0) return;
        if (uiManager == null) return;

        int randomIndex = Random.Range(0, eventPool.Count);
        GameEvent selectedEvent = eventPool[randomIndex];

        uiManager.ShowEvent(selectedEvent);
        Debug.Log($"触发事件: {selectedEvent.eventTitle}");
    }

    /// <summary>
    /// 结算选项带来的影响
    /// </summary>
    public void ResolveOption(EventOption option)
    {
        if (option == null) return;
        if (!GameManager.HasInstance) return;

        Debug.Log($"<color=cyan>[事件结算] 玩家选择: {option.optionText}</color>");

        // 1. 金币 (Coins)
        if (option.coinChange > 0)
            GameManager.Instance.AddCoins(option.coinChange);
        else if (option.coinChange < 0)
            GameManager.Instance.TrySpendCoins(-option.coinChange); // 传入正数进行扣除

        // 2. 建材 (Materials) [新增]
        if (option.materialChange > 0)
            GameManager.Instance.AddMaterials(option.materialChange);
        else if (option.materialChange < 0)
            GameManager.Instance.TrySpendMaterials(-option.materialChange);

        // 3. 人口 (Population)
        if (option.populationChange != 0)
        {
            int newPop = GameManager.Instance.Population + option.populationChange;
            GameManager.Instance.SetPopulation(newPop);
        }

        // 4. 满意度 (Satisfaction)
        if (option.satisfactionChange != 0)
        {
            int newSatisfaction = GameManager.Instance.Satisfaction + option.satisfactionChange;
            GameManager.Instance.SetSatisfaction(newSatisfaction);
        }

        // 5. 繁荣度 (Prosperity) [新增]
        if (option.prosperityChange != 0)
        {
            // GameManager 已有 AddProsperity 方法，它内部调用了 SetProsperity(current + amount)
            // 所以直接传入正负数即可 (e.g. AddProsperity(-5) 会减少)
            GameManager.Instance.AddProsperity(option.prosperityChange);
        }
    }
}