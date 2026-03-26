using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingPanelUI : MonoBehaviour
{
    private const string CostColor = "#FF5A5A";
    private const string RefundColor = "#63D66B";

    [Header("必须在 Inspector 里拖好的组件")]
    public TextMeshProUGUI buildingNameText;
    public TextMeshProUGUI buildingIntroText;
    public Image buildingImage;

    public Button upgradeButton;
    public Button demolishButton;

    public TextMeshProUGUI coinCostText;
    public TextMeshProUGUI manpowerCostText;
    public TextMeshProUGUI materialCostText;

    public TextMeshProUGUI coinRefundText;
    public TextMeshProUGUI manpowerRefundText;
    public TextMeshProUGUI materialRefundText;

    private void OnEnable()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.OnBuildingSelected += RefreshData;
        }

        if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
        if (demolishButton != null) demolishButton.onClick.AddListener(OnDemolishClicked);
    }

    private void OnDisable()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.OnBuildingSelected -= RefreshData;
        }

        if (upgradeButton != null) upgradeButton.onClick.RemoveListener(OnUpgradeClicked);
        if (demolishButton != null) demolishButton.onClick.RemoveListener(OnDemolishClicked);
    }

    private void RefreshData(BuildingEntity entity)
    {
        if (entity == null || entity.data == null) return;
        BuildingSO data = entity.data;

        // 基础信息
        if (buildingNameText != null) buildingNameText.text = data.buildingTitle;
        if (buildingIntroText != null) buildingIntroText.text = data.buildingIntro;
        if (buildingImage != null && data.buildingIcon != null) buildingImage.sprite = data.buildingIcon;

        // 升级消耗
        BuildingSO nextData = data.nextLevelSO;
        if (nextData != null)
        {
            if (upgradeButton != null) upgradeButton.interactable = true;
            SetResourceText(coinCostText, nextData.costCoins, CostColor, "铜钱");
            SetResourceText(manpowerCostText, nextData.costPopulation, CostColor, "人力");
            SetResourceText(materialCostText, nextData.costMaterial, CostColor, "建材");
        }
        else
        {
            if (upgradeButton != null) upgradeButton.interactable = false;
            SetResourceText(coinCostText, 0, CostColor, "铜钱");
            SetResourceText(manpowerCostText, 0, CostColor, "人力");
            SetResourceText(materialCostText, 0, CostColor, "建材");
        }

        // 拆除返还
        SetResourceText(coinRefundText, data.refundCoins, RefundColor, "铜钱");
        SetResourceText(manpowerRefundText, data.refundPopulation, RefundColor, "人力");
        SetResourceText(materialRefundText, data.refundMaterial, RefundColor, "建材");
    }

    private void SetResourceText(TextMeshProUGUI textComp, int val, string colorHex, string res)
    {
        if (textComp == null) return;
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

    private void OnUpgradeClicked()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.RequestUpgrade();
            // Debug.Log("111");
        }
    }

    private void OnDemolishClicked()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.RequestDemolish();
            // Debug.Log("2322");
        }
    }
}