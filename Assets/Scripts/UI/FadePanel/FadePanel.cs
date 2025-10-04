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
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 1;
        }
    }

    private void OnEnable()
    {
        canvasGroup.DOFade(0,1f).SetEase(Ease.InOutQuart);
    }
}
