using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using PixelCrushers.DialogueSystem;

// Экспериментальный игровой контроллер, исползующийся для некоторой ритм-игры
public class GameControllerRythm : GameControllerAbstract
{
    private DialogueSystemController _dialogueSystemController;
    private RythmUIController _rythmUIController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;

    private BeatTimeline _beatTimeline;// Карта битов данного трека
    private int _nextBeatIndex = -1;// Индекс предстоящего бита, на который ещёё предстоит среагировать. -1, если битов нет совсем. _beatTimeline.Count, если биты закончились.
    [SerializeField]private float _reactionDelta = 0.15f;// Как сильно может отличаться время реагирования от времени бита, чтобы всё ещё засчитывать попадание по биту.
    private Queue<int> _beatsToReact = new Queue<int>();// Биты, на которые нужно среагировать

    private float _time;// Текущее время в треке

    [SerializeField] private int bpm = 120;// Кол-во битов в минуту
    [SerializeField] private float trackLength = 60;// Продолжительность трека в секундах

    [SerializeField] private float trackStartTime = 5f;
    bool _isTrackStarted = false;

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

        _isTrackStarted = false;

        base.initialize();
    }


    private void Start()
    {
        _pauseMenu.Close();

        CreateBeatTimeline();
        _nextBeatIndex = _beatTimeline.GetNextBeatIndexAfterTime( -trackStartTime );
        _rythmUIController.SetBeatTimeline( _beatTimeline );
        _rythmUIController.reactionDelta = _reactionDelta;
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

        _time += Time.deltaTime;
        float currentTime;
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
        while( _nextBeatIndex >= 0 && _nextBeatIndex < _beatTimeline.Count && Mathf.Abs( _beatTimeline[_nextBeatIndex].Time - currentTime ) <= _reactionDelta ) {
            _beatsToReact.Enqueue( _nextBeatIndex );
            _nextBeatIndex++;
        }

        // Удаляем биты, у которых истёк срок годности (реакции)
        bool canPeek = true;
        while( canPeek ) {
            canPeek = false;
            if( _beatsToReact.Count > 0 ) {
                int beatIndex = _beatsToReact.Peek();
                if( currentTime - _beatTimeline[beatIndex].Time > _reactionDelta ) {
                    _rythmUIController.ShowBeatHitFail();
                    _beatsToReact.Dequeue();
                    canPeek = true;// Также смотрим на следующие биты.
                }
            }
        }

        // Действия игрока
        if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
            if( _beatsToReact.Count == 0 ) {
                _rythmUIController.ShowBeatHitFail();
            } else {
                int beatIndex = _beatsToReact.Dequeue();
                _rythmUIController.ShowBeatHitSuccess();
                _rythmUIController.HitBeat( _beatTimeline[beatIndex] );
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
