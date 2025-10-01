using UnityEngine;

public class UIManager : MonoBehaviour
{
    public ImageListPanel imageListPanel;

    [ContextMenu("打开山海经角色列表")]
    public void OpenImageListPanel()
    {
        imageListPanel.RefreshUI();
        imageListPanel.SetCenterItem(2);
    }
}
