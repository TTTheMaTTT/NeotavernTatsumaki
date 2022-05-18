using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Shakes text
    /// </summary>
    public class ShakeEffect : ITextEffect
    {

        // IDialogueEditing
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Animation_Shake );

            _range = effectParams.Range;
            Dictionary<string, string> attributes = effectParams.Attributes;

            if( attributes.ContainsKey( TextEffectsAttributes.FrequencyAttribute ) ) {
                _frequency = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.FrequencyAttribute] );
            }
            Assert.IsTrue( _frequency >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.AmplitudeAttribute ) ) {
                _amplitude = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.AmplitudeAttribute] );
            }
            Assert.IsTrue( _amplitude >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.CoverageAttribute ) ) {
                Assert.IsTrue( Tools.StringToEnum( attributes[TextEffectsAttributes.CoverageAttribute],  ref _coverage ) );
            }
        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Do nothing.
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            switch( _coverage ) {
                case TextEffectCoverage.Character:

                    for( int c = System.Math.Max( _range.From, 0 ); c < System.Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                        ShakeCharactersInRange( textInfo, vertices, originVertices, new TextRange( c, c + 1 ) );
                    }
                    break;
                case TextEffectCoverage.Word:
                    foreach( var wordInfo in textInfo.wordInfo ) {
                        int first = wordInfo.firstCharacterIndex;
                        int last = first + wordInfo.characterCount;
                        if( first < _range.From ) {
                            continue;
                        } else if( first >= _range.To ) {
                            break;
                        }
                        ShakeCharactersInRange( textInfo, vertices, originVertices,
                            new TextRange( System.Math.Max( first, 0 ), System.Math.Min( _range.To, last ) ) );
                    }
                    break;
                case TextEffectCoverage.Text:
                    ShakeCharactersInRange( textInfo, vertices, originVertices,
                        new TextRange( System.Math.Max( _range.From, 0 ), System.Math.Min( _range.To, textInfo.characterCount ) ) );
                    break;
                default:
                    Assert.IsTrue( false );
                    break;
            }
        }

        private void ShakeCharactersInRange( TMP_TextInfo textInfo, Vector3[] vertices,
            ReadOnlyCollection<Vector3> originVertices, TextRange range )
        {
            // use origin vertices in order to combine this effect with others
            Vector3 offset = Shake( DialogueTime.time + range.From );
            for( int c = range.From; c < range.To; c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }

                for( int j = 0; j < 4; j++ ) {
                    vertices[charInfo.vertexIndex + j] = originVertices[charInfo.vertexIndex + j] + offset;
                }
            }
        }

        private Vector2 Shake( float time )
        {
            Random.InitState( Mathf.CeilToInt( time * _frequency ) );
            return new Vector2( Random.value * 2 - 1, Random.value * 2 - 1 ).normalized * _amplitude;
        }

        private TextRange _range;// handled text range

        private float _amplitude = 2f;// how much to shake
        private float _frequency = 20;// number of shakes in second

        private TextEffectCoverage _coverage = TextEffectCoverage.Character;// minimum text unit which will be shaken separately from other units
    }

}
