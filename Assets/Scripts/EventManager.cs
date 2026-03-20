using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("事件配置")]
    [Tooltip("所有可能发生的随机事件列表")]
    public List<GameEvent> eventPool = new List<GameEvent>();

    [Header("模块引用")]
    [SerializeField] private EventUIManager uiManager;

    private void Awake()
    {
        // 单例模式初始化
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 自动查找 UI 管理器（如果在 Inspector 中未赋值）
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<EventUIManager>();
            if (uiManager == null)
            {
                Debug.LogError("EventManager: 场景中找不到 EventUIManager！");
            }
        }
    }

    /// <summary>
    /// 触发一个随机事件
    /// </summary>
    [ContextMenu("Test Random Event")] // 允许在编辑器中右键测试
    public void TriggerRandomEvent()
    {
        if (eventPool == null || eventPool.Count == 0)
        {
            Debug.LogWarning("EventManager: 事件池为空，无法触发事件。");
            return;
        }

        if (uiManager == null)
        {
            Debug.LogError("EventManager: UI Manager 未连接，无法显示事件。");
            return;
        }

        // 随机选取一个事件
        int randomIndex = Random.Range(0, eventPool.Count);
        GameEvent selectedEvent = eventPool[randomIndex];

        // 通知 UI 层显示
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

        // --- 预留接口：在此处对接 GameManager ---
        Debug.Log($"<color=cyan>[事件结算] 玩家选择了: {option.optionText}</color>");
        
        // 打印数值变化
        Debug.Log($"\t金币变化: {option.goldChange}");
        Debug.Log($"\t人口变化: {option.populationChange}");
        Debug.Log($"\t满意度变化: {option.satisfactionChange}");

        // 对接 GameManager 的示例代码 (根据你的 GameManager 上下文)
        if (GameManager.HasInstance)
        {
            // 处理金币
            if (option.goldChange > 0)
                GameManager.Instance.AddCoins(option.goldChange);
            else if (option.goldChange < 0)
                GameManager.Instance.TrySpendCoins(-option.goldChange);

            // 处理人口
            int newPop = GameManager.Instance.Population + option.populationChange;
            GameManager.Instance.SetPopulation(newPop);

            // 处理民怨 (假设满意度增加 = 民怨减少)
            int newAnger = GameManager.Instance.PersonAnger - option.satisfactionChange;
            GameManager.Instance.SetPersonAnger(newAnger);
        }
    }
}