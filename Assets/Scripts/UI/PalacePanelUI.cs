using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class PalacePanelUI : MonoBehaviour
{
    private const string CostColor = "#FF5A5A";
    private const string RefundColor = "#63D66B";

    [Header("基础信息")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingIntroText;
    public Image buildingImage;

    [Header("升级相关")]
    public Button upgradeButton;
    public TextMeshProUGUI coinCostText;
    public TextMeshProUGUI manpowerCostText;
    public TextMeshProUGUI materialCostText;
    public TextMeshProUGUI prospReqText;

    [Header("繁荣度兑换")]
    public TMP_InputField coinInputField;
    public TMP_InputField popInputField;
    public TextMeshProUGUI exchangePreviewText;
    public Button exchangeButton;

    [Header("全局增益显示")]
    public TextMeshProUGUI currentProsperityText;
    public TextMeshProUGUI productionBuffText;
    public TextMeshProUGUI angerReductionText;

    private BuildingEntity currentPalace;

    private void Awake()
    {
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
        }
        if (exchangeButton != null)
        {
            exchangeButton.onClick.RemoveAllListeners();
            exchangeButton.onClick.AddListener(OnExchangeClicked);
        }
    }

    // 绝对的数据注射口，不再靠自己监听事件瞎猜
    public void BindPalaceData(BuildingEntity entity)
    {
        currentPalace = entity;
        Debug.Log("[UI] 皇宫数据强制注入成功！");
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentPalace == null || currentPalace.data == null) return;
        BuildingSO data = currentPalace.data;

        if (buildingNameText != null) buildingNameText.text = data.buildingTitle;
        if (buildingIntroText != null) buildingIntroText.text = data.buildingIntro;
        if (buildingImage != null && data.buildingIcon != null) buildingImage.sprite = data.buildingIcon;

        BuildingSO nextData = data.nextLevelSO;
        if (nextData != null)
        {
            bool canAfford = GameManager.Instance.Coins >= nextData.costCoins &&
                             GameManager.Instance.Population >= nextData.costPopulation &&
                             GameManager.Instance.Materials >= nextData.costMaterial &&
                             GameManager.Instance.Prosperity >= nextData.requiredProsperity;

            if (upgradeButton != null) upgradeButton.interactable = canAfford;

            SetResourceText(coinCostText, nextData.costCoins, CostColor, "铜钱");
            SetResourceText(manpowerCostText, nextData.costPopulation, CostColor, "人力");
            SetResourceText(materialCostText, nextData.costMaterial, CostColor, "建材");

            string prospColor = GameManager.Instance.Prosperity >= nextData.requiredProsperity ? RefundColor : CostColor;
            SetResourceText(prospReqText, nextData.requiredProsperity, prospColor, "繁荣要求");
        }
        else
        {
            if (upgradeButton != null) upgradeButton.interactable = false;
            SetResourceText(coinCostText, 0, CostColor, "铜钱");
            SetResourceText(manpowerCostText, 0, CostColor, "人力");
            SetResourceText(materialCostText, 0, CostColor, "建材");
            SetResourceText(prospReqText, 0, CostColor, "繁荣要求");
        }

        // 把你注释的解封了！
        // if (currentProsperityText != null) currentProsperityText.text = GameManager.Instance.Prosperity.ToString();

        if (BuffManager.Instance != null && BuffManager.Instance.prosperityBuff != null)
        {
            float mult = BuffManager.Instance.prosperityBuff.coinMult;
            int anger = BuffManager.Instance.prosperityBuff.angerAdd;

            if (productionBuffText != null) productionBuffText.text = "全局产出增益：" + $"{(mult - 1f) * 100:F1}%";
            if (angerReductionText != null) angerReductionText.text = "每回合压制民怨：" + Mathf.Abs(anger).ToString();
        }

        UpdateExchangePreview("");
    }

    private void SetResourceText(TextMeshProUGUI textComp, int val, string colorHex, string res)
    {
        if (textComp == null) return;
        // 核心修复：改回 >= 0，不然要求是 0 的时候字就消失了
        if (val >= 0)
        {
            textComp.gameObject.SetActive(true);
            textComp.text = res + " : " + $"<color={colorHex}>{val}</color>";
        }
        else
        {
            textComp.gameObject.SetActive(false);
        }
    }

    private void UpdateExchangePreview(string _)
    {
        int coins = 0;
        int pop = 0;

        int.TryParse(coinInputField != null ? coinInputField.text : "0", out coins);
        int.TryParse(popInputField != null ? popInputField.text : "0", out pop);

        int total = coins + pop;
        if (exchangePreviewText != null) exchangePreviewText.text = "预计繁荣度：+" + total.ToString();

        bool canAfford = GameManager.Instance.Coins >= coins && GameManager.Instance.Population >= pop && total > 0;
        if (exchangeButton != null) exchangeButton.interactable = canAfford;
    }

    private void OnUpgradeClicked()
    {
        if (PalaceManager.Instance != null && currentPalace != null)
        {
            bool success = PalaceManager.Instance.TryUpgradeCoreBuilding(currentPalace);
            if (success)
            {
                // 1. 拿到新皇宫的物理指针
                currentPalace = PalaceManager.Instance.palaceStages[PalaceManager.Instance.currentStageIndex];

                // 2. 极其关键！强制大管家同步指针，防止底层依然锁定在旧皇宫上
                if (BuildingInteractManager.Instance != null)
                {
                    BuildingInteractManager.Instance.ForceSetTarget(currentPalace);
                }

                // 3. 强制重绘UI文本
                RefreshUI();

                // 4. 打破 Unity UI 的焦点死锁，让按钮立刻变灰
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                }
            }
        }
    }

    private void OnExchangeClicked()
    {
        int coins = 0;
        int pop = 0;
        int.TryParse(coinInputField.text, out coins);
        int.TryParse(popInputField.text, out pop);

        int total = coins + pop;
        if (total <= 0) return;

        if (GameManager.Instance.TryExchangeProsperity(coins, pop))
        {
            if (BuffManager.Instance != null)
            {
                BuffManager.Instance.UpdateProsperityBuff(GameManager.Instance.Prosperity);
            }

            if (coinInputField != null) coinInputField.text = "";
            if (popInputField != null) popInputField.text = "";
            RefreshUI();
        }
    }
}