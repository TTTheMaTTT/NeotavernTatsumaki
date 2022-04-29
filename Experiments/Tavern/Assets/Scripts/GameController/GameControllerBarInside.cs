using System.Collections;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.Assertions;

public class GameControllerBarInside : GameControllerAbstract
{
    [System.Serializable]
    private struct BarVisitor
    {
        [ActorPopup]
        public string Actor;
        [ConversationPopup]
        public string Conversation;
    }

    private Dragger _currentDragger;
    // Объект, с которым взаимодействуем в данный момент
    private InteractableAbstract _currentInteractable;
    // Объект, с которым можем провзаимодействовать
    private InteractableAbstract _possibleInteraction;
    // Игровой курсор
    private CursorAbstract _gameCursor;
    private Vector2 _prevCursorPosition;

    private DialogueSystemController _dialogueSystemController;
    private VisualNovelSceneUI _dialogueScene;

    [SerializeField] private BarVisitor firstVisitor;
    private bool _isGameStarted = false;

    private void Awake()
    {
        var cursors = FindObjectsOfType<CursorAbstract>();
        if( cursors.Length > 1 ) {
            Debug.LogWarning( "There are more than 1 objects of type CursorAbstract" );
            for( int i = cursors.Length - 1; i >= 1; i-- ) {
                Destroy( cursors[i] );
            }
        }
        if( cursors.Length == 1 ) {
            _gameCursor = cursors[0];
            _prevCursorPosition = (Vector2)_gameCursor.transform.position - new Vector2( -1, -1 );
        }
    }


    private void Start()
    {
        Cursor.visible = false;
        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();
        var scenes = FindObjectsOfType<VisualNovelSceneUI>();
        if( scenes.Length >= 1 ) {
            if( scenes.Length > 1 ) {
                Debug.LogWarning( "There are more than 1 objects of type VisualNovelSceneUI" );
            }
            _dialogueScene = scenes[0];
        }
        _isGameStarted = false;
    }
    

    private void Update()
    {

        if( _gameCursor != null ) {
            if( _prevCursorPosition != (Vector2)_gameCursor.transform.position ) {
                _prevCursorPosition = _gameCursor.transform.position;
                Vector2 cursorPos = _gameCursor.transform.position;
                Collider2D[] cols = Physics2D.OverlapPointAll( cursorPos );
                InteractableAbstract newInteractable = null;
                int interactableLayer = int.MinValue;
                foreach( var col in cols ) {
                    var interactable = col.GetComponent<InteractableAbstract>();
                    if( interactable != null && interactableLayer < interactable.interactionLayer ) {
                        newInteractable = interactable;
                        interactableLayer = interactable.interactionLayer;
                    }
                }
                if( newInteractable != _possibleInteraction ) {
                    if( _currentInteractable == null ) {
                        if( _possibleInteraction != null ) {
                            _possibleInteraction.HideInteraction();
                        }
                    }
                    _possibleInteraction = newInteractable;
                    if( _currentInteractable == null && _possibleInteraction != null ) {
                        _possibleInteraction.ShowInteraction();
                    }
                }
            }
        }

        CheckInputs();

        if( !_isGameStarted ) {
            if( _dialogueSystemController != null ) {
                if( _dialogueScene != null ) {
                    _dialogueScene.SetActorPermanence( _dialogueSystemController.GetActorId( firstVisitor.Actor ), true );
                }
                _dialogueSystemController.StartConversation( firstVisitor.Conversation );
            }
            _isGameStarted = true;
        }
    }


    private void CheckInputs()
    {
        if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
            if( _possibleInteraction != null && _currentInteractable == null ) {
                StartInteraction( _possibleInteraction );
            }
        }
        if( Input.GetKeyUp( KeyCode.Mouse0 ) ) {
            if( _currentInteractable != null ) {
                StopInteraction( _currentInteractable );
            }
        }
        if( Input.GetKeyDown( KeyCode.Mouse1 ) ) {
            if( _currentInteractable != null ) {
                IDraggable _draggable = _currentInteractable.GetComponent<IDraggable>();
                if( _draggable != null ) {
                    _draggable.StartRotate();
                }
            }
            if( _currentDragger != null ) {
                _currentDragger.SetRotationMode( true );
            }
            if( _gameCursor != null ) {
                _gameCursor.SetMoveability( false );
            }
            Cursor.visible = true;
        }
        if( Input.GetKeyUp( KeyCode.Mouse1 ) ) {
            if( _currentInteractable != null ) {
                IDraggable _draggable = _currentInteractable.GetComponent<IDraggable>();
                if( _draggable != null ) {
                    _draggable.StopRotate();
                }
            }
            if( _currentDragger != null ) {
                _currentDragger.SetRotationMode( false );
            }
            if( _gameCursor != null ) {
                _gameCursor.SetMoveability( true );
            }
            Cursor.visible = false;
        }
    }


    private void StartInteraction( InteractableAbstract interactable )
    {
        _currentInteractable = interactable;
        var draggable = interactable.GetComponent<IDraggable>();
        if( _gameCursor != null && draggable != null ) {
            draggable.StartDrag();
            _currentDragger = new GameObject( "dragger" ).AddComponent<Dragger>();
            _currentDragger.Initialize( _gameCursor, draggable );
        }

        // Больше ни с чем нельзя начать взаимодействие - мы его уже начали и производим прямо сейчас.
        if( _possibleInteraction != null ) {
            _possibleInteraction.HideInteraction();
        }
    }


    private void StopInteraction( InteractableAbstract interactable )
    {
        _currentInteractable = null;
        var draggable = interactable.GetComponent<IDraggable>();
        if( draggable != null ) {
            if( _currentDragger != null ) {
                _currentDragger.Detach();
                Destroy( _currentDragger.gameObject );
                _currentDragger = null;
            }
            draggable.StopDrag();
        }
        if( _gameCursor != null ) {
            _gameCursor.SetMoveability( true );
        }

        // Покажем, что можно производить новые взаимодействия
        if( _possibleInteraction != null ) {
            _possibleInteraction.ShowInteraction();
        }
    }

}
