using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace PixelCrushers.DialogueSystem
{

    public class DialogueSceneUIBase : MonoBehaviour, IDialogueSceneUI
    {
        // Clear all service structure and destroy actors UI
        public virtual void Reset()
        {
            // do nothing
        }


        // Update scene with information from entry
        public virtual void ChangeScene( DialogueEntry entry, List<CharacterInfo> entryActorsInfo )
        {
            // do nothing
        }
    }
}
