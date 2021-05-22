using System.Collections.ObjectModel;
using UnityEngine;
using TMPro;

namespace PixelCrushers.DialogueSystem
{
    public interface ITextEffect
    {
        /// <summary>
        /// Initialization by params
        /// </summary>
        void Initialize( TextEffectParams effectParams );

        // colors - vertices colors, that editted by effect
        // vertices - vertices positions, that editted by effect
        // originVertices - vertices positions before all processing
        /// <summary>
        /// Effect preparation at his start effect frame
        /// </summary>
        void StartEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices );
        /// <summary>
        /// Refresh effect state on the new frame
        /// </summary>
        void UpdateEffect( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices );
    }
}
