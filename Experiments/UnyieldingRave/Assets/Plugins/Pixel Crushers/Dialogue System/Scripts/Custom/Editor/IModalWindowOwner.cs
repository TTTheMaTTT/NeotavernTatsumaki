using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    public interface IModalWindowOwner
    {
        /// <summary>
        /// Called when the associated modal is closed.
        /// </summary>
        void ModalClosed( ModalWindow window );
    }
}