using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее окрашивание текста в радугу
    /// </summary>
    public class CColorRainbowEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Color_Rainbow );

            _range = editingParams.Range;

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.FrequencyAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.FrequencyAttribute].ParseToFloat( out _frequency ) );

            }
            Assert.IsTrue( _frequency >= 0 );

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.LengthAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.LengthAttribute].ParseToFloat( out _length ) );
            }
            Assert.IsTrue( _length >= 0 );

        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Ничего не делает.
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( _range.To, textInfo.characterCount ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }

                for( int j = 0; j < 4; j++ ) {
                    colors[charInfo.vertexIndex + j] = GetRainbowColor( Time.unscaledTime + _length * vertices[charInfo.vertexIndex + j].x );
                }
            }
        }

        private Color GetRainbowColor( float time ) {
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

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        float _frequency = 4f;
        float _length = 0.01f;
    }

}
