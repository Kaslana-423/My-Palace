using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct AuditData
{
    public int deadlineRound;
    public int coinCost;
    public int targetProsperity;
    public int maxAnger;
    public int minPopulation;
}

public class AuditManager : MonoBehaviour
{
    public static AuditManager Instance { get; private set; }

    public List<AuditData> auditTimeline;
    private int currentIndex = 0;

    public event Action<AuditData> OnAuditPassed;
    public event Action<AuditData, string> OnAuditFailed;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        auditTimeline.Sort((a, b) => a.deadlineRound.CompareTo(b.deadlineRound));
    }

    private void Start()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged += CheckAudit;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.RoundsChanged -= CheckAudit;
        }
    }

    private void CheckAudit(int currentRound)
    {
        if (currentIndex >= auditTimeline.Count) return;

        AuditData currentAudit = auditTimeline[currentIndex];

        if (currentRound == currentAudit.deadlineRound)
        {
            bool passed = true;
            string failReason = "";

            if (GameManager.Instance.Prosperity < currentAudit.targetProsperity)
            {
                passed = false;
                failReason += $"繁荣度未达标 (需 {currentAudit.targetProsperity}) ";
            }
            if (GameManager.Instance.PersonAnger > currentAudit.maxAnger)
            {
                passed = false;
                failReason += $"民怨沸腾 (超阈值 {currentAudit.maxAnger}%) ";
            }
            if (GameManager.Instance.Population < currentAudit.minPopulation)
            {
                passed = false;
                failReason += $"人口流失严重 (需 {currentAudit.minPopulation}) ";
            }
            if (GameManager.Instance.Coins < currentAudit.coinCost)
            {
                passed = false;
                failReason += $"库银亏空 (需 {currentAudit.coinCost}) ";
            }

            currentIndex++;

            if (passed)
            {
                GameManager.Instance.TrySpendCoins(currentAudit.coinCost);
                OnAuditPassed?.Invoke(currentAudit);
                Debug.Log("pass!!");
            }
            else
            {
                OnAuditFailed?.Invoke(currentAudit, failReason);
                Debug.Log(failReason);
            }
        }
    }
}