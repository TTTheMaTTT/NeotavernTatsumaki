using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRythmController : CharacterController, IBeatResponsive
{
    protected BasicCharacterRythmVisual _visual;

    protected virtual void Awake()
    {
        _visual = GetComponent<BasicCharacterRythmVisual>();
    }

    public virtual void ConfigureBeats( float bpm, float beatOffset )
    {
        _visual?.ConfigureBeats( bpm, beatOffset );
    }


    public void TakeDamage()
    {
        _state.Health--;
        if( _state.Health > 0 ) {
            _visual?.Damage();
        } else {
            Death();
        }
    }


    public void Death()
    {
        _visual?.Death();
    }


    public virtual void OnBeat()
    {}
}
