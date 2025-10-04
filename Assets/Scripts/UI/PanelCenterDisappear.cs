using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class  PanelCenterDisappear : MonoBehaviour
{
    [Header("动画设置")]
    public float shrinkDuration = 1f;
    public Ease easeType = Ease.OutCubic;
    
    [Header("柔化设置")]
    [Range(1, 10)]
    public int softnessLayers = 5;
    public float maxShrinkDistance = 300f;
    
    [Header("动画完成后")]
    public bool hidePanelAfterShrink = true; // 收缩后隐藏Panel
    public bool disableMasksAfterShrink = true; // 收缩后禁用遮罩
    
    [Header("测试按钮")]
    public bool testShrink = false;
    public bool testExpand = false;
    public bool resetState = false;
    
    private List<RectTransform> leftMasks;
    private List<RectTransform> rightMasks;
    private List<GameObject> maskObjects; // 保存所有遮罩对象
    private CanvasGroup canvasGroup;
    private Sequence tweenSequence;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        CreateSoftMasks();
    }

    void CreateSoftMasks()
    {
        ClearOldMasks();
        
        leftMasks = new List<RectTransform>();
        rightMasks = new List<RectTransform>();
        maskObjects = new List<GameObject>();
        
        for (int i = 0; i < softnessLayers; i++)
        {
            CreateMaskPair(i);
        }
    }

    void CreateMaskPair(int layerIndex)
    {
        // 创建左边遮罩
        GameObject leftMask = new GameObject($"LeftSoftMask_{layerIndex}");
        leftMask.transform.SetParent(transform);
        //leftMask.transform.SetAsFirstSibling();
        
        Image leftImage = leftMask.AddComponent<Image>();
        leftImage.color = new Color(0, 0, 0, 0.2f);
        
        RectTransform leftRect = leftMask.GetComponent<RectTransform>();
        SetupLeftMask(leftRect, layerIndex);
        leftMasks.Add(leftRect);
        maskObjects.Add(leftMask);
        
        // 创建右边遮罩
        GameObject rightMask = new GameObject($"RightSoftMask_{layerIndex}");
        rightMask.transform.SetParent(transform);
        //rightMask.transform.SetAsFirstSibling();
        
        Image rightImage = rightMask.AddComponent<Image>();
        rightImage.color = new Color(0, 0, 0, 0.2f);
        
        RectTransform rightRect = rightMask.GetComponent<RectTransform>();
        SetupRightMask(rightRect, layerIndex);
        rightMasks.Add(rightRect);
        maskObjects.Add(rightMask);
    }

    void SetupLeftMask(RectTransform rect, int layerIndex)
    {
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 0.5f);
        rect.sizeDelta = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(0, 0);
    }

    void SetupRightMask(RectTransform rect, int layerIndex)
    {
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 0.5f);
        rect.sizeDelta = new Vector2(0, 0);
        rect.anchoredPosition = new Vector2(0, 0);
    }

    void ClearOldMasks()
    {
        if (maskObjects != null)
        {
            foreach (var mask in maskObjects)
            {
                if (mask != null) DestroyImmediate(mask);
            }
        }
        leftMasks?.Clear();
        rightMasks?.Clear();
        maskObjects?.Clear();
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (testShrink)
        {
            testShrink = false;
            ShrinkToCenter();
        }
        
        if (testExpand)
        {
            testExpand = false;
            ExpandFromCenter();
        }
        
        if (resetState)
        {
            resetState = false;
            ResetState();
        }
        #endif
    }

    /// <summary>
    /// 两边往中间收缩 - 修复黑屏问题
    /// </summary>
    public void ShrinkToCenter(float duration = -1, System.Action onComplete = null)
    {
        if (duration <= 0) duration = shrinkDuration;
        
        tweenSequence?.Kill();
        tweenSequence = DOTween.Sequence();
        
        // 确保Panel是激活的
        gameObject.SetActive(true);
        
        // 确保遮罩是激活的
        SetMasksActive(true);
        
        for (int i = 0; i < softnessLayers; i++)
        {
            if (i >= leftMasks.Count || i >= rightMasks.Count) continue;
            
            float layerDistance = maxShrinkDistance * (1f - (float)i / softnessLayers);
            float layerAlpha = 0.8f * (1f - (float)i / softnessLayers);
            float layerDelay = i * 0.05f;
            float layerDuration = duration * 0.8f;
            
            // 左边遮罩
            var leftMask = leftMasks[i];
            var leftImage = leftMask.GetComponent<Image>();
            
            tweenSequence.Insert(layerDelay, leftMask.DOSizeDelta(
                new Vector2(layerDistance, 0), layerDuration).SetEase(easeType));
            tweenSequence.Join(leftImage.DOFade(layerAlpha, layerDuration));
            
            // 右边遮罩
            var rightMask = rightMasks[i];
            var rightImage = rightMask.GetComponent<Image>();
            
            tweenSequence.Insert(layerDelay, rightMask.DOSizeDelta(
                new Vector2(layerDistance, 0), layerDuration).SetEase(easeType));
            tweenSequence.Join(rightImage.DOFade(layerAlpha, layerDuration));
        }
        
        tweenSequence.OnComplete(() => 
        {
            
            // 关键修复：动画完成后处理遮罩
            if (disableMasksAfterShrink)
            {
                SetMasksActive(false);
            }
            
            if (hidePanelAfterShrink)
            {
                gameObject.SetActive(false);
            }
            
            onComplete?.Invoke();
            tweenSequence = null;
        });
    }

    /// <summary>
    /// 从中间向两边展开
    /// </summary>
    public void ExpandFromCenter(float duration = -1, System.Action onComplete = null)
    {
        if (duration <= 0) duration = shrinkDuration;
        
        
        tweenSequence?.Kill();
        
        // 确保Panel是激活的
        gameObject.SetActive(true);
        
        // 重置遮罩状态并激活
        ResetMasksState();
        SetMasksActive(true);
        
        tweenSequence = DOTween.Sequence();
        
        for (int i = 0; i < softnessLayers; i++)
        {
            if (i >= leftMasks.Count || i >= rightMasks.Count) continue;
            
            float layerDelay = (softnessLayers - i - 1) * 0.05f;
            float layerDuration = duration * 0.8f;
            
            // 左边遮罩
            var leftMask = leftMasks[i];
            var leftImage = leftMask.GetComponent<Image>();
            
            tweenSequence.Insert(layerDelay, leftMask.DOSizeDelta(
                new Vector2(0, 0), layerDuration).SetEase(easeType));
            tweenSequence.Join(leftImage.DOFade(0, layerDuration));
            
            // 右边遮罩
            var rightMask = rightMasks[i];
            var rightImage = rightMask.GetComponent<Image>();
            
            tweenSequence.Insert(layerDelay, rightMask.DOSizeDelta(
                new Vector2(0, 0), layerDuration).SetEase(easeType));
            tweenSequence.Join(rightImage.DOFade(0, layerDuration));
        }
        
        tweenSequence.OnComplete(() => 
        {
            onComplete?.Invoke();
            tweenSequence = null;
        });
    }

    /// <summary>
    /// 设置所有遮罩的激活状态
    /// </summary>
    void SetMasksActive(bool active)
    {
        if (maskObjects == null) return;
        
        foreach (var mask in maskObjects)
        {
            if (mask != null)
            {
                mask.SetActive(active);
            }
        }
    }

    /// <summary>
    /// 重置遮罩状态
    /// </summary>
    void ResetMasksState()
    {
        foreach (var leftMask in leftMasks)
        {
            if (leftMask != null)
            {
                leftMask.sizeDelta = new Vector2(maxShrinkDistance, 0);
                leftMask.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            }
        }
        
        foreach (var rightMask in rightMasks)
        {
            if (rightMask != null)
            {
                rightMask.sizeDelta = new Vector2(maxShrinkDistance, 0);
                rightMask.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);
            }
        }
    }

    public void ResetState()
    {
        tweenSequence?.Kill();
        tweenSequence = null;
        
        // 重置遮罩
        foreach (var leftMask in leftMasks)
        {
            if (leftMask != null)
            {
                leftMask.sizeDelta = new Vector2(0, 0);
                leftMask.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            }
        }
        
        foreach (var rightMask in rightMasks)
        {
            if (rightMask != null)
            {
                rightMask.sizeDelta = new Vector2(0, 0);
                rightMask.GetComponent<Image>().color = new Color(0, 0, 0, 0.2f);
            }
        }
        
        // 确保Panel激活
        gameObject.SetActive(true);
        SetMasksActive(true);
    }

    void OnDestroy()
    {
        tweenSequence?.Kill();
    }
}