using System;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class MenuPanel : MonoBehaviour
{
    private UIControls uiControls;

    [Header("广播")] public ObjectEventSO MenuPanelClicked;

    private void Awake()
    {
        uiControls ??= new UIControls();
    }

    private void OnEnable()
    {
        uiControls.UI.Enable();
        uiControls.UI.Click.started += ClickAnyWhere;
    }

    private void OnDisable()
    {
        uiControls.UI.Click.started -= ClickAnyWhere;
        uiControls.UI.Disable();
    }
    
    private void ClickAnyWhere(InputAction.CallbackContext obj)
    {
        MenuPanelClicked.RaiseEvent(null,this);
    }
}
