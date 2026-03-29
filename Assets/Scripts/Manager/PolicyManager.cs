using System.Collections.Generic;
using UnityEngine;

public class PolicyManager : MonoBehaviour
{
    public static PolicyManager Instance { get; private set; }

    [Header("大政方针配置")]
    [Tooltip("每隔多少回合触发一次国策选择 (默认10)")]
    public int triggerInterval = 10; 
    public List<PolicyData> allPolicies = new List<PolicyData>();

    [Header("组件引用")]
    public PolicyUIManager uiManager;

    private int lastTriggerRound = 0; // 记录上次触发回合，防止同一回合重复弹窗

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
        if (uiManager == null) uiManager = FindObjectOfType<PolicyUIManager>();

        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged += OnRoundChanged;

            // [新增] 游戏刚启动时，如果回合已经是1，手动触发第一回合的检查
            if (GameManager.Instance.Rounds == 1)
            {
                OnRoundChanged(1);
            }
        }
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged -= OnRoundChanged;
        }
    }

    private void OnRoundChanged(int currentRound)
    {
        // [核心修改] 触发条件：第1回合，或者正好是 triggerInterval 的倍数 (10, 20, 30...)
        bool isFirstRound = (currentRound == 1);
        bool isIntervalRound = (currentRound > 0 && currentRound % triggerInterval == 0);

        if ((isFirstRound || isIntervalRound) && currentRound != lastTriggerRound)
        {
            lastTriggerRound = currentRound;
            TriggerPolicySelection();
        }
    }

    [ContextMenu("测试触发国策")]
    public void TriggerPolicySelection()
    {
        if (allPolicies.Count < 3)
        {
            Debug.LogWarning("[PolicyManager] 国策总池不足 3 个！无法抽取。");
            return;
        }

        // 洗牌算法抽取 3 个互不相同的不重复国策
        List<PolicyData> poolCopy = new List<PolicyData>(allPolicies);
        List<PolicyData> choices = new List<PolicyData>();

        for (int i = 0; i < 3; i++)
        {
            int r = Random.Range(0, poolCopy.Count);
            choices.Add(poolCopy[r]);
            poolCopy.RemoveAt(r); // 保证不重复
        }

        uiManager.ShowPolicies(choices);
    }

    public void ConfirmPolicy(PolicyData selectedPolicy)
    {
        Debug.Log($"<color=orange>[国策颁布] 玩家选择了：{selectedPolicy.policyName}</color>");

        if (BuffManager.Instance != null && selectedPolicy.policyBuff != null)
        {
            GlobalBuff newBuff = selectedPolicy.policyBuff.Clone();
            
            // 计算这个 Buff 该持续多久
            // 如果是第1回合颁布，它需要撑到第10回合，也就是持续 9 回合。
            // 之后每次间隔都是 triggerInterval (10 回合)。
            int currentRound = GameManager.Instance.Rounds;
            newBuff.remainRounds = (currentRound == 1) ? (triggerInterval - 1) : triggerInterval; 
            
            BuffManager.Instance.AddBuff(newBuff);
        }

        uiManager.HidePanel();
    }
}