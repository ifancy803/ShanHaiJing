using UnityEngine;
using UnityEngine.UI;

public class ToggleAIChatWindow : MonoBehaviour
{
    [Header("要控制的AI对话框")]
    public GameObject aiChatWindow;

    private Button _button;

    void Start()
    {
        // 获取按钮组件
        _button = GetComponent<Button>();
        if (_button != null)
        {
            // 点击时调用Toggle方法
            _button.onClick.AddListener(ToggleWindow);
        }
    }

    // 切换窗口的显示/隐藏
    void ToggleWindow()
    {
        if (aiChatWindow != null)
        {
            // 取反当前的active状态：显示变隐藏，隐藏变显示
            bool isActive = aiChatWindow.activeSelf;
            aiChatWindow.SetActive(!isActive);
        }
    }
}