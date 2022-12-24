using UnityEngine;

namespace Experiments.Slasher
{

    public class MagogController : EnemyController
    {

        [SerializeField] private float _escapeDistance;// Если цель ближе, чем это расстояие, нужно убегать от цели
        [SerializeField] private Transform _shootPosition;// Откуда происходит выстрел
        [SerializeField] private GameObject _projectile;// Снаряд

        private Vector2 _shootDirection;

        protected override void Awake()
        {
            base.Awake();
            var players = GameObject.FindGameObjectsWithTag( WellKnownParameters.PlayerTag );
            if( !(players is null) && players.Length > 0 ) {
                _target = players[0];
            }

            if( _shootPosition is null ) {
                _shootPosition = transform.FindDeepChild( "ShootPosition" );
            }
        }

        protected override void ProcessDefaultState()
        {
            base.ProcessDefaultState();
            // Обычное преследование игрока
            if( !(_target is null) ) {
                Vector2 delta = _target.transform.position - transform.position;

                _animator.SetBool( "EnemyUpper", delta.y > Mathf.Abs( delta.x ) );
                _animator.SetBool( "EnemyDowner", delta.y < -Mathf.Abs( delta.x ) );

                if( delta.sqrMagnitude <= _escapeDistance * _escapeDistance ) {
                    _movement = -delta.normalized * _moveSpeed;// Убегаем если слишком близко
                } else if( delta.sqrMagnitude <= _attackDistance * _attackDistance ) {
                    Attack();// Стрельба, если враг на расстоянии выстрела
                } else if( delta.sqrMagnitude <= _aggroDistance * _aggroDistance ) {
                    _movement = delta.normalized * _moveSpeed;// Преследуем, если далеко
                } else {
                    _movement = Vector2.zero;// Если слишком далеко, то просто стоим
                }

            }
        }


        protected override void Attack()
        {
            base.Attack();
            Vector2 delta = _target.transform.position - transform.position;
            _animator.SetBool( "TurnLeft", delta.x < 0 );

            _shootDirection = delta.normalized;
            _movement = Vector2.zero;
        }   

        private void Shoot()
        {
            if( _shootPosition == null || _projectile == null ) {
                return;
            }
            var projectile = Instantiate( _projectile, _shootPosition.position, Quaternion.identity );
            var fireball = projectile.GetComponent<FireballController>();
            if( fireball is null ) {
                return;
            }
            fireball.Owner = gameObject;
            fireball.Direction = _shootDirection;
            fireball.Damage = _damage;
        }

    }


}