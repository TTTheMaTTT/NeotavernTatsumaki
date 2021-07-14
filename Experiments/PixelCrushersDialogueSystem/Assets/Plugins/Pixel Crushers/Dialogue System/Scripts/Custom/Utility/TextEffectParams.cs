using System.Collections.Generic;
using UnityEngine.Assertions;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Which effects are supported
    /// </summary>
    public enum TextEffectType { 
        Color_Transition,
        Color_Blink,
        Color_Rainbow,
        Animation_Wave,
        Animation_Wobble,
        Animation_Shake,
        Animation_SizeTransition
    }

    public static class TextEffectTables
    {
        public static readonly Dictionary<string, TextEffectType> StringToEffectType = new Dictionary<string, TextEffectType> {
            { "color_transition", TextEffectType.Color_Transition },
            { "color_blink", TextEffectType.Color_Blink },
            { "color_rainbow", TextEffectType.Color_Rainbow },
            { "rainbow", TextEffectType.Color_Rainbow },
            { "animation_wave", TextEffectType.Animation_Wave },
            { "wave", TextEffectType.Animation_Wave },
            { "animation_wobble", TextEffectType.Animation_Wobble },
            { "wobble", TextEffectType.Animation_Wobble },
            { "animation_shake", TextEffectType.Animation_Shake },
            { "shake", TextEffectType.Animation_Shake },
            { "size_transition", TextEffectType.Animation_SizeTransition },
            { "animation_size_transition", TextEffectType.Animation_SizeTransition }
        };
    }


    /// <summary>
    /// Text range (for clean text). From is included, To is excluded.
    /// To must be >= From, and if To == From then interval is empty.
    /// </summary>
    public class TextRange
    {
        public TextRange( int from, int to ) {
            Assert.IsTrue( from <= to );
            From = from;
            To = to;
        }

        public int From { get; }
        public int To { get; }
    }

    /// <summary>
    /// Effect attributes names
    /// </summary>
    public static class TextEffectsAttributes
    {
        public const string ColorFromAttribute = "colorfrom";
        public const string ColorToAttribute = "colorto";
        public const string FrequencyAttribute = "frequency";
        public const string Frequency1Attribute = "frequency1";
        public const string Frequency2Attribute = "frequency2";
        public const string LengthAttribute = "length";
        public const string AmplitudeAttribute = "amplitude";
        public const string SpeedAttribute = "speed";
        public const string SizeFromAttribute = "sizefrom";
        public const string SizeToAttribute = "sizeto";
        public const string CoverageAttribute = "coverage";
    }

    // The minimum text unit which is affected by effect.
    public enum TextEffectCoverage
    {
        Character,
        Word,
        Text
    }

    /// <summary>
    /// Describes, how to use effect.
    /// </summary>
    public class TextEffectParams
    {
        public TextEffectType EffectType { get; set; }// What kind of effect to use?
        public TextRange Range { get; set; }// Affected text interval (on clean text without tags)
        public Dictionary<string, string> Attributes { // effect parameters
            get { return _attributes; }
            set { _attributes = new Dictionary<string, string>( value ); } 
        }

        private Dictionary<string, string> _attributes;
    }
}