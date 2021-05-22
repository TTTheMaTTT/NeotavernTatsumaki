using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Dialogue;
using Databox;
using UnityEngine.Assertions;

namespace GameController { 

    // Синглтон, контролирующий потоки игровых событий
    public class CGameController : MonoBehaviour
    {
        public static TGameMode GameMode;

        private static CGameController _instance;

        /// <summary>
        /// Доступ к контролеру
        /// </summary>
        /// <returns>Экземпляр игрового контроллера</returns>
        public static CGameController Instance()
        {
            if( _instance == null ) { 
                _instance = FindObjectOfType<CGameController>();
                if( _instance == null ) {
                    _instance = new GameObject().AddComponent<CGameController>();
                }
            }

            if( !_instance._isInitialized ) {
                _instance.initialize();
            }

            return _instance;
        }

        /// <summary>
        /// Доступ к диалоговой системе
        /// </summary>
        public IDialogueSystem DialogueSystem { get { return _dialogueSystem; } }

        /// <summary>
        /// Выставлеие режима игры
        /// </summary>
        /// <param name="gameMode">Выставляемый режим</param>
        public void SetGameMode( TGameMode gameMode )
        {
            GameMode = gameMode;
            switch( gameMode ) {
                case TGameMode.Action:
                    Time.timeScale = 0f;
                    break;
                case TGameMode.Dialogue:
                case TGameMode.Pause:
                    Time.timeScale = 1f;
                    break;
                default:
                    Assert.IsTrue( false );
                    break;
            }
        }

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

        private void Start()
        {
        }

        private void Update()
        {
            if( !showDialog && _dialogueToShow != null ) {
                showDialog = true;
                _dialogueSystem.StartDialogue( _dialogueToShow );// Потом удалю эту строку.
            }
        }

        // Проинициализировать все составляющие контроллер компоненты.
        private void initialize() 
        {
            GameMode = TGameMode.Action;

            _dialogueSystem = new CDialogueSystem();

            _isInitialized = true;
        }


        [SerializeField] private string _dialogueToShow;// Временная переменная для проверки диалоговой системы. Потом удалю

        private bool showDialog = false;

        [SerializeField]private DataboxObject _dialoguesDatabox;
        [SerializeField] private DataboxObject _textsDatabox;
        private IDialogueSystem _dialogueSystem;

        private bool _isInitialized = false;// Был ли проинициализирован контроллер

    }

}

