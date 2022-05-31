using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

// ƒоступ ко всем битам трека, а также доступ ко следующему биту по времени
public class BeatTimeline
{
    private List<Beat>? _beats;
    
    //  ол-во битов в треке
    public int Count
    {
        get {
            return _beats?.Count ?? 0 ;
        }
    }


    // ƒлительность трека
    public float Duration
    {
        get {
            return Count > 0? _beats[Count - 1].Time : 0f;
        }
    }

    
    // ƒоступ к биту по времени
    public Beat this[int index]
    {
        get {
            Assert.IsTrue( index >= 0 && index < Count );
            return _beats[index];
        }
    }


    // ѕолучить индекс бита, который первым идЄт после указанного времени, т.е. его врем€ >= указанного. ¬озвращает размер массива, если такого бита нет
    // “ак можно получить предсто€щий бит, т.е. доступ к биту по времени
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
