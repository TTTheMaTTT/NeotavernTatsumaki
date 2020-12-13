using System.Collections.ObjectModel;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее анимацию качения
    /// </summary>
    public class CWobbleEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Animation_Wobble );

            _range = editingParams.Range;
            Dictionary<string, string> attributes = editingParams.Attributes;

            if( attributes.ContainsKey( DialogueEditingAttributes.Frequency1Attribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.Frequency1Attribute].ParseToFloat( out _frequency1 ) );
            }
            Assert.IsTrue( _frequency1 >= 0 );

            if( attributes.ContainsKey( DialogueEditingAttributes.Frequency2Attribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.Frequency2Attribute].ParseToFloat( out _frequency2 ) );

            }
            Assert.IsTrue( _frequency2 >= 0 );

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
            // Ничего не делаем
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            switch( _coverage ) {
                case TEditingCoverage.Character:

                    for( int c = System.Math.Max( _range.From, 0 ); c < System.Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                        WobbleCharactersInRange( textInfo, vertices, originVertices, new CTextRange( c, c + 1 ) );
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
                        WobbleCharactersInRange( textInfo, vertices, originVertices,
                            new CTextRange( System.Math.Max( first, 0 ), System.Math.Min( _range.To, last ) ) );
                    }
                    break;
                case TEditingCoverage.Text:
                    WobbleCharactersInRange( textInfo, vertices, originVertices,
                        new CTextRange( System.Math.Max( _range.From, 0 ), System.Math.Min( _range.To, textInfo.characterCount ) ) );
                    break;
                default:
                    Assert.IsTrue( false );
                    break;
            }
        }

        private void WobbleCharactersInRange( TMP_TextInfo textInfo, Vector3[] vertices,
            ReadOnlyCollection<Vector3> originVertices, CTextRange range ) 
        {
            Vector3 offset = Wobble( Time.unscaledTime + range.From );
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

        private Vector2 Wobble( float time ) {
            return new Vector2( Mathf.Sin( time * _frequency1 ), Mathf.Cos( time * _frequency2 ) ) * _amplitude;
        }

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        private float _frequency1 = 4f;// Частота по оси x
        private float _frequency2 = 5f;// Частота по оис y
        private float _amplitude = 1f;
        private TEditingCoverage _coverage = TEditingCoverage.Character;// Минимальная единица редактирования.
    }

}
