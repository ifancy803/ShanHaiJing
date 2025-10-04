using UnityEngine;
using System.Collections;
using System;

public class UIImageDissolveController : MonoBehaviour
{
    [Header("溶解控制")]
    [Range(0, 1)]
    public float dissolveAmount = 0f;
    
    [Header("动画设置")]
    public float dissolveDuration = 2f;
    public AnimationCurve dissolveCurve = AnimationCurve.Linear(0, 0, 1, 1);
    
    [Header("测试按钮")]
    public bool testDissolve = false;
    public bool testReverse = false;
    public bool resetDissolve = false;
    
    private CanvasGroup canvasGroup;
    private Coroutine dissolveCoroutine;

    // 定义回调委托
    public System.Action OnDissolveComplete;
    public System.Action OnReverseDissolveComplete;

    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        ResetDissolve();
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (testDissolve)
        {
            testDissolve = false;
            StartDissolve();
        }
        
        if (testReverse)
        {
            testReverse = false;
            StartReverseDissolve();
        }
        
        if (resetDissolve)
        {
            resetDissolve = false;
            ResetDissolve();
        }
        #endif

        UpdateDissolveEffect();
    }

    void UpdateDissolveEffect()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - dissolveAmount;
            canvasGroup.interactable = dissolveAmount < 0.99f;
            canvasGroup.blocksRaycasts = dissolveAmount < 0.99f;
        }
    }

    /// <summary>
    /// 开始溶解动画
    /// </summary>
    public void StartDissolve(float duration = -1, System.Action onComplete = null)
    {
        if (duration <= 0) duration = dissolveDuration;
        
        if (dissolveCoroutine != null)
            StopCoroutine(dissolveCoroutine);
            
        dissolveCoroutine = StartCoroutine(DissolveCoroutine(dissolveAmount, 1f, duration, onComplete ?? OnDissolveComplete));
    }

    /// <summary>
    /// 开始反向溶解动画
    /// </summary>
    public void StartReverseDissolve(float duration = -1, System.Action onComplete = null)
    {
        if (duration <= 0) duration = dissolveDuration;
        
        if (dissolveCoroutine != null)
            StopCoroutine(dissolveCoroutine);
            
        dissolveCoroutine = StartCoroutine(DissolveCoroutine(dissolveAmount, 0f, duration, onComplete ?? OnReverseDissolveComplete));
    }

    private IEnumerator DissolveCoroutine(float fromAmount, float toAmount, float duration, System.Action onComplete)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float curveValue = dissolveCurve.Evaluate(progress);
            
            dissolveAmount = Mathf.Lerp(fromAmount, toAmount, curveValue);
            yield return null;
        }
        
        dissolveAmount = toAmount;
        dissolveCoroutine = null;
        
        // 调用回调函数
        onComplete?.Invoke();
    }

    /// <summary>
    /// 等待溶解完成的协程
    /// </summary>
    public IEnumerator WaitForDissolveComplete()
    {
        while (IsAnimating())
        {
            yield return null;
        }
    }

    /// <summary>
    /// 设置溶解程度
    /// </summary>
    public void SetDissolveAmount(float amount)
    {
        dissolveAmount = Mathf.Clamp01(amount);
    }

    /// <summary>
    /// 重置溶解状态
    /// </summary>
    public void ResetDissolve()
    {
        dissolveAmount = 0f;
        if (dissolveCoroutine != null)
        {
            StopCoroutine(dissolveCoroutine);
            dissolveCoroutine = null;
        }
    }

    /// <summary>
    /// 检查是否正在播放动画
    /// </summary>
    public bool IsAnimating()
    {
        return dissolveCoroutine != null;
    }
}