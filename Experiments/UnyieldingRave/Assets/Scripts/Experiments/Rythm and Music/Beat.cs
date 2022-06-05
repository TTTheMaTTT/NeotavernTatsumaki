using System;
using System.Collections.Generic;

// —труктура, содержаща€ в себе информацию о битах
public struct Beat
{
    public int Id;// Id бита, чтобы можно было их различать
    public float Time;// ¬рем€, когда должен произойти бит
}

public class BeatComparer : IComparer<Beat>
{
    int IComparer<Beat>.Compare( Beat x, Beat y )
    {
        return Math.Sign( x.Time - y.Time );
    }
}