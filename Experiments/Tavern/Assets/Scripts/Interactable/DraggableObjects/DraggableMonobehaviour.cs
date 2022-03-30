using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DraggableMonobehaviour : DraggableAbstract
{
    sealed public override void StopDrag() 
    {
        StopRotate();
        StopDragInternal();
    }

    protected virtual void StopDragInternal()
    {
    }

}
