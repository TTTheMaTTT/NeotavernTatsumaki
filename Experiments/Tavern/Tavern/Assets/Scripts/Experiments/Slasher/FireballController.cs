using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Experiments.Slasher
{
    public class FireballController : MonoBehaviour
    {
        [SerializeField] private float _speed;
        [SerializeField] private float _lifeTime;

        private Vector2 _direction = Vector2.zero;
        private float _lifeTimer;
        private HitBox _hitBox;
        private Animator _animator;

        bool _isExploding = false;

        public float Speed
        {
            get => _speed; set => _speed = value;
        }

        public Vector2 Direction
        {
            get => _direction;
            set {
                _direction = value.normalized;
                transform.rotation = Quaternion.FromToRotation( Vector2.right, _direction );
            }
        }


        public int Damage
        {
            get {
                return _hitBox?.Damage ?? 0;
            }
            set {
                if( _hitBox != null ) {
                    _hitBox.Damage = value;
                }
            }
        }

        public GameObject Owner
        {
            get; set;
        }

        private void Awake()
        {
            Direction = _direction.normalized;

            _hitBox = GetComponentInChildren<HitBox>( true );
            _animator = GetComponent<Animator>();
            _isExploding = false;
            _lifeTimer = 0f;
        }


        private void FixedUpdate()
        {
            if( !_isExploding ) {
                transform.position = (Vector2)transform.position + _speed * _direction * Time.fixedDeltaTime;
                _lifeTimer += Time.fixedDeltaTime;
                if( _lifeTimer > _lifeTime ) {
                    Explode();
                }
            }
        }


        private void OnTriggerEnter2D( Collider2D collision )
        {
            if( collision.gameObject == Owner ) {
                return;
            }
            Explode();
        }


        private void Explode()
        {
            _isExploding = true;
            _animator.SetTrigger( "StartExplosion" );
        }


        private void OnExplosionEnd()
        {
            Destroy(gameObject);
        }

    }
}