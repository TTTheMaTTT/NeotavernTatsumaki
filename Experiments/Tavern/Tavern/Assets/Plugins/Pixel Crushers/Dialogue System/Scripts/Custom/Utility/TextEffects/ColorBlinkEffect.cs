using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Blink between two colors
    /// </summary>
    public class ColorBlinkEffect : ITextEffect
    {

        // ITextEffect
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Color_Blink );

            _range = effectParams.Range;

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.FrequencyAttribute ) ) {
                _frequency = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.FrequencyAttribute] );
            }
            Assert.IsTrue( _frequency >= 0 );

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.ColorFromAttribute ) ) {
                _colorFrom = Tools.WebColor( effectParams.Attributes[TextEffectsAttributes.ColorFromAttribute] );
            }

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.ColorToAttribute ) ) {
                _colorTo = Tools.WebColor( effectParams.Attributes[TextEffectsAttributes.ColorToAttribute] );
            }

        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Do nothing
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Blink
            Color newColor = Color.Lerp( _colorFrom, _colorTo, (Mathf.Sin( Time.unscaledTime * _frequency ) + 1) / 2 );

            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( _range.To, textInfo.characterCount ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }
                int vertexIndex = charInfo.vertexIndex;
                for( int j = 0; j < 4; j++ ) {
                    colors[vertexIndex + j] = newColor;
                }
            }
        }

        private TextRange _range;// handled text range

        //Transition color
        private Color _colorFrom = Color.white;
        private Color _colorTo = Color.black;

        private float _frequency = 10f;

    }

}
