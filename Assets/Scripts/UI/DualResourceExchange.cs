using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DualResourceExchange : MonoBehaviour
{
    [Header("UI 引用")]
    public TMP_InputField inputCoin;
    public TMP_InputField inputManpower;
    public TextMeshProUGUI txtExpectedProsperity;
    public Button btnExchange;

    [Header("兑换比例设置")]
    [Tooltip("1个铜钱等于多少繁荣度")]
    public float coinToProsperityRate = 1.0f; 
    [Tooltip("1个人力等于多少繁荣度")]
    public float manpowerToProsperityRate = 1.0f;

    // 内部记录当前的输入值
    private int currentCoinInput = 0;
    private int currentManpowerInput = 0;
    private int totalProsperityGain = 0;

    void Start()
    {
        // 监听两个输入框的变动
        inputCoin.onValueChanged.AddListener(UpdateProsperityPreview);
        inputManpower.onValueChanged.AddListener(UpdateProsperityPreview);
        
        // 监听兑换按钮
        btnExchange.onClick.AddListener(OnExchangeClicked);

        // 初始化显示
        UpdateProsperityPreview("");
    }

    // 无论哪个输入框变动，都调用这个方法重新计算总和
    private void UpdateProsperityPreview(string dummyInput)
    {
        // 1. 解析铜钱输入（如果为空或乱码，默认就是0）
        int.TryParse(inputCoin.text, out currentCoinInput);
        currentCoinInput = Mathf.Max(0, currentCoinInput);
        
        // 2. 解析人力输入
        int.TryParse(inputManpower.text, out currentManpowerInput);
        currentManpowerInput = Mathf.Max(0, currentManpowerInput);

        // 3. 计算总繁荣度 (这里向下取整，你也可以用 Mathf.RoundToInt 四舍五入)
        totalProsperityGain = Mathf.FloorToInt(
            (currentCoinInput * coinToProsperityRate) + 
            (currentManpowerInput * manpowerToProsperityRate)
        );

        // 4. 更新 UI 文本
        txtExpectedProsperity.text = $"预计繁荣度: +{totalProsperityGain}";
    }

    // 点击兑换按钮执行逻辑
    private void OnExchangeClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("找不到 GameManager，无法执行兑换！");
            return;
        }

        if (currentCoinInput <= 0 && currentManpowerInput <= 0)
        {
            Debug.LogWarning("请输入要兑换的资源数量！");
            return;
        }

        // manpower 在 GameManager 中对应 Population，使用原子扣费避免部分扣除。
        bool spent = GameManager.Instance.TrySpendResources(currentCoinInput, currentManpowerInput, 0);
        if (!spent)
        {
            Debug.LogError("资源不足，无法兑换！");
            return;
        }

        if (totalProsperityGain > 0)
        {
            GameManager.Instance.AddProsperity(totalProsperityGain);
        }

        Debug.Log($"兑换成功！耗费 {currentCoinInput}铜, {currentManpowerInput}人力，获得 {totalProsperityGain}繁荣度。");

        // 兑换成功后清空输入框并刷新预览
        inputCoin.text = "";
        inputManpower.text = "";
        UpdateProsperityPreview("");
    }
}