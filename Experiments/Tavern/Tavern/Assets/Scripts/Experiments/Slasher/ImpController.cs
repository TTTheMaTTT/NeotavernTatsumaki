using UnityEngine;

namespace Experiments.Slasher
{

    public class ImpController : EnemyController
    {
        [SerializeField] private float _attackDistance = 1f;

        private GameObject _target;



        protected override void Awake()
        {
            base.Awake();
            var players = GameObject.FindGameObjectsWithTag( WellKnownParameters.PlayerTag );
            if( !(players is null) && players.Length > 0 ) {
                _target = players[0];
            }
        }

        protected override void ProcessDefaultState()
        {
            base.ProcessDefaultState();
            // Обычное преследование игрока
            if( !(_target is null) ) {
                Vector2 delta = _target.transform.position - transform.position;
                if( delta.sqrMagnitude <= _aggroDistance * _aggroDistance ) {
                    _movement = delta.normalized * _moveSpeed;
                } else {
                    _movement = Vector2.zero;
                }

                if( delta.sqrMagnitude <= _attackDistance * _attackDistance ) {
                    Attack();
                }
            }
        }


        protected override void Attack()
        {
            base.Attack();
            _movement = Vector2.zero;
        }


        private void FixedUpdate()
        {
            _rb?.MovePosition( _rb.position + _movement.normalized * _moveSpeed * Time.fixedDeltaTime );
        }

    }


}