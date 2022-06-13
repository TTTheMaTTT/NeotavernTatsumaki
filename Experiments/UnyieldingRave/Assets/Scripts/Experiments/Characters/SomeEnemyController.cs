using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SomeEnemyController : EnemyController
{
    private bool _isPreparingAttack;
    private bool _isRecovering;

    [SerializeField] private int attackRecoveryTime = 2;
    private int _recoveryCounter;
    [SerializeField] [Range( 0f, 1f )] private float attackProbability = 0.5f;


    public override void OnRythmAction()
    {
        base.OnRythmAction();
        Action();
    }


    private void Action()
    {
        if( _isPreparingAttack ) {
            _state.Action = CharacterAction.Attack;
            _isPreparingAttack = false;
            _recoveryCounter = 0;
            _isRecovering = true;
        } else if( _isRecovering ) {
            _recoveryCounter++;
            _isRecovering = _recoveryCounter < attackRecoveryTime;
            _state.Action = CharacterAction.None;
        } else {
            if( Random.Range( 0f, 1f ) <= attackProbability ) {
                _isPreparingAttack = true;
                _state.Action = CharacterAction.None;
            } else {
                _state.Action = CharacterAction.Defend;
            }
        }

        Animate();
    }


    private void Animate()
    {
        switch( _state.Action ) {
            case CharacterAction.None:
                if( _isPreparingAttack ) {
                    _visual.PrepareAttack();
                } else {
                    _visual?.Idle();
                }
                break;
            case CharacterAction.Attack:
                _visual?.Attack();
                break;
            case CharacterAction.Defend:
                _visual?.Defend();
                break;
        }
    }
}
