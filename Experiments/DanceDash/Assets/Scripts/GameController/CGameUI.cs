using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Dialogue;
using UnityEngine.Assertions;

// Коллекция UI элементов игры. Синглтон.
public class CGameUI : MonoBehaviour
{

    private static CGameUI _instance;
    public static CGameUI Instance() 
    {
        if( _instance == null ) {
            _instance = FindObjectOfType<CGameUI>();
            if( _instance == null ) {
                _instance = new GameObject().AddComponent<CGameUI>();
            }
        }

        if( !_instance._isInitialized ) {
            _instance.initialize();
        }

        return _instance;
    }

    public IDialogueWindow DialogueWindow { get { return _dialogueWindow; } }

    // Start is called before the first frame update
    private void Awake()
    {
        if( _instance != null ) {
            Destroy( this );
            return;
        }

        _instance = this;

        if( !_isInitialized ) {
            initialize();
        }
    }

    // Инициализация коллекции
    private void initialize() 
    {
        _dialogueWindow = GetComponentInChildren<CDialogueWindow>();
        Assert.IsTrue( _dialogueWindow != null );

        _isInitialized = true;
    }

    private IDialogueWindow _dialogueWindow;
    private bool _isInitialized = false;
}

