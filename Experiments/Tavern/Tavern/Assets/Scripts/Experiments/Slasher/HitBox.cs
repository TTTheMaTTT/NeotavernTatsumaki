using UnityEngine;

namespace Experiments.Slasher
{
    public class HitBox : MonoBehaviour
    {
        public int Damage
        {
            get; set;
        } = 0;

        public int PoiseDamage
        {
            get; set;
        } = 0;

        public Vector2 PushForce
        {
            get; set;
        } = Vector2.zero;

        public GameObject Owner
        {
            get; set;
        }

        private void OnTriggerEnter2D( Collider2D collision )
        {
            if( collision.gameObject == Owner ) {
                return;// не причинияем себе вред своей же атакой
            }
            var damageable = collision.gameObject.GetComponent<IDamageable>();
            damageable?.TakeDamage( Damage );

            var poiseDamageable = collision.gameObject.GetComponent<IPoiseDamageable>();
            poiseDamageable?.TakePoiseDamage( PoiseDamage );

            var rigid = collision.gameObject.GetComponent<Rigidbody2D>();
            if( rigid != null ) {
                rigid.AddForce( PushForce );
            }
        }
    }
}

