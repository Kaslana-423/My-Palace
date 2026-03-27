using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // [新增] 用于处理鼠标悬停事件

public class EventUIManager : MonoBehaviour
{
    [Header("UI 组件绑定")]
    public GameObject eventPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Transform optionsContainer;

    [Header("悬停提示 (Tooltip)")]
    [Tooltip("指向场景中的 Tooltip 预制体/物体")]
    public EventTooltip tooltip; // [新增引用]

    [Header("资源预制体")]
    public GameObject optionButtonPrefab;

    private void Awake()
    {
        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }

    public void ShowEvent(GameEvent currentEvent)
    {
        if (currentEvent == null) return;

        if (titleText != null) titleText.text = currentEvent.eventTitle;
        if (descText != null) descText.text = currentEvent.eventDescription;

        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }

        if (currentEvent.options != null && optionButtonPrefab != null)
        {
            foreach (EventOption option in currentEvent.options)
            {
                CreateOptionButton(option);
            }
        }

        if (eventPanel != null)
        {
            eventPanel.SetActive(true);
        }
    }

    private void CreateOptionButton(EventOption option)
    {
        GameObject btnObj = Instantiate(optionButtonPrefab, optionsContainer);

        TextMeshProUGUI btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = option.optionText;
        }

        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            EventOption tempOption = option;

            btn.onClick.AddListener(() =>
            {
                OnOptionClicked(tempOption);
            });

            // --- [新增] 动态添加悬停触发器 (PointerEnter / PointerExit) ---
            EventTrigger trigger = btnObj.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = btnObj.AddComponent<EventTrigger>();
            }

            // 1. 鼠标进入 (Hover)
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter;
            entryEnter.callback.AddListener((data) => {
                if (tooltip != null) tooltip.ShowTooltip(tempOption.optionDescription);
            });
            trigger.triggers.Add(entryEnter);

            // 2. 鼠标离开 (Exit)
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit;
            entryExit.callback.AddListener((data) => {
                if (tooltip != null) tooltip.HideTooltip();
            });
            trigger.triggers.Add(entryExit);
        }
    }

    private void OnOptionClicked(EventOption option)
    {
        // [新增] 点击按钮后，确保隐藏 tooltip
        if (tooltip != null) tooltip.HideTooltip();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.ResolveOption(option);
        }

        if (eventPanel != null)
        {
            eventPanel.SetActive(false);
        }
    }
}