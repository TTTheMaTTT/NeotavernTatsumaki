using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее мигание между двумя цветами
    /// </summary>
    public class CColorBlinkEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Color_Blink );

            _range = editingParams.Range;

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.FrequencyAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.FrequencyAttribute].ParseToFloat( out _frequency ) );

            }
            Assert.IsTrue( _frequency >= 0 );

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.ColorFromAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.ColorFromAttribute].ParseToColor( out _colorFrom ) );
            }

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.ColorToAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.ColorToAttribute].ParseToColor( out _colorTo ) );
            }

        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Ничего не делает.
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Мигание
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

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        //Цвета перехода
        private Color _colorFrom = Color.white;
        private Color _colorTo = Color.black;

        private float _frequency = 10f;

    }

}
