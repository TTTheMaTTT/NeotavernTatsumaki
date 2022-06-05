using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpectrum : MonoBehaviour
{

    public static float spectrumValue
    {
        get; private set;
    }

    private float[] _audioSpectrum;

    private void Start()
    {
        _audioSpectrum = new float[128];
    }

    private void Update()
    {
        AudioListener.GetSpectrumData( _audioSpectrum, 0, FFTWindow.Hamming );
        if( _audioSpectrum != null && _audioSpectrum.Length > 0 ) {
            spectrumValue = _audioSpectrum[0] * 100;
        }
    }
}
