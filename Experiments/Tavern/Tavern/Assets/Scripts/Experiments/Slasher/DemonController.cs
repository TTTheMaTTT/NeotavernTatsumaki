using UnityEngine;

namespace Experiments.Slasher
{

    public class DemonController : EnemyController
    {
        [SerializeField] private float _chargeCooldown = 10f;
        private float _chargeCooldownTimer;

        [SerializeField] private int _defaultMaxPoise = 1;
        [SerializeField] private int _chargingMaxPoise = 2;
        [SerializeField] private int _chargeAttackMaxPoise = 3;

        [SerializeField] private float _chargeAttackSpeed = 10f;

        private bool _isCharging = false;

        private Vector2 _prevTargetPosition;
        private Vector2 _targetVelocity;
        private Vector2 _predictedVelocity;

        protected override void Awake()
        {
            base.Awake();
            var players = GameObject.FindGameObjectsWithTag( WellKnownParameters.PlayerTag );
            if( !(players is null) && players.Length > 0 ) {
                _target = players[0];
            }
            _isCharging = false;
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if( !_isCharging && _chargeCooldownTimer > 0 ) {
                _chargeCooldownTimer -= Time.fixedDeltaTime;
            } else if( _target != null ) {
                _targetVelocity = ((Vector2)_target.transform.position - _prevTargetPosition) / Time.fixedDeltaTime;
                _prevTargetPosition = _target.transform.position;
            }
        }

        protected override void ProcessDefaultState()
        {
            base.ProcessDefaultState();
            // Обычное преследование игрока
            if( !(_target is null) ) {
                Vector2 delta = _target.transform.position - transform.position;
                bool isAgressive = delta.sqrMagnitude <= _aggroDistance * _aggroDistance;
                if( isAgressive ) {
                    _movement = delta.normalized * _moveSpeed;
                } else {
                    _movement = Vector2.zero;
                }
                _animator.SetBool( "EnemyUpper", delta.y > Mathf.Abs( delta.x ) );
                _animator.SetBool( "EnemyDowner", delta.y < -Mathf.Abs( delta.x ) );

                if( isAgressive && _chargeCooldownTimer <= 0f ) {
                    StartCharging();
                } else if( delta.sqrMagnitude <= _attackDistance * _attackDistance ) {
                    Attack();
                }

            }
        }


        private void StartCharging()
        {
            _state = CharacterState.Attacking;
            _movement = Vector2.zero;
            _animator.SetTrigger("StartCharge");
            _maxPoise = _chargingMaxPoise;
            _poise = _maxPoise;
            _isCharging = true;
            _chargeCooldownTimer = _chargeCooldown;

        }

        private void OnChargeAttackStart()
        {
            _maxPoise = _chargeAttackMaxPoise;
            _poise = _maxPoise;

            // Удар с некоторым упреждением
            if( _target != null && _chargeAttackSpeed != 0 ) {
                var targetPos = (Vector2)_target.transform.position;
                float t = (targetPos - (Vector2)transform.position).magnitude / _chargeAttackSpeed;
                Vector2 finalPos = targetPos + _predictedVelocity * t;
                Vector2 delta = (finalPos - (Vector2)transform.position).normalized;
                _animator.SetBool( "EnemyUpper", delta.y > Mathf.Abs( delta.x ) );
                _animator.SetBool( "EnemyDowner", delta.y < -Mathf.Abs( delta.x ) );
                _movement = delta * _chargeAttackSpeed;
                _animator.SetBool( "TurnLeft", _movement.x < 0 );
            }

        }

        private void OnChargeAttackEnd()
        {
            ResetState();
            _state = CharacterState.None;
        }

        private void PredictTargetVelocity()
        {
            _predictedVelocity = _targetVelocity;
        }

        private void ResetState()
        {
            _isCharging = false;
            _maxPoise = _defaultMaxPoise;
            _poise = _maxPoise;
        }

        protected override void Attack()
        {
            base.Attack();
            _movement = Vector2.zero;
        }

        protected override void PoiseBreak()
        {
            base.PoiseBreak();
            ResetState();
        }

        protected override void RestoreBalance()
        {
            base.RestoreBalance();
            ResetState();
        }

        public override void Death()
        {
            base.Death();
            ResetState();
        }

    }


}