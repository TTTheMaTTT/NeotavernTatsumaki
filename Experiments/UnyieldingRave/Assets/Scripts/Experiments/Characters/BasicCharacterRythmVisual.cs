using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicCharacterRythmVisual : CharacterRythmVisualAbstract
{
    protected virtual string idleAnimationStateName => "Idle";
    protected virtual string attackAnimationStateName => "Attack";
    protected virtual string defendAnimationStateName => "Defend";
    protected virtual string prepareAttackAnimationStateName => "PrepareAttack";
    protected virtual string damageAnimationStateName => "Damage";
    protected virtual string deathAnimationStateName => "Dead";


    public void PrepareAttack()
    {
        PlayAnimation( prepareAttackAnimationStateName, _moveAnimationLayerId );
    }

    public void Attack()
    {
        PlayAnimation( attackAnimationStateName, _moveAnimationLayerId );
    }


    public void Defend()
    {
        PlayAnimation( defendAnimationStateName, _moveAnimationLayerId );
    }


    public void Idle()
    {
        PlayAnimation( idleAnimationStateName, _moveAnimationLayerId );
    }


    public void Damage()
    {
        PlayAnimation( damageAnimationStateName, _damageAnimationLayerId );
    }


    public void Death()
    {
        PlayAnimation( deathAnimationStateName, _damageAnimationLayerId );
    }
}
