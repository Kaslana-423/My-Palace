using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PolicyCardUI : MonoBehaviour
{
    [Header("正面组件")]
    public GameObject frontPanel;
    public Image illustrationImage;
    public TextMeshProUGUI titleTextFront;
    public TextMeshProUGUI buffDescText;
    public Button selectButton; // 选定国策的按钮

    [Header("背面组件")]
    public GameObject backPanel;
    public TextMeshProUGUI titleTextBack;
    public TextMeshProUGUI backgroundStoryText;
    
    [Header("翻转控制")]
    public Button flipButton; // 点击卡牌进行翻转的按钮

    private PolicyData currentData;
    private bool isShowingBack = false;
    private bool isFlipping = false;

    private void Awake()
    {
        // Debug排查 1：检查按钮是否成功赋值
        if (flipButton != null)
        {
            flipButton.onClick.AddListener(FlipCard);
            Debug.Log($"<color=#00FF00>[PolicyCardUI] {gameObject.name} 的 FlipButton 绑定成功！</color>");
        }
        else
        {
            Debug.LogError($"<color=red>[PolicyCardUI] {gameObject.name} 的 FlipButton 丢失（未在 Inspector 中拖拽赋值）！</color>");
        }

        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnSelectClicked);
        }
        else
        {
            Debug.LogError($"<color=red>[PolicyCardUI] {gameObject.name} 的 SelectButton 丢失！</color>");
        }
    }

    public void InitCard(PolicyData data)
    {
        currentData = data;
        
        if (titleTextFront != null) titleTextFront.text = data.policyName;
        if (buffDescText != null) buffDescText.text = data.buffDesc;
        if (illustrationImage != null && data.illustration != null) illustrationImage.sprite = data.illustration;

        if (titleTextBack != null) titleTextBack.text = data.policyName;
        if (backgroundStoryText != null) backgroundStoryText.text = data.backgroundStory;

        isShowingBack = false;
        isFlipping = false;
        transform.rotation = Quaternion.identity; 
        frontPanel.SetActive(true);
        backPanel.SetActive(false);
    }

    private void FlipCard()
    {
        // Debug排查 2：确认代码是否接收到了鼠标点击
        Debug.Log($"<color=#00FFFF>[PolicyCardUI] 收到 FlipButton 的点击事件！当前是否正在翻转中: {isFlipping}</color>");

        if (isFlipping) return;
        StartCoroutine(FlipRoutine());
    }

    private IEnumerator FlipRoutine()
    {
        Debug.Log($"<color=yellow>[PolicyCardUI] 开始播放翻转动画...</color>");
        
        isFlipping = true;
        float duration = 0.4f;
        float time = 0f;
        
        Quaternion startRot = transform.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0f, 180f, 0f); 

        bool targetShowingBack = !isShowingBack;

        while (time < duration)
        {
            // 防踩坑：这里改成 unscaledDeltaTime，无视游戏暂停时间（Time.timeScale = 0）
            time += Time.unscaledDeltaTime; 
            float t = time / duration;
            
            float smoothT = t * t * (3f - 2f * t); 
            transform.rotation = Quaternion.Slerp(startRot, endRot, smoothT);

            if (t >= 0.5f && isShowingBack != targetShowingBack)
            {
                isShowingBack = targetShowingBack;
                frontPanel.SetActive(!isShowingBack);
                backPanel.SetActive(isShowingBack);
                Debug.Log($"<color=yellow>[PolicyCardUI] 动画进度过半，切换正反面显示状态方案：背面显示为 {isShowingBack}</color>");
            }

            yield return null;
        }

        transform.rotation = endRot;
        isFlipping = false;
        Debug.Log($"<color=yellow>[PolicyCardUI] 翻转动画播放完毕！</color>");
    }

    private void OnSelectClicked()
    {
        Debug.Log($"<color=orange>[PolicyCardUI] 点击了颁布国策！准备通知 Manager。</color>");
        if (PolicyManager.Instance != null && currentData != null)
        {
            PolicyManager.Instance.ConfirmPolicy(currentData);
        }
    }
}