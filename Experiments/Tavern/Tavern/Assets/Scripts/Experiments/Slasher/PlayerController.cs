using UnityEngine;

namespace Experiments.Slasher
{
    public class PlayerController : DestructibleAbstract
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private int _damage = 1;
        [SerializeField] private int _poiseDamage = 1;
        [SerializeField] private float _hitForce = 100f;

        private CharacterState _state = CharacterState.None;
        private Vector2 _movement;

        private Rigidbody2D _rb;
        private Animator _animator;
        private HitBox _hitBox;
        private Direction _direction;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _hitBox = GetComponentInChildren<HitBox>( true );
            _hitBox.Damage = _damage;
            _hitBox.PoiseDamage = _poiseDamage;
            _hitBox.Owner = gameObject;
        }

        void Update()
        {
            switch( _state ) {
                case CharacterState.None: {
                    if( Input.GetButtonDown( "Fire1" ) ) {
                        Attack();
                        break;
                    }

                    _movement.x = Input.GetAxis( "Horizontal" );
                    _movement.y = Input.GetAxis( "Vertical" );
                    _animator.SetFloat( "Horizontal", _movement.x );
                    _animator.SetFloat( "Vertical", _movement.y );
                    _animator.SetFloat( "Speed", _movement.sqrMagnitude );
                    ProcessDirection();
                    break;
                }
                default:
                    break;
            }
        }

        private void ProcessDirection()
        {
            Direction prevDirection = _direction;
            if( Mathf.Abs( _movement.x ) > Mathf.Abs( _movement.y ) ) {
                _direction = _movement.x < 0 ? Direction.Left : (_movement.x > 0 ? Direction.Right : _direction);
            } else {
                _direction = _movement.y < 0 ? Direction.Down : (_movement.y > 0 ? Direction.Up : _direction);
            }

            if( prevDirection != _direction ) {
                _animator.SetBool( "DirectionLeft", _direction == Direction.Left );
                _animator.SetBool( "DirectionRight", _direction == Direction.Right );
                _animator.SetBool( "DirectionUp", _direction == Direction.Up );
                _animator.SetBool( "DirectionDown", _direction == Direction.Down );
                int x = _direction == Direction.Right ? 1 : _direction == Direction.Left ? -1 : 0;
                int y = _direction == Direction.Up ? 1 : _direction == Direction.Down ? -1 : 0;
                _hitBox.PushForce = new Vector2( x, y ) * _hitForce;
            }
        }

        private void Attack()
        {
            _movement = Vector2.zero;
            _state = CharacterState.Attacking;
            _animator.SetTrigger( "StartAttack" );
        }

        private void StopAttack()
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

        private void FixedUpdate()
        {
            _rb.MovePosition( _rb.position + _movement.normalized * _moveSpeed * Time.fixedDeltaTime );
        }

    }
}
