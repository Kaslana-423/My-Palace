using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class Basic : MonoBehaviour
{
    public TextMeshProUGUI coinText; // 显示金币数量的 UI 文本
    public TextMeshProUGUI manpowerText; // 显示人力数量的 UI 文本
    public TextMeshProUGUI materialText; // 显示材料数量的 UI 文本
    public TextMeshProUGUI populationText; // 显示人口数量的 UI 文本
    public TextMeshProUGUI prosperityText; // 显示繁荣度的 UI 文本
    public TextMeshProUGUI personAngerText; // 显示民怨值(百分比)的 UI 文本
    public TextMeshProUGUI roundsText; // 显示轮数的 UI 文本

    private void Start()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            RefreshCoinDisplay(GameManager.Instance.Coins);
            GameManager.Instance.ManpowerChanged += OnManpowerChanged;
            RefreshManpowerDisplay(GameManager.Instance.Manpower);
            GameManager.Instance.MaterialsChanged += OnMaterialsChanged;
            RefreshMaterialDisplay(GameManager.Instance.Materials);
            GameManager.Instance.PopulationChanged += OnPopulationChanged;
            RefreshPopulationDisplay(GameManager.Instance.Population);
            GameManager.Instance.ProsperityChanged += OnProsperityChanged;
            RefreshProsperityDisplay(GameManager.Instance.Prosperity);
            GameManager.Instance.PersonAngerChanged += OnPersonAngerChanged;
            RefreshPersonAngerDisplay(GameManager.Instance.PersonAnger);
            GameManager.Instance.RoundsChanged += OnRoundsChanged;
            RefreshRoundsDisplay(GameManager.Instance.Rounds);
        }
    }

    private void OnDisable()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.ManpowerChanged -= OnManpowerChanged;
            GameManager.Instance.MaterialsChanged -= OnMaterialsChanged;
            GameManager.Instance.PopulationChanged -= OnPopulationChanged;
            GameManager.Instance.ProsperityChanged -= OnProsperityChanged;
            GameManager.Instance.PersonAngerChanged -= OnPersonAngerChanged;
            GameManager.Instance.RoundsChanged -= OnRoundsChanged;
        }
    }

    private void OnCoinsChanged(int currentCoins)
    {
        RefreshCoinDisplay(currentCoins);
    }

    private void OnManpowerChanged(int currentManpower)
    {
        RefreshManpowerDisplay(currentManpower);
    }

    private void OnMaterialsChanged(int currentMaterials)
    {
        RefreshMaterialDisplay(currentMaterials);
    }

    private void OnPopulationChanged(int currentPopulation)
    {
        RefreshPopulationDisplay(currentPopulation);
    }

    private void OnProsperityChanged(int currentProsperity)
    {
        RefreshProsperityDisplay(currentProsperity);
    }

    private void OnPersonAngerChanged(int currentPersonAnger)
    {
        RefreshPersonAngerDisplay(currentPersonAnger);
    }

    private void OnRoundsChanged(int currentRounds)
    {
        RefreshRoundsDisplay(currentRounds);
    }
    private void RefreshRoundsDisplay(int value)
    {
        if (roundsText == null)
        {
            return;
        }
        Debug.Log($"刷新轮数显示: {value}");
        roundsText.text = "第 " + value.ToString() + " 轮";
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

    private void RefreshManpowerDisplay(int value)
    {
        if (manpowerText == null)
        {
            return;
        }
        Debug.Log($"刷新人力显示: {value}");
        manpowerText.text = value.ToString();
    }

    private void RefreshMaterialDisplay(int value)
    {
        if (materialText == null)
        {
            return;
        }
        Debug.Log($"刷新材料显示: {value}");
        materialText.text = value.ToString();
    }

    private void RefreshPopulationDisplay(int value)
    {
        if (populationText == null)
        {
            return;
        }
        Debug.Log($"刷新人口显示: {value}");
        populationText.text = value.ToString();
    }

    private void RefreshProsperityDisplay(int value)
    {
        if (prosperityText == null)
        {
            return;
        }
        Debug.Log($"刷新繁荣度显示: {value}");
        prosperityText.text = value.ToString();
    }

    private void RefreshPersonAngerDisplay(int value)
    {
        if (personAngerText == null)
        {
            return;
        }
        Debug.Log($"刷新民怨显示: {value}%");
        personAngerText.text = value.ToString() + "%";
    }
}
