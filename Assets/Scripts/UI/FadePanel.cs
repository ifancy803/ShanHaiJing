using System;
using DG.Tweening;
using UnityEngine;

public class FadePanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void OnEnable()
    {
        canvasGroup.blocksRaycasts = false;
        FadeOut();
    }
    public void FadeOut()
    {
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, 1f);
    }
    
}