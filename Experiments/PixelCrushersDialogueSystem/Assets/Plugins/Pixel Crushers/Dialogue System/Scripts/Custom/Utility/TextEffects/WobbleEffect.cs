using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Wobble animation
    /// </summary>
    public class WobbleEffect : ITextEffect
    {

        // IDialogueEditing
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Animation_Wobble );

            _range = effectParams.Range;
            Dictionary<string, string> attributes = effectParams.Attributes;

            if( attributes.ContainsKey( TextEffectsAttributes.Frequency1Attribute ) ) {
                _frequency1 = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.Frequency1Attribute] );
            }
            Assert.IsTrue( _frequency1 >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.Frequency2Attribute ) ) {
                _frequency2 = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.Frequency2Attribute] );

            }
            Assert.IsTrue( _frequency2 >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.AmplitudeAttribute ) ) {
                _amplitude = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.AmplitudeAttribute] );
            }
            Assert.IsTrue( _amplitude >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.CoverageAttribute ) ) {
                Assert.IsTrue( Tools.StringToEnum( attributes[TextEffectsAttributes.CoverageAttribute], ref _coverage ) );
            }
        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // do nothing
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            switch( _coverage ) {
                case TextEffectCoverage.Character:

                    for( int c = System.Math.Max( _range.From, 0 ); c < System.Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                        WobbleCharactersInRange( textInfo, vertices, originVertices, new TextRange( c, c + 1 ) );
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
                        WobbleCharactersInRange( textInfo, vertices, originVertices,
                            new TextRange( System.Math.Max( first, 0 ), System.Math.Min( _range.To, last ) ) );
                    }
                    break;
                case TextEffectCoverage.Text:
                    WobbleCharactersInRange( textInfo, vertices, originVertices,
                        new TextRange( System.Math.Max( _range.From, 0 ), System.Math.Min( _range.To, textInfo.characterCount ) ) );
                    break;
                default:
                    Assert.IsTrue( false );
                    break;
            }
        }

        private void WobbleCharactersInRange( TMP_TextInfo textInfo, Vector3[] vertices,
            ReadOnlyCollection<Vector3> originVertices, TextRange range )
        {
            // use origin vertices in order to combine this effect with others
            Vector3 offset = Wobble( DialogueTime.time + range.From );
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

        private Vector2 Wobble( float time )
        {
            return new Vector2( Mathf.Sin( time * _frequency1 ), Mathf.Cos( time * _frequency2 ) ) * _amplitude;
        }

        private TextRange _range;// handled text range

        private float _frequency1 = 4f;// frequency for the x-axis
        private float _frequency2 = 5f;// frequence for the y-axis
        private float _amplitude = 1f;
        private TextEffectCoverage _coverage = TextEffectCoverage.Character;// minimum text unit which will be wobbled separately from other units
    }

}
