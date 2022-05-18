using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

// Синглтон, контролирующий потоки игровых событий
public class GameController : GameControllerAbstract
{
    // Поставлена ли игра на паузу
    private bool _isPaused;

    [SerializeField]
    private SpriteRenderer background;
    [SerializeField]
    private Sprite reserveBackgroundImage;

    private DialogueSystemController _dialogueSystemController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;


    // Проинициализировать все составляющие контроллер компоненты.
    protected override void initialize()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        Assert.IsNotNull( _pauseMenu );

        _eventSystem = FindObjectOfType<EventSystem>();
        Assert.IsNotNull( _eventSystem );

        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();

        base.initialize();
    }


    private void Start()
    {
        _pauseMenu.Close();
    }


    private void Update()
    {
        if( Input.GetButtonDown( "Cancel" ) ) {
            if( !_isPaused ) {
                SetOnPause();
            } else {
                SetOnPlay();
            }
        }
    }

    public override void SetOnPause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _pauseMenu.Open();
        _eventSystem.SetSelectedGameObject( null );
        _dialogueSystemController?.SetDialogueSystemInput( false );
    }


    public override void SetOnPlay()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pauseMenu.Close();
        _dialogueSystemController?.SetDialogueSystemInput( true );
    }

    // Сохранение (при необходимости) и выход из игры
    public override void ExitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void ChangeBackground()
    {
        background.sprite = reserveBackgroundImage;
    }

}
