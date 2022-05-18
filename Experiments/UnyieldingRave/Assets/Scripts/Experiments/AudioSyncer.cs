using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncer : MonoBehaviour
{
    [SerializeField] protected float bias;
    [SerializeField] protected float timeStep = 0.15f;
    [SerializeField] protected float timeToBeat = 0.05f;
    [SerializeField] protected float restSmoothTime = 2f;

    private float _previousAudioValue;
    private float _audioValue;
    private float _timer;

    protected bool _isBeat;


    void Start()
    {
        
    }


    private void Update()
    {
        OnUpdate();    
    }


    protected virtual void OnUpdate()
    {
        _previousAudioValue = _audioValue;
        _audioValue = AudioSpectrum.spectrumValue;

        if( _previousAudioValue > bias && _audioValue <= bias ||
            _previousAudioValue <= bias && _audioValue > bias ) 
        {
            if( _timer > timeStep ) {
                OnBeat();
            }
        }

        _timer += Time.deltaTime;
    }


    protected virtual void OnBeat()
    {
        _timer = 0;
        _isBeat = true;
    }
}
