using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Menu : MonoBehaviour
{
    private UIControls uiControls;
    [Header("广播")] public ObjectEventSO MenuPanelClicked;

    private void Awake()
    {
        uiControls ??= new UIControls();
    }

    private void OnEnable()
    {
        uiControls.UI.Clicked.Enable();
        uiControls.UI.Clicked.started += MenuClicked;
        //TODO:fade动画
    }

    private void OnDisable()
    {
        uiControls.UI.Clicked.started -= MenuClicked;
        uiControls.UI.Clicked.Disable();
    }
    
    private void MenuClicked(InputAction.CallbackContext obj)
    {
        MenuPanelClicked.RaiseEvent(null,this);
    }
}
