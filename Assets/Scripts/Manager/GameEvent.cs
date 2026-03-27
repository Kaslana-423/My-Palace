using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventOption
{
    [Tooltip("选项显示的文本")]
    public string optionText;

    [Tooltip("后果描述文字 (鼠标悬停时显示)")]
    [TextArea(2, 5)]
    public string optionDescription; // [新增字段]

    [Header("核心资源变更")]
    [Tooltip("金币 (Coin) 变化量 (+增加, -减少)")]
    public int coinChange; 

    [Tooltip("建材 (Materials) 变化量 (+增加, -减少)")]
    public int materialChange; 

    [Tooltip("人口 (Population) 变化量 (+增加, -减少)")]
    public int populationChange;

    [Header("城市数值变更")]
    [Tooltip("繁荣度 (Prosperity) 变化量 (+增加, -衰退)")]
    public int prosperityChange; 

    [Tooltip("民怨 (Person Anger) 变化量 (+增加民怨, -减少民怨)")]
    public int personAngerChange; // [修改] 统一为民怨
}

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