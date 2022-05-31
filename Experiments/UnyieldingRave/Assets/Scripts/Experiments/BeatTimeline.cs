using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

// ������ �� ���� ����� �����, � ����� ������ �� ���������� ���� �� �������
public class BeatTimeline
{
    private List<Beat>? _beats;
    
    // ���-�� ����� � �����
    public int Count
    {
        get {
            return _beats?.Count ?? 0 ;
        }
    }


    // ������������ �����
    public float Duration
    {
        get {
            return Count > 0? _beats[Count - 1].Time : 0f;
        }
    }

    
    // ������ � ���� �� �������
    public Beat this[int index]
    {
        get {
            Assert.IsTrue( index >= 0 && index < Count );
            return _beats[index];
        }
    }


    // �������� ������ ����, ������� ������ ��� ����� ���������� �������, �.�. ��� ����� >= ����������. ���������� ������ �������, ���� ������ ���� ���
    // ��� ����� �������� ����������� ���, �.�. ������ � ���� �� �������
    public int GetNextBeatIndexAfterTime( float time )
    {
        if( _beats is null ) {
            return 0;
        }

        int beatIndex = _beats.BinarySearch( new Beat() { Time = time, Id = -1 }, new BeatComparer() );

        return beatIndex >= 0 ? beatIndex + 1 : -beatIndex - 1;
    }

    public BeatTimeline( List<Beat> beats )
    {
        _beats = beats.OrderBy( x => x.Time ).ToList();
    }
}
