using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EventOption
{
    public string optionText;
    [TextArea(2, 5)] public string optionDescription;
    
    [Header("立即生效：核心资源变更")]
    [Tooltip("金币临时增减")] public int coinChange; 
    [Tooltip("建材临时增减")] public int materialChange; 
    [Tooltip("人口临时增减")] public int populationChange;

    [Header("立即生效：城市数值变更")]
    [Tooltip("繁荣度临时增减")] public int prosperityChange; 
    [Tooltip("民怨临时增减")] public int personAngerChange; 

    [Header("持续生效：结算 Buff (可选)")]
    [Tooltip("勾选后，该选项将额外施加一个全局 Buff")]
    public bool hasBuff;
    
    [Tooltip("配置 Buff 的详细数值")]
    public GlobalBuff buffResult = new GlobalBuff(); 
}

[CreateAssetMenu(fileName = "NewGameEvent", menuName = "Game/Game Event")]
public class GameEvent : ScriptableObject
{
    [Header("事件基本信息")]
    public string eventTitle;
    [TextArea(3, 10)] public string eventDescription;
    
    [Tooltip("抽取权重，数值越大越容易被抽中（默认100）")]
    public int weight = 100;

    [Header("选项列表")]
    public List<EventOption> options;
}