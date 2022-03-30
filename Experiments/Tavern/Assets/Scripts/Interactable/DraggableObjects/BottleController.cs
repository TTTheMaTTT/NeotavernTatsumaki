using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Управление бутылкой
public class BottleController : DraggableMonobehaviour
{
    //public Transform pourPosition;

    private Rigidbody2D _rigidbody;
    // Можно ли выливать жидкость
    bool _canPourLiquid = false;

    public float maxEmission = 3000f;
    private ParticleSystem _liquidEffect;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _liquidEffect = GetComponentInChildren<ParticleSystem>();
    }

    public override void StartRotate()
    {
        _canPourLiquid = true;
    }

    public override void StopRotate()
    {
        _canPourLiquid = false;
    }

    public override bool HaveMomentumOnDetach()
    {
        return _rigidbody != null;
    }

    public override bool ImitateInertia()
    {
        return true;
    }

    private void Update()
    {
        if( _liquidEffect == null ) {
            return;
        }
        if( _canPourLiquid ) {
            // В зависимости от угла начинаем выливать жидкость
            // Чем ближе угол к 180 градусов, тем сильнее поток. Если угол меньше 90 и больше 270 потока нет
            float angle = Mathf.Repeat( transform.eulerAngles.z, 360f );
            float flowStrength = Mathf.Clamp01( -2 * Mathf.Abs( angle / 180 - 1 ) + 1 );
            _liquidEffect.emissionRate = maxEmission * flowStrength;
        } else {
            _liquidEffect.emissionRate = 0f;
        }


    }


}
