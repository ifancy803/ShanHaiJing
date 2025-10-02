using DG.Tweening;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    public GameObject imageListPanel;
    [HideInInspector]public int picIndex = -1;

    [Header("广播")] public ObjectEventSO closeImageListPanelEvent;
    
    [ContextMenu("打开角色选择面板")]
    public void OpenImageListPanel() { ToggleScale(imageListPanel,Ease.OutElastic,true,1); }

    [ContextMenu("关闭角色选择面板")]
    public void CloseImageListPanel()
    {
        closeImageListPanelEvent.RaiseEvent(null,this);
        ToggleScale(imageListPanel,Ease.InElastic,false,1); 
    }
    
    /// <summary>
    /// 放大 Panel
    /// </summary>
    private void ScaleUp(GameObject obj,Ease et,float duration)
    {
        if(obj == null) return;
        obj.SetActive(true);
        obj.transform.localScale = Vector3.zero;
        // 停止当前可能在播放的动画
        obj.transform.DOKill();
        
        obj.transform.DOScale(Vector3.one,duration).SetEase(et);
    }

    /// <summary>
    /// 缩回原始大小
    /// </summary>
    private void ScaleDown(GameObject obj,Ease et,float duration)
    {
        if (obj == null) return;
        imageListPanel.SetActive(true);
        obj.transform.localScale = Vector3.one;
        obj.transform.DOKill();
        obj.transform.DOScale(Vector3.zero, duration).SetEase(et).OnComplete(() => obj.SetActive(false));
    }

    /// <summary>
    /// 可直接切换缩放
    /// </summary>
    public void ToggleScale(GameObject obj,Ease et,bool scaleUp,float duration)
    {
        if(scaleUp)
            ScaleUp(obj,et,duration);
        else
            ScaleDown(obj,et,duration);
    }

    public void SetPicIndex(int index)
    {
        picIndex = index;
        Debug.Log(picIndex);
    }
}
