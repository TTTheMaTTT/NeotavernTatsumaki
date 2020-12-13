using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {
    /// <summary>
    /// Редактирование, производяющее переход размера
    /// </summary>
    public class CSizeTransitionEditing : IDialogueEditing
    {

        // IDialogueEditing
        public void Initialize( CDialogueEditingParams editingParams ) 
        {
            Assert.IsTrue( editingParams.EditingType == TEditingType.Animation_SizeTransition );

            _range = editingParams.Range;
            Dictionary<string, string> attributes = editingParams.Attributes;

            if( attributes.ContainsKey( DialogueEditingAttributes.SpeedAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.SpeedAttribute].ParseToFloat( out _speed ) );
            }
            Assert.IsTrue( _speed >= 0 );

            if( attributes.ContainsKey( DialogueEditingAttributes.SizeFromAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.SizeFromAttribute].ParseToFloat( out _sizeFrom ) );
            }
            Assert.IsTrue( _sizeFrom >= 0 );

            if( attributes.ContainsKey( DialogueEditingAttributes.SizeToAttribute ) ) {
                Assert.IsTrue( attributes[DialogueEditingAttributes.SizeToAttribute].ParseToFloat( out _sizeTo ) );
            }
            Assert.IsTrue( _sizeTo >= 0 );
        }

        public void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            _startTransitionTime = Time.unscaledTime;
        }

        public void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            // Увеличение символов из точки
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }
                Vector3 originCenter = (originVertices[charInfo.vertexIndex] + originVertices[charInfo.vertexIndex + 2]) / 2;
                Vector3 center = (vertices[charInfo.vertexIndex] + vertices[charInfo.vertexIndex + 2]) / 2;
                for( int j = 0; j < 4; j++ ) {
                    Vector3 orig = originVertices[charInfo.vertexIndex + j];
                    Vector3 deltaFrom = (orig - originCenter) * _sizeFrom, deltaTo = (orig - center) * _sizeTo;
                    vertices[charInfo.vertexIndex + j] = Vector3.Lerp( center + deltaFrom, 
                                                                    center + deltaTo, 
                                                                    Mathf.Clamp( _speed * (Time.unscaledTime - _startTransitionTime), 0f, 1f ) );
                }
            }
        }

        private CTextRange _range;// Диапазон текста, с которым работает редактирование

        private float _startTransitionTime;// Время начала перехода.

        // Считаем за 1 оригинальный размер символа
        private float _sizeFrom = 0f;
        private float _sizeTo = 1f;
        private float _speed = 2f;
    }

}
