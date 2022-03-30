using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassLiquidTrigger : MonoBehaviour
{
    ParticleSystem ps;

    // these lists are used to contain the particles which match
    // the trigger conditions each frame.
    List<ParticleSystem.Particle> enter = new List<ParticleSystem.Particle>();

    [SerializeField]
    private Liquid liquid;

    void OnEnable()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnParticleTrigger()
    {
        if( liquid == null ) {
            return;
        }

        // get the particles which matched the trigger conditions this frame
        int numEnter = ps.GetTriggerParticles( ParticleSystemTriggerEventType.Enter, enter, out var colliderData );

        // iterate through the particles which entered the trigger and make them red
        for( int i = 0; i < numEnter; i++ ) {
            ParticleSystem.Particle p = enter[i];
            if( colliderData.GetColliderCount( i ) >= 1 ) {
                var glassLiquidController = colliderData.GetCollider( i, 0 ).gameObject.GetComponent<GlassLiquidController>();
                if( glassLiquidController != null ) {
                    glassLiquidController.AddLiquidParticle( liquid );
                }
            }

            //p.startColor = new Color32( 255, 0, 0, 255 );
            //enter[i] = p;
        }

        // re-assign the modified particles back into the particle system
        //ps.SetTriggerParticles( ParticleSystemTriggerEventType.Enter, enter );
    }
}
