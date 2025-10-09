using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageListPanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    #region 数据结构与枚举

    [System.Serializable]
    private struct ItemInfo
    {
        public string name;
        public Sprite sprite;
        
        [Header("图片描述")]
        [TextArea] public string description;
        
        [Header("图片介绍")]
        [TextArea] public string displayDescription;
        
        public ItemInfo(Sprite sprite, string description, string name, string displayDescription)
        {
            this.sprite = sprite;
            this.description = description;
            this.name = name;
            this.displayDescription = displayDescription;
        }
    }

    public enum ScreenAdaptationMode
    {
        FitWidth,   // 适配宽度
        FitHeight,  // 适配高度
        FitBoth,    // 同时适配宽高
        None        // 不进行适配
    }

    #endregion

    #region Inspector 属性

    [Header("UI元素")]
    [SerializeField, Tooltip("列表子物体对象预制体")] private GameObject itemPrefab;
    [SerializeField, Tooltip("子物体的父物体")] private RectTransform itemParent;
    [SerializeField, Tooltip("描述文字")] private TMP_Text descriptionText;
    [SerializeField, Tooltip("简介文字")] private TMP_Text displayDescriptionText;
    [SerializeField, Tooltip("名字文字")] private TMP_Text nameText;
    [SerializeField, Tooltip("描述面板")] public GameObject descriptionBar;
    [SerializeField, Tooltip("角色精灵")] public Image characterImage;

    [Header("列表数据")]
    [SerializeField, Tooltip("子物体信息")] private ItemInfo[] itemInfos;
    [SerializeField, Tooltip("显示数量(尽量填奇数)")] private int displayNumber;

    [Header("滚动配置")]
    [SerializeField, Tooltip("选项间隔")] private float itemSpace;
    [SerializeField, Tooltip("移动插值")] private float moveSmooth;
    [SerializeField, Tooltip("拖动速度")] private float dragSpeed;
    [SerializeField, Tooltip("缩放倍率")] private float scaleMultiplying;
    [SerializeField, Tooltip("透明度倍率")] private float alphaMultiplying;

    [Header("屏幕适配")]
    [SerializeField, Tooltip("是否启用屏幕适配")] private bool enableScreenAdaptation = true;
    [SerializeField, Tooltip("屏幕适配模式")] private ScreenAdaptationMode adaptationMode = ScreenAdaptationMode.FitWidth;
    [SerializeField, Tooltip("最小缩放比例")] private float minScale = 0.5f;
    [SerializeField, Tooltip("最大缩放比例")] private float maxScale = 2.0f;

    [Header("广播")] public IntEventSO sendIndexToPuzzle;

    #endregion

    #region 私有变量

    private SelectHorizontalScrollItem[] items;
    private float displayWidth;
    private int offsetTimes;
    private bool isDrag;
    private int currentItemIndex;
    private float[] distances;
    private float selectItemX;
    private bool isSelectMove;

    private bool isOpenDescription = false;

    // 屏幕适配相关变量
    private float screenScale = 1f;
    private Vector2 originalScreenSize;
    private Vector2 currentScreenSize;
    private bool isClick = false;
    
    public UIImageDissolveController dissolveController;
    

    #endregion

    #region Unity 生命周期

    private void Awake()
    {
        displayNumber = itemInfos.Length;
        originalScreenSize = new Vector2(Screen.width, Screen.height);
        currentScreenSize = originalScreenSize;
        dissolveController = GetComponent<UIImageDissolveController>();
    }

    private void OnEnable()
    {
        if (items == null || items.Length == 0) Init();
        isClick = false;
        CalculateScreenScale();
        RefreshUI();
        if(UIManager.Instance.picIndexList.Count > 0)
            SetCenterItem((int)UIManager.Instance.picIndexList[^1]);
        dissolveController.ResetDissolve();
    }

    private void Update()
    {
        if (!isDrag) Adsorption();

        // 修复 offsetTimes 计算
        int currentOffsetTimes = Mathf.RoundToInt(itemParent.localPosition.x / (itemSpace * screenScale));
        if (currentOffsetTimes != offsetTimes)
        {
            offsetTimes = currentOffsetTimes;
            MoveItems(offsetTimes);
        }
        
        ItemControl();
    }

    #endregion

    #region 初始化

    private void Init()
    {
        if (items != null) return;
        
        displayWidth = (displayNumber - 1) * itemSpace;
        items = new SelectHorizontalScrollItem[displayNumber];

        for (int i = 0; i < displayNumber; i++)
        {
            var item = Instantiate(itemPrefab, itemParent).GetComponent<SelectHorizontalScrollItem>();
            item.itemIndex = i;
            items[i] = item;
        }
        
        foreach (var item in items)
        {
            item.blurController.SetBlurRadius(20);
            item.canClick = false;
        }
    }

    #endregion

    #region 屏幕适配

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
            case ScreenAdaptationMode.FitWidth: screenScale = scaleX; break;
            case ScreenAdaptationMode.FitHeight: screenScale = scaleY; break;
            case ScreenAdaptationMode.FitBoth: screenScale = Mathf.Min(scaleX, scaleY); break;
        }

        screenScale = Mathf.Clamp(screenScale, minScale, maxScale);
        ApplyScreenScale();
    }

    private void ApplyScreenScale()
    {
        if (itemParent != null)
            itemParent.localScale = Vector3.one * screenScale;
    }

    #endregion

    #region UI 刷新与滚动逻辑

    public void RefreshUI()
    {
        //if (items == null || items.Length == 0) Init();

        MoveItems(offsetTimes);
        ItemControl();
    }

    private void MoveItems(int offsetTimes)
    {
        float scaledItemSpace = itemSpace * screenScale;
        float scaledDisplayWidth = displayWidth * screenScale;

        for (int i = 0; i < displayNumber; i++)
        {
            float x = scaledItemSpace * (i - offsetTimes) - scaledDisplayWidth / 2;
            items[i].transform.localPosition = new Vector2(x, items[i].rectTransform.localPosition.y);
        }

        // 计算中间索引
        int middle = offsetTimes >= 0
            ? itemInfos.Length - (offsetTimes % itemInfos.Length)
            : (-offsetTimes) % itemInfos.Length;

        // 设置右半部分
        int infoIndex = middle;
        for (int i = Mathf.FloorToInt(displayNumber / 2f); i < displayNumber; i++)
        {
            if (infoIndex >= itemInfos.Length) infoIndex = 0;
            items[i].SetInfo(
                itemInfos[infoIndex].sprite, 
                itemInfos[infoIndex].name, 
                itemInfos[infoIndex].description, 
                itemInfos[infoIndex].displayDescription,
                infoIndex,  // 确保这里传递正确的索引
                this
            );
            infoIndex++;
        }

        // 设置左半部分
        infoIndex = middle - 1;
        for (int i = Mathf.FloorToInt(displayNumber / 2f) - 1; i >= 0; i--)
        {
            if (infoIndex < 0) infoIndex = itemInfos.Length - 1;
            items[i].SetInfo(
                itemInfos[infoIndex].sprite, 
                itemInfos[infoIndex].name, 
                itemInfos[infoIndex].description,
                itemInfos[infoIndex].displayDescription,
                infoIndex,  // 确保这里传递正确的索引
                this
            );
            infoIndex--;
        }
        UpdateItemsUnlockState();
    }

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
        displayDescriptionText.text = items[minIndex].displayDescription;
        characterImage.sprite = items[minIndex].image.sprite;
        nameText.text = items[minIndex].nameText.text + "信息";
    
        // 修复：使用 items[minIndex].infoIndex 而不是 items[minIndex].itemIndex
        currentItemIndex = items[minIndex].infoIndex; // 这里应该是 infoIndex，不是 itemIndex
    }

    private void Adsorption()
    {
        float targetX;

        if (!isSelectMove)
        {
            float currentX = itemParent.localPosition.x;
            float scaledItemSpace = itemSpace * screenScale;
            float remainder = currentX % scaledItemSpace;

            targetX = Mathf.Abs(remainder) < scaledItemSpace * 0.5f
                ? currentX - remainder
                : currentX + (scaledItemSpace - remainder) * Mathf.Sign(remainder);
        }
        else
        {
            targetX = -selectItemX;
        }

        itemParent.localPosition = new Vector2(
            Mathf.Lerp(itemParent.localPosition.x, targetX, moveSmooth / 10),
            itemParent.localPosition.y
        );
    }

    #endregion

    #region 用户交互

    public void OnPointerUp(PointerEventData eventData)
    {
        if(isOpenDescription||isClick)
            return;
        EventSystem.current.SetSelectedGameObject(gameObject);
        isDrag = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(isOpenDescription||isClick)
            return;
        EventSystem.current.SetSelectedGameObject(gameObject);
        isDrag = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(isOpenDescription || isClick)
            return;
        EventSystem.current.SetSelectedGameObject(gameObject);
        isSelectMove = false;
        itemParent.localPosition = new Vector2(itemParent.localPosition.x + eventData.delta.x * dragSpeed, itemParent.localPosition.y);
    }

    public void Select(int itemIndex, int infoIndex, RectTransform itemRectTransform, bool canClick)
    {
        // 只处理当前中心项
        if (infoIndex != currentItemIndex)
        {
            isSelectMove = true;
            selectItemX = itemRectTransform.localPosition.x;
            return;
        }
    
        // 是中心项，检查是否可以点击
        if (canClick)
        {
            isOpenDescription = true;
            UIManager.Instance.ToggleScale(descriptionBar, Ease.InOutQuart, true, 0.5f);
        }
        else
        {
            if(isClick)return;
            isClick = true;
            UnLockItemsBlur(UIManager.Instance.picIndexList);
            sendIndexToPuzzle.RaiseEvent(infoIndex, this);
            //TODO:关闭panel的溶解shader
            StartCoroutine(StartDissolve());
        }
    }
    
    IEnumerator StartDissolve()
    {
        
        // 溶解完成后执行特定方法
        dissolveController.StartDissolve(2f, () => 
        {
            UIManager.Instance.CloseImageListPanel();
        });
        
        yield break;
    }

    private void UnLockItemsBlur(ArrayList countList)
    {
        UpdateItemsUnlockState();
    }
    
    private void UpdateItemsUnlockState()
    {
        if (UIManager.Instance.picIndexList == null || UIManager.Instance.picIndexList.Count == 0)
            return;

        foreach (var item in items)
        {
            // 检查当前item的infoIndex是否在解锁列表中
            bool unlocked = false;
            foreach (int unlockedIndex in UIManager.Instance.picIndexList)
            {
                if (item.infoIndex == unlockedIndex)
                {
                    unlocked = true;
                    break;
                }
            }

            if (unlocked)
            {
                item.blurController.SetMosaicSize(1);
                item.canClick = true;
            }
            else
            {
                item.blurController.SetMosaicSize(80);
                item.canClick = false;
            }
        }
    }

    public void SetCenterItem(int targetIndex)
    {
        var targetItem = items.FirstOrDefault(i => i.infoIndex == targetIndex);

        if (targetItem == null)
        {
            if(targetIndex >= 0)
                Debug.LogWarning($"未找到 infoIndex = {targetIndex} 的 item");
            return;
        }

        selectItemX = targetItem.rectTransform.localPosition.x;
        isSelectMove = true;
        Adsorption();
    }

    #endregion

    public void CloseContainer(object value)
    {
        descriptionBar.transform.localScale = Vector3.one;
        descriptionBar.transform.DOKill();
        descriptionBar.transform.DOScale(Vector3.zero,0.5f).SetEase(Ease.InOutQuart).OnComplete(() =>
        {
            descriptionBar.SetActive(false);
        });
        isOpenDescription = false;
    }
}