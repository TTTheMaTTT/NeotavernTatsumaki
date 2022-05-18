using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using PixelCrushers.DialogueSystem;

// Меню паузы и его UI. Синглтон.
public class PauseMenu : MonoBehaviour
{
    private static PauseMenu _instance;

    [SerializeField]
    private PixelCrushers.UIPanel _panel;

    private void Awake()
    {
        if( _instance != null ) {
            Destroy( this );
            return;
        }

        _instance = this;

        Assert.IsNotNull( _panel );
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void Open()
    {
        if( !_panel.isOpen ) {
            _panel.Open();
        }
    }

    public void Close()
    {
        if( _panel.isOpen ) {
            _panel.Close();
        }
    }

    public void Continue()
    {
        GameController.Instance().SetOnPlay();
    }

    public void Exit()
    {
        GameController.Instance().ExitGame();
    }

}
