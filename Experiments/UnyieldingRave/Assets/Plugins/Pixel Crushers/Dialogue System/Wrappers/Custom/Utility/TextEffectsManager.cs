// Recompile at 7/14/2021 11:23:44 AM
// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem.Wrappers
{

#if TMP_PRESENT

    /// <summary>
    /// This wrapper class keeps references intact if you switch between the 
    /// compiled assembly and source code versions of the original class.
    /// </summary>
    [AddComponentMenu( "Pixel Crushers/Dialogue System/UI/TextMesh Pro/Effects/Text Effects Manager" )]
    [DisallowMultipleComponent]
    public class TextEffectsManager : PixelCrushers.DialogueSystem.TextEffectsManager
    {
    }

#else

    [HelpURL("https://pixelcrushers.com/dialogue_system/manual2x/html/dialogue_u_is.html#dialogueUITypewriterEffect")]
    [AddComponentMenu("")]
    public class TextEffectsManager : PixelCrushers.DialogueSystem.TextEffectsManager
    {
        private void Reset()
        {
            Debug.LogWarning("Support for " + GetType().Name + " must be enabled using Tools > Pixel Crushers > Dialogue System > Welcome Window.", this);
        }
    }

#endif
}
