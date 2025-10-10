using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    public Button quitGameButton;

    public GameObject quitGameImg;
    
    public Button yesBtn;
    public Button noBtn;
    
    private void Start()
    {
        quitGameButton.onClick.AddListener(OpenImg);
        yesBtn.onClick.AddListener(QuitGame);
        noBtn.onClick.AddListener(CloseImg);
    }

    void OpenImg()
    {
        quitGameImg.SetActive(true);
        //UIManager.Instance.ToggleScale(quitGameImg,Ease.InOutQuart,true,0.5f);
        
        UIManager.Instance.ScaleUp(quitGameImg,Ease.InOutQuart,0.5f);
    }
    
    void CloseImg()
    {
        quitGameImg.SetActive(false);
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
