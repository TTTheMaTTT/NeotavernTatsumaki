using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ��������� ��� ���� �������, ����������� �� ���
public interface IBeatResponsive
{
    void ConfigureBeats( float bpm, float beatOffset );
    void OnBeat();
}
