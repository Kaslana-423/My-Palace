using UnityEngine;
using TMPro;

public class EventTooltip : MonoBehaviour
{
    [Tooltip("显示描述的文本组件")]
    public TextMeshProUGUI tipText;
    
    [Tooltip("相对于鼠标指针右上角的偏移量")]
    public Vector2 offset = new Vector2(15f, -15f); 

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // 初始化时隐藏
        gameObject.SetActive(false);
    }

    private void Update()
    {
        // 实时跟随鼠标位置
        // 使用 Input.mousePosition 获取屏幕坐标
        transform.position = (Vector2)Input.mousePosition + offset;
    }

    public void ShowTooltip(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        
        tipText.text = text;
        gameObject.SetActive(true);
        
        // 确保在一激活时立刻更新一次位置，防止闪烁
        transform.position = (Vector2)Input.mousePosition + offset;
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}