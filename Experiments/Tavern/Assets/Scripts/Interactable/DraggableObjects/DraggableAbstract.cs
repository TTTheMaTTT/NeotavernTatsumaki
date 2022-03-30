using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DraggableAbstract : InteractableAbstract, IDraggable
{

    public struct AngleLimitations
    {
        public bool HasLimitations;
        public float Limitations;

        public AngleLimitations( bool hasLimitations, float limitations )
        {
            HasLimitations = hasLimitations;
            Limitations = limitations;
        }
    }

    public virtual AngleLimitations AngularLimitations {
        get 
        {
            return new AngleLimitations( false, 0f);
        }
        set 
        {
            //do nothing 
        }
    }

    public virtual void StartDrag()
    {
    }

    public virtual void StopDrag()
    {
    }

    public virtual void StartRotate()
    {
    }

    public virtual void StopRotate()
    {
    }

    public virtual bool HaveMomentumOnDetach()
    {
        return false;
    }

    public virtual bool ImitateInertia()
    {
        return false;
    }

}
