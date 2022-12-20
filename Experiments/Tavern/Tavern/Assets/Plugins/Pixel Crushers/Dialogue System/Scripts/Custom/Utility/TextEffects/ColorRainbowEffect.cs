using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Make texts change color like a rainbow
    /// </summary>
    public class ColorRainbowEffect : ITextEffect
    {

        // ITextEffect
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Color_Rainbow );

            _range = effectParams.Range;

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.FrequencyAttribute ) ) {
                _frequency= Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.FrequencyAttribute] );

            }
            Assert.IsTrue( _frequency >= 0 );

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.LengthAttribute ) ) {
                _length = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.LengthAttribute] );
            }
            Assert.IsTrue( _length >= 0 );
        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Do Nothing
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( _range.To, textInfo.characterCount ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }

                for( int j = 0; j < 4; j++ ) {
                    colors[charInfo.vertexIndex + j] = GetRainbowColor( DialogueTime.time + _length * vertices[charInfo.vertexIndex + j].x );
                }
            }
        }

        private Color GetRainbowColor( float time )
        {
            /*
            return new Color( 
                Mathf.Sin( _rainbowFrequence * time ) + 1 / 2,
                Mathf.Sin( _rainbowFrequence * time + Mathf.PI * 2 / 3) + 1 / 2,
                Mathf.Sin( _rainbowFrequence * time + Mathf.PI * 4 / 3 ) + 1 / 2
                );
            */
            return new Color(
                Mathf.Clamp( Mathf.Sin( _frequency * time + Mathf.PI * 4 / 3 ), 0, 1 ),
                Mathf.Clamp( Mathf.Sin( _frequency * time + Mathf.PI * 2 / 3 ), 0, 1 ),
                Mathf.Clamp( Mathf.Sin( _frequency * time ), 0, 1 )
                );
        }

        private TextRange _range;// handled text range

        float _frequency = 4f;
        float _length = 0.01f;
    }

}
