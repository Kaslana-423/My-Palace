using System.Collections.Generic;
using UnityEngine;

public class PolicyUIManager : MonoBehaviour
{
    public GameObject policyPanel; // 遮罩底板
    public List<PolicyCardUI> cardUIs; // 必须正好 3 个

    private void Awake()
    {
        if (policyPanel != null) policyPanel.SetActive(false);
    }

    public void ShowPolicies(List<PolicyData> choices)
    {
        if (choices.Count != 3 || cardUIs.Count != 3)
        {
            Debug.LogError("[PolicyUI] 国策数量与卡牌槽位数量不匹配（必须是3）！");
            return;
        }

        for (int i = 0; i < 3; i++)
        {
            cardUIs[i].InitCard(choices[i]);
        }

        policyPanel.SetActive(true);
    }

    public void HidePanel()
    {
        policyPanel.SetActive(false);
    }
}