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
        
        // 方便测试
        // Invoke("TriggerRandomEvent", 1f);
    }

    // 按下 T 键测试
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            TriggerRandomEvent();
        }
    }

    /// <summary>
    /// 触发一个随机事件
    /// </summary>
    [ContextMenu("Test Random Event")] // 允许在编辑器中右键测试
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
    /// <param name="option">玩家选择的选项数据</param>
    public void ResolveOption(EventOption option)
    {
        if (option == null) return;

        Debug.Log($"<color=cyan>[事件结算] 玩家选择: {option.optionText}</color>");

        if (GameManager.HasInstance)
        {
            // [修改] 1. GoldChange -> CoinChange
            if (option.coinChange > 0)
                GameManager.Instance.AddCoins(option.coinChange);
            else if (option.coinChange < 0)
                GameManager.Instance.TrySpendCoins(-option.coinChange);

            // 2. Population
            int newPop = GameManager.Instance.Population + option.populationChange;
            GameManager.Instance.SetPopulation(newPop);

            // [修改] 3. Satisfaction
            // 之前的逻辑是：民怨 - satisfactionChange
            // 现在的逻辑是：满意度 + satisfactionChange (正加负减)
            int newSatisfaction = GameManager.Instance.Satisfaction + option.satisfactionChange;
            GameManager.Instance.SetSatisfaction(newSatisfaction);
        }
    }
}