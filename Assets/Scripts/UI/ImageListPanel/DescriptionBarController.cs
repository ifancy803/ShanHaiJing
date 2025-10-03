using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DescriptionBarController : MonoBehaviour
{ 
    [Header("广播")]
    public ObjectEventSO closeContainerEvent;
    
    [Header("UI Elements")]
    public GameObject characterImage;
    public GameObject descriptionText;
    public GameObject characterContext;
    public Button characterDisplayBtn;
    public Button characterContextBtn;
    public Button EscBtn;

    private void Start()
    {
        characterDisplayBtn.onClick.AddListener(characterDisplayBtnClicked);
        characterContextBtn.onClick.AddListener(characterContextBtnClicked);
        EscBtn.onClick.AddListener(EscBtnClicked);
    }

    private void OnEnable()
    {
        characterDisplayBtnClicked();
    }

    private void characterDisplayBtnClicked()
    {
        characterImage.SetActive(true);
        descriptionText.SetActive(true);
        characterContext.SetActive(false);
    }

    private void characterContextBtnClicked()
    {
        characterImage.SetActive(false);
        descriptionText.SetActive(false);
        characterContext.SetActive(true);
    }

    private void EscBtnClicked()
    {
        closeContainerEvent.RaiseEvent(null,this);
    }
    
    public void CloseImageListPanel(object value)
    {
        closeContainerEvent.RaiseEvent(null,this);
    }
}
