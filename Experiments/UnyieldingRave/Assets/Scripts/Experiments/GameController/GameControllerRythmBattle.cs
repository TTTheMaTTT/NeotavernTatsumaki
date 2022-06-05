using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

// Экспериментальный игровой контроллер, исползующийся для некоторой ритм-игры
public class GameControllerRythmBattle : GameControllerAbstract
{
    private DialogueSystemController _dialogueSystemController;
    private RythmUIController _rythmUIController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;

    private List<IBeatResponsive> beatResponsiveObjects;

    private PlayerController _playerController;
    private string _playerTag = "Player";

    private EnemyController _enemy;
    private string _enemyTag = "Enemy";

    private BeatTimeline _beatTimeline;// Карта битов данного трека
    private int _nextBeatToReactIndex = -1;// Индекс предстоящего бита, на который ещё предстоит среагировать. -1, если битов нет совсем. _beatTimeline.Count, если биты закончились.
    private int _nextBeatIndex = -1;// Индекс ближайшего бита, который ещё не произошёл
    [SerializeField]private float _reactionDelta = 0.15f;// Как сильно может отличаться время реагирования от времени бита, чтобы всё ещё засчитывать попадание по биту.
    private BeatReactionType _reactionType = BeatReactionType.OnlyBeforeBeat;
    private Queue<int> _beatsToReact = new Queue<int>();// Биты, на которые нужно среагировать

    private float _time;// Текущее время в треке
    [SerializeField] private float currentTime;

    [SerializeField] private int bpm = 120;// Кол-во битов в минуту
    [SerializeField] private float trackLength = 60;// Продолжительность трека в секундах

    [SerializeField] private float trackStartTime = 5f;
    bool _isTrackStarted = false;

    [SerializeField] private int _playerStartHealth = 3;
    [SerializeField] private int _enemyStartHealth = 10;

    // Проинициализировать все составляющие контроллер компоненты.
    protected override void initialize()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        Assert.IsNotNull( _pauseMenu );

        _eventSystem = FindObjectOfType<EventSystem>();
        Assert.IsNotNull( _eventSystem );

        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();
        _rythmUIController = FindObjectOfType<RythmUIController>();
        Assert.IsNotNull( _rythmUIController );

        beatResponsiveObjects = FindObjectsOfType<CharacterController>().Select( x => x as IBeatResponsive ).ToList();

        var players = GameObject.FindGameObjectsWithTag( _playerTag );
        Assert.IsTrue( players.Length == 1, "One and only one player is allowed!" );
        _playerController = players[0].GetComponent<PlayerController>();
        Assert.IsNotNull( _playerController );
        var state = _playerController.State;
        state.Health = _playerStartHealth;
        _playerController.State = state;

        var enemyObjects = GameObject.FindGameObjectsWithTag( _enemyTag );
        if( enemyObjects.Length > 0 ) {
            _enemy = enemyObjects[0].GetComponent<EnemyController>();
            var enemyState = _enemy.State;
            enemyState.Health = _enemyStartHealth;
            _enemy.State = enemyState;
        }

        _isTrackStarted = false;

        base.initialize();
    }


    private void Start()
    {
        _pauseMenu.Close();

        CreateBeatTimeline();
        _nextBeatToReactIndex = _beatTimeline.GetNextBeatIndexAfterTime( -trackStartTime );
        _nextBeatIndex = _nextBeatToReactIndex;
        _rythmUIController.SetBeatTimeline( _beatTimeline );
        _rythmUIController.reactionDelta = _reactionDelta;
        _rythmUIController.reactionType = _reactionType;

        var rythmControllers = FindObjectsOfType<CharacterController>();
        foreach( var beatResponsive in beatResponsiveObjects ) {
            beatResponsive.ConfigureBeats( bpm, -trackStartTime );
        }
    }


    private void Update()
    {
        bool isBeatHitted = ManageBeats();
        if( isBeatHitted ) {
            ManagePlayerActions();
        }
        LaunchBeatActions();
    }


    // Управление линией битов
    private bool ManageBeats()
    {
        bool isBeatHitted = false;

        if( Input.GetButtonDown( "Cancel" ) ) {
            if( !_isPaused ) {
                SetOnPause();
            } else {
                SetOnPlay();
            }
        }

        _time += Time.deltaTime;
        if( !_isTrackStarted ) {
            currentTime = _time - trackStartTime;
            if( _time >= trackStartTime ) {
                _isTrackStarted = true;
                _time = 0f;
                currentTime = _time;
            }
        } else {
            currentTime = _time;
        }

        _rythmUIController.SetTrackTime( currentTime );

        // Добавляем новые биты на реагирование в очередь
        while( _nextBeatToReactIndex >= 0 && _nextBeatToReactIndex < _beatTimeline.Count && Mathf.Abs( _beatTimeline[_nextBeatToReactIndex].Time - currentTime ) <= _reactionDelta ) {
            _beatsToReact.Enqueue( _nextBeatToReactIndex );
            _nextBeatToReactIndex++;
        }

        // Удаляем биты, у которых истёк срок годности (реакции)
        bool canPeek = true;
        while( canPeek ) {
            canPeek = false;
            if( _beatsToReact.Count > 0 ) {
                int beatIndex = _beatsToReact.Peek();
                if( IsBeatHappened( beatIndex ) ) {
                    _rythmUIController.ShowBeatHitFail();
                    _beatsToReact.Dequeue();
                    canPeek = true;// Также смотрим на следующие биты.
                }
            }
        }

        if( Input.GetKeyDown( KeyCode.Mouse0 ) || Input.GetKeyDown( KeyCode.Mouse1 ) ) {
            if( _beatsToReact.Count == 0 ) {
                _rythmUIController.ShowBeatHitFail();
            } else {
                isBeatHitted = true;
                int beatIndex = _beatsToReact.Dequeue();
                _rythmUIController.ShowBeatHitSuccess();
                _rythmUIController.HitBeat( _beatTimeline[beatIndex] );
            }
        }
        return isBeatHitted;
    }


    // Управление от игрока
    private void ManagePlayerActions()
    {
        // Действия игрока
        if( _playerController.State.Health <= 0 ) {
            return;
        }
        if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
            _playerController.Attack();
        } else if( Input.GetKeyDown( KeyCode.Mouse1 ) ) {
            _playerController.Defend();
        }
    }


    // Запуск реакции объектов на бит
    private void LaunchBeatActions()
    {
        while( _nextBeatIndex < _beatTimeline.Count && IsBeatHappened( _nextBeatIndex ) ) {
            _nextBeatIndex++;
            foreach( var beatResponsive in beatResponsiveObjects ) {
                beatResponsive.OnBeat();
            }
            CalculateActions();
        }
    }


    // Считаем ли, что бит с указанным индексом уже произошёл
    private bool IsBeatHappened( int beatIndex )
    {
        Assert.IsTrue( beatIndex >= 0 && beatIndex < _beatTimeline.Count );
        switch( _reactionType ) {
            case BeatReactionType.OnlyBeforeBeat:
                return currentTime >= _beatTimeline[beatIndex].Time;
            case BeatReactionType.BeforeAndAfterBeat:
                return currentTime - _beatTimeline[beatIndex].Time > _reactionDelta;
            default:
                throw new Exception( $"Wrong reaction type {_reactionType}" );
        }
    }


    // Расчёт последствий действий персонажей
    private void CalculateActions()
    {
        CharacterState playerState = _playerController.State;
        if( _enemy != null ) {
            CharacterState enemyState = _enemy.State;
            if( playerState.Action == CharacterAction.Attack && enemyState.Action != CharacterAction.Defend ) {
                _enemy.TakeDamage();
                if( _enemy.State.Health <= 0 ) {
                    _enemy = null;
                }
            }
            if( enemyState.Action == CharacterAction.Attack && playerState.Action != CharacterAction.Defend ) {
                _playerController.TakeDamage();
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


    private void CreateBeatTimeline()
    {
        float timeBetweenBeats = 60f / bpm;
        List<Beat> beats = new List<Beat>();
        float beatTime = 0f;
        int beatId = 0;
        while( beatTime < trackLength ) {
            beats.Add( new Beat() { Id = beatId, Time = beatTime } );
            beatTime += timeBetweenBeats;
            beatId++;
        }

        _beatTimeline = new BeatTimeline( beats );
    }

}
