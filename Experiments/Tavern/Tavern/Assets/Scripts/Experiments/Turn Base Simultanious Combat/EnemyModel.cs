using System.Collections.Generic;
using UnityEngine;

namespace Experiments.TurnBaseSimultaniousCombat
{
    public class EnemyModel
    {
        private CharacterActionType _currentAction;
        private CharacterOrientation _currentOrientation;
        private Vector2 _moveDirection;
        private Transform _transform;
        private Vector2 _currentPosition;

        private const string DefaultLayerName = "Default";
        private int _movementMask;

        public CharacterOrientation CurrentOrientation
        {
            get {
                return _currentOrientation;
            }
        }

        public Vector2 CurrentPosition
        {
            get {
                return _currentPosition;
            }
        }

        public void Initialize( Transform transform, CharacterOrientation initialOrientation )
        {
            _transform = transform;
            _currentOrientation = initialOrientation;
            _currentPosition = transform.position;
            _movementMask = LayerMask.GetMask( DefaultLayerName );
        }

        public void DecideAction()
        {
            _moveDirection = _currentOrientation == CharacterOrientation.Right ? new Vector2( 1f, 0f ) : new Vector2( -1f, 0f );
            Vector3 curPos = GameControllerAbstract.Instance().GetPositionOnGrid( _currentPosition );
            Vector3 nextPos = GameControllerAbstract.Instance().GetPositionOnGridWithOffset( _currentPosition, _moveDirection );
            Vector3 relPos = nextPos - curPos;
            var raycastHit = Physics2D.Raycast( curPos, relPos, relPos.magnitude, _movementMask );
            if( raycastHit.collider == null ) {
                _currentAction = CharacterActionType.Move;
            } else {
                _currentAction = CharacterActionType.Turn;
            }
        }

        public void Action()
        {
            switch( _currentAction ) {
                case CharacterActionType.Move:
                {
                    _currentPosition = GameControllerAbstract.Instance().GetPositionOnGridWithOffset( _transform.position, _moveDirection );
                    break;
                }
                case CharacterActionType.Turn: 
                {
                    _currentOrientation = _currentOrientation == CharacterOrientation.Left ? 
                        CharacterOrientation.Right : CharacterOrientation.Left;
                    break;
                }
                default:
                    break;
            }
        }
    }
}

