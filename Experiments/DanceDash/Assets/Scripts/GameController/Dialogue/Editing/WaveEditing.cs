using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее анимацию волны 
    /// </summary>
    public class CWaveEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Animation_Wave );

            _range = editingParams.Range;

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.FrequencyAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.FrequencyAttribute].ParseToFloat( out _frequency ) );

            }
            Assert.IsTrue( _frequency >= 0 );

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.LengthAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.LengthAttribute].ParseToFloat( out _length ) );
            }
            Assert.IsTrue( _length >= 0 );

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.AmplitudeAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.AmplitudeAttribute].ParseToFloat( out _amplitude ) );
            }
            Assert.IsTrue( _amplitude >= 0 );
        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Ничего не делает.
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Анимация волны
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }

                float posX = originVertices[charInfo.vertexIndex].x;
                for( int j = 0; j < 4; j++ ) {
                    var orig = originVertices[charInfo.vertexIndex + j];
                    vertices[charInfo.vertexIndex + j] = orig + new Vector3( 0, Mathf.Sin( Time.unscaledTime * _frequency + posX * _length ) * _amplitude, 0 );
                }
            }
        }

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        private float _frequency = 10f;
        private float _length = 0.04f;
        private float _amplitude = 10;
    }

}
