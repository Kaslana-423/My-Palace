using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("事件池配置")]
    public List<GameEvent> eventPool = new List<GameEvent>();

    [Header("模块引用")]
    [SerializeField] private EventUIManager uiManager;

    [Header("回合检定参数 (Turn-based)")]
    [Tooltip("每回合基础触发概率 (0~1)")]
    [Range(0f, 1f)] public float baseTurnProbability = 0.15f;
    [Tooltip("每次未触发时，下回合增加的概率")]
    [Range(0f, 1f)] public float prdTurnIncrement = 0.05f;

    [Header("操作检定参数 (Action-based)")]
    [Tooltip("每次玩家操作(建造/拆除)的基础触发概率")]
    [Range(0f, 1f)] public float baseActionProbability = 0.05f;
    [Tooltip("每次操作未触发时增加的概率")]
    [Range(0f, 1f)] public float prdActionIncrement = 0.02f;

    [Header("冷却机制")]
    [Tooltip("触发事件后，几回合内绝对不会再触发随机事件")]
    public int globalCooldownRounds = 2;

    // --- 内部运行状态 ---
    private float currentTurnProb;
    private float currentActionProb;
    private int cooldownCounter = 0; // 大于0表示在CD中

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        
        // 初始化概率
        currentTurnProb = baseTurnProbability;
        currentActionProb = baseActionProbability;
    }

    private void Start()
    {
        if (uiManager == null) uiManager = FindObjectOfType<EventUIManager>();

        // 订阅 GameManager 的回合事件
        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged += OnRoundPassed;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged -= OnRoundPassed;
        }
    }

    // ================== 触发检定逻辑 ==================

    /// <summary>
    /// 当回合改变时自动调用（回合检定）
    /// </summary>
    private void OnRoundPassed(int currentRound)
    {
        // 1. 处理冷却
        if (cooldownCounter > 0)
        {
            cooldownCounter--;
            return; // CD中，本回合不触发
        }

        // 2. Roll 点
        float roll = Random.Range(0f, 1f);
        if (roll <= currentTurnProb)
        {
            // 命中！触发事件
            ExecuteRandomEvent();
            // 重置回合概率，并将操作概率也同时重置（防止连发）
            currentTurnProb = baseTurnProbability;
            currentActionProb = baseActionProbability;
        }
        else
        {
            // 未命中，PRD 概率补偿累加
            currentTurnProb += prdTurnIncrement;
        }
    }

    /// <summary>
    /// 供外部调用：玩家执行建造/拆除等关键操作时（操作检定）
    /// 例如：EventManager.Instance.CheckActionTrigger();
    /// </summary>
    public void CheckActionTrigger()
    {
        if (cooldownCounter > 0) return; // CD 中禁止触发

        float roll = Random.Range(0f, 1f);
        if (roll <= currentActionProb)
        {
            ExecuteRandomEvent();
            currentActionProb = baseActionProbability;
            currentTurnProb = baseTurnProbability;
        }
        else
        {
            currentActionProb += prdActionIncrement;
        }
    }

    // ================== 事件执行与抽取 ==================

    [ContextMenu("Test Force Event")] 
    public void TestForceEvent()
    {
        ExecuteRandomEvent();
    }

    /// <summary>
    /// 核心执行：按权重抽签并弹出UI
    /// </summary>
    private void ExecuteRandomEvent()
    {
        if (eventPool == null || eventPool.Count == 0) return;
        if (uiManager == null) return;

        // 进入冷却
        cooldownCounter = globalCooldownRounds;

        // 根据权重随机抽取 (Weighted Random)
        int totalWeight = 0;
        foreach (var evt in eventPool)
        {
            totalWeight += evt.weight;
        }

        int randomPoint = Random.Range(0, totalWeight);
        int currentWeight = 0;
        GameEvent selectedEvent = eventPool[0]; // 默认兜底

        foreach (var evt in eventPool)
        {
            currentWeight += evt.weight;
            if (randomPoint < currentWeight)
            {
                selectedEvent = evt;
                break;
            }
        }

        uiManager.ShowEvent(selectedEvent);
        Debug.Log($"<color=yellow>随机事件触发啦: {selectedEvent.eventTitle}</color>");
    }

    /// <summary>
    /// 玩家点击UI选项后的结算
    /// </summary>
    public void ResolveOption(EventOption option)
    {
        if (option == null || !GameManager.HasInstance) return;

        Debug.Log($"<color=cyan>[事件结算] 玩家选择: {option.optionText}</color>");

        // --- 1. 处理立即生效的一次性加减 ---
        if (option.coinChange > 0) GameManager.Instance.AddCoins(option.coinChange);
        else if (option.coinChange < 0) GameManager.Instance.TrySpendCoins(-option.coinChange); 

        if (option.materialChange > 0) GameManager.Instance.AddMaterials(option.materialChange);
        else if (option.materialChange < 0) GameManager.Instance.TrySpendMaterials(-option.materialChange);

        if (option.populationChange != 0) GameManager.Instance.SetPopulation(GameManager.Instance.Population + option.populationChange);
        
        if (option.prosperityChange != 0) GameManager.Instance.AddProsperity(option.prosperityChange);
        
        if (option.personAngerChange != 0) GameManager.Instance.SetPersonAnger(GameManager.Instance.PersonAnger + option.personAngerChange);

        // --- 2. 处理持续生效的 Buff ---
        if (option.hasBuff && option.buffResult != null && option.buffResult.remainRounds > 0)
        {
            if (BuffManager.Instance != null)
            {
                // 注意：必须调用 Clone()，否则多个同名事件会共享同一个 SO 里的回合倒计时限制
                BuffManager.Instance.AddBuff(option.buffResult.Clone());
                Debug.Log($"<color=green>[事件结算] 添加了全局 Buff，持续 {option.buffResult.remainRounds} 回合</color>");
            }
            else
            {
                Debug.LogWarning("[EventManager] 选项包含 Buff，但场景中不存在 BuffManager！");
            }
        }
    }
}