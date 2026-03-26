using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class BuildingPanelStyleAdapter : MonoBehaviour
{
    [Header("目标组件")]
    public Building targetBuilding;

    [Header("资源行样式")]
    public float resourceLineFontSize = 36f;
    public Vector2 resourceIconSize = new Vector2(48f, 48f);
    public Vector2 resourceIconOffset = new Vector2(14f, 0f);

    [Header("字体设置")]
    public TMP_FontAsset panelFont;
    public bool applyPanelFontOnRefresh = true;

    [Header("自动应用")]
    public bool applyOnEnable = true;
    public bool applyOnValidateInEditMode = true;

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            ApplyStyle();
        }
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && applyOnValidateInEditMode)
        {
            ApplyStyle();
        }
    }

    [ContextMenu("Apply Style To Building Panel")]
    public void ApplyStyle()
    {
        if (targetBuilding == null)
        {
            targetBuilding = GetComponent<Building>();
        }

        if (targetBuilding == null)
        {
            return;
        }

        targetBuilding.resourceLineFontSize = resourceLineFontSize;
        targetBuilding.resourceIconSize = resourceIconSize;
        targetBuilding.resourceIconOffset = resourceIconOffset;
        targetBuilding.applyPanelFontOnRefresh = applyPanelFontOnRefresh;

        if (panelFont != null)
        {
            targetBuilding.panelFont = panelFont;
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(targetBuilding);
        EditorUtility.SetDirty(this);
#endif
    }
}
