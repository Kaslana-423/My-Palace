using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BuildingPanelUI))]
public class BuildingPanelAutoBinder : MonoBehaviour
{
    [ContextMenu("Auto Bind Components (Editor Only)")]
    public void AutoBind()
    {
        BuildingPanelUI ui = GetComponent<BuildingPanelUI>();
        if (ui == null) return;

        TextMeshProUGUI[] allTexts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in allTexts)
        {
            string n = t.name.ToLower();
            if (n.Contains("title") || n.Contains("name")) ui.buildingNameText = t;
            else if (n.Contains("intro") || n.Contains("desc")) ui.buildingIntroText = t;
            else if (n.Contains("coin") && n.Contains("cost")) ui.coinCostText = t;
            else if (n.Contains("manpower") && n.Contains("cost")) ui.manpowerCostText = t;
            else if (n.Contains("material") && n.Contains("cost")) ui.materialCostText = t;
            else if (n.Contains("coin") && n.Contains("refund")) ui.coinRefundText = t;
            else if (n.Contains("manpower") && n.Contains("refund")) ui.manpowerRefundText = t;
            else if (n.Contains("material") && n.Contains("refund")) ui.materialRefundText = t;
        }

        Button[] allBtns = GetComponentsInChildren<Button>(true);
        foreach (var b in allBtns)
        {
            string n = b.name.ToLower();
            if (n.Contains("up") || n.Contains("升")) ui.upgradeButton = b;
            if (n.Contains("demolish") || n.Contains("拆")) ui.demolishButton = b;
        }

        // 记得让他把文案配到 SO 里，别再写死在代码里了。

#if UNITY_EDITOR
        EditorUtility.SetDirty(ui);
#endif
        Debug.Log("[Binder] 自动绑定完成，请检查有无遗漏。");
    }
}