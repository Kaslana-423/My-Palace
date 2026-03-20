using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventOption
{
    [Tooltip("选项显示的文本")]
    public string optionText;

    [Header("数值影响")]
    [Tooltip("金币 (Coin) 变化量 (+增加, -减少)")]
    // [修改] 统一变量名 goldChange -> coinChange
    public int coinChange; 

    [Tooltip("人口 (Population) 变化量 (+增加, -减少)")]
    public int populationChange;

    [Tooltip("满意度 (Satisfaction) 变化量 (+满意, -愤怒)")]
    // 逻辑已统一：正数增加满意度
    public int satisfactionChange;
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