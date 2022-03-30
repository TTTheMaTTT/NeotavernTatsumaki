using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameControllerBarInside : GameControllerAbstract
{
    private Dragger _currentDragger;
    private InteractableAbstract _currentInteractable;
    private CursorAbstract _gameCursor;

    private void Awake()
    {
        var cursors = FindObjectsOfType<CursorAbstract>();
        if( cursors.Length > 1 ) {
            Debug.LogWarning( "There are more than 1 objects of type CursorAbstract" );
            for( int i = cursors.Length - 1; i >= 1; i-- ) {
                Destroy( cursors[i] );
            }
        }
        _gameCursor = cursors[0];
    }

    private void Start()
    {
        Cursor.visible = false;
    }

    private void Update()
    {
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

    public void StartInteraction( InteractableAbstract interactable )
    {
        _currentInteractable = interactable;
        var draggable = interactable.GetComponent<IDraggable>();
        if( _gameCursor != null && draggable != null ) {
            draggable.StartDrag();
            _currentDragger = new GameObject( "dragger" ).AddComponent<Dragger>();
            _currentDragger.Initialize( _gameCursor, draggable );
        }
    }

    public void StopInteraction( InteractableAbstract interactable )
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
    }

}
