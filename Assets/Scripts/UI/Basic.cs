using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Basic : MonoBehaviour
{
    public TextMeshProUGUI coinText; // 显示金币数量的 UI 文本

    private void Start()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            RefreshCoinDisplay(GameManager.Instance.Coins);
        }
    }

    private void OnDisable()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
        }
    }

    private void OnCoinsChanged(int currentCoins)
    {
        RefreshCoinDisplay(currentCoins);
    }

    private void RefreshCoinDisplay()
    {
        if (coinText == null || !GameManager.HasInstance)
        {
            return;
        }

        coinText.text = GameManager.Instance.Coins.ToString();
    }

    private void RefreshCoinDisplay(int value)
    {
        if (coinText == null)
        {
            return;
        }
        Debug.Log($"刷新金币显示: {value}");
        coinText.text = value.ToString();
    }
}
