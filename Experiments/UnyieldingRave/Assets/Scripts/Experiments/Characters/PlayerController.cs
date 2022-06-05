using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  онтроллер персонажа, управл€емого игроком
public class PlayerController : CharacterRythmController
{
    [Range( 0f, 1f )]
    private float _beatResetProportion = 0.5f;
    private float _beatResetTime;
    private Coroutine _beatResetRoutine;

    public override void ConfigureBeats( float bpm, float beatOffset )
    {
        base.ConfigureBeats( bpm, beatOffset );

        float beatTime = 60f / bpm;
        _beatResetTime = _beatResetProportion * beatTime;
    }


    public void Attack()
    {
        _state.Action = CharacterAction.Attack;
    }


    public void Defend()
    {
        _state.Action = CharacterAction.Defend;
    }


    public override void OnBeat()
    {
        Animate();
        if( _beatResetRoutine != null ) {
            StopCoroutine( _beatResetRoutine );
        }
        _beatResetRoutine = StartCoroutine( BeatResetRoutine( _beatResetTime ) );
    }


    private void Animate()
    {
        switch( _state.Action ) {
            case CharacterAction.None:
                _visual?.Idle();
                break;
            case CharacterAction.Attack:
                _visual?.Attack();
                break;
            case CharacterAction.Defend:
                _visual?.Defend();
                break;
        }
    }


    private IEnumerator BeatResetRoutine( float resetTime )
    {
        yield return new WaitForSeconds( resetTime );
        _state.Action = CharacterAction.None;
    }
}
