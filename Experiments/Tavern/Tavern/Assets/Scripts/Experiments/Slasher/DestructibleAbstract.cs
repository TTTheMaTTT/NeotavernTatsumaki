using UnityEngine;

namespace Experiments.Slasher
{
    public class DestructibleAbstract : MonoBehaviour, IDestructible
    {
        [SerializeField] protected int _maxHealth;
        [SerializeField] protected int _health;

        protected bool _isDead = false;

        protected virtual void Awake()
        {
            _health = _maxHealth;
        }

        public virtual void TakeDamage( int damage )
        {
            if( _isDead ) {
                return;
            }
            _health = Mathf.Clamp( _health - damage, 0, _maxHealth );
            if( _health == 0 ) {
                Death();
            }
        }

        public virtual void Death()
        {
            _isDead = true;
        }

    }
}