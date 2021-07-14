using System;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// Transition between two colors
    /// </summary>
    public class ColorTransitionEffect : ITextEffect
    {

        // ITextEffect
        public void Initialize( TextEffectParams effectParams )
        {
            Assert.IsTrue( effectParams.EffectType == TextEffectType.Color_Transition );

            _range = effectParams.Range;

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.SpeedAttribute ) ) {
                _transitionSpeed = Tools.StringToFloat( effectParams.Attributes[TextEffectsAttributes.SpeedAttribute] );
            }
            Assert.IsTrue( _transitionSpeed >= 0 );

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.ColorFromAttribute ) ) {
                _colorFrom = Tools.WebColor( effectParams.Attributes[TextEffectsAttributes.ColorFromAttribute] );
            }

            if( effectParams.Attributes.ContainsKey( TextEffectsAttributes.ColorToAttribute ) ) {
                _colorTo = Tools.WebColor( effectParams.Attributes[TextEffectsAttributes.ColorToAttribute] );
            }

        }

        public void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            _startTransitionTime = DialogueTime.time;
        }

        public void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices )
        {
            Color newColor = Color.Lerp( _colorFrom, _colorTo, Mathf.Clamp( _transitionSpeed * (DialogueTime.time - _startTransitionTime), 0f, 1f ) );

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

        private TextRange _range;// handled text range

        //transition colors
        private Color _colorFrom = Color.white;
        private Color _colorTo = Color.black;

        private float _transitionSpeed = 1f;// transition speed

        private float _startTransitionTime;
    }

}
