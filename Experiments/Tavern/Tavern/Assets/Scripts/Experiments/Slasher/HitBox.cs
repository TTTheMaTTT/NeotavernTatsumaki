using UnityEngine;

namespace Experiments.Slasher
{
    public class HitBox : MonoBehaviour
    {
        public int Damage
        {
            get; set;
        } = 0;

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
            if( damageable != null ) {
                damageable.TakeDamage( Damage );
            }
        }
    }
}

