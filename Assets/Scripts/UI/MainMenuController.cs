using UnityEngine;
using UnityEngine.SceneManagement; // 必须加上这句，这是负责切场景的“老司机”

public class MainMenuController : MonoBehaviour
{
    // 绑定给“开始游戏（肇建新城）”按钮
    public void StartNewGame()
    {
        // ⚠️ 极其重要：把引号里的 "GameScene" 换成你实际游玩场景的具体名字！
        SceneManager.LoadScene("SampleScene"); 
    }

    // 绑定给“退出游戏（封卷暂歇）”按钮
    public void QuitGame()
    {
        // 顺便在控制台打印一句话，因为在 Unity 编辑器里按退出是看不出效果的
        Debug.Log("封卷暂歇，游戏正在退出..."); 
        Application.Quit();
    }
}