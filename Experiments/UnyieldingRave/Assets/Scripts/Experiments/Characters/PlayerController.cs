using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//  онтроллер персонажа, управл€емого игроком
public class PlayerController : CharacterRythmController
{

    public void Idle()
    {
        _state.Action = CharacterAction.None;
    }


    public void Attack()
    {
        _state.Action = CharacterAction.Attack;
    }


    public void Defend()
    {
        _state.Action = CharacterAction.Defend;
    }


    public override void OnRythmAction()
    {
        Action();
    }


    private void Action()
    {
        Animate();
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

}
