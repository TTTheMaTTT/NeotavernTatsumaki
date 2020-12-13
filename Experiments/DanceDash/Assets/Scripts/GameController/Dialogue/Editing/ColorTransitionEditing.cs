using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее переход цвета
    /// </summary>
    public class CColorTransitionEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Color_Transition );

            _range = editingParams.Range;

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.SpeedAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.SpeedAttribute].ParseToFloat( out _transitionSpeed ) );
            }
            Assert.IsTrue( _transitionSpeed >= 0 );

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.ColorFromAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.ColorFromAttribute].ParseToColor( out _colorFrom ) );
            }

            if( editingParams.Attributes.ContainsKey( DialogueEditingAttributes.ColorToAttribute ) ) {
                Assert.IsTrue( editingParams.Attributes[DialogueEditingAttributes.ColorToAttribute].ParseToColor( out _colorTo ) );
            }

        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            _startTransitionTime = Time.unscaledTime;
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            Color newColor = Color.Lerp( _colorFrom, _colorTo, Mathf.Clamp( _transitionSpeed * (Time.unscaledTime - _startTransitionTime), 0f, 1f ) );

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

        private float _transitionSpeed = 1f;// Скорость перехода.

        private float _startTransitionTime;// Время начала перехода.
    }

}
