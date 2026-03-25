using System.Collections.Generic;
using UnityEngine;

// 定义单个选项的数据结构
[System.Serializable]
public class EventOption
{
    [Tooltip("选项显示的文本")]
    public string optionText;

    [Header("核心资源变更")]
    [Tooltip("金币 (Coin) 变化量 (+增加, -减少)")]
    public int coinChange; 

    [Tooltip("建材 (Materials) 变化量 (+增加, -减少)")]
    public int materialChange; // [新增]

    [Tooltip("人口 (Population) 变化量 (+增加, -减少)")]
    public int populationChange;

    [Header("城市数值变更")]
    [Tooltip("满意度 (Satisfaction) 变化量 (+满意, -愤怒)")]
    public int satisfactionChange;

    [Tooltip("繁荣度 (Prosperity) 变化量 (+增加, -衰退)")]
    public int prosperityChange; // [新增]
}

// 定义事件主体
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