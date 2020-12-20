using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Dialogue
{

    /// <summary>
    /// Окно, отображающее персонажей диалога
    /// </summary>
    public class CDialogueActorsPanel : MonoBehaviour
    {

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            AnimateActors();
        }

        public void Initialize()
        {
            _allActors = new List<CActorInfo>();
            _actorNameToIndex = new Dictionary<string, int>();
            _shownActorsIndices = new HashSet<int>();
            _removedActorsIndices = new HashSet<int>();
            _prevActorsCount = 0;
        }

        /// <summary>
        /// Вызывается при начале диалога
        /// </summary>
        public void OnStartDialogue()
        {
            _allActors.Clear();
            _actorNameToIndex.Clear();
            _shownActorsIndices.Clear();
            _removedActorsIndices.Clear();
            _leadActorIndex = -1;
            for( int i = transform.childCount - 1; i >= 0; i-- ) {
                DestroyImmediate( transform.GetChild( i ).gameObject );
            }
        }

        /// <summary>
        /// Отобразить действующих лиц диалога
        /// </summary>
        /// <param name="arrangements">Какие изменения нужно произвести в расположении лиц</param>
        /// <param name="actorImages">Какие изображения должны быть у лиц диалога</param>
        /// <param name="leadingActor">Имя говорящего лица</param>
        public void ShowActors( CDialogueActorsArrangements arrangements, CDialogueActorImage[] actorImages, string leadingActor )
        {
            // Уберём ненужные последствия с предыдущей сцены
            ClearPrevScene();

            // Удалим и добавим актёров
            PrepareActors( arrangements, out bool mustChangeArrangement );

            // Учтём главное лицо (обеспечим, чтобы оно точно отобразилось)
            if( _actorNameToIndex.ContainsKey( leadingActor ) ) {
                if( !_shownActorsIndices.Contains( _actorNameToIndex[leadingActor] ) ) {
                    mustChangeArrangement = true;
                }
                MakeLeading( leadingActor );
            }

            DefineShownIndices();

            // Изменим расположение всех актёров с учётом новой информации
            if( mustChangeArrangement ) {
                MakeArrangements();
            }

            // Меняем изображения
            SetActorsImages( actorImages );
            
            MoveActors();

            _prevActorsCount = _allActors.Count;
        }

        // Вспомогательная структура, хранящая информацию о лице
        private class CActorInfo
        {
            public string ActorName;
            public GameObject ActorObject;// Игровой объект, соответствующий лицу
            public TDialogueActorArrangementPosition PositionType;// Какой тип расположения у лица (в какой части экрана расположить)
            public Vector2 Position;// Где расположить
            public bool UseAutosize;// Автоматически определять размер.
            public bool UseCustomScale;// Использовать значение поля Scale для задавания изображения. Имеет смысл, если UseAutoSize == false и UseCustomScale == true.
            public float Scale;// Во сколько раз увеличить размер относительно оригинального размера спрайта
            public Vector2 Size;// Выставляемый размер изображению. Имеет смысл только если UseAutoSize и UseCustomScale выставлены в false. 
            // Индексы списка _allActors, которые показывают, между какими лицами находилось бы данное лицо, если бы их расположили в порядке давности самой последней реплики (начиная от самой недавней).
            // Нужно для отображения самых последних говорящих при большом кол-ве лиц диалога.
            public int Index;// Индекс лица в списке
            public int PrevActorIndex = -1;// Если -1, значит, данное лицо говорило последним
            public int NextActorIndex = -1;// Если -1, значит, данное лицо мы не слышали дольше всех
        }

        // Очистка от последствий на предыдущей сцене.
        // Файктически мгновенно перемещает нужных лиц к их позициям и удаляет то, что должно быть удалено.
        private void ClearPrevScene()
        {
            // Сразу выставим лица из предыдущей сцены на их позиции, елси они их не достигли
            foreach( int index in _shownActorsIndices ) {
                Assert.IsTrue( _allActors[index].ActorObject != null );
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = _allActors[index].Position;
            }

            // Удаляем изображения со убранными лицами, если они ещё не были удалены
            foreach( int index in _removedActorsIndices ) {
                if( _allActors[index].ActorObject != null ) {
                    Destroy( _allActors[index].ActorObject );
                }
            
            }
        }

        // Удаление и добавление лиц
        private void PrepareActors( CDialogueActorsArrangements arrangements, out bool mustChangeArrangement )
        {
            _removedActorsIndices.Clear();
            // Сначала определим изменения в расположении лиц
            mustChangeArrangement = false;

            int prevMaxActorsOnScreen = _maxActorsOnScreen;
            _maxActorsOnScreen = arrangements.UseDefaultMaxActorsOnScreen ? DialogueStatementDefaultValues.MaxActorsOnScreen : arrangements.MaxActorsOnScreen;
            if( prevMaxActorsOnScreen != _maxActorsOnScreen ) {
                mustChangeArrangement = true;
            }

            // Рассмотрим все описания аранжировок, добавим и удалим лица при необходимости, определим, нужно ли менять расположение лиц диалога.
            foreach( var arrangement in arrangements.Arrangements ) {
                switch( arrangement.ArrangementAction ) {
                    case TDialogueActorArrangementAction.Appear:
                        AddActor( arrangement.ActorName );
                        mustChangeArrangement = true;
                        break;
                    case TDialogueActorArrangementAction.Leave:
                        DeleteActor( arrangement.ActorName );
                        mustChangeArrangement = true;
                        break;
                    case TDialogueActorArrangementAction.ChangePosition:
                        Assert.IsTrue( _actorNameToIndex.ContainsKey( arrangement.ActorName ), "Arrangement with type 'changePosition' is applied to the absent actor" );
                        CActorInfo actorInfo = _allActors[_actorNameToIndex[arrangement.ActorName]];
                        if( actorInfo.PositionType != arrangement.Position ||
                            (arrangement.Position == TDialogueActorArrangementPosition.Custom && arrangement.ExactPosistion != actorInfo.Position) ) {
                            mustChangeArrangement = true;
                        }
                        break;
                    default:
                        Assert.IsTrue( false );
                        break;
                }

                if( arrangement.ArrangementAction != TDialogueActorArrangementAction.Leave ) {
                    CActorInfo actorInfo = _allActors[_actorNameToIndex[arrangement.ActorName]];
                    actorInfo.PositionType = arrangement.Position;
                    if( arrangement.Position == TDialogueActorArrangementPosition.Custom ) {
                        actorInfo.Position = arrangement.ExactPosistion;
                    }
                    actorInfo.UseAutosize = arrangement.UseAutoSize;
                    actorInfo.UseCustomScale = arrangement.UseCustomScale;
                    actorInfo.Scale = arrangement.Scale;
                    actorInfo.Size = arrangement.Size;

                }
            }
        }

        /// Добавление нового лица в диалог
        private void AddActor( string actorName )
        {
            Assert.IsTrue( !_actorNameToIndex.ContainsKey( actorName ), "Arrangement with type 'appear' is applied to the present actor" );
            CActorInfo actorInfo = new CActorInfo();
            actorInfo.ActorName = actorName;

            // Создаём объект с изображением диалогового лица
            var actorImage = new GameObject().AddComponent<Image>();
            actorImage.transform.SetParent( transform );
            actorImage.transform.position = new Vector3( 0f, 0f, 0f);
            actorImage.transform.localScale = new Vector3( 1f, 1f, 1f );
            actorInfo.ActorObject = actorImage.gameObject;
            actorInfo.Index = _allActors.Count();
            _actorNameToIndex.Add( actorName, _allActors.Count );

            // Выставим дефолтную позицию, если не указана
            if( actorInfo.PositionType != TDialogueActorArrangementPosition.Custom ) {
                actorInfo.Position.x = GetDefaultPosition( actorInfo.PositionType );
            }

            _allActors.Add( actorInfo );

            // Всегда пытаемся отобразить новое лицо, даже если оно ничего не сказало
            MakeLeading( actorName );
        }

        // Удаление действующего лица из диалога
        private void DeleteActor( string actorName )
        {
            Assert.IsTrue( _actorNameToIndex.ContainsKey( actorName ), "Arrangement with type 'leave' is applied to the absent actor" );
            int index = _actorNameToIndex[actorName];
            int prevIndex = _allActors[index].PrevActorIndex;
            int nextIndex = _allActors[index].NextActorIndex;
            if( prevIndex != -1 ) {
                _allActors[prevIndex].NextActorIndex = nextIndex;
            }
            if( nextIndex != -1 ) {
                _allActors[nextIndex].PrevActorIndex = prevIndex;
            }
            if( _leadActorIndex == index ) {
                _leadActorIndex = nextIndex;
            }
            _actorNameToIndex.Remove( actorName );
            _removedActorsIndices.Add( index );
        }

        private void MakeLeading( string actorName )
        {
            Assert.IsTrue( _actorNameToIndex.ContainsKey( actorName ), "Trying to make leading absent actor" );
            int index = _actorNameToIndex[actorName];
            if( index == _leadActorIndex ) {
                return;
            }
            int prevIndex = _allActors[index].PrevActorIndex, nextIndex = _allActors[index].NextActorIndex;
            if( prevIndex != -1 ) {
                _allActors[prevIndex].NextActorIndex = nextIndex;
            }
            if( nextIndex != -1 ) {
                _allActors[nextIndex].PrevActorIndex = prevIndex;
            }
            if( _leadActorIndex != -1 ) {
                _allActors[_leadActorIndex].PrevActorIndex = index;
            }
            _allActors[index].NextActorIndex = _leadActorIndex;
            _allActors[index].PrevActorIndex = -1;
            _leadActorIndex = index;
        }

        void DefineShownIndices()
        {
            // Определим, какие лица будут присутствовать на сцене
            _shownActorsIndices.Clear();
            int currentIndex = _leadActorIndex;
            while( currentIndex != -1 && _shownActorsIndices.Count < _maxActorsOnScreen ) {
                // Если кастомная позиция, следим, чтобы она была внутри ограничивающего прямоугольника
                if( _allActors[currentIndex].PositionType != TDialogueActorArrangementPosition.Custom ||
                    _allActors[currentIndex].Position.x >= _boundaries.xMin && _allActors[currentIndex].Position.x < _boundaries.xMax ) {
                    _shownActorsIndices.Add( currentIndex );
                }
                currentIndex = _allActors[currentIndex].NextActorIndex;
            }
        }

        // Расположить изображения по назначенным позициям
        private void MakeArrangements()
        {

            // Разбиваем экран на три части: левая, центральная и правая части
            // Определим, на каких частях экрана и в каком порядке будут расположены актёры
            ManageActorsOrder( out List<int> leftActors, out List<int> centerActors, out List<int> rightActors );

            // Уточним позиции лиц
            SpecifyPositions( leftActors, TDialogueActorArrangementPosition.Left );
            SpecifyPositions( centerActors, TDialogueActorArrangementPosition.Center );
            SpecifyPositions( rightActors, TDialogueActorArrangementPosition.Right );

            // Также выставим позиции для убираемых лиц
            foreach( int index in _removedActorsIndices ) {
                _allActors[index].Position.y = _appearanceHeight;
            }
        }

        // Определяем, на каких частях экрана расположить лиц и в каком порядке они будут стоять (слева направо)
        private void ManageActorsOrder( out List<int> leftActorsIndices, out List<int> centerActorsIndices, out List<int> rightActorsIndices )
        {
            leftActorsIndices = new List<int>();
            centerActorsIndices = new List<int>();
            rightActorsIndices = new List<int>();
            List<CActorInfo> leftActors = new List<CActorInfo>();
            List<CActorInfo> centerActors = new List<CActorInfo>();
            List<CActorInfo> rightActors = new List<CActorInfo>();

            List<int> indices = _shownActorsIndices.ToList();

            foreach( int index in indices.OrderBy( i => i ) ) {
                switch( _allActors[index].PositionType ) {
                    case TDialogueActorArrangementPosition.Left:
                        leftActors.Add( _allActors[index] );
                        break;
                    case TDialogueActorArrangementPosition.Center:
                        centerActors.Add( _allActors[index] );
                        break;
                    case TDialogueActorArrangementPosition.Right:
                        rightActors.Add( _allActors[index] );
                        break;
                    case TDialogueActorArrangementPosition.Custom: 
                    {
                        float position = _allActors[index].Position.x;
                        if( IsInRegion( position, TDialogueActorArrangementPosition.Left ) ) {
                            leftActors.Add( _allActors[index] );
                        } else if( IsInRegion( position, TDialogueActorArrangementPosition.Center ) ) {
                            centerActors.Add( _allActors[index] );
                        } else if( IsInRegion( position, TDialogueActorArrangementPosition.Right ) ) {
                            rightActors.Add( _allActors[index] );
                        } else {
                            Assert.IsTrue( false );
                        }
                        break;
                    }
                }
            }

            // Составляем упорядоченный список индексов лиц. Так они будут расположены слева направо.
            foreach( var actor in leftActors.OrderBy( a => a.Position.x ) ) { leftActorsIndices.Add( actor.Index ); }
            foreach( var actor in centerActors.OrderBy( a => a.Position.x ) ) { centerActorsIndices.Add( actor.Index ); }
            foreach( var actor in rightActors.OrderBy( a => a.Position.x ) ) { rightActorsIndices.Add( actor.Index ); }
        }

        // Выставляет точные координаты лицам
        private void SpecifyPositions( List<int> actorsIndices, TDialogueActorArrangementPosition regionType )
        {
            // Выставим позиции вдоль оси x.
            float regionLeft = GetRegionLeft( regionType ), regionRight = GetRegionRight( regionType );
            float left = regionLeft, right = regionLeft;
            int leftIndex = 0, rightIndex = 0;
            for( int i = 0; i <= actorsIndices.Count; i++ ) {
                if( i == actorsIndices.Count ) {
                    rightIndex = actorsIndices.Count;
                    right = regionRight;
                } else if( _allActors[actorsIndices[i]].PositionType == TDialogueActorArrangementPosition.Custom ) {
                    rightIndex = i;
                    right = _allActors[actorsIndices[i]].Position.x;
                }
                if( rightIndex > leftIndex ) {
                    float actorRegionWidth = (right - left) / (rightIndex - leftIndex);
                    // Выставим позиции лицам между leftIndex (включительно) и rightIndex (не включительно)
                    for( int j = leftIndex; j < rightIndex; j++ ) {
                        _allActors[actorsIndices[j]].Position.x = left + actorRegionWidth * ( j + 0.5f );
                    }
                    leftIndex = rightIndex + 1;
                }
            }
        }

        // Перемещает лица (а точнее игровой объект, ему соответствующий) в нужные позиции
        private void MoveActors()
        {
            foreach( var element in _actorNameToIndex ) {
                if( _shownActorsIndices.Contains( element.Value ) ) {
                    bool wasActive = _allActors[element.Value].ActorObject.activeInHierarchy;
                    _allActors[element.Value].ActorObject.SetActive( true );
                    if( !wasActive ) {
                        // Если персонаж был на сцене, но был не виден, а стал виден, то сразу выставим ему позицию
                        _allActors[element.Value].ActorObject.GetComponent<RectTransform>().anchoredPosition = _allActors[element.Value].Position;
                    }
                } else {
                    _allActors[element.Value].ActorObject.SetActive( false );
                }
            }

            foreach( int index in _shownActorsIndices ) {
                Vector3 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                pos.z = 0f;
                if( _allActors[index].PositionType != TDialogueActorArrangementPosition.Custom ) {
                    pos.y = _allActors[index].Position.y;
                }
                // Перемещаем новых лиц в нижнюю часть экрана
                if( index >= _prevActorsCount ) {
                    pos.x = _allActors[index].Position.x;
                    pos.y = _appearanceHeight;
                }
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        // Выставить изображения лицам
        // Также меняет положение по оси Y и размер изображениям
        private void SetActorsImages( CDialogueActorImage[] actorsImages )
        {
            foreach( CDialogueActorImage actorImage in actorsImages ) {
                Assert.IsTrue( _actorNameToIndex.ContainsKey( actorImage.ActorName ), $"There is no actor with name {actorImage.ActorName}. Check actor images descriptions." );
                // Выставляем изображение
                CActorInfo actorInfo = _allActors[_actorNameToIndex[actorImage.ActorName]];
                Image imageComponent = actorInfo.ActorObject.GetComponent<Image>();
                imageComponent.sprite = actorImage.Image;
            }

            int siblingIndex = transform.childCount - 1;
            int currentIndex = _leadActorIndex;

            for( int i = 0; i < _shownActorsIndices.Count; i++ ) {
                // Выставляем размер изображениям
                CActorInfo actorInfo = _allActors[currentIndex];
                Image imageComponent = actorInfo.ActorObject.GetComponent<Image>();
                Rect imageRect = new Rect();
                if( actorInfo.UseAutosize ) {
                    imageRect.size = imageComponent.sprite.rect.size * _actorDefaultScale;
                } else {
                    if( actorInfo.UseCustomScale ) {
                        imageRect.size = imageComponent.sprite.rect.size * actorInfo.Scale;
                    } else {
                        imageRect.size = actorInfo.Size;
                    }
                }

                imageComponent.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, imageRect.width );
                imageComponent.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, imageRect.height );

                // Выставляем позицию по оси y
                if( actorInfo.PositionType != TDialogueActorArrangementPosition.Custom ) {
                    actorInfo.Position.y = _boundaries.yMin + imageRect.height / 2;
                }

                actorInfo.ActorObject.transform.SetSiblingIndex( siblingIndex-- );
                currentIndex = actorInfo.NextActorIndex;
            }
        }

        private const float LeaveEps = 0.1f;

        // Анимировать переходы
        private void AnimateActors()
        {
            foreach( int index in _shownActorsIndices ) {
                Vector3 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp( pos, _allActors[index].Position, _transitionSpeed );
            }
            foreach( int index in _removedActorsIndices ) {
                if( _allActors[index].ActorObject == null ) {
                    continue;
                }
                Vector2 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                if( (pos - _allActors[index].Position).magnitude < LeaveEps ) {
                    Destroy( _allActors[index].ActorObject );
                } else {
                    _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp( pos, _allActors[index].Position, _transitionSpeed );
                }
            }
        }

        // Находится ли данная позиция в регионе данного типа
        private bool IsInRegion( float position, TDialogueActorArrangementPosition regionType )
        {
            float regionLeft = GetRegionLeft( regionType ), regionRight = GetRegionRight( regionType );
            return position >= regionLeft && position < regionRight;
        }

        // Получить дефолтную позицию по оси x для данного типа позиции
        private float GetDefaultPosition( TDialogueActorArrangementPosition positionType )
        {
            switch( positionType ) {
                case TDialogueActorArrangementPosition.Left:
                    return _boundaries.xMin;
                case TDialogueActorArrangementPosition.Center:
                    return (_boundaries.xMax + _boundaries.xMin) / 2;
                case TDialogueActorArrangementPosition.Right:
                    return _boundaries.xMax;
                case TDialogueActorArrangementPosition.Custom:
                    return 0f;
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }

        // Левая граница региона заданного типа
        private float GetRegionLeft( TDialogueActorArrangementPosition regionType )
        {
            switch( regionType ) {
                case TDialogueActorArrangementPosition.Left:
                    return _boundaries.xMin;
                case TDialogueActorArrangementPosition.Center:
                    return GetCenterLeft();
                case TDialogueActorArrangementPosition.Right:
                    return GetCenterRight();
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }

        // Правая граница региона заданного типа
        private float GetRegionRight( TDialogueActorArrangementPosition regionType )
        {
            switch( regionType ) {
                case TDialogueActorArrangementPosition.Left:
                    return GetCenterLeft();
                case TDialogueActorArrangementPosition.Center:
                    return GetCenterRight();
                case TDialogueActorArrangementPosition.Right:
                    return _boundaries.xMax;
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }

        // Левая граница центрального региона
        private float GetCenterLeft()
        {
            return _boundaries.xMin + _boundaries.width / 3;
        }

        // Правая граница центрального региона
        private float GetCenterRight()
        {
            return _boundaries.xMin + _boundaries.width * 2 / 3;
        }

        [SerializeField]private float _actorDefaultScale = 0.4f;// Насколько нужно изменить размер оригинальной картинки персонажа
        [SerializeField]private Rect _boundaries;// ограничивающий прямоугольник
        [SerializeField] private float _appearanceHeight;// на какой координате появляются и исчезают лица
        [SerializeField]private float _transitionSpeed = 1f;// Время перехода (перемещений спрайтов)

        private int _maxActorsOnScreen;// Максимальное кол-во отображаемых лиц диалога
        private List<CActorInfo> _allActors;// Информация обо всех лицах диалога, что участвовали в нём 
        private Dictionary<string, int> _actorNameToIndex;// Отображение из имении диалога к его индексу в _allActors. Также указывает, какие лица в данный момент присутствуют в диалоге
        private HashSet<int> _shownActorsIndices;// Индексы отображаемых лиц
        private HashSet<int> _removedActorsIndices;// Индексы убираемых лиц
        private int _prevActorsCount;// размер списка _allActors при предыдущем распределении
        private int _leadActorIndex;// Индекс последнего говорившего        
    }
}