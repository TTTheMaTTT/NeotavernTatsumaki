using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Experiments.TopDownMovement
{
    public class CharacterController : MonoBehaviour
    {
        [SerializeField]
        private float _speed;

        private Rigidbody2D _rb;
        private Vector2 _movement;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            _movement.x = Input.GetAxis( "Horizontal" );
            _movement.y = Input.GetAxis( "Vertical" );
            if( _movement.magnitude > 1 ) {
                _movement.Normalize();
            }
        }

        private void FixedUpdate()
        {
            _rb?.MovePosition( _rb.position + _movement * _speed * Time.fixedDeltaTime );
        }
    }
}

