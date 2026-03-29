using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    public List<GlobalBuff> activeBuffs = new List<GlobalBuff>();
    public GlobalBuff prosperityBuff;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }
    private void Start()
    {
        // 开局直接捏一个常驻光环塞进总线，-1 代表无限回合，物理免疫你的 RemoveAll 清理
        prosperityBuff = new GlobalBuff { remainRounds = -1 };
        AddBuff(prosperityBuff);
    }

    public void AddBuff(GlobalBuff buff)
    {
        activeBuffs.Add(buff);
    }

    public void ProcessGlobalYields(ref int coins, ref int mats, ref int pops, ref int pros, ref int anger)
    {
        float fCoins = coins;
        float fMats = mats;
        float fPops = pops;
        float fPros = pros;
        float fAnger = anger;

        foreach (var buff in activeBuffs)
        {
            fCoins = fCoins * buff.coinMult + buff.coinAdd;
            fMats = fMats * buff.materialMult + buff.materialAdd;
            fPops = fPops * buff.popMult + buff.popAdd;
            fPros = fPros * buff.prosMult + buff.prosAdd;
            fAnger = fAnger * buff.angerMult + buff.angerAdd;

            if (buff.remainRounds > 0)
            {
                buff.remainRounds--;
            }
        }

        activeBuffs.RemoveAll(b => b.remainRounds == 0);

        coins = Mathf.RoundToInt(fCoins);
        mats = Mathf.RoundToInt(fMats);
        pops = Mathf.RoundToInt(fPops);
        pros = Mathf.RoundToInt(fPros);
        anger = Mathf.RoundToInt(fAnger);
    }
    public void UpdateProsperityBuff(int currentProsperity)
    {
        if (prosperityBuff == null) return;

        // 产出乘区：假设每 100 点繁荣度，产出增加 1%
        float multiplier = 1.0f + (currentProsperity / 100) * 0.01f;
        prosperityBuff.coinMult = multiplier;
        prosperityBuff.materialMult = multiplier;
        prosperityBuff.popMult = multiplier;
        // 注意：如果你底层民怨是加法增加，这里就是给 angerAdd 赋负值来抵消
        prosperityBuff.angerAdd = -((currentProsperity / 100) * 1);
    }
}