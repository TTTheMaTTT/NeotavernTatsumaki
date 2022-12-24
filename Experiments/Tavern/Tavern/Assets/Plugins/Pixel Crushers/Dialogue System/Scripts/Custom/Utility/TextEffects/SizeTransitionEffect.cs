using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Transition between two sizes
    /// </summary>
    public class SizeTransitionEffect : ITextEffect
    {

        // IDialogueEditing
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Animation_SizeTransition );

            _range = effectParams.Range;
            Dictionary<string, string> attributes = effectParams.Attributes;

            if( attributes.ContainsKey( TextEffectsAttributes.SpeedAttribute ) ) {
                _speed = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.SpeedAttribute] );
            }
            Assert.IsTrue( _speed >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.SizeFromAttribute ) ) {
                _sizeFrom = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.SizeFromAttribute] );
            }
            Assert.IsTrue( _sizeFrom >= 0 );

            if( attributes.ContainsKey( TextEffectsAttributes.SizeToAttribute ) ) {
                _sizeTo = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.SizeToAttribute] );
            }
            Assert.IsTrue( _sizeTo >= 0 );
        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            _startTransitionTime = Time.unscaledTime;
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            for( int c = Math.Max( _range.From, 0 ); c < Math.Min( textInfo.characterCount, _range.To ); c++ ) {
                var charInfo = textInfo.characterInfo[c];
                if( !charInfo.isVisible ) {
                    continue;
                }
                // use origin vertices in order to combine this effect with others                
                Vector3 originCenter = (originVertices[charInfo.vertexIndex] + originVertices[charInfo.vertexIndex + 2]) / 2;
                Vector3 center = (vertices[charInfo.vertexIndex] + vertices[charInfo.vertexIndex + 2]) / 2;
                for( int j = 0; j < 4; j++ ) {
                    Vector3 orig = originVertices[charInfo.vertexIndex + j];
                    Vector3 deltaFrom = (orig - originCenter) * _sizeFrom, deltaTo = (orig - center) * _sizeTo;
                    vertices[charInfo.vertexIndex + j] = Vector3.Lerp( center + deltaFrom,
                                                                    center + deltaTo,
                                                                    Mathf.Clamp( _speed * (DialogueTime.time - _startTransitionTime), 0f, 1f ) );
                }
            }
        }

        private TextRange _range;// Handled text range

        private float _startTransitionTime;

        // Consider 1 as the original character size
        private float _sizeFrom = 0f;
        private float _sizeTo = 1f;
        private float _speed = 2f;
    }

}
