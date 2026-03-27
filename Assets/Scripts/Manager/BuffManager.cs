using System.Collections.Generic;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance { get; private set; }

    public List<GlobalBuff> activeBuffs = new List<GlobalBuff>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
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
}