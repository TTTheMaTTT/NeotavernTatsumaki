using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class RythmUIController : MonoBehaviour
{
    // Есть несколько основных понятий. Во-первых, это ритм-линия (или таймлайн) и биты
    // Биты находятся на таймлайне таким образом, что они соотносятся (по некоторому правилу, но для простоты, используем линейные преобразования) с реальным положением битов в проигрываемой музыке.
    // Расстояние между битами соотносится с расстоянием между битами в проигрываемом треке.
    // Карта битов - это полная информация обо всех битах в музыке. Её можно представить в виде линии с расположенными на неё точками в некотором масштабе.
    // На линии в игре не обязательно отображаются все биты. Сколько битов отобразиться зависит от масштаба отображения и от текущего момента трека.
    // По мере проигрывания трека происходит прорутка всей карты битов на ритм-линии.
    // Положение битов отображается на линии в виде штрихов. Текущее положение на линии тоже отображается в виде штриха - это ритм-курсор.
    // Когда трек проигрывается, ритм-курсор движется вдоль ритм-линии по карте битов. На экране же это выглядит так, что ритм-линия движется, а ритм-курсор неподвижен.
    // Когда положение ритм-курсора совпадает с положением битом - это должно означать, что в данный момент в треке должен быть слышен бит.
    // Основная механика игры состоит в том, чтобы как-то реагировать на момент этого совпадения

    [SerializeField] private GameObject beatStrokePrefab;// Объект, обозначающий положения бита на таймлайне

    private BeatTimeline _beatTimeline;// Карта битов данного трека

    private float _earliestTime;
    private float _latestTime;
    // Индекс отображаемого в данный момент самого раннего по времени бита
    // -1, значит, не инициализирован, либо битов ещё не было
    // _beatTimeline.Count, значит, трек уже прошёл
    private int _earliestBeatIndex = -1;
    // Время самого раннего отображаемого бита
    private float _earliestBeatTime;
    // Индекс отображаемого в данный момент самого позднего по времени бита
    private int _latestBeatIndex = -1;
    // Время самого позднего отображаемого бита
    private float _latestBeatTime;
    // Индекс бита, который идёт после самого позднего отображаемого
    private int _afterLatestBeatIndex;
    // Время бита, который идёт после самого позднего отображаемого
    private float _afterLatestBeatTime;

    [SerializeField] private float trackLeftPos;// Позиция на экране, которая соответствует самому раннему моменту трека, отображаемому на экране.
    [SerializeField] private float trackCurrentPos;// Позиция на экране, соответствующая текущему моменту трека
    [SerializeField] private float trackRightPos;// Позиция на экране, кооторая соответствует самому последнему отображаемому моменту трека
    [SerializeField] private float shownTrackTimeLength;// Сколько секунд трека должно отображатся на экране
    private float _prevTimeLength;
    private float _prevTrackLength;

    bool _shouldChange = false;

    private GameObject _rythmLineObject;
    private const string rythmLineObjectName = "RythmLine";
    private Dictionary<int, GameObject> _beatIdToStrokeObject = new Dictionary<int, GameObject>();
    private HashSet<int> _hittedBeats = new HashSet<int>();
    private GameObject _trackCurrentPosObject;
    private const string trackCurrentPosObjectName = "TrackCurrentPos";

    // Параметры, управляющие отображение результата реагирования игрока на биты.
    //
    private Color _deactivateColor = new Color( 0f, 0f, 0f, 0f );
    private Color _failColor = new Color( 1f, 0f, 0f, 1f );
    private Color _successColor = new Color( 0f, 1f, 0f, 1f );
    private Color _targetColor;
    private GameObject _beatReactionObject;
    private Image _beatReactionSprite;
    private const string _beatReactionObjectName = "BeatReaction";
    private const float _reactionTime = 0.15f;// Как быстро происходит отображение эффекта реагирования на бит
    private const float _reactionSpeed = 20f;
    private float _reactionDelta = 0f;// Время на реагирование игрока на бит
    public float reactionDelta
    {
        set {
            _shouldChange = _shouldChange || _reactionDelta != value;
            _reactionDelta = value;
        }
    }

    private BeatReactionType _reactionType = BeatReactionType.BeforeAndAfterBeat;
    public BeatReactionType reactionType
    {
        set {
            _shouldChange = _shouldChange || _reactionType != value;
            _reactionType = value;
        }
    }

    private Coroutine changeColorRoutine;


    [SerializeField] private float trackTimer = 0f;
    private float _timer = 0f;

    private void Start()
    {
        _timer = 0f;
        _rythmLineObject = transform.Find( rythmLineObjectName )?.gameObject;
        _trackCurrentPosObject = _rythmLineObject?.transform.Find( trackCurrentPosObjectName )?.gameObject;
        _beatReactionObject = transform.Find( _beatReactionObjectName )?.gameObject;
        _beatReactionSprite = _beatReactionObject?.GetComponent<Image>();
        _prevTimeLength = -1f;
        _prevTrackLength = -1f;
    }


    private void Update()
    {
        if( _beatReactionSprite != null ) {
            _beatReactionSprite.color = Color.Lerp( _beatReactionSprite.color, _targetColor, _reactionSpeed * Time.deltaTime );
        }
        //Debug.Log( _beatIdToStrokeObject.Count );
    }


    public void ResetBeatTimeline()
    {
        DeleteAllStrokes();
        _hittedBeats.Clear();
        ResetTrackTimes();
    }


    // Установить карту битов. Сбрасывает все предыдущие объекты
    public void SetBeatTimeline( BeatTimeline beatTimeline )
    {
        _beatTimeline = beatTimeline;
        ResetBeatTimeline();
    }


    #region TrackMovement

    // Установить время трека, которое надо отобразить.
    public void SetTrackTime( float time )
    {
        _timer = time;
        trackTimer = _timer;
        if( _afterLatestBeatIndex == -1 || _timer < _earliestTime || _timer > _latestTime ) {
            CreateBeatsVisualization();
        } else {
            CorrectBeatsVisualization();
        }
    }


    // Заново создать отображение битов
    private void CreateBeatsVisualization()
    {
        DeleteAllStrokes();
        CorrectBoundariesIfNeeded();
        if( _beatTimeline.Count == 0 ) {
            return;
        }

        float length = trackRightPos - trackLeftPos;
        _earliestTime = _timer - (length > 0f ? (trackCurrentPos - trackLeftPos) / length : 0f) * shownTrackTimeLength;
        _latestTime = _timer + (length > 0f ? (trackRightPos - trackCurrentPos) / length : 0f) * shownTrackTimeLength;

        if( _beatTimeline[0].Time > _latestTime ) {
            return;
        }

        // Находим первый бит, который находится после latestTime
        _afterLatestBeatIndex = _beatTimeline.GetNextBeatIndexAfterTime( _latestTime );
        Assert.IsTrue( _afterLatestBeatIndex >= 0 && _afterLatestBeatIndex <= _beatTimeline.Count );
        _afterLatestBeatTime = _afterLatestBeatIndex < _beatTimeline.Count ? _beatTimeline[_afterLatestBeatIndex].Time : _latestTime;
        _earliestBeatIndex = _afterLatestBeatIndex;
        _earliestBeatTime = _latestTime;

        CorrectTrackTimes();
        MoveBeatStrokes();
    }


    // Подкорректировать отображение битов, так как новое время не сильно отличается от отображаемого ранее
    private void CorrectBeatsVisualization()
    {
        CorrectBoundariesIfNeeded();
        if( _beatTimeline.Count == 0 ) {
            return;
        }

        float length = trackRightPos - trackLeftPos;
        _earliestTime = _timer - (length > 0f ? (trackCurrentPos - trackLeftPos) / length : 0f) * shownTrackTimeLength;
        _latestTime = _timer + (length > 0f ? (trackRightPos - trackCurrentPos) / length : 0f) * shownTrackTimeLength;

        CorrectTrackTimes();
        MoveBeatStrokes();
    }


    // Сдвинуть временные рамки битов, добавить новые биты, которые в них умещаются, и убрать старые, которые из рамок выпадают
    private void CorrectTrackTimes()
    {
        Assert.IsTrue( _beatTimeline.Count > 0 );
        if( _beatTimeline[0].Time > _latestTime ) {
            SetTrackTimesToBegin();
            return;
        } else if( _beatTimeline.Duration < _earliestTime ) {
            SetTrackTimesToEnd();
            return;
        }

        Assert.IsTrue( _earliestBeatIndex >= 0 && _afterLatestBeatIndex >= _earliestBeatIndex );
        Assert.IsTrue( _latestBeatIndex <= _beatTimeline.Count && _afterLatestBeatIndex <= _beatTimeline.Count );

        while( _earliestBeatIndex - 1 >= 0 && _beatTimeline[_earliestBeatIndex - 1].Time >= _earliestTime ) {
            _earliestBeatIndex--;
            AddBeatStroke( _beatTimeline[_earliestBeatIndex] );
        }
        while( _earliestBeatIndex < _beatTimeline.Count && _beatTimeline[_earliestBeatIndex].Time < _earliestTime ) {
            DeleteBeatStroke( _beatTimeline[_earliestBeatIndex].Id );
            _earliestBeatIndex++;
        }

        Assert.IsTrue( _earliestBeatIndex >= 0 && _earliestBeatIndex < _beatTimeline.Count );
        _earliestBeatTime = _beatTimeline[_earliestBeatIndex].Time;

        while( _afterLatestBeatIndex > 0 && _beatTimeline[_afterLatestBeatIndex - 1].Time > _latestTime ) {
            DeleteBeatStroke( _beatTimeline[_afterLatestBeatIndex - 1].Id );
            _afterLatestBeatIndex--;
        }
        while( _afterLatestBeatIndex < _beatTimeline.Count && _beatTimeline[_afterLatestBeatIndex].Time < _latestTime ) {
            AddBeatStroke( _beatTimeline[_afterLatestBeatIndex] );
            _afterLatestBeatIndex++;
        }

        Assert.IsTrue( _afterLatestBeatIndex > 0 && _afterLatestBeatIndex <= _beatTimeline.Count );
        _afterLatestBeatTime = _afterLatestBeatIndex < _beatTimeline.Count ? _beatTimeline[_afterLatestBeatIndex].Time : _beatTimeline.Duration;

        _latestBeatIndex = _afterLatestBeatIndex - 1;
        _latestBeatTime = _beatTimeline[_latestBeatIndex].Time;
    }


    private void ResetTrackTimes()
    {
        _earliestBeatIndex = -1;
        _latestBeatIndex = -1;
        _afterLatestBeatIndex = -1;

        _earliestBeatTime = 0f;
        _latestBeatTime = 0f;
        _afterLatestBeatTime = 0f;
    }


    private void SetTrackTimesToBegin()
    {
        Assert.IsTrue( _beatTimeline.Count > 0 );

        _earliestBeatIndex = -1;
        _latestBeatIndex = -1;
        _afterLatestBeatIndex = 0;

        _earliestBeatTime = 0f;
        _latestBeatTime = 0f;
        _afterLatestBeatTime = _beatTimeline[0].Time;
    }


    private void SetTrackTimesToEnd()
    {
        int beatCount = _beatTimeline.Count;
        float trackDuration = _beatTimeline.Duration;

        _earliestBeatIndex = beatCount;
        _latestBeatIndex = beatCount;
        _afterLatestBeatIndex = beatCount;

        _earliestBeatTime = trackDuration;
        _latestBeatTime = trackDuration;
        _afterLatestBeatTime = trackDuration;
    }


    public void HitBeat( Beat beat )
    {
        _hittedBeats.Add( beat.Id );
        DeleteBeatStroke( beat.Id );
    }


    private void AddBeatStroke( Beat beat )
    {
        if( _beatIdToStrokeObject.ContainsKey( beat.Id ) || _hittedBeats.Contains( beat.Id )) {
            return;
        }
        var newBeatStroke = GameObject.Instantiate( beatStrokePrefab );
        newBeatStroke.transform.SetParent( _rythmLineObject.transform );
        newBeatStroke.transform.position = new Vector3( 0f, 0f, 0f );
        newBeatStroke.transform.localScale = new Vector3( 1f, 1f, 1f );
        newBeatStroke.GetComponent<RectTransform>().anchoredPosition = new Vector2( 0f, 0f );
        float beatReactionAreaWidth = _reactionDelta / shownTrackTimeLength * (trackRightPos - trackLeftPos) * (_reactionType == BeatReactionType.BeforeAndAfterBeat? 2 : 1);
        var beatStroke = newBeatStroke.GetComponent<BeatStroke>();
        beatStroke?.SetReactionArea( beatReactionAreaWidth, _reactionType );
        _beatIdToStrokeObject.Add( beat.Id, newBeatStroke );
    }


    private void DeleteBeatStroke( int id )
    {
        if( !_beatIdToStrokeObject.ContainsKey( id ) ) {
            return;
        }
        GameObject.Destroy( _beatIdToStrokeObject[id] );
        _beatIdToStrokeObject.Remove( id );
    }


    private void DeleteAllStrokes()
    {
        foreach( var keyValue in _beatIdToStrokeObject ) {
            GameObject.Destroy( keyValue.Value );
        }

        _beatIdToStrokeObject.Clear();
    }


    // Переместить все объекты, соответствующие битам, на их позиции
    private void MoveBeatStrokes()
    {
        Assert.IsTrue( (_earliestBeatIndex >= 0 && _earliestBeatIndex < _beatTimeline.Count &&
                        _afterLatestBeatIndex >= _earliestBeatIndex && _afterLatestBeatIndex <= _beatTimeline.Count) ||
                    _earliestBeatIndex == _latestBeatIndex );
        Assert.IsTrue( _latestTime >= _earliestTime );
        float timeLength = _latestTime - _earliestTime;
        float length = trackRightPos - trackLeftPos;
        for( int i = _earliestBeatIndex; i < _afterLatestBeatIndex; i++ ) {
            if( !_beatIdToStrokeObject.ContainsKey( _beatTimeline[i].Id ) ) {
                continue;
            }
            GameObject beatStroke = _beatIdToStrokeObject[_beatTimeline[i].Id];
            var position = beatStroke.GetComponent<RectTransform>().anchoredPosition;
            position.x = timeLength == 0f ? trackCurrentPos : trackLeftPos + (_beatTimeline[i].Time - _earliestTime) / timeLength * length;
            beatStroke.GetComponent<RectTransform>().anchoredPosition = position;
        }
    }


    private void CorrectBoundariesIfNeeded()
    {
        shownTrackTimeLength = Mathf.Max( 0f, shownTrackTimeLength );
        trackRightPos = Mathf.Max( trackLeftPos, trackRightPos );
        trackCurrentPos = Mathf.Clamp( trackCurrentPos, trackLeftPos, trackRightPos );
        if( _trackCurrentPosObject != null ) {
            var position = _trackCurrentPosObject.GetComponent<RectTransform>().anchoredPosition;
            _trackCurrentPosObject.GetComponent<RectTransform>().anchoredPosition = new Vector2( trackCurrentPos, position.y );
        }
        float newTrackLength = trackRightPos - trackLeftPos;
        if( newTrackLength != _prevTrackLength || shownTrackTimeLength != _prevTimeLength || _shouldChange ) {
            float beatReactionAreaWidth = _reactionDelta / shownTrackTimeLength * newTrackLength * (_reactionType == BeatReactionType.BeforeAndAfterBeat ? 2 : 1);
            foreach( var keyValue in _beatIdToStrokeObject ) {
                var beatStroke = keyValue.Value.GetComponent<BeatStroke>();
                beatStroke.SetReactionArea( beatReactionAreaWidth, _reactionType );
            }
        }
        _prevTrackLength = newTrackLength;
        _prevTimeLength = shownTrackTimeLength;
        _shouldChange = false;
    }

    #endregion //TrackMovement


    // Отобразить успешное попадание по биту
    public void ShowBeatHitSuccess()
    {
        if( changeColorRoutine != null ) {
            StopCoroutine( changeColorRoutine );
        }
        changeColorRoutine = StartCoroutine( BeatReaction( _successColor ) );
    }


    // Отобразить промах по биту
    public void ShowBeatHitFail()
    {
        if( changeColorRoutine != null ) {
            StopCoroutine( changeColorRoutine );
        }
        changeColorRoutine = StartCoroutine( BeatReaction( _failColor ) );
    }


    private IEnumerator BeatReaction( Color color )
    {
        _targetColor = color;
        yield return new WaitForSeconds( _reactionTime );
        _targetColor = _deactivateColor;
    }

}
