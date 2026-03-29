using UnityEngine;

[CreateAssetMenu(fileName = "NewPolicy", menuName = "Game/Policy Data")]
public class PolicyData : ScriptableObject
{
    [Header("正面：基本信息")]
    public string policyName;                   // 国策名称
    public Sprite illustration;                 // 封面插画
    [TextArea(2, 4)] public string buffDesc;    // Buff 效果描述 (例如: "+20% 建材产出")

    [Header("背面：背景故事")]
    [TextArea(4, 8)] public string backgroundStory; // 背面显示的传说/背景文字

    [Header("增益效果")]
    [Tooltip("国策带来的全局 Buff")]
    public GlobalBuff policyBuff = new GlobalBuff();
}