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

    private SelectHorizontalScrollItem[] items;
    private float displayWidth;
    private int offsetTimes;
    private bool isDrag;
    private int currentItemIndex;
    private float[] distances;
    private float selectItemX;
    private bool isSelectMove;

    private bool isSelected;

    private void Awake()
    {
        displayNumber = itemInfos.Length;
    }

    private void Start()
    {
        Init();
        MoveItems(0);
    }

    private void Update()
    {
        if (!isDrag)
            Adsorption();
        int currentOffsetTimes = Mathf.FloorToInt(itemParent.localPosition.x / itemSpace);
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
    /// 设置子物体信息
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
        for (int i = 0; i < displayNumber; i++)
        {
            float x = itemSpace * (i - offsetTimes) - displayWidth / 2;
            items[i].transform.localPosition = new Vector2(x, items[i].rectTransform.localPosition.y);
        }

        int middle;
        if(offsetTimes > 0)
            middle = itemInfos.Length - offsetTimes % itemInfos.Length;
        else
            middle = -offsetTimes % itemInfos.Length;
        
        int infoIndex = middle;
        for (int i = Mathf.FloorToInt(displayNumber/2f); i < displayNumber; i++)
        {
            if(infoIndex >= itemInfos.Length)
                infoIndex = 0;
            items[i].SetInfo(itemInfos[infoIndex].sprite,itemInfos[infoIndex].name,itemInfos[infoIndex].description,infoIndex,this);
            infoIndex++;
        }
        infoIndex = middle - 1;
        for (int i = Mathf.FloorToInt(displayNumber/2f) - 1; i >=0; i--)
        {
            if(infoIndex <= -1)
                infoIndex = itemInfos.Length -1;
            items[i].SetInfo(itemInfos[infoIndex].sprite,itemInfos[infoIndex].name,itemInfos[infoIndex].description,infoIndex,this);
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
            float distance = itemParent.localPosition.x % itemSpace;
            int times = Mathf.FloorToInt(itemParent.localPosition.x / itemSpace);
            if (distance > 0)
            {
                if(distance < itemSpace /2)
                    targetX = times * itemSpace;
                else
                    targetX = (times + 1) * itemSpace;
            }
            else
            {
                if(distance < -itemSpace / 2)
                    targetX = times * itemSpace;
                else
                    targetX = (times + 1) * itemSpace;
            }
        }
        else
            targetX = -selectItemX;
        
        itemParent.localPosition = new Vector2(Mathf.Lerp(itemParent.localPosition.x,targetX,moveSmooth / 10),itemParent.localPosition.y);
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
}
