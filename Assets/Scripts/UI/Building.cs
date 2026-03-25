using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class Building : MonoBehaviour
{
    private const string CostColor = "#FF5A5A";
    private const string RefundColor = "#63D66B";

    [Header("资源图标占位(可替换为TMP sprite标签)")]
    public string costCoinIcon = "[铜]";
    public string costManpowerIcon = "[人]";
    public string costMaterialIcon = "[材]";
    public string refundCoinIcon = "[铜]";
    public string refundManpowerIcon = "[人]";
    public string refundMaterialIcon = "[材]";
    public TMP_SpriteAsset resourceSpriteAsset;

    [Header("PNG图标(可选，优先于占位符)")]
    public Image costCoinIconImage;
    public Image costManpowerIconImage;
    public Image costMaterialIconImage;
    public Image refundCoinIconImage;
    public Image refundManpowerIconImage;
    public Image refundMaterialIconImage;
    public Sprite coinIconSprite;
    public Sprite manpowerIconSprite;
    public Sprite materialIconSprite;

    private readonly Dictionary<TextMeshProUGUI, Image> autoIconMap = new Dictionary<TextMeshProUGUI, Image>();

    [Header("资源数值文本样式")]
    public float resourceLineFontSize = 44f;
    public Vector2 resourceIconSize = new Vector2(100f, 100f);
    public Vector2 resourceIconOffset = new Vector2(16f, 0f);

    [Header("UI 组件引用 (请在面板上拖拽赋值)")]
    public Button upgradeButton; // 升级按钮
    public Button demolishButton; // 拆除按钮
    public Image buildingImage; // 显示建筑图标的 Image 组件
    public Image buildingFrameImage; // 显示建筑框(可选)
    public TextMeshProUGUI coinCostText; // 显示升级消耗-铜钱
    public TextMeshProUGUI manpowerCostText; // 显示升级消耗-人力
    public TextMeshProUGUI materialCostText; // 显示升级消耗-材料
    public TextMeshProUGUI buildingNameText; // 显示建筑名称的文本
    public TextMeshProUGUI buildingIntroText; // 显示建筑介绍文本
    public TextMeshProUGUI buildModeKeyText; // 显示开启建造模式按键
    public TextMeshProUGUI coinRefundText; // 显示拆除返还-铜钱
    public TextMeshProUGUI manpowerRefundText; // 显示拆除返还-人力
    public TextMeshProUGUI materialRefundText; // 显示拆除返还-材料

    [Header("文字样式配置")]
    public TMP_FontAsset panelFont; // 统一字体（可选）
    public bool applyPanelFontOnRefresh = true;

    [Header("运行时介绍文本布局 (可调)")]
    public Vector2 runtimeIntroAnchoredPos = new Vector2(120f, 150f);
    public Vector2 runtimeIntroSize = new Vector2(280f, 220f);
    public float runtimeIntroFontSize = 32f;
    public Color runtimeIntroColor = new Color(0.92f, 0.92f, 0.92f, 1f);
    public TextAlignmentOptions runtimeIntroAlignment = TextAlignmentOptions.TopLeft;
    public bool runtimeIntroWordWrap = true;
    public TextOverflowModes runtimeIntroOverflow = TextOverflowModes.Overflow;
    public Vector4 runtimeIntroMargin = Vector4.zero;
    public float runtimeIntroLineSpacing = 0f;
    public float runtimeIntroWordSpacing = 0f;
    public float runtimeIntroCharacterSpacing = 0f;
    public float runtimeIntroParagraphSpacing = 0f;

    [Header("各等级图标配置")]
    public List<Sprite> mingju; // 民居等级图标列表
    public List<Sprite> guanfu; // 官府等级图标列表

    [Header("编辑器预览(非Play可用)")]
    public bool enableEditorPreview = true;
    public bool autoRefreshPreviewOnValidate = false;
    public string previewBuildingName = "民居";
    [Range(0, 3)] public int previewLevel = 0;

    // 记录当前面板正在显示的建筑坐标，防止别的建筑升级刷新了你的面板
    private Vector3Int currentBuildingPos;

    private void OnEnable()
    {
        if (PlaceSysManager.Instance != null)
        {
            PlaceSysManager.Instance.BuildingUpgraded += OnBuildingUpgraded;
        }
    }

    private void OnDisable()
    {
        if (PlaceSysManager.Instance != null)
        {
            PlaceSysManager.Instance.BuildingUpgraded -= OnBuildingUpgraded;
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureRequiredReferences();

        if (autoRefreshPreviewOnValidate && enableEditorPreview)
        {
            ApplyEditorPreview();
        }
    }

    // 监听到有建筑升级时调用
    private void OnBuildingUpgraded(Vector3Int position)
    {
        // 只有当升级的建筑是当前面板正在显示的建筑时，才刷新 UI
        if (position == currentBuildingPos)
        {
            RefreshUI(currentBuildingPos);
        }
    }

    // ---------------------------------------------------------
    // 新增核心方法：外部调用这个方法来打开面板！
    // ---------------------------------------------------------
    public void OpenPanel(Vector3Int position)
    {
        EnsureRequiredReferences();
        ClearResourceLines();
        currentBuildingPos = position;
        this.gameObject.SetActive(true); // 激活面板显示
        CenterPanelOnScreen();
        SetPanelTitleText("读取中...");
        Debug.Log($"[BuildingPanel] OpenPanel at {position}");
        RefreshUI(currentBuildingPos);   // 刷新当前坐标的数据
    }

    [ContextMenu("Panel Preview/Apply Sample Data")]
    private void ApplyEditorPreview()
    {
        if (Application.isPlaying || !enableEditorPreview)
        {
            return;
        }

        EnsureRequiredReferences();
        gameObject.SetActive(true);

        int safeLevel = Mathf.Clamp(previewLevel, 0, 3);
        BuildingData sample = new BuildingData(previewBuildingName, safeLevel);
        RefreshUI(sample);

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    [ContextMenu("Panel Preview/Clear Sample Data")]
    private void ClearEditorPreview()
    {
        if (Application.isPlaying)
        {
            return;
        }

        EnsureRequiredReferences();
        SetPanelTitleText(string.Empty);
        SetTextIfNotNull(buildingIntroText, string.Empty);
        SetTextIfNotNull(buildModeKeyText, string.Empty);
        ClearResourceLines();

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    private void EnsureRequiredReferences()
    {
        if (buildingNameText != null)
        {
            AutoBindOptionalReferences();
            return;
        }

        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        if (allTexts == null || allTexts.Length == 0)
        {
            return;
        }

        TextMeshProUGUI best = null;
        float bestScore = float.MinValue;
        foreach (TextMeshProUGUI t in allTexts)
        {
            if (t == null)
            {
                continue;
            }

            float score = 0f;
            string n = t.name;
            if (!string.IsNullOrEmpty(n))
            {
                string lower = n.ToLowerInvariant();
                if (lower.Contains("title") || lower.Contains("name") || n.Contains("标题"))
                {
                    score += 1000f;
                }
            }

            RectTransform rt = t.transform as RectTransform;
            if (rt != null)
            {
                score += rt.anchoredPosition.y;
            }

            if (score > bestScore)
            {
                bestScore = score;
                best = t;
            }
        }

        if (best != null)
        {
            buildingNameText = best;
            Debug.LogWarning($"[BuildingPanel] 未手动绑定 buildingNameText，已自动绑定到: {best.name}");
        }

        AutoBindOptionalReferences();
    }

    private void AutoBindOptionalReferences()
    {
        if (buildingIntroText == null)
        {
            buildingIntroText = FindTextByNameContains("Intro", "介绍", "Desc", "Description");
        }

        if (coinCostText == null)
        {
            coinCostText = FindTextByNameContains("Coin", "铜钱", "Gold");
        }

        if (manpowerCostText == null)
        {
            manpowerCostText = FindTextByNameContains("Manpower", "人力");
        }

        if (materialCostText == null)
        {
            materialCostText = FindTextByNameContains("Material", "材料");
        }

        if (coinRefundText == null)
        {
            coinRefundText = FindTextByNameContains("Refund_Coin", "CoinRefund", "铜钱返还");
        }

        if (manpowerRefundText == null)
        {
            manpowerRefundText = FindTextByNameContains("Refund_Manpower", "人力返还");
        }

        if (materialRefundText == null)
        {
            materialRefundText = FindTextByNameContains("Refund_Material", "材料返还");
        }

        // 自动绑定资源图标Image，避免因未手动拖拽导致尺寸/位置控制失效。
        if (costCoinIconImage == null)
        {
            costCoinIconImage = FindImageByNameContains("Cost_Coin_Icon", "CostCoinIcon", "Cost_Coin", "CoinCost", "消耗铜");
        }

        if (costManpowerIconImage == null)
        {
            costManpowerIconImage = FindImageByNameContains("Cost_Manpower_Icon", "CostManpowerIcon", "Cost_Manpower", "ManpowerCost", "消耗人");
        }

        if (costMaterialIconImage == null)
        {
            costMaterialIconImage = FindImageByNameContains("Cost_Material_Icon", "CostMaterialIcon", "Cost_Material", "MaterialCost", "消耗材");
        }

        if (refundCoinIconImage == null)
        {
            refundCoinIconImage = FindImageByNameContains("Refund_Coin_Icon", "RefundCoinIcon", "Refund_Coin", "CoinRefund", "返还铜");
        }

        if (refundManpowerIconImage == null)
        {
            refundManpowerIconImage = FindImageByNameContains("Refund_Manpower_Icon", "RefundManpowerIcon", "Refund_Manpower", "ManpowerRefund", "返还人");
        }

        if (refundMaterialIconImage == null)
        {
            refundMaterialIconImage = FindImageByNameContains("Refund_Material_Icon", "RefundMaterialIcon", "Refund_Material", "MaterialRefund", "返还材");
        }

        if (buildingIntroText == null)
        {
            Debug.LogWarning("[BuildingPanel] buildingIntroText 未绑定且未自动匹配到节点，建筑介绍不会显示");
        }
        else
        {
            ApplyIntroTextStyle(buildingIntroText);
        }
    }

    private void ApplyIntroTextStyle(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rt = text.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = runtimeIntroAnchoredPos;
            rt.sizeDelta = runtimeIntroSize;
        }

        text.fontSize = runtimeIntroFontSize;
        text.color = runtimeIntroColor;
        text.alignment = runtimeIntroAlignment;
        text.enableWordWrapping = runtimeIntroWordWrap;
        text.overflowMode = runtimeIntroOverflow;
        text.margin = runtimeIntroMargin;
        text.lineSpacing = runtimeIntroLineSpacing;
        text.wordSpacing = runtimeIntroWordSpacing;
        text.characterSpacing = runtimeIntroCharacterSpacing;
        text.paragraphSpacing = runtimeIntroParagraphSpacing;
    }

    private void CenterPanelOnScreen()
    {
        RectTransform rect = transform as RectTransform;
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
    }

    // 刷新 UI 表现
    public void RefreshUI(Vector3Int position)
    {
        if (!GameManager.HasInstance) return;

        // 从字典里抓取数据
        if (!GameManager.Instance.buildingDataDict.TryGetValue(position, out BuildingData data))
        {
            SetPanelTitleText("未登记建筑");
            Debug.LogWarning($"未在 buildingDataDict 找到坐标 {position} 的数据，面板显示为未登记建筑");
            return;
        }

        RefreshUI(data);
    }

    // 允许外部系统直接传入数据，便于 prefab 在不同项目中复用
    public void RefreshUI(BuildingData data)
    {
        if (data == null)
        {
            return;
        }

        ApplyPanelFontIfNeeded();

        // 1. 更新名字和等级
        string resolvedTitle = GetBuildingTitle(data.buildingName, data.level);
        SetPanelTitleText(resolvedTitle);
        Debug.Log($"[BuildingPanel] title={resolvedTitle}, buildingName={data.buildingName}, level={data.level}");

        if (buildingIntroText != null)
        {
            ApplyIntroTextStyle(buildingIntroText);
            buildingIntroText.text = GetBuildingIntro(data.buildingName, data.level);
        }

        if (buildModeKeyText != null)
        {
            buildModeKeyText.text = "开启建造模式按键: Tab";
        }

        // 2. 更新图标
        if (data.buildingName == "民居")
        {
            if (data.level >= 0 && data.level < mingju.Count)
            {
                SetBuildingSprite(mingju[data.level]);
            }
        }
        else if (data.buildingName == "官府")
        {
            if (data.level >= 0 && data.level < guanfu.Count)
            {
                SetBuildingSprite(guanfu[data.level]);
            }
        }

        // 3. 更新消耗和升级按钮状态
        if (data.coinCost != null && data.level < data.coinCost.Count)
        {
            int coinCost = GetCostAtLevel(data.coinCost, data.level);
            int manpowerCost = GetCostAtLevel(data.manpowerCost, data.level);
            int materialCost = GetCostAtLevel(data.materialCost, data.level);

            SetResourceLine(coinCostText, costCoinIconImage, coinIconSprite, costCoinIcon, coinCost, false, CostColor);
            SetResourceLine(manpowerCostText, costManpowerIconImage, manpowerIconSprite, costManpowerIcon, manpowerCost, false, CostColor);
            SetResourceLine(materialCostText, costMaterialIconImage, materialIconSprite, costMaterialIcon, materialCost, false, CostColor);
            if (upgradeButton != null)
            {
                upgradeButton.interactable = true;
            }
        }
        else
        {
            SetResourceLine(coinCostText, costCoinIconImage, coinIconSprite, costCoinIcon, 0, false, CostColor);
            SetResourceLine(manpowerCostText, costManpowerIconImage, manpowerIconSprite, costManpowerIcon, 0, false, CostColor);
            SetResourceLine(materialCostText, costMaterialIconImage, materialIconSprite, costMaterialIcon, 0, false, CostColor);
            if (upgradeButton != null)
            {
                upgradeButton.interactable = false; // 禁用升级按钮
            }
        }

        // 4. 更新拆除返还 (增加防越界保护)
        int refundLevel = Math.Max(0, data.level);
        int coinRefund = GetRefundAtLevel(data.coinCost, refundLevel);
        int manpowerRefund = GetRefundAtLevel(data.manpowerCost, refundLevel);
        int materialRefund = GetRefundAtLevel(data.materialCost, refundLevel);

        SetResourceLine(coinRefundText, refundCoinIconImage, coinIconSprite, refundCoinIcon, coinRefund, false, RefundColor);
        SetResourceLine(manpowerRefundText, refundManpowerIconImage, manpowerIconSprite, refundManpowerIcon, manpowerRefund, false, RefundColor);
        SetResourceLine(materialRefundText, refundMaterialIconImage, materialIconSprite, refundMaterialIcon, materialRefund, false, RefundColor);
    }

    private void SetTextIfNotNull(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void ClearResourceLines()
    {
        ClearResourceLine(coinCostText);
        ClearResourceLine(manpowerCostText);
        ClearResourceLine(materialCostText);
        ClearResourceLine(coinRefundText);
        ClearResourceLine(manpowerRefundText);
        ClearResourceLine(materialRefundText);
    }

    private void ClearResourceLine(TextMeshProUGUI text)
    {
        if (text == null)
        {
            return;
        }

        text.text = string.Empty;
        text.gameObject.SetActive(false);
    }

    private void SetResourceLine(TextMeshProUGUI text, Image iconImage, Sprite iconSprite, string placeholderIcon, int value, bool showWhenZero, string colorHex)
    {
        if (text == null)
        {
            return;
        }

        Image resolvedIconImage = iconImage;

        bool visible = showWhenZero || value > 0;
        text.gameObject.SetActive(visible);
        
        if (!visible)
        {
            if (resolvedIconImage != null)
            {
                resolvedIconImage.gameObject.SetActive(false);
            }
            return;
        }

        if (resolvedIconImage != null)
        {
            resolvedIconImage.gameObject.SetActive(visible);
            if (iconSprite != null)
            {
                resolvedIconImage.sprite = iconSprite;
            }
        }

        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.fontSize = GetAppliedResourceFontSize();
        text.alignment = TextAlignmentOptions.Left;
        text.richText = true;
        if (resourceSpriteAsset != null)
        {
            text.spriteAsset = resourceSpriteAsset;
        }

        // 如果配置了PNG图标Image，则文本仅显示数字；否则回退到占位符文本。
        if (resolvedIconImage != null)
        {
            text.text = $"<color={colorHex}>{value}</color>";

            RectTransform iconRect = resolvedIconImage.transform as RectTransform;
            if (iconRect != null)
            {
                UpdateIconPositionToTextLeft(text, iconRect);
            }
        }
        else
        {
            text.text = $"{placeholderIcon}:<color={colorHex}>{value}</color>";
        }
    }

    private void UpdateIconPositionToTextLeft(TextMeshProUGUI text, RectTransform iconRect)
    {
        if (text == null || iconRect == null)
        {
            return;
        }

        RectTransform textRect = text.transform as RectTransform;
        if (textRect == null)
        {
            return;
        }

        text.ForceMeshUpdate();
        Vector2 appliedSize = GetAppliedResourceIconSize();
        float gap = Mathf.Abs(resourceIconOffset.x);

        // 强制同步锚点，避免Prefab原锚点差异导致图标跑到数字右侧。
        iconRect.anchorMin = textRect.anchorMin;
        iconRect.anchorMax = textRect.anchorMax;
        iconRect.pivot = textRect.pivot;
        iconRect.sizeDelta = appliedSize;
        iconRect.anchoredPosition = textRect.anchoredPosition + new Vector2(-gap - appliedSize.x, resourceIconOffset.y);
    }

    private float GetAppliedResourceFontSize()
    {
        // 兼容旧Prefab/场景覆盖值：若还是旧小字号，运行时自动放大2倍。
        return resourceLineFontSize <= 24f ? resourceLineFontSize * 2f : resourceLineFontSize;
    }

    private Vector2 GetAppliedResourceIconSize()
    {
        // 兼容旧Prefab/场景覆盖值：若还是旧小图标，运行时自动放大5倍。
        if (resourceIconSize.x <= 30f && resourceIconSize.y <= 30f)
        {
            return resourceIconSize * 5f;
        }

        return resourceIconSize;
    }

    private Image GetOrCreateAutoIconImage(TextMeshProUGUI text)
    {
        if (autoIconMap.TryGetValue(text, out Image cached) && cached != null)
        {
            return cached;
        }

        RectTransform textRect = text.transform as RectTransform;
        if (textRect == null)
        {
            return null;
        }

        GameObject go = new GameObject(text.name + "_AutoIcon", typeof(RectTransform), typeof(Image));
        Transform parent = text.transform.parent != null ? text.transform.parent : text.transform;
        go.transform.SetParent(parent, false);

        RectTransform iconRect = go.GetComponent<RectTransform>();
        iconRect.anchorMin = textRect.anchorMin;
        iconRect.anchorMax = textRect.anchorMax;
        iconRect.pivot = textRect.pivot;
        iconRect.sizeDelta = resourceIconSize;

        Image icon = go.GetComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;

        autoIconMap[text] = icon;
        return icon;
    }

    private void ApplyPanelFontIfNeeded()
    {
        if (!applyPanelFontOnRefresh || panelFont == null)
        {
            return;
        }

        SetTextFont(buildingNameText);
        SetTextFont(buildingIntroText);
        SetTextFont(buildModeKeyText);
        SetTextFont(coinCostText);
        SetTextFont(manpowerCostText);
        SetTextFont(materialCostText);
        SetTextFont(coinRefundText);
        SetTextFont(manpowerRefundText);
        SetTextFont(materialRefundText);
    }

    private void SetTextFont(TextMeshProUGUI text)
    {
        if (text != null)
        {
            text.font = panelFont;
        }
    }

    private void SetPanelTitleText(string title)
    {
        bool wrote = false;

        if (buildingNameText != null)
        {
            buildingNameText.text = title;
            wrote = true;
        }

        TextMeshProUGUI namedTitle = FindTextByName("BuildingName");
        if (namedTitle != null)
        {
            namedTitle.text = title;
            if (buildingNameText == null)
            {
                buildingNameText = namedTitle;
            }
            wrote = true;
        }

        // 最终兜底：没有命名节点时，写到最上方文本，避免面板无标题。
        if (!wrote)
        {
            TextMeshProUGUI fallback = FindTopMostText();
            if (fallback != null)
            {
                fallback.text = title;
                buildingNameText = fallback;
                wrote = true;
                Debug.LogWarning($"[BuildingPanel] 使用兜底标题节点: {fallback.name}");
            }
        }

        if (!wrote)
        {
            Debug.LogError("[BuildingPanel] 未找到可写入的标题文本节点（buildingNameText / BuildingName）");
        }
    }

    private TextMeshProUGUI FindTextByName(string targetName)
    {
        if (string.IsNullOrEmpty(targetName))
        {
            return null;
        }

        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text != null && text.name == targetName)
            {
                return text;
            }
        }

        return null;
    }

    private TextMeshProUGUI FindTextByNameContains(params string[] keywords)
    {
        if (keywords == null || keywords.Length == 0)
        {
            return null;
        }

        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text == null || string.IsNullOrEmpty(text.name))
            {
                continue;
            }

            string lower = text.name.ToLowerInvariant();
            foreach (string key in keywords)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (lower.Contains(key.ToLowerInvariant()))
                {
                    return text;
                }
            }
        }

        return null;
    }

    private Image FindImageByNameContains(params string[] keywords)
    {
        if (keywords == null || keywords.Length == 0)
        {
            return null;
        }

        Image[] allImages = GetComponentsInChildren<Image>(true);
        foreach (Image image in allImages)
        {
            if (image == null || string.IsNullOrEmpty(image.name))
            {
                continue;
            }

            string lower = image.name.ToLowerInvariant();
            foreach (string key in keywords)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (lower.Contains(key.ToLowerInvariant()))
                {
                    return image;
                }
            }
        }

        return null;
    }

    private TextMeshProUGUI FindTopMostText()
    {
        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        TextMeshProUGUI best = null;
        float bestY = float.MinValue;
        foreach (TextMeshProUGUI t in allTexts)
        {
            if (t == null)
            {
                continue;
            }

            RectTransform rt = t.transform as RectTransform;
            if (rt == null)
            {
                continue;
            }

            if (rt.anchoredPosition.y > bestY)
            {
                bestY = rt.anchoredPosition.y;
                best = t;
            }
        }

        return best;
    }

    private string GetBuildingTitle(string buildingName, int level)
    {
        string[] guanfuTitles =
        {
            "官府九品",
            "官府七品",
            "官府四品",
            "官府一品"
        };

        string[] minjuTitles =
        {
            "民居草创",
            "民居立构",
            "民居丹楹",
            "民居极制"
        };

        int safeLevel = Mathf.Max(0, level);
        if (buildingName == "官府")
        {
            int idx = Mathf.Min(safeLevel, guanfuTitles.Length - 1);
            return guanfuTitles[idx];
        }

        if (buildingName == "民居")
        {
            int idx = Mathf.Min(safeLevel, minjuTitles.Length - 1);
            return minjuTitles[idx];
        }

        return buildingName + "-lv." + (safeLevel + 1).ToString();
    }

    private int GetCostAtLevel(List<int> costList, int level)
    {
        if (costList == null || level < 0 || level >= costList.Count)
        {
            return 0;
        }

        return costList[level];
    }

    private int GetRefundAtLevel(List<int> costList, int level)
    {
        if (costList == null || costList.Count == 0 || level < 0 || level >= costList.Count)
        {
            return 0;
        }

        return Mathf.RoundToInt(costList[level] * 0.75f);
    }

    private string GetBuildingIntro(string buildingName, int level)
    {
        string[] guanfuIntro =
        {
            "单进小院，砖木结构，屋顶覆青瓦。建筑简朴无华，仅设公堂与几间厢房，供基层官吏处理政务。",
            "多进院落，厅堂规整，门窗施以简单雕花。布局对称严谨，具备议事、办公、仓储等功能，体现州府威仪。",
            "规模宏大，多路多进，建有重檐大堂与附属院落。台基较高，装饰精美，可举行大型政务与礼仪活动。",
            "重檐庑殿顶，红墙彩画，石狮仪仗。可以受理天下奏章，举行大典，朝廷处理政务及官员议事之所。"
        };

        string[] minjuIntro =
        {
            "土坯墙或木骨泥墙，茅草覆顶，面阔一至两间。仅能满足基本居住需求，是底层百姓最常见的简陋居所。",
            "砖木结构，青瓦覆顶，围合成独立小院。正房与厢房布局规整，无多余装饰，实用舒适，为小康之家住所。",
            "多进院落，青砖黛瓦，木构梁架施以雕刻。功能分区明确，设厅堂、书斋、内眷居室，体现富足生活。",
            "多路多进，高墙深院，建有亭台楼阁。雕梁画栋，装饰繁丽，占地广阔，为世家望族居住的大型府邸。"
        };

        int safeLevel = Math.Max(0, level);

        if (buildingName == "官府")
        {
            int idx = Math.Min(safeLevel, guanfuIntro.Length - 1);
            return guanfuIntro[idx];
        }

        if (buildingName == "民居")
        {
            int idx = Math.Min(safeLevel, minjuIntro.Length - 1);
            return minjuIntro[idx];
        }

        return "暂无建筑介绍";
    }

    private void SetBuildingSprite(Sprite sprite)
    {
        if (buildingImage == null || sprite == null) return;
        
        buildingImage.sprite = sprite;
        // 【救命注释】：千万不要用 buildingImage.SetNativeSize(); 
        // 否则你刚才辛辛苦苦拼好的边框UI，会被原图原本的巨大分辨率瞬间撑爆！
    }
}