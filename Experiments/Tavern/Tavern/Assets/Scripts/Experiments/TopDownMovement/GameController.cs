using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Experiments.TopDownMovement
{
    public class GameController : GameControllerAbstract
    {
        public const string PlayerTag = "Player";
        public const string DefaultLayerName = "Default";

        private Grid _grid;
        private Transform _player;
        private Vector3 _playerPosition;

        [SerializeField] private float _inputCooldownTime = 1f;
        [SerializeField] private float _movementSpeed = 0f;
        private int _movementMask;

        bool _isInInputCooldown = false;

        protected override void initialize()
        {
            var players = GameObject.FindGameObjectsWithTag( PlayerTag );
            Assert.IsTrue( players.Length == 1, "One and only one player is allowed!" );
            _player = players[0].transform;

            var grids = FindObjectsOfType<Grid>();
            Assert.IsTrue( grids.Length > 0, "Can't find any grid" );
            if( grids.Length > 1 ) {
                Debug.LogWarning( "Found more than one grid" );
            }
            _grid = grids[0];
            _player.position = SnapPositionToGrid( _player.position );
            _playerPosition = _player.position;

            _movementMask = LayerMask.GetMask( DefaultLayerName );
        }


        void Update()
        {
            if( !_isInInputCooldown ) {
                float moveX = Input.GetAxis( "Horizontal" );
                float moveY = Input.GetAxis( "Vertical" );
                Vector3 position = _playerPosition;
                if( moveX != 0f ) {
                    position.x += (_grid.cellSize.x + _grid.cellGap.x) * Mathf.Sign( moveX );
                    position = SnapPositionToGrid( position );
                } else if( moveY != 0f ) {
                    position.y += (_grid.cellSize.y + _grid.cellGap.y) * Mathf.Sign( moveY );
                    position = SnapPositionToGrid( position );
                }
                if( _playerPosition != position ) {
                    Vector3 relPos = position - _playerPosition;
                    var raycastHit = Physics2D.Raycast( _playerPosition, relPos, relPos.magnitude, _movementMask );
                    if( raycastHit.collider == null ) {
                        _playerPosition = position;
                    }
                    StartCoroutine( InputCooldown() );
                }
            }
            _player.position = Vector2.Lerp( _player.position, _playerPosition, _movementSpeed * Time.deltaTime );
        }


        private IEnumerator InputCooldown()
        {
            _isInInputCooldown = true;
            yield return new WaitForSeconds( _inputCooldownTime );
            _isInInputCooldown = false;
        }


        private Vector2 SnapPositionToGrid( Vector3 position )
        {
            Vector3 relPosition = position - _grid.transform.position;
            relPosition.x = relPosition.x - Mathf.Repeat( relPosition.x, _grid.cellSize.x + _grid.cellGap.x ) + _grid.cellSize.x / 2;
            relPosition.y = relPosition.y - Mathf.Repeat( relPosition.y, _grid.cellSize.y + _grid.cellGap.y ) + _grid.cellSize.y / 2;
            return _grid.transform.position + relPosition;
        }
    }
}
