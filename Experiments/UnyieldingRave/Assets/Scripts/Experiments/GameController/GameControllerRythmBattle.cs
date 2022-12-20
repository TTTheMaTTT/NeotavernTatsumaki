using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;


// Игровой контроллер управляет действиями пресонажей, а точнее указывает, в какие моменты действия должны происходить
// Этот енам содержит в себе режимы работы контроллера в вопросе моментов вызова ритмичного игрового события.
public enum RythmGameActionType
{
    OnBeat,// Событие произойдёт в момент окончания окна реакции на бит.
    OnPlayerInput// Событие произойдёт либо на инпут игрока при реакции на бит, либо в конце окна реакции (если игрок не успеет среагировать).
}

// Экспериментальный игровой контроллер, исползующийся для некоторой ритм-игры
public class GameControllerRythmBattle : GameControllerAbstract
{
    private DialogueSystemController _dialogueSystemController;
    private RythmUIController _rythmUIController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;

    private List<IBeatResponsive> _beatResponsiveObjects;
    private List<IRythmGameActor> _rythmGameActors;

    private PlayerController _playerController;
    private string _playerTag = "Player";

    private EnemyController _enemy;
    private string _enemyTag = "Enemy";

    // Определяет, в какие моменты происходит событие ритм-игры. В момент бита или реакции игрока на бит.
    // Однако, если rythmActionType == OnBeat и reactionType == BeforeAndAfterBeat, то события будут происходить, когда закончится окно реакции.
    [SerializeField] private RythmGameActionType rythmActionType = RythmGameActionType.OnBeat;
    // Тип окна реакции.
    [SerializeField] private BeatReactionType reactionType = BeatReactionType.BeforeAndAfterBeat;
    private BeatTimeline _beatTimeline;// Карта битов данного трека
    private int _nextBeatToReactIndex = -1;// Индекс предстоящего бита, на который ещё предстоит среагировать. -1, если битов нет совсем. _beatTimeline.Count, если биты закончились.
    private int _nextBeatIndex = -1;// Индекс ближайшего бита, который ещё не произошёл
    private int _rythmActionBeatIndex = -1;// Индекс ближайшего бита, вместе с которым должно произойти следующее ритм-событие
    [SerializeField] private float _reactionDelta = 0.15f;// Как сильно может отличаться время реагирования от времени бита, чтобы всё ещё засчитывать попадание по биту.
    private Queue<int> _beatsToReact = new Queue<int>();// Биты, на которые нужно среагировать

    private Dictionary<int, CharacterAction> _playerReactionsOnBeats = new Dictionary<int, CharacterAction>();// Каким действием игрок среагировал на бит

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

        _beatResponsiveObjects = FindObjectsOfType<UnityEngine.Object>().Where(x => x as IBeatResponsive != null ).Select( x => x as IBeatResponsive ).ToList();
        _rythmGameActors = FindObjectsOfType<CharacterController>().Select( x => x as IRythmGameActor ).ToList();

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

        _beatsToReact = new Queue<int>();
        _playerReactionsOnBeats = new Dictionary<int, CharacterAction>();

        CreateBeatTimeline();
        _nextBeatToReactIndex = _beatTimeline.GetNextBeatIndexAfterTime( -trackStartTime );
        _nextBeatIndex = _nextBeatToReactIndex;
        _rythmActionBeatIndex = _nextBeatToReactIndex;
        _rythmUIController.SetBeatTimeline( _beatTimeline );
        _rythmUIController.reactionDelta = _reactionDelta;
        _rythmUIController.reactionType = reactionType;

        var rythmControllers = FindObjectsOfType<CharacterController>();
        foreach( var beatResponsive in _beatResponsiveObjects ) {
            beatResponsive.ConfigureBeats( bpm, -trackStartTime );
        }
    }


    private void Update()
    {
        ManageBeats();
        LaunchBeatActions();
    }


    // Управление линией битов
    private void ManageBeats()
    {
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

        // Реакция игрока на биты
        if( Input.GetKeyDown( KeyCode.Mouse0 ) || Input.GetKeyDown( KeyCode.Mouse1 ) ) {
            if( _beatsToReact.Count == 0 ) {
                _rythmUIController.ShowBeatHitFail();
            } else {
                int beatIndex = _beatsToReact.Dequeue();// Игрок больше не сможет среагировать на бит, на который он уже среагировал
                _rythmUIController.ShowBeatHitSuccess();
                _rythmUIController.HitBeat( _beatTimeline[beatIndex] );
                ManagePlayerBeatReaction( beatIndex );
                if( rythmActionType == RythmGameActionType.OnPlayerInput ) {
                    // Сразу запускаем ритм-действие
                    Assert.IsTrue( _rythmActionBeatIndex == beatIndex );// Сверимся с битом ритм-действия
                    RythmEvent( _rythmActionBeatIndex );
                    _rythmActionBeatIndex++;// Увеличиваем индекс, чтобы повторно не вызвался метод на момент конца реакции бита
                }

            }
        }

        // Удаляем биты, у которых истёк срок годности (реакции). Игрок больше не сможет на них среагировать.
        bool canPeek = true;
        while( canPeek ) {
            canPeek = false;
            if( _beatsToReact.Count > 0 ) {
                int beatIndex = _beatsToReact.Peek();
                if( IsBeatReactionHappened( beatIndex ) ) {
                    _rythmUIController.ShowBeatHitFail();
                    _beatsToReact.Dequeue();
                    canPeek = true;// Также смотрим на следующие биты.
                }
            }
        }
    }


    // Управление от игрока
    private void ManagePlayerBeatReaction( int beatIndex )
    {
        // Действия игрока
        if( _playerController.State.Health <= 0 ) {
            return;
        }
        if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
            _playerReactionsOnBeats.Add( beatIndex, CharacterAction.Attack );
        } else if( Input.GetKeyDown( KeyCode.Mouse1 ) ) {
            _playerReactionsOnBeats.Add( beatIndex, CharacterAction.Defend );
        }
    }


    // Запуск реакции объектов на бит
    private void LaunchBeatActions()
    {
        while( _nextBeatIndex < _beatTimeline.Count && IsBeatHappened( _nextBeatIndex ) ) {
            _nextBeatIndex++;
            foreach( var beatResponsive in _beatResponsiveObjects ) {
                beatResponsive.OnBeat();
            }
        }
        while( _rythmActionBeatIndex < _beatTimeline.Count && IsBeatReactionHappened( _rythmActionBeatIndex ) ) {
            // Сюда должны попадать, если
            // rythmActionType == OnPlayerInput и игрок не среагировал на бит с индексом _rythmActionBeatIndex.
            // rythmActionType == OnBeat, и просто подошёл конец окна реакции на бит с индексом _rythmActionBeatIndex. Игрок мог как среагировать, так и не среагировать.
            // Т.е. независимо от режима rythmActionType, если выполнение кода попало сюда, значит надо вызывать ритм-действие.
            RythmEvent( _rythmActionBeatIndex );
            _rythmActionBeatIndex++;
        }

    }


    // Считаем ли, что бит с указанным индексом уже произошёл, 
    private bool IsBeatHappened( int beatIndex )
    {
        Assert.IsTrue( beatIndex >= 0 && beatIndex < _beatTimeline.Count );
        return currentTime >= _beatTimeline[beatIndex].Time;
    }


    // Считаем ли, что бит с указанным индексом уже произошёл, а также прошло время для реагирования на этот бит
    private bool IsBeatReactionHappened( int beatIndex )
    {
        Assert.IsTrue( beatIndex >= 0 && beatIndex < _beatTimeline.Count );
        switch( reactionType ) {
            case BeatReactionType.OnlyBeforeBeat:
                return currentTime >= _beatTimeline[beatIndex].Time;
            case BeatReactionType.BeforeAndAfterBeat:
                return currentTime - _beatTimeline[beatIndex].Time > _reactionDelta;
            default:
                throw new Exception( $"Wrong reaction type {reactionType}" );
        }
    }


    // Игровое ритм-событие, запускающее ритм-действие у персонажей, связанные с битом с индексом beatIndex
    private void RythmEvent( int beatIndex )
    {
        Assert.IsTrue( beatIndex >= 0 && beatIndex < _beatTimeline.Count );
        ManagePlayerAction( beatIndex );

        foreach( var rythmGameActor in _rythmGameActors ) {
            rythmGameActor.OnRythmAction();
        }
        CalculateActions();
    }


    private void ManagePlayerAction( int beatIndex )
    {
        if( !_playerReactionsOnBeats.ContainsKey( beatIndex ) ) {
            _playerController.Idle();
            return;
        }
        switch( _playerReactionsOnBeats[ beatIndex ] ) {
            case CharacterAction.Attack:
                _playerController.Attack();
                break;
            case CharacterAction.Defend:
                _playerController.Defend();
                break;
            case CharacterAction.None:
                _playerController.Idle();
                break;
            default:
                Assert.IsTrue( false, "Wrong character state action" );
                break;
        }
        _playerReactionsOnBeats.Remove( beatIndex );
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
