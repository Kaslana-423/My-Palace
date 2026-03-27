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
            if (uiManager == null)
            {
                Debug.LogError("[EventManager] 场景中找不到 EventUIManager！");
            }
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
        if (eventPool == null || eventPool.Count == 0)
        {
            Debug.LogWarning("[EventManager] 事件池为空！");
            return;
        }
        if (uiManager == null) return;

        int randomIndex = Random.Range(0, eventPool.Count);
        GameEvent selectedEvent = eventPool[randomIndex];

        uiManager.ShowEvent(selectedEvent);
        // Debug.Log($"触发事件: {selectedEvent.eventTitle}");
    }

    /// <summary>
    /// 结算选项带来的影响 (核心对接点)
    /// </summary>
    public void ResolveOption(EventOption option)
    {
        if (option == null) return;
        if (!GameManager.HasInstance)
        {
            Debug.LogError("[EventManager] 找不到 GameManager，无法结算资源！");
            return;
        }

        Debug.Log($"<color=cyan>[事件结算] 玩家选择: {option.optionText}</color>");

        // 1. 金币 (Coins)
        if (option.coinChange > 0)
            GameManager.Instance.AddCoins(option.coinChange);
        else if (option.coinChange < 0)
            GameManager.Instance.TrySpendCoins(-option.coinChange); // 传入正数扣除

        // 2. 建材 (Materials)
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

        // 4. 繁荣度 (Prosperity)
        if (option.prosperityChange != 0)
        {
            // GameManager 如果用的是 AddProsperity：
            GameManager.Instance.AddProsperity(option.prosperityChange);
            
            // 如果 GameManager 里只有 SetProsperity，则用下面这行：
            // int newProsperity = GameManager.Instance.Prosperity + option.prosperityChange;
            // GameManager.Instance.SetProsperity(newProsperity);
        }

        // 5. 民怨 (Person Anger) [已对齐]
        if (option.personAngerChange != 0)
        {
            int newAnger = GameManager.Instance.PersonAnger + option.personAngerChange;
            GameManager.Instance.SetPersonAnger(newAnger);
        }
    }
}