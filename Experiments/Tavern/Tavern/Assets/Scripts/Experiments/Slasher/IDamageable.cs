using UnityEngine;

namespace Experiments.Slasher
{
    public interface IDamageable
    {
        void TakeDamage( int damage );
        
        void AddToHealth( int amount )
        {
            TakeDamage( -amount );
        } 
    }
}
