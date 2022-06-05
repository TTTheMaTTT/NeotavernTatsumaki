using System;
using System.Collections.Generic;

// ���������, ���������� � ���� ���������� � �����
public struct Beat
{
    public int Id;// Id ����, ����� ����� ���� �� ���������
    public float Time;// �����, ����� ������ ��������� ���
}

public class BeatComparer : IComparer<Beat>
{
    int IComparer<Beat>.Compare( Beat x, Beat y )
    {
        return Math.Sign( x.Time - y.Time );
    }
}