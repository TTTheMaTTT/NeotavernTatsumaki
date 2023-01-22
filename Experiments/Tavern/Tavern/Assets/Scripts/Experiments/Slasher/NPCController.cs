using UnityEngine;
using PixelCrushers.DialogueSystem;


namespace Experiments.Slasher
{

    public class NPCController : MonoBehaviour, IInteractable
    {

        private DialogueSystemTrigger _dialogueTrigger;

        private void Awake()
        {
            _dialogueTrigger = GetComponent<DialogueSystemTrigger>();
        }

        public void Interact()
        {
            _dialogueTrigger.TryStart( null );
        }

        public void OnSelect()
        {

        }

        public void OnDeselect()
        {

        }

    }
}