using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

// Экспериментальный игровой контроллер, использующийся внутри комнат
public class GameControllerTavernInside : GameControllerAbstract
{
    private DialogueSystemController _dialogueSystemController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;
    private PlayerTavernInside _player;


    // Проинициализировать все составляющие контроллер компоненты.
    protected override void initialize()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        Assert.IsNotNull( _pauseMenu );

        _eventSystem = FindObjectOfType<EventSystem>();
        Assert.IsNotNull( _eventSystem );

        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();

        var players = FindObjectsOfType<PlayerTavernInside>();
        Assert.IsTrue( players.Length == 1, "One and only one player is allowed!" );
        _player = players[0];

        base.initialize();
    }


    private void Start()
    {
        _pauseMenu.Close();
        _player.SetMovability( true );
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

    public void OnConversationStart()
    {
        _player.SetMovability( false );
    }

    public void OnConversationEnd()
    {
        _player.SetMovability( true );
    }

}
