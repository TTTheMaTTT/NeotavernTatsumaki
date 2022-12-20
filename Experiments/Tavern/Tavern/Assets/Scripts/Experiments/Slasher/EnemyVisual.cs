using UnityEngine;

namespace Experiments.Slasher
{
    public class EnemyVisual : MonoBehaviour
    {
        private float _horizontal;
        public float Horizontal
        {
            get {
                return _horizontal;
            }
            set {
                _horizontal = value;
                if( transform.localScale.x * value < 0 ) {
                    Turn();
                }
            }
        }

        public float MoveSpeed
        {
            get; set;
        }

        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetFloat( "Speed", MoveSpeed );
        }

        private void Turn()
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign( _horizontal ) * Mathf.Abs( scale.x );
            transform.localScale = scale;
        }
    }
}
