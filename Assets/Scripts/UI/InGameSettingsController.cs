using UnityEngine;
using UnityEngine.SceneManagement;

public class InGameSettingsController : MonoBehaviour
{
    [Header("拖入黑底面板 SettingsPanel")]
    public GameObject settingsPanel;

    private bool isPaused = false;

    private void Start()
    {
        // 游戏开始时确保它是隐藏的
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        // 随时监听 Esc 键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        settingsPanel.SetActive(true);
        Time.timeScale = 0f; // 冻结时间
        isPaused = true;
    }

    public void ResumeGame()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1f; // 恢复时间
        isPaused = false;
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f; // 退出前必须恢复时间
        SceneManager.LoadScene("MainMenu"); 
    }
}