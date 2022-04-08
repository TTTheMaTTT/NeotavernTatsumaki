using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Wave animation
    /// </summary>
    public class WaveEffect : ITextEffect
    {

        // IDialogueEditing
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Animation_Wave );

            _range = effectParams.Range;

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.FrequencyAttribute ) ) {
                _frequency = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.FrequencyAttribute] );
            }
            Assert.IsTrue( _frequency >= 0 );

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.LengthAttribute ) ) {
                _length = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.LengthAttribute] );
            }
            Assert.IsTrue( _length >= 0 );

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.AmplitudeAttribute ) ) {
                _amplitude = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.AmplitudeAttribute] );
            }
            Assert.IsTrue( _amplitude >= 0 );
        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Do Nothing
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }

                float posX = originVertices[charInfo.vertexIndex].x;
                for( int j = 0; j < 4; j++ ) {
                    var orig = originVertices[charInfo.vertexIndex + j];
                    vertices[charInfo.vertexIndex + j] = orig + new Vector3( 0, Mathf.Sin( DialogueTime.time * _frequency + posX * _length ) * _amplitude, 0 );
                }
            }
        }

        private TextRange _range;

        private float _frequency = 10f;
        private float _length = 0.04f;
        private float _amplitude = 4f;
    }

}
