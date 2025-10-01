using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ImageListPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    /// <summary>
    /// 列表物体信息
    /// </summary>
    [System.Serializable]
    private struct ItemInfo
    {
        public string name;
        public Sprite sprite;
        public string description;

        public ItemInfo(Sprite sprite, string description,string name)
        {
            this.sprite = sprite;
            this.description = description;
            this.name = name;
        }
    }

    [SerializeField,Tooltip("列表子物体对象预制体")] 
    private GameObject itemPrefab;
    [SerializeField,Tooltip("子物体的父物体")]
    private RectTransform itemParent;
    [SerializeField,Tooltip("描述文字")]
    private TMP_Text descriptionText;
    [SerializeField,Tooltip("子物体信息")]
    private ItemInfo[] itemInfos;
    [SerializeField,Tooltip("显示数量(尽量填奇数)")] 
    private int displayNumber;
    [SerializeField,Tooltip("选项间隔")] 
    private float itemSpace;
    [SerializeField,Tooltip("移动插值")] 
    private float moveSmooth;
    [SerializeField,Tooltip("拖动速度")] 
    private float dragSpeed;
    [SerializeField,Tooltip("缩放倍率")] 
    private float scaleMultiplying;
    [SerializeField,Tooltip("透明度倍率")] 
    private float alphaMultiplying;
    [SerializeField,Tooltip("是否启用屏幕适配")] 
    private bool enableScreenAdaptation = true;
    [SerializeField,Tooltip("屏幕适配模式")] 
    private ScreenAdaptationMode adaptationMode = ScreenAdaptationMode.FitWidth;
    [SerializeField,Tooltip("最小缩放比例")] 
    private float minScale = 0.5f;
    [SerializeField,Tooltip("最大缩放比例")] 
    private float maxScale = 2.0f;

    public enum ScreenAdaptationMode
    {
        FitWidth,    // 适配宽度
        FitHeight,    // 适配高度
        FitBoth,     // 同时适配宽高
        None         // 不进行适配
    }

    private SelectHorizontalScrollItem[] items;
    private float displayWidth;
    private int offsetTimes;
    private bool isDrag;
    private int currentItemIndex;
    private float[] distances;
    private float selectItemX;
    private bool isSelectMove;

    private bool isSelected;
    
    // 屏幕适配相关变量
    private float screenScale = 1f;
    private Vector2 originalScreenSize;
    private Vector2 currentScreenSize;

    private void Awake()
    {
        displayNumber = itemInfos.Length;
        // 记录原始屏幕尺寸
        originalScreenSize = new Vector2(Screen.width, Screen.height);
        currentScreenSize = originalScreenSize;
    }

    private void Start()
    {
        Init();
        CalculateScreenScale();
        MoveItems(0);
    }

    private void Update()
    {
        // 检查屏幕尺寸是否发生变化
        CheckScreenSizeChange();
        
        if (!isDrag)
            Adsorption();
        
        // 修复offsetTimes计算，确保吸附后正确更新items
        int currentOffsetTimes = Mathf.RoundToInt(itemParent.localPosition.x / (itemSpace * screenScale));
        if (currentOffsetTimes != offsetTimes)
        {
            offsetTimes = currentOffsetTimes;
            MoveItems(offsetTimes);
        }
        ItemControl();
    }

    
    /// <summary>
    /// 初始化
    /// </summary>
    private void Init()
    {
        displayWidth = (displayNumber -1) * itemSpace;
        items = new SelectHorizontalScrollItem[displayNumber];
        for (var i = 0; i < displayNumber; i++)
        {
            var item = Instantiate(itemPrefab, itemParent).GetComponent<SelectHorizontalScrollItem>();
            item.itemIndex = i;
            items[i] = item;
        }
    }

    /// <summary>
    /// 计算屏幕缩放比例
    /// </summary>
    private void CalculateScreenScale()
    {
        if (!enableScreenAdaptation || adaptationMode == ScreenAdaptationMode.None)
        {
            screenScale = 1f;
            return;
        }

        Vector2 currentScreen = new Vector2(Screen.width, Screen.height);
        float scaleX = currentScreen.x / originalScreenSize.x;
        float scaleY = currentScreen.y / originalScreenSize.y;

        switch (adaptationMode)
        {
            case ScreenAdaptationMode.FitWidth:
                screenScale = scaleX;
                break;
            case ScreenAdaptationMode.FitHeight:
                screenScale = scaleY;
                break;
            case ScreenAdaptationMode.FitBoth:
                screenScale = Mathf.Min(scaleX, scaleY);
                break;
        }

        // 限制缩放范围
        screenScale = Mathf.Clamp(screenScale, minScale, maxScale);
        
        // 应用缩放
        ApplyScreenScale();
    }

    /// <summary>
    /// 应用屏幕缩放
    /// </summary>
    private void ApplyScreenScale()
    {
        if (itemParent != null)
        {
            itemParent.localScale = Vector3.one * screenScale;
        }
    }

    /// <summary>
    /// 检查屏幕尺寸变化
    /// </summary>
    private void CheckScreenSizeChange()
    {
        Vector2 newScreenSize = new Vector2(Screen.width, Screen.height);
        if (newScreenSize != currentScreenSize)
        {
            currentScreenSize = newScreenSize;
            CalculateScreenScale();
        }
    }

    /// <summary>
    /// 设置子物体信息(测试用)
    /// </summary>
    /// <param name="names">名称</param>
    /// <param name="sprites">图片</param>
    /// <param name="descriptions">描述</param>
    public void SetItemInfo(string[] names, Sprite[] sprites, string[] descriptions)
    {
        if (names.Length != sprites.Length || sprites.Length != descriptions.Length || names.Length != descriptions.Length)
        {
            Debug.LogError("选择数据不完整");
            return;
        }
        
        itemInfos = new ItemInfo[names.Length];
        for (var i = 0; i < itemInfos.Length; i++)
        {
            itemInfos[i] = new ItemInfo(sprites[i],descriptions[i],names[i]);
        }

        isSelected = false;
    }

    /// <summary>
    /// 点击选择
    /// </summary>
    /// <param name="itemIndex">点击的选项索引</param>
    /// <param name="infoIndex">点击的选择信息索引</param>
    /// <param name="itemRectTransform">选项坐标</param>
    public void Select(int itemIndex,int infoIndex,RectTransform itemRectTransform)
    {
        if (!isSelected && itemIndex == currentItemIndex)
            isSelected = true;
        else
        {
            isSelectMove = true;
            selectItemX = itemRectTransform.localPosition.x;
        }
    }

    /// <summary>
    /// 移动列表
    /// </summary>
    /// <param name="offsetTimes">偏移次数</param>
    private void MoveItems(int offsetTimes)
    {
        // 计算缩放后的间距
        float scaledItemSpace = itemSpace * screenScale;
        float scaledDisplayWidth = displayWidth * screenScale;
        
        for (int i = 0; i < displayNumber; i++)
        {
            float x = scaledItemSpace * (i - offsetTimes) - scaledDisplayWidth / 2;
            items[i].transform.localPosition = new Vector2(x, items[i].rectTransform.localPosition.y);
        }

        // 修复索引计算逻辑
        int middle;
        if (offsetTimes >= 0)
        {
            middle = itemInfos.Length - (offsetTimes % itemInfos.Length);
        }
        else
        {
            middle = (-offsetTimes) % itemInfos.Length;
        }
        
        // 设置中间及右侧的items
        int infoIndex = middle;
        for (int i = Mathf.FloorToInt(displayNumber/2f); i < displayNumber; i++)
        {
            if (infoIndex >= itemInfos.Length)
                infoIndex = 0;
            items[i].SetInfo(itemInfos[infoIndex].sprite, itemInfos[infoIndex].name, itemInfos[infoIndex].description, infoIndex, this);
            infoIndex++;
        }
        
        // 设置左侧的items
        infoIndex = middle - 1;
        for (int i = Mathf.FloorToInt(displayNumber/2f) - 1; i >= 0; i--)
        {
            if (infoIndex < 0)
                infoIndex = itemInfos.Length - 1;
            items[i].SetInfo(itemInfos[infoIndex].sprite, itemInfos[infoIndex].name, itemInfos[infoIndex].description, infoIndex, this);
            infoIndex--;
        }
    }
    
    /// <summary>
    /// 控制选项的透明度缩放，获取中间选项
    /// </summary>
    private void ItemControl()
    {
        distances = new float[displayNumber];
        for (int i = 0; i < displayNumber; i++)
        {
            float distance = Mathf.Abs(items[i].rectTransform.position.x - transform.position.x);
            distances[i] = distance;
            float scale = 1 - distance * scaleMultiplying;
            items[i].rectTransform.localScale = new Vector3(scale, scale, 1);
            items[i].SetAlpha(1 - distance * alphaMultiplying);
        }
        
        float minDistance = itemSpace * displayNumber;
        int minIndex = 0;
        for (int i = 0; i < displayNumber; i++)
        {
            if (distances[i] < minDistance)
            {
                minDistance = distances[i];
                minIndex = i;
            }
        }
        descriptionText.text = items[minIndex].description;
        currentItemIndex = items[minIndex].itemIndex;
    }
    
    /// <summary>
    /// 自动吸附
    /// </summary>
    private void Adsorption()
    {
        float targetX;
        if (!isSelectMove)
        {
            // 修复吸附计算逻辑，确保吸附到最近的itemSpace位置
            float currentX = itemParent.localPosition.x;
            float scaledItemSpace = itemSpace * screenScale;
            float remainder = currentX % scaledItemSpace;
        
            if (Mathf.Abs(remainder) < scaledItemSpace * 0.5f)
            {
                // 距离当前位置更近
                targetX = currentX - remainder;
            }
            else
            {
                // 距离下一个位置更近
                targetX = currentX + (scaledItemSpace - remainder) * Mathf.Sign(remainder);
            }
        }
        else
            targetX = -selectItemX;
    
        itemParent.localPosition = new Vector2(Mathf.Lerp(itemParent.localPosition.x, targetX, moveSmooth / 10), itemParent.localPosition.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDrag = false;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        isDrag = true;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        isSelectMove = false;
        itemParent.localPosition = new Vector2(itemParent.localPosition.x +
            eventData.delta.x * dragSpeed,itemParent.localPosition.y);
    }

    /// <summary>
    /// 设置屏幕适配模式
    /// </summary>
    /// <param name="mode">适配模式</param>
    public void SetAdaptationMode(ScreenAdaptationMode mode)
    {
        adaptationMode = mode;
        CalculateScreenScale();
    }

    /// <summary>
    /// 启用或禁用屏幕适配
    /// </summary>
    /// <param name="enable">是否启用</param>
    public void SetScreenAdaptation(bool enable)
    {
        enableScreenAdaptation = enable;
        CalculateScreenScale();
    }

    /// <summary>
    /// 设置缩放范围
    /// </summary>
    /// <param name="min">最小缩放</param>
    /// <param name="max">最大缩放</param>
    public void SetScaleRange(float min, float max)
    {
        minScale = min;
        maxScale = max;
        CalculateScreenScale();
    }

    /// <summary>
    /// 获取当前屏幕缩放比例
    /// </summary>
    /// <returns>当前缩放比例</returns>
    public float GetCurrentScreenScale()
    {
        return screenScale;
    }
}
