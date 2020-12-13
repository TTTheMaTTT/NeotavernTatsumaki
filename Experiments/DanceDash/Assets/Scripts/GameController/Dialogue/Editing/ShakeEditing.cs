using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее анимацию дрожания
    /// </summary>
    public class CShakeEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Animation_Shake );

            _range = editingParams.Range;
            Dictionary<string, string> attributes = editingParams.Attributes;

            if( attributes.ContainsKey( DialogueEditingAttributes.FrequencyAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.FrequencyAttribute].ParseToFloat( out _frequency ) );

            }
            Assert.IsTrue( _frequency >= 0 );

            if( attributes.ContainsKey( DialogueEditingAttributes.AmplitudeAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.AmplitudeAttribute].ParseToFloat( out _amplitude ) );
            }
            Assert.IsTrue( _amplitude >= 0 );

            if( attributes.ContainsKey( DialogueEditingAttributes.CoverageAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.CoverageAttribute].ParseToEnum( ref _coverage ) );
            }
        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Ничего не делает.
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            switch( _coverage ) {
                case TEditingCoverage.Character:

                    for( int c = System.Math.Max( _range.From, 0 ); c < System.Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                        ShakeCharactersInRange( textInfo, vertices, originVertices, new CTextRange( c, c + 1 ) );
                    }
                    break;
                case TEditingCoverage.Word:
                    foreach( var wordInfo in textInfo.wordInfo ) {
                        int first = wordInfo.firstCharacterIndex;
                        int last = first + wordInfo.characterCount;
                        if( first < _range.From ) {
                            continue;
                        } else if( first >= _range.To ) {
                            break;
                        }
                        ShakeCharactersInRange( textInfo, vertices, originVertices,
                            new CTextRange( System.Math.Max( first, 0 ), System.Math.Min( _range.To, last ) ) );
                    }
                    break;
                case TEditingCoverage.Text:
                    ShakeCharactersInRange( textInfo, vertices, originVertices,
                        new CTextRange( System.Math.Max( _range.From, 0 ), System.Math.Min( _range.To, textInfo.characterCount ) ) );
                    break;
                default:
                    Assert.IsTrue( false );
                    break;
            }
        }

        private void ShakeCharactersInRange( TMP_TextInfo textInfo, Vector3[] vertices,
            ReadOnlyCollection<Vector3> originVertices, CTextRange range ) {
            Vector3 offset = Shake( Time.unscaledTime + range.From );
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

        private Vector2 Shake( float time ) {
            Random.InitState( Mathf.CeilToInt( time * _frequency ) );
            return new Vector2( Random.value * 2 - 1, Random.value * 2 - 1 ).normalized * _amplitude;
        }

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        private float _amplitude = 2f;// Сила дрожания
        private float _frequency = 20;// кол-во дрожаний в секунду

        private TEditingCoverage _coverage = TEditingCoverage.Character;// Минимальная единица редактирования.
    }

}
