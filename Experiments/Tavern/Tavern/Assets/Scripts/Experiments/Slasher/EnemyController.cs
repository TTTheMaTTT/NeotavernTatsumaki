using UnityEngine;

namespace Experiments.Slasher
{

    public abstract class EnemyController : DestructibleAbstract, IPoiseDamageable
    {
        [SerializeField] protected float _aggroDistance = 5f;
        [SerializeField] protected float _attackDistance = 1f;
        [SerializeField] protected int _damage = 1;

        [SerializeField] protected float _moveSpeed = 2f;
        private float _acceleration = 20f;
        private Vector2 _objectiveVelocity;

        protected GameObject _target;

        [SerializeField] protected int _maxPoise = 1;
        protected int _poise;
        //protected float _poiseFloat;
        //protected float _poiseRecoverySpeed;

        protected CharacterState _state = CharacterState.None;
        protected Vector2 _movement;

        protected Rigidbody2D _rb;
        protected Animator _animator;
        protected HitBox _hitBox;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponentInChildren<Animator>();
            _hitBox = GetComponentInChildren<HitBox>( true );
            _hitBox.Damage = _damage;
            _hitBox.Owner = gameObject;
            _poise = _maxPoise;
        }


        private void Update()
        {
            switch( _state ) {
                case CharacterState.None: 
                {
                    ProcessDefaultState();
                    break;
                }
                default:
                    break;
            }
        }


        protected virtual void FixedUpdate()
        {
            _rb.velocity = Vector2.Lerp( _rb.velocity, _movement, _acceleration * Time.fixedDeltaTime );
        }


        protected virtual void ProcessDefaultState()
        {
            _animator.SetFloat( "Horizontal", _movement.x );
            _animator.SetFloat( "Speed", _movement.sqrMagnitude );
            if( _movement.x != 0 ) {
                _animator.SetBool( "TurnLeft", _movement.x < 0 );
            }
        }


        protected virtual void Attack()
        {
            _state = CharacterState.Attacking;
            _animator.SetTrigger( "StartAttack" );
        }


        protected virtual void StopAttack()
        {
            _state = CharacterState.None;
        }


        public override void TakeDamage( int damage )
        {
            base.TakeDamage( damage );
            if( !_isDead ) {
                _animator.SetTrigger( "TakeDamage" );
            }
        }

        
        public override void Death()
        {
            base.Death();
            _state = CharacterState.Dead;
            _animator.SetTrigger( "Death" );
            _movement = Vector2.zero;
        }


        public virtual void TakePoiseDamage( int poiseDamage )
        {
            _poise = Mathf.Clamp( _poise - poiseDamage, 0, _maxPoise );
            if( _poise == 0 ) {
                PoiseBreak();
            }
        }

        protected virtual void PoiseBreak()
        {
            if( _isDead ) {
                return;
            }
            _state = CharacterState.PoiseBreak;
            _animator.SetTrigger( "PoiseBreak" );
            _movement = Vector2.zero;
        }

        protected virtual void RestoreBalance()
        {
            _state = CharacterState.None;
            _poise = _maxPoise;
        }

    }


}