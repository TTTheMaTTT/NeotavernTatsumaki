using System.Collections;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Dialogue {

    public enum TEditingType { 
        Color,
        Color_Transition,
        Color_Blink,
        Color_Rainbow,
        Animation_Wave,
        Animation_Wobble,
        Animation_Shake,
        Animation_SizeTransition
    }

    /// <summary>
    /// Диапазон текста от From включительно до To невключительно.
    /// To должен быть >= From, причём, если равен, то считаем, что интервал пуст.
    /// </summary>
    public class CTextRange
    {
        public CTextRange( int from, int to ) {
            Assert.IsTrue( from <= to );
            From = from;
            To = to;
        }

        public int From { get; }
        public int To { get; }
    }

    /// <summary>
    /// Коллекция названий атрибутов редактирований
    /// </summary>
    public static class DialogueEditingAttributes
    {
        public const string ColorAttribute = "color";
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

    // Минимальная единица текста, которую отдельно обрабатывает редактирование
    public enum TEditingCoverage
    {
        Character,
        Word,
        Text
    }

    /// <summary>
    /// Параметры диалогового редактирования
    /// </summary>
    public class CDialogueEditingParams
    {

        // Тип редактирования
        public TEditingType EditingType { get; set; }
        public CTextRange Range { get; set; }// Диапазон текста, на которое распространяется редактирование
        public Dictionary<string, string> Attributes { // Набор атрибутов, задающие специфичную настройку для конкретных редактирований.
            get { return _attributes; }
            set { _attributes = new Dictionary<string, string>( value ); } 
        }

        private Dictionary<string, string> _attributes;
    }
}