using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Interface of the UI panel with the scene of the current dialogue.
    /// </summary>
    public interface IDialogueSceneUI
    {
        void Reset();
        void ChangeScene( DialogueEntry entry, List<CharacterInfo> entryActorsInfo );
    }
}