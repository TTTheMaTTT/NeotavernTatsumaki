using UnityEngine;

namespace Experiments.Slasher
{

    public class ImpController : EnemyController
    {
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
                _animator.SetBool( "EnemyUpper", delta.y > Mathf.Abs( delta.x ) );
                _animator.SetBool( "EnemyDowner", delta.y < -Mathf.Abs( delta.x ) );
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


    }


}