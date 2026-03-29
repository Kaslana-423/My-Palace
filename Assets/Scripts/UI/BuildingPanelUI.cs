using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

    public void RefreshData(BuildingEntity entity)
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
            // [核心新增] 普通建筑查三项资源
            bool canAfford = GameManager.Instance.Coins >= nextData.costCoins &&
                             GameManager.Instance.Population >= nextData.costPopulation &&
                             GameManager.Instance.Materials >= nextData.costMaterial;

            if (upgradeButton != null) upgradeButton.interactable = canAfford;

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
            // 1. 发送升级请求，底层销毁旧建筑并生成新建筑
            BuildingInteractManager.Instance.RequestUpgrade();

            // 2. 强制顺着最新的底层指针再刷一遍 UI，绝对防范事件总线的延迟错位
            RefreshData(BuildingInteractManager.Instance.GetCurrentTarget());

            // 3. 剥夺按钮焦点，立刻触发 interactable 的视觉变暗
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
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