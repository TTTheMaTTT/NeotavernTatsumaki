using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

// Синглтон, контролирующий потоки игровых событий
public class GameController : MonoBehaviour
{

    private static GameController _instance;

    public Sprite reserveBackground;
    public GameObject backgroundObject;
    
    // Был ли проинициализирован контроллер
    private bool _isInitialized = false;
    // Поставлена ли игра на паузу
    private bool _isPaused;

    private DialogueSystemController _dialogueSystemController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;

    /// <summary>
    /// Доступ к контролеру
    /// </summary>
    /// <returns>Экземпляр игрового контроллера</returns>
    public static GameController Instance()
    {
        if( _instance == null ) {
            _instance = FindObjectOfType<GameController>();
            if( _instance == null ) {
                _instance = new GameObject().AddComponent<GameController>();
            }
        }

        if( !_instance._isInitialized ) {
            _instance.initialize();
        }

        return _instance;
    }

    private void Awake()
    {
        if( _instance != null ) {
            Destroy( this );
            return;
        }
        _instance = this;

        if( !_isInitialized ) {
            initialize();
        }
    }


    // Проинициализировать все составляющие контроллер компоненты.
    private void initialize()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        Assert.IsNotNull( _pauseMenu );

        _eventSystem = FindObjectOfType<EventSystem>();
        Assert.IsNotNull( _eventSystem );

        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();

        _isInitialized = true;
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

    public void SetOnPause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _pauseMenu.Open();
        _eventSystem.SetSelectedGameObject( null );
        _dialogueSystemController?.SetDialogueSystemInput( false );
    }


    public void SetOnPlay()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pauseMenu.Close();
        _dialogueSystemController?.SetDialogueSystemInput( true );
    }

    // Сохранение (при необходимости) и выход из игры
    public void ExitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }

    public void ChangeBackground()
    {
        backgroundObject.GetComponent<SpriteRenderer>().sprite = reserveBackground;
    }

}
