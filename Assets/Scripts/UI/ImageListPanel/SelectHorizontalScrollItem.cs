using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectHorizontalScrollItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField,Tooltip("图片")] 
    private Image image;
    [SerializeField,Tooltip("名称文字")] 
    public TMP_Text nameText;
    [SerializeField,Tooltip("画布组")] 
    private CanvasGroup canvasGroup;

    [Tooltip("选项索引")] public int itemIndex;
    [Tooltip("信息索引")] public int infoIndex;
    [Tooltip("描述文本")] public string description;
    [Tooltip("简介文字")] public string displayDescription;

    private ImageListPanel selectHorizontalScroll;
    [HideInInspector] public RectTransform rectTransform;

    private bool isDrag;
    public bool canClick = false;
    
    public BlurController blurController;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    /// 设置信息
    /// </summary>
    /// <param name="sprite">图片</param>
    /// <param name="name">图片名称</param>
    /// <param name="description">图片描述</param>
    /// <param name="infoIndex">信息索引</param>
    /// <param name="selectHorizontalScroll">水平滚动选择列表脚本</param>
    public void SetInfo(Sprite sprite, string name, string description,string displayDescription, int infoIndex, ImageListPanel selectHorizontalScroll)
    {
        image.sprite = sprite;
        nameText.text = name;
        this.description = description;
        this.infoIndex = infoIndex;
        this.selectHorizontalScroll = selectHorizontalScroll;
        this.displayDescription =  displayDescription;
    }

    /// <summary>
    /// 改变图片透明度
    /// </summary>
    /// <param name="alpha">透明度</param>
    public void SetAlpha(float alpha)
    {
        canvasGroup.alpha = alpha;
    }

    /// <summary>
    /// 鼠标按下时(未拖拽)
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = false;
        selectHorizontalScroll.OnPointerDown(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(!isDrag)
            selectHorizontalScroll.Select(itemIndex, infoIndex,rectTransform,canClick);
        selectHorizontalScroll.OnPointerUp(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        isDrag = true;
        selectHorizontalScroll.OnDrag(eventData);
    }
}
