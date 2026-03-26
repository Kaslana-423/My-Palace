using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Basic : MonoBehaviour
{
    [Header("资源栏布局")]
    public GridLayoutGroup resourceGridLayout;
    public bool applyLayoutOnStart = true;
    public bool applyLayoutOnValidate = true;
    public Vector2 itemCellSize = new Vector2(200f, 80f);
    public Vector2 itemSpacing = new Vector2(24f, 0f);

    [Header("显示格式")]
    public string coinFormat = "{0}";
    public string manpowerFormat = "{0}";
    public string materialFormat = "{0}";
    public string populationFormat = "{0}";
    public string prosperityFormat = "{0}";
    public string personAngerFormat = "{0}%";
    public string roundsFormat = "第 {0} 轮";

    public TextMeshProUGUI coinText; // 显示金币数量的 UI 文本
    public TextMeshProUGUI manpowerText; // 显示人力数量的 UI 文本
    public TextMeshProUGUI materialText; // 显示材料数量的 UI 文本
    public TextMeshProUGUI populationText; // 显示人口数量的 UI 文本
    public TextMeshProUGUI prosperityText; // 显示繁荣度的 UI 文本
    public TextMeshProUGUI personAngerText; // 显示民怨值(百分比)的 UI 文本
    public TextMeshProUGUI roundsText; // 显示轮数的 UI 文本

    private void OnEnable()
    {
        AutoBindTextIfMissing();
        TryApplyLayout();

        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged += OnCoinsChanged;
            RefreshCoinDisplay(GameManager.Instance.Coins);
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

    private void OnValidate()
    {
        AutoBindTextIfMissing();

        if (!Application.isPlaying && applyLayoutOnValidate)
        {
            TryApplyLayout();
        }
    }

    private void OnDisable()
    {
        if (GameManager.HasInstance)
        {
            GameManager.Instance.CoinsChanged -= OnCoinsChanged;
            GameManager.Instance.MaterialsChanged -= OnMaterialsChanged;
            GameManager.Instance.PopulationChanged -= OnPopulationChanged;
            GameManager.Instance.ProsperityChanged -= OnProsperityChanged;
            GameManager.Instance.PersonAngerChanged -= OnPersonAngerChanged;
            GameManager.Instance.RoundsChanged -= OnRoundsChanged;
        }
    }

    [ContextMenu("Basic/自动绑定文本引用")]
    private void AutoBindTextIfMissing()
    {
        coinText = coinText != null ? coinText : FindTextByPaths("Coin/Coin/Text (TMP)", "Coin/Text (TMP)");
        manpowerText = manpowerText != null ? manpowerText : FindTextByPaths("Manpower/Manpower/Text (TMP)", "Manpower/Text (TMP)");
        materialText = materialText != null ? materialText : FindTextByPaths("Material/Material/Text (TMP)", "Material/Text (TMP)");
        populationText = populationText != null ? populationText : FindTextByPaths("Population/Human/Text (TMP)", "Population/Text (TMP)");
        prosperityText = prosperityText != null ? prosperityText : FindTextByPaths("Prosperity/Prosperity/Text (TMP)", "Prosperity/Text (TMP)");
        personAngerText = personAngerText != null ? personAngerText : FindTextByPaths("PeopleAnger/PpAg/Text (TMP)", "PeopleAnger/Text (TMP)");
        roundsText = roundsText != null ? roundsText : FindTextByPaths("Rounds/Rounds/Text (TMP)", "Rounds/Text (TMP)");
    }

    [ContextMenu("Basic/强制刷新实时资源")]
    private void ForceRefreshNow()
    {
        if (!GameManager.HasInstance)
        {
            Debug.LogWarning("[Basic] GameManager 不存在，无法刷新实时资源");
            return;
        }

        RefreshCoinDisplay(GameManager.Instance.Coins);
        RefreshMaterialDisplay(GameManager.Instance.Materials);
        RefreshPopulationDisplay(GameManager.Instance.Population);
        RefreshProsperityDisplay(GameManager.Instance.Prosperity);
        RefreshPersonAngerDisplay(GameManager.Instance.PersonAnger);
        RefreshRoundsDisplay(GameManager.Instance.Rounds);
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
        roundsText.text = string.Format(roundsFormat, value);
    }
    private void RefreshCoinDisplay(int value)
    {
        if (coinText == null)
        {
            return;
        }
        Debug.Log($"刷新金币显示: {value}");
        coinText.text = string.Format(coinFormat, value);
    }

    private void RefreshManpowerDisplay(int value)
    {
        if (manpowerText == null)
        {
            return;
        }
        Debug.Log($"刷新人力显示: {value}");
        manpowerText.text = string.Format(manpowerFormat, value);
    }

    private void RefreshMaterialDisplay(int value)
    {
        if (materialText == null)
        {
            return;
        }
        Debug.Log($"刷新材料显示: {value}");
        materialText.text = string.Format(materialFormat, value);
    }

    private void RefreshPopulationDisplay(int value)
    {
        if (populationText == null)
        {
            return;
        }
        Debug.Log($"刷新人口显示: {value}");
        populationText.text = string.Format(populationFormat, value);
    }

    private void RefreshProsperityDisplay(int value)
    {
        if (prosperityText == null)
        {
            return;
        }
        Debug.Log($"刷新繁荣度显示: {value}");
        prosperityText.text = string.Format(prosperityFormat, value);
    }

    private void RefreshPersonAngerDisplay(int value)
    {
        if (personAngerText == null)
        {
            return;
        }
        Debug.Log($"刷新民怨显示: {value}%");
        personAngerText.text = string.Format(personAngerFormat, value);
    }

    private void TryApplyLayout()
    {
        if (!applyLayoutOnStart && Application.isPlaying)
        {
            return;
        }

        GridLayoutGroup grid = resourceGridLayout;
        if (grid == null)
        {
            grid = GetComponent<GridLayoutGroup>();
        }

        if (grid == null)
        {
            return;
        }

        grid.cellSize = itemCellSize;
        grid.spacing = itemSpacing;
    }

    private TextMeshProUGUI FindTextByPaths(params string[] paths)
    {
        if (paths == null)
        {
            return null;
        }

        foreach (string path in paths)
        {
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            Transform t = transform.Find(path);
            if (t == null)
            {
                continue;
            }

            TextMeshProUGUI text = t.GetComponent<TextMeshProUGUI>();
            if (text != null)
            {
                return text;
            }
        }

        return null;
    }
}
