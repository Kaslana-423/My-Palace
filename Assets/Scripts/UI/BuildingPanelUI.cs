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
            SetResourceText(coinCostText, nextData.costCoins, CostColor);
            SetResourceText(manpowerCostText, nextData.costPopulation, CostColor);
            SetResourceText(materialCostText, nextData.costMaterial, CostColor);
        }
        else
        {
            if (upgradeButton != null) upgradeButton.interactable = false;
            SetResourceText(coinCostText, 0, CostColor);
            SetResourceText(manpowerCostText, 0, CostColor);
            SetResourceText(materialCostText, 0, CostColor);
        }

        // 拆除返还
        SetResourceText(coinRefundText, data.refundCoins, RefundColor);
        SetResourceText(manpowerRefundText, data.refundPopulation, RefundColor);
        SetResourceText(materialRefundText, data.refundMaterial, RefundColor);
    }

    private void SetResourceText(TextMeshProUGUI textComp, int val, string colorHex)
    {
        if (textComp == null) return;
        if (val > 0)
        {
            textComp.gameObject.SetActive(true);
            textComp.text = $"<color={colorHex}>{val}</color>";
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
            Debug.Log("111");
        }
    }

    private void OnDemolishClicked()
    {
        if (BuildingInteractManager.Instance != null)
        {
            BuildingInteractManager.Instance.RequestDemolish();
            Debug.Log("2322");
        }
    }
}