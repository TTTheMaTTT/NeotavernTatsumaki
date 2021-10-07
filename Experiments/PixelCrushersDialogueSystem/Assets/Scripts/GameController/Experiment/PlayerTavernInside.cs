using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

// Управление персонаем игрока внутри комнаты.
public class PlayerTavernInside : MonoBehaviour
{
    [SerializeField]
    private float IconSpeed = 2f;

    private bool _isMoveabilityEnabled;// Может ли двигаться персонаж

    private Vector2 startPosition;
    private Vector2 targetPosition;

    void Awake()
    {
        startPosition = transform.position;
        targetPosition = startPosition;
        _isMoveabilityEnabled = true;
    }

    private const float eps = 0.01f;
    

    // Update is called once per frame
    void Update()
    {
        if( _isMoveabilityEnabled ) {
            if( Vector2.Distance( transform.position, targetPosition ) > 0 ) {
                transform.position = Vector2.Lerp( transform.position, targetPosition, IconSpeed );
                if( Vector2.Distance( transform.position, targetPosition ) < eps ) {
                    transform.position = targetPosition;
                }
            }
        }
    }

    private const string playerPositionObjectName = "PlayerPosition";

    public void SelectUsable( Usable usable )
    {
        if( usable == null ) {
            targetPosition = startPosition;
            return;
        }

        Transform playerPosTransform = usable.transform.Find( playerPositionObjectName );
        if( playerPosTransform != null ) {
            targetPosition = playerPosTransform.position;
        } else {
            targetPosition = usable.transform.position;
        }
    }


    public void DeselectUsable( Usable usable )
    {
        targetPosition = startPosition;
    }


    // Установить подвижность игроку. Может ли он двигаться или нет.
    public void SetMovability( bool isMoveabilityEnabled )
    {
        _isMoveabilityEnabled = isMoveabilityEnabled;
    }

}
