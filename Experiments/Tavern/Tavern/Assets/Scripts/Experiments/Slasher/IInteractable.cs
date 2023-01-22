using UnityEngine;


namespace Experiments.Slasher
{
    public interface IInteractable
    {
        void Interact();

        void OnSelect();
        void OnDeselect();
    }

}
