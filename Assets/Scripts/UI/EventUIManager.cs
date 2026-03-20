using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventUIManager : MonoBehaviour
{
    [Header("UI 组件绑定")]
    [Tooltip("包含整个事件弹窗的根节点")]
    public GameObject eventPanel;

    [Tooltip("显示标题的文本组件")]
    public TextMeshProUGUI titleText;

    [Tooltip("显示描述的文本组件")]
    public TextMeshProUGUI descText;

    [Tooltip("选项按钮生成的父容器 (Layout Group)")]
    public Transform optionsContainer;

    [Header("资源预制体")]
    [Tooltip("选项按钮预制体 (必须包含 Button 和 TextMeshProUGUI)")]
    public GameObject optionButtonPrefab;

    private void Awake()
    {
        // 游戏开始时隐藏面板
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 显示指定的事件数据到 UI 上
    /// </summary>
    public void ShowEvent(GameEvent currentEvent)
    {
        if (currentEvent == null) return;

        // 1. 设置文本内容
        if (titleText != null) titleText.text = currentEvent.eventTitle;
        if (descText != null) descText.text = currentEvent.eventDescription;

        // 2. 清理旧的选项按钮
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        // 3. 生成新的选项按钮
        if (currentEvent.options != null && optionButtonPrefab != null)
        {
            foreach (EventOption option in currentEvent.options)
            {
                CreateOptionButton(option);
            }
        }

        // 4. 显示面板
        if (eventPanel != null)
        {
            eventPanel.SetActive(true);
        }
    }

    private void CreateOptionButton(EventOption option)
    {
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);

        // 设置按钮文字
        // 尝试获取子物体中的 TMP，或者物体本身的 TMP
        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = option.optionText;
        }

        // 绑定点击事件
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            // 【关键】防止闭包陷阱，虽然 C# 5.0+ foreach 已经修复，
            // 但显式定义局部变量 tempOption 是一种更安全稳健的写法。
            EventOption tempOption = option;

            btn.onClick.AddListener(() =>
            {
                OnOptionClicked(tempOption);
            });
        }
    }

    private void OnOptionClicked(EventOption option)
    {
        // 1. 通知 EventManager 进行逻辑结算
        if (EventManager.Instance != null)
        {
            EventManager.Instance.ResolveOption(option);
        }

        // 2. 关闭界面
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }
}