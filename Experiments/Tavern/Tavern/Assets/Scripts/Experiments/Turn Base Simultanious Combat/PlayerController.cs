using System.Collections;
using UnityEngine;

namespace Experiments.TurnBaseSimultaniousCombat
{
    public class PlayerController : CharacterController
    {
        [SerializeField] private float _inputCooldownTime = 1f;
        private Vector2 _targetPosition;

        public const string DefaultLayerName = "Default";
        private int _movementMask;

        bool _isInInputCooldown = false;


        protected override void Awake()
        {
            base.Awake();
            _movementMask = LayerMask.GetMask( DefaultLayerName );
            _targetPosition = transform.position;
        }


        private void Update()
        {
            if( !_isInInputCooldown ) {
                float moveX = Input.GetAxis( "Horizontal" );
                float moveY = Input.GetAxis( "Vertical" );
                bool haveInput = moveX != 0 || moveY != 0;
                Vector3 position = transform.position;
                if( moveX != 0f ) {
                    position = GameControllerAbstract.Instance().GetPositionOnGridWithOffset( 
                                                                    position, new Vector2( Mathf.Sign( moveX ), 0f ) );
                } else if( moveY != 0f ) {
                    position = GameControllerAbstract.Instance().GetPositionOnGridWithOffset(
                                                                    position, new Vector2( 0f, Mathf.Sign( moveY ) ) );
                }
                if( transform.position != position ) {
                    Vector3 relPos = position - transform.position;
                    var raycastHit = Physics2D.Raycast( transform.position, relPos, relPos.magnitude, _movementMask );
                    if( raycastHit.collider == null ) {
                        _targetPosition = position;
                    }
                }
                if( haveInput ) {
                    GameControllerAbstract.Instance().InitiateGameAction();
                    StartCoroutine( InputCooldown() );

                }
            }
            transform.position = Vector2.Lerp( transform.position, _targetPosition, Settings.MovementSpeed * Time.deltaTime );
        }


        private IEnumerator InputCooldown()
        {
            _isInInputCooldown = true;
            yield return new WaitForSeconds( _inputCooldownTime );
            _isInInputCooldown = false;
        }

    }
}