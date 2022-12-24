using UnityEngine;

namespace Experiments.Slasher
{
    public class PlayerVisual : MonoBehaviour
    {
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
        public float MoveSpeed { get; set; }
    
        private Animator animator;

        void Awake()
        {
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            animator.SetFloat( "Horizontal", Horizontal );
            animator.SetFloat( "Vertical", Vertical );
            animator.SetFloat( "Speed", MoveSpeed );

            Vector3 scale = transform.localScale;
            if( Mathf.Abs( Horizontal ) > Mathf.Abs( Vertical ) ) {
                scale.x = Mathf.Sign( Horizontal ) * Mathf.Abs( scale.x );
            } else {
                scale.x = Mathf.Abs( scale.x );
            }
            transform.localScale = scale;
        }
    }
}