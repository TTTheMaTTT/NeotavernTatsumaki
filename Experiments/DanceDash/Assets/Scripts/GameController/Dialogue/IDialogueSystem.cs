/// <summary>
/// Интерфейс диалоговой системы
/// </summary>

namespace Dialogue {
    public interface IDialogueSystem
    {
        void StartDialogue( string dialogueId );
        void StartDialogue( CDialogue dialogue );
    }
}