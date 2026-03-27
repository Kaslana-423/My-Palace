using System;

[Serializable] // 必须加上这个，才能在 ScriptableObject 的面板中显示
public class GlobalBuff
{
    // 百分比乘区 (默认 1f 表示不变，1.2f 表示增加 20%)
    public float coinMult = 1f;
    public int coinAdd = 0;

    public float materialMult = 1f;
    public int materialAdd = 0;

    public float popMult = 1f;
    public int popAdd = 0;

    public float prosMult = 1f;
    public int prosAdd = 0;

    public float angerMult = 1f;
    public int angerAdd = 0;

    // 持续回合数
    public int remainRounds = 0;

    // [新增] 拷贝方法，用于将 SO 里的模板数值复制一份实际投入运行
    public GlobalBuff Clone()
    {
        return (GlobalBuff)this.MemberwiseClone();
    }
}