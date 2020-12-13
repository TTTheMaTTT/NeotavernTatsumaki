using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, меняющее цвет символам текста
    /// </summary>
    public class CColorEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Color );

            _range = editingParams.Range;

            Assert.IsTrue( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.ColorAttribute ) );
            Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.ColorAttribute].ParseToColor( out _color ) );
        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            for( int c = Math.Max( _range.From, 0); c < Math.Min( _range.To, textInfo.characterCount ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }
                int vertexIndex = charInfo.vertexIndex;
                for( int i = 0; i < 4; i++ ) {
                    colors[vertexIndex + i] = _color;
                }
            }
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Ничего не делает.   
        }

        private Color _color;// Цвет, который должны иметь символы
        private CTextRange _range;// Диапазон текста, с которым работает редактирование
    }

}
