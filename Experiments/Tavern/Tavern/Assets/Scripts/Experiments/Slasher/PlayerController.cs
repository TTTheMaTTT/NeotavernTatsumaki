using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Experiments.Slasher
{
    public class PlayerController : DestructibleAbstract
    {
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private int _damage = 1;

        private CharacterState _state = CharacterState.None;
        private Vector2 _movement;

        private Rigidbody2D _rb;
        private Animator _animator;
        private HitBox _hitBox;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
            _hitBox = GetComponentInChildren<HitBox>( true );
            _hitBox.Damage = _damage;
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

                    if( Mathf.Abs( _movement.x ) > Mathf.Abs( _movement.y ) ) {
                        _animator.SetBool( "DirectionLeft", _movement.x < 0 );
                        _animator.SetBool( "DirectionRight", _movement.x > 0 );
                        _animator.SetBool( "DirectionUp", false );
                        _animator.SetBool( "DirectionDown", false );

                    } else if (_movement.y != 0) {
                        _animator.SetBool( "DirectionLeft", false );
                        _animator.SetBool( "DirectionRight", false );
                        _animator.SetBool( "DirectionUp", _movement.y > 0 );
                        _animator.SetBool( "DirectionDown", _movement.y < 0 );
                    }
                    break;
                }
                default:
                    break;
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
