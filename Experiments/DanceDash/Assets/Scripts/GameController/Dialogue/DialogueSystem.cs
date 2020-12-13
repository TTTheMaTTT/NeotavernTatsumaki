using System.Collections;
using System.Xml;
using System.Collections.Generic;
using UnityEngine;

using GameController;

namespace Dialogue {
    /// <summary>
    /// Класс, управляющий всем процессом диалога
    /// </summary>
    public class CDialogueSystem : IDialogueSystem
    {

        public CDialogueSystem( string iniPath = "DialogueSystem.xml") 
        {
            _currentStatements = new Queue<CDialogueStatement>();

            XmlDocument iniDoc = new XmlDocument();
            iniDoc.Load( iniPath );

            _dialogueWindow = CGameUI.Instance().DialogueWindow;
            _dialogueWindow.Initialize( nextStatementCallback, iniDoc );
        }

        /// <summary>
        /// Реализовать процесс диалога
        /// </summary>
        /// <param name="dialogue">Переменная, описывающая диалог</param>
        public void StartDialogue( CDialogue dialogue ) 
        {
            if( dialogue.Statements.Length <= 0 ) {
                _dialogueWindow.Close();
                return;
            }

            CGameController.Instance().SetGameMode( TGameMode.Dialogue );

            _currentStatements.Clear();
            foreach( CDialogueStatement statement in dialogue.Statements ) {
                _currentStatements.Enqueue( statement );
            }

            // Показываем первый стейтмент из диалога
            _dialogueWindow.Open();
            _dialogueWindow.ShowDialogueStatement( _currentStatements.Dequeue() );
        }

        /// <summary>
        /// Завершить текущий диалог.
        /// </summary>
        public void StopDialogue()
        {
            _dialogueWindow.Close();
            CGameController.Instance().SetGameMode( TGameMode.Action );
        }

        // Переход к следуюшему выражению
        private void nextStatementCallback() 
        {
            if( _currentStatements.Count <= 0 ) {
                _dialogueWindow.Close();
            } else {
                _dialogueWindow.ShowDialogueStatement( _currentStatements.Dequeue() );
            }
        }

        private Queue<CDialogueStatement> _currentStatements;

        private IDialogueWindow _dialogueWindow;// Окно, отображающее диалог пользователю

    }
}