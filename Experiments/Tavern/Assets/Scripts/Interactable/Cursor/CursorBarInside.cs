using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Курсор, который видно в игре.
// Что-то типа руки бармена
public class CursorBarInside : CursorAbstract
{
    public Sprite CursorHandOpen;
    public Sprite CursorHandClose;

    private SpriteRenderer spriteRenderer;

    bool _moveable;
    bool _moveToMousePrecisely = true;
    float _mouseSpeed = 15f;
    float _mouseDelta = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        _moveToMousePrecisely = true;
        _moveable = true;
    }


    public override void SetMoveability( bool moveable )
    {
        _moveable = moveable;
        if( !moveable ) {
            _moveToMousePrecisely = false;
        }
    }


    private void Update()
    {
        if( Input.GetKeyDown( KeyCode.Mouse0 ) ) {
            spriteRenderer.sprite = CursorHandClose;
        }
        if( Input.GetKeyUp( KeyCode.Mouse0 ) ) {
            spriteRenderer.sprite = CursorHandOpen;
        }

        if( !_moveable ) {
            return;
        }

        Vector2 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        if( _moveToMousePrecisely ) {
            transform.position = mousePos;
        } else {
            transform.position = Vector3.Lerp( transform.position, mousePos, _mouseSpeed * Time.deltaTime );
            if( ((Vector2)transform.position - mousePos).magnitude < _mouseDelta ) {
                _moveToMousePrecisely = true;
            }
        }

    }
}
