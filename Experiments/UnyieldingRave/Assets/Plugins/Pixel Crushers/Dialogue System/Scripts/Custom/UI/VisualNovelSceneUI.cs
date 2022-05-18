using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{

    // Окошко с персонажами диалога, которые отображаются в виде 2D спрайтов. 
    // Умеет выставлять, перемещать и выставлять актёров на сцене, в зависимости от параметров текущего DialogueEntry.
    public class VisualNovelSceneUI : DialogueSceneUIBase
    {

        private const int DefaultMaxActorsOnScreen = 6;
        private const string ActorArrangementActionFieldName = "Actor Arrangement Action";
        private const string PositionTypeFieldName = "Position Type";

        // Structure, that helps to organize actors on the scene
        private class ActorInfo
        {
            public int ActorID;
            public GameObject ActorObject;// GameObject - UI-element, that represents actor
            public ActorScreenPositionType PositionType;// In which part of screen to place an actor?
            public Vector2 Position;// Exact position of an actor.
            // Actors indices in _allActorsList
            // This indices also shows who spoke a dialogue entry recently and therefore must be shown on scene.
            public int Index;// Index of this actor in 
            public int PrevActorIndex = -1;// Если -1, значит, данное лицо говорило последним
            public int NextActorIndex = -1;// Если -1, значит, данное лицо мы не слышали дольше всех
        }

        #region Serialized Fields

        [SerializeField] private float _actorDefaultScale = 0.4f;// Scaling of actor's original image on this window

        [SerializeField] private Rect _boundaries;// boundaries, that limits actors positioning

        [SerializeField] private float _instantiatedActorYPos;// default y-coordinate of actors

        [SerializeField] private float _transitionSpeed = 1f;// speed of image's position transitions

        #endregion //Serialized Fields

        #region Properties & Private Fields

        private int _maxActorsOnScreen = DefaultMaxActorsOnScreen;// Maximum of actors count, that are shown

        // Info about all actors, that participate and participated in conversation. 
        // Two elements can correspond to one actor, but only one of them can be currently used.
        private List<ActorInfo> _allActors;
        private Dictionary<int, int> _actorIdToIndex;// Map from actor's id to its index in _allActors. Also shows, which actors are CURRENTLY participating in conversation
        private HashSet<int> _shownActorsIndices;// indices of shown actors
        private HashSet<int> _removedActorsIndices;// Indices of removed actors
        private int _prevActorsCount;// _allActors size before last update of the dialogue scene
        private int _leadActorIndex;// Index of the leading actor who is currently speaking

        #endregion //Properties & Private Fields

        #region Initialization

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            _allActors = new List<ActorInfo>();
            _actorIdToIndex = new Dictionary<int, int>();
            _shownActorsIndices = new HashSet<int>();
            _removedActorsIndices = new HashSet<int>();
            _prevActorsCount = 0;
        }

        #endregion // Initialization

        private void Update()
        {
            MoveActors();
        }


        // Clear all service structure and destroy actors UI
        public override void Reset()
        {
            _allActors.Clear();
            _actorIdToIndex.Clear();
            _shownActorsIndices.Clear();
            _removedActorsIndices.Clear();
            _prevActorsCount = 0;
            _leadActorIndex = -1;
            for( int i = transform.childCount - 1; i >= 0; i-- ) {
                DestroyImmediate( transform.GetChild( i ).gameObject );
            }
        }
        

        // Make all actors leave the scene. They will move to the bottom of screen
        public override void CloseScene()
        {
            foreach( int index in _shownActorsIndices ) {
                DeleteActor( _allActors[index].ActorID );
            }
            DefineShownIndices();
            MakeArrangements();
        }


        // Update scene with information from entry
        public override void ChangeScene( DialogueEntry entry, List<CharacterInfo> entryActorsInfo )
        {
            if( entry == null || entryActorsInfo  == null) {
                return;
            }
            // Prepare for the next state of the scene
            PrepareForNextSceneState();
            
            // Consider actors
            PrepareActors( entry.actorsStates, out bool mustChangeArrangement );

            // Consider leading actor (who speaks current line)
            if( _actorIdToIndex.ContainsKey( entry.ActorID ) ) {
                if( !_shownActorsIndices.Contains( _actorIdToIndex[entry.ActorID] ) ) {
                    mustChangeArrangement = true;
                }
                MakeLeading( entry.ActorID );
            }

            DefineShownIndices();

            // Change actors positions
            if( mustChangeArrangement ) {
                MakeArrangements();
            }

            // Change actors images
            SetActorsImages( entryActorsInfo );

            // Showing and hiding actors
            SetActorsVisibility();

            _prevActorsCount = _allActors.Count;
        }


        // Finish work that must be done in previous state.
        // Actors moved to their positions, designated by prevous state.
        // Instantly delete actors, that should be deleted im previous state.
        private void PrepareForNextSceneState()
        {
            // Actors positions
            foreach( int index in _shownActorsIndices ) {
                Assert.IsTrue( _allActors[index].ActorObject != null );
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = _allActors[index].Position;
            }

            // Actors deletions
            foreach( int index in _removedActorsIndices ) {
                if( _allActors[index].ActorObject != null ) {
                    Destroy( _allActors[index].ActorObject );
                }

            }
        }


        // Consider which actors are new and which must be removed
        private void PrepareActors( List<ActorState> actorStates, out bool mustChangeArrangement )
        {
            _removedActorsIndices.Clear();

            mustChangeArrangement = false;
            foreach( var actorState in actorStates ) {
                if( !Field.FieldExists( actorState.fields, ActorArrangementActionFieldName ) ) {
                    continue;
                }
                string actionTypeString = Field.LookupValue( actorState.fields, ActorArrangementActionFieldName );
                ActorArrangementActionType actionType = (ActorArrangementActionType)Enum.Parse( typeof( ActorArrangementActionType ), actionTypeString );

                // Manage actor infos
                switch( actionType ) {
                    case ActorArrangementActionType.None:
                        break;

                    case ActorArrangementActionType.ChangePosition:
                        if( !_actorIdToIndex.ContainsKey( actorState.ActorID ) ) {
                            AddActor( actorState.ActorID );
                            mustChangeArrangement = true;
                        }
                        Assert.IsTrue( _actorIdToIndex.ContainsKey( actorState.ActorID ), "Arrangement with type 'changePosition' is applied to the absent actor" );
                        ActorInfo actorInfo = _allActors[_actorIdToIndex[actorState.ActorID]];
                        Assert.IsTrue( Field.FieldExists( actorState.fields, PositionTypeFieldName ), "Actor state doesn't have Position Type field when it's needed" );
                        string positionTypeString = Field.LookupValue( actorState.fields, PositionTypeFieldName );
                        ActorScreenPositionType positionType = (ActorScreenPositionType)Enum.Parse( typeof( ActorScreenPositionType ), positionTypeString );
                        if( actorInfo.PositionType != positionType ){
                            mustChangeArrangement = true;
                        }
                        // Update actorInfo
                        actorInfo.PositionType = positionType;
                        break;

                    case ActorArrangementActionType.Leave:
                        DeleteActor( actorState.ActorID );
                        mustChangeArrangement = true;
                        break;
                    default:
                        Assert.IsTrue( false );
                        break;
                }
            }
        }


        /// Add actor to the scene
        private void AddActor( int actorId )
        {
            Assert.IsTrue( !_actorIdToIndex.ContainsKey( actorId ), "Adding the present actor" );
            ActorInfo actorInfo = new ActorInfo();
            actorInfo.ActorID = actorId;

            // Creating GameObject with image of this actor
            var actorImage = new GameObject().AddComponent<Image>();
            actorImage.transform.SetParent( transform );
            actorImage.transform.position = new Vector3( 0f, 0f, 0f );
            actorImage.transform.localScale = new Vector3( 1f, 1f, 1f );
            actorInfo.ActorObject = actorImage.gameObject;
            actorInfo.Index = _allActors.Count;
            _actorIdToIndex.Add( actorId, _allActors.Count );

            // Set default position on the scene
            actorInfo.Position.x = GetDefaultPosition( actorInfo.PositionType );

            _allActors.Add( actorInfo );

            // Always try to make new actor leading even if he didn't say anything
            MakeLeading( actorId );
        }


        // Delete active actor from scene
        private void DeleteActor( int actorId )
        {
            Assert.IsTrue( _actorIdToIndex.ContainsKey( actorId ), "Arrangement with type 'leave' is applied to the absent actor" );
            int index = _actorIdToIndex[actorId];
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
            _actorIdToIndex.Remove( actorId );
            _removedActorsIndices.Add( index );
        }


        // Make specified actor leading. That means that he will be shown and emphasized
        private void MakeLeading( int actorId )
        {
            Assert.IsTrue( _actorIdToIndex.ContainsKey( actorId ), "Trying to make leading absent actor" );
            int index = _actorIdToIndex[actorId];
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


        // Define which actors should be shown
        void DefineShownIndices()
        {
            _shownActorsIndices.Clear();
            int currentIndex = _leadActorIndex;
            while( currentIndex != -1 && _shownActorsIndices.Count < _maxActorsOnScreen ) {
                _shownActorsIndices.Add( currentIndex );
                currentIndex = _allActors[currentIndex].NextActorIndex;
            }
        }


        // Specify positions on the scene for every actor
        private void MakeArrangements()
        {
            // Divide panel on three parts: left, center, right.
            // Define in which parts and at what order actors should be.
            ManageActorsOrder( out List<int> leftActors, out List<int> centerActors, out List<int> rightActors );

            // Set actors exact positions
            SpecifyPositions( leftActors, ActorScreenPositionType.Left );
            SpecifyPositions( centerActors, ActorScreenPositionType.Center );
            SpecifyPositions( rightActors, ActorScreenPositionType.Right );

            // Specify positions for removed actors
            foreach( int index in _removedActorsIndices ) {
                _allActors[index].Position.y = _instantiatedActorYPos;
            }
        }

        // Define on which parts of screen and at what order place actors
        private void ManageActorsOrder( out List<int> leftActorsIndices, out List<int> centerActorsIndices, out List<int> rightActorsIndices )
        {
            leftActorsIndices = new List<int>();
            centerActorsIndices = new List<int>();
            rightActorsIndices = new List<int>();
            List<ActorInfo> leftActors = new List<ActorInfo>();
            List<ActorInfo> centerActors = new List<ActorInfo>();
            List<ActorInfo> rightActors = new List<ActorInfo>();

            List<int> indices = _shownActorsIndices.ToList();

            foreach( int index in indices.OrderBy( i => i ) ) {
                switch( _allActors[index].PositionType ) {
                    case ActorScreenPositionType.Left:
                        leftActors.Add( _allActors[index] );
                        break;
                    case ActorScreenPositionType.Center:
                        centerActors.Add( _allActors[index] );
                        break;
                    case ActorScreenPositionType.Right:
                        rightActors.Add( _allActors[index] );
                        break;
                    default:
                        Assert.IsTrue( false );
                        break;
                }
            }
            foreach( var actor in leftActors.OrderBy( a => a.Position.x ) ) {
                leftActorsIndices.Add( actor.Index );
            }
            foreach( var actor in centerActors.OrderBy( a => a.Position.x ) ) {
                centerActorsIndices.Add( actor.Index );
            }
            foreach( var actor in rightActors.OrderBy( a => a.Position.x ) ) {
                rightActorsIndices.Add( actor.Index );
            }
        }


        // Calculate x-coordinates for actors
        private void SpecifyPositions( List<int> actorsIndices, ActorScreenPositionType regionType )
        {
            float regionLeft = GetRegionLeft( regionType ), regionRight = GetRegionRight( regionType );
            float left = regionLeft, right = regionLeft;
            int leftIndex = 0, rightIndex = 0;
            for( int i = 0; i <= actorsIndices.Count; i++ ) {
                if( i == actorsIndices.Count ) {
                    rightIndex = actorsIndices.Count;
                    right = regionRight;
                }
                if( rightIndex > leftIndex ) {
                    float actorRegionWidth = (right - left) / (rightIndex - leftIndex);
                    // find positions for actors from leftIndex (including) to rightIndex (not including)
                    for( int j = leftIndex; j < rightIndex; j++ ) {
                        _allActors[actorsIndices[j]].Position.x = left + actorRegionWidth * (j + 0.5f);
                    }
                    leftIndex = rightIndex + 1;
                }
            }
        }


        // Set actors images
        // Also sets it's dedicated y-coordinate and size in order to fit on the screen
        private void SetActorsImages( List<CharacterInfo> entryActorsInfo )
        {
            foreach( CharacterInfo info in entryActorsInfo ) {
                if( !_actorIdToIndex.ContainsKey( info.id ) ) {
                    continue;
                }
                // Выставляем изображение
                ActorInfo actorInfo = _allActors[_actorIdToIndex[info.id]];
                Image imageComponent = actorInfo.ActorObject.GetComponent<Image>();
                imageComponent.sprite = info.portrait;

                Rect imageRect = new Rect();
                imageRect.size = imageComponent.sprite.rect.size * _actorDefaultScale;

                imageComponent.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, imageRect.width );
                imageComponent.rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, imageRect.height );

                // Set actors y-coordinate
                var rectTransform = actorInfo.ActorObject.GetComponent<RectTransform>();
                actorInfo.Position.y = _boundaries.yMin + imageRect.height / 2;
                var newPos = rectTransform.anchoredPosition;
                newPos.y = actorInfo.Position.y;
                rectTransform.anchoredPosition = newPos;

            }

            int siblingIndex = transform.childCount - 1;
            int currentIndex = _leadActorIndex;

            for( int i = 0; i < _shownActorsIndices.Count; i++ ) {
                // Set images size
                ActorInfo actorInfo = _allActors[currentIndex];

                // Set transform order
                actorInfo.ActorObject.transform.SetSiblingIndex( siblingIndex-- );
                currentIndex = actorInfo.NextActorIndex;
            }
        }

        // Show actors that must be shown
        private void SetActorsVisibility()
        {
            foreach( var element in _actorIdToIndex ) {
                if( !_shownActorsIndices.Contains( element.Value ) ) {
                    _allActors[element.Value].ActorObject.SetActive( false );
                }
            }

            foreach( int index in _shownActorsIndices ) {
                bool wasActive = _allActors[index].ActorObject.activeSelf;
                _allActors[index].ActorObject.SetActive( true );
                Vector3 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                pos.z = 0f;
                // Перемещаем новых лиц в нижнюю часть экрана
                if( index >= _prevActorsCount ) {
                    pos.x = _allActors[index].Position.x;
                    pos.y = _instantiatedActorYPos;
                } else if( !wasActive ) {
                    pos.x = GetDefaultPosition( _allActors[index].PositionType );
                    pos.y = _allActors[index].Position.y;
                }
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = pos;
            }
        }

        private const float LeaveEps = 0.1f;


        private void MoveActors()
        {
            float delta = _transitionSpeed * DialogueTime.deltaTime;
            foreach( int index in _shownActorsIndices ) {
                Vector3 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp( pos, _allActors[index].Position, delta );
            }
            foreach( int index in _removedActorsIndices ) {
                if( _allActors[index].ActorObject == null ) {
                    continue;
                }
                Vector2 pos = _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition;
                if( (pos - _allActors[index].Position).magnitude < LeaveEps ) {
                    Destroy( _allActors[index].ActorObject );
                } else {
                    _allActors[index].ActorObject.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp( pos, _allActors[index].Position, delta );
                }
            }
        }

        // Get default x-coordinate for positionType
        private float GetDefaultPosition( ActorScreenPositionType positionType )
        {
            switch( positionType ) {
                case ActorScreenPositionType.Left:
                    return _boundaries.xMin;
                case ActorScreenPositionType.Center:
                    return (_boundaries.xMax + _boundaries.xMin) / 2;
                case ActorScreenPositionType.Right:
                    return _boundaries.xMax;
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }


        // Left boundary of screen region
        private float GetRegionLeft( ActorScreenPositionType regionType )
        {
            switch( regionType ) {
                case ActorScreenPositionType.Left:
                    return _boundaries.xMin;
                case ActorScreenPositionType.Center:
                    return GetCenterLeft();
                case ActorScreenPositionType.Right:
                    return GetCenterRight();
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }


        // Right boundary of screen region
        private float GetRegionRight( ActorScreenPositionType regionType )
        {
            switch( regionType ) {
                case ActorScreenPositionType.Left:
                    return GetCenterLeft();
                case ActorScreenPositionType.Center:
                    return GetCenterRight();
                case ActorScreenPositionType.Right:
                    return _boundaries.xMax;
                default:
                    Assert.IsTrue( false );
                    return 0f;
            }
        }


        private float GetCenterLeft()
        {
            return _boundaries.xMin + _boundaries.width / 3;
        }


        private float GetCenterRight()
        {
            return _boundaries.xMin + _boundaries.width * 2 / 3;
        }
    }
}
