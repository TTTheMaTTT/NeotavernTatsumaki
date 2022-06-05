using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Интерфейс для всех классов, реагирующих на бит
public interface IBeatResponsive
{
    void ConfigureBeats( float bpm, float beatOffset );
    void OnBeat();
}
