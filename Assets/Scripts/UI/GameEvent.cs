using System.Collections.Generic;
using UnityEngine;

// 定义单个选项的数据结构
[System.Serializable]
public class EventOption
{
    [Tooltip("选项显示的文本")]
    public string optionText;

    [Header("数值影响")]
    [Tooltip("金币变化量 (+增加, -减少)")]
    public int goldChange;

    [Tooltip("人口变化量 (+增加, -减少)")]
    public int populationChange;

    [Tooltip("满意度/民怨变化量 (注意: +满意度通常意味着 -民怨，具体看 GameManager 逻辑)")]
    public int satisfactionChange;
}

// 定义事件主体，继承自 ScriptableObject 以便在 Project 窗口创建
[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game/Game Event")]
public class GameEvent : ScriptableObject
{
    [Header("事件基本信息")]
    public string eventTitle;

    [TextArea(3, 10)]
    public string eventDescription;

    [Header("选项列表")]
    public List<EventOption> options;
}