using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DescriptionBarController : MonoBehaviour
{ 
    [Header("广播")]
    public ObjectEventSO closeContainer;
    
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
        characterDisplayBtnClicked();
        closeContainer.RaiseEvent(this,this);
    }
}
