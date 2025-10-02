using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject imageListPanel;

    [ContextMenu("打开角色选择面板")]
    public void OpenImageListPanel()
    {
        imageListPanel.SetActive(true);
    }
}
