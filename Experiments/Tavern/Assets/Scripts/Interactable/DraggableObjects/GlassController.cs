using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Управление стаканом
public class GlassController : DraggableMonobehaviour
{
    private Rigidbody2D _rigidbody;
    private AngleLimitations _angleLimitations;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _angleLimitations = new AngleLimitations( false, 0f );
    }

    public override AngleLimitations AngularLimitations
    {
        get {
            return _angleLimitations;
        }
        set {
            _angleLimitations = value;
        }
    }

    public override void StartRotate()
    {

    }

    public override void StopRotate()
    {

    }

    protected override void StopDragInternal()
    {
        base.StopDragInternal();
        Vector3 rotation = transform.eulerAngles;
        rotation.z = 0f;
        transform.eulerAngles = rotation;
        _rigidbody.angularVelocity = 0f;
    }


    public override bool HaveMomentumOnDetach()
    {
        return _rigidbody != null;
    }

    public override bool ImitateInertia()
    {
        return false;
    }


}
