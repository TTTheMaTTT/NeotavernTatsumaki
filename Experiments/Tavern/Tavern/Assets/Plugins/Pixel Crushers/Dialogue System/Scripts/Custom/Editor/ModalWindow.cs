using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace PixelCrushers.DialogueSystem.DialogueEditor
{
    public enum ModalWindowResult
    {
        None,
        Ok,
        Cancel,
        Invalid,
        LostFocus
    }

    /// <summary>
    /// Define a popup window that return a result.
    /// </summary>
    public abstract class ModalWindow : EditorWindow
    {
        protected IModalWindowOwner owner;

        protected ModalWindowResult result = ModalWindowResult.None;

        public ModalWindowResult Result
        {
            get {
                return result;
            }
        }

        protected virtual void OnLostFocus()
        {
            result = ModalWindowResult.LostFocus;

            if( owner != null )
                owner.ModalClosed( this );
        }

        protected virtual void Cancel()
        {
            result = ModalWindowResult.Cancel;

            if( owner != null )
                owner.ModalClosed( this );

            Close();
        }

        protected virtual void Ok()
        {
            result = ModalWindowResult.Ok;

            if( owner != null )
                owner.ModalClosed( this );

            Close();
        }

    }



}