using System;
using System.Collections;
using UnityEngine;

public static class StringExtensions
{
    public static bool ParseToColor( this string colorString, out Color color ) {
        if( colorString.Length != 6 && colorString.Length != 8 ) {
            color = new Color();
            return false;
        }

        long colorInt;
        try {
            colorInt = Int64.Parse( colorString, System.Globalization.NumberStyles.HexNumber );
        } catch( Exception /*e*/ ) {
            color = new Color();
            return false;
        }

        if( colorString.Length == 6 ) {
            color = new Color(
                (float)(colorInt >> 16) / 0x100,
                (float)(colorInt >> 8 & 0x00ff) / 0x100,
                (float)(colorInt & 0x0000ff) / 0x100
                );
            return true;
        } else {
            color = new Color(
                (float)(colorInt >> 24) / 0x100,
                (float)(colorInt >> 16 & 0x00ff) / 0x100,
                (float)(colorInt >> 8 & 0x0000ff) / 0x100,
                (float)(colorInt & 0x000000ff) / 0x100
                );
            return true;
        }

    }

    public static bool ParseToFloat( this string floatString, out float value ) {


        try {
            value = float.Parse( floatString, System.Globalization.CultureInfo.InvariantCulture );
        } catch( Exception /*e*/ ) {
            value = 0f;
            return false;
        }

        return true;

    }

    public static bool ParseToEnum<T>( this string enumString, ref T value, bool ignoreCase = true ) {
        try {
            value = (T)Enum.Parse( typeof(T), enumString, ignoreCase );
        } catch( Exception /*e*/ ) {
            return false;
        }

        return true;

    }
}

