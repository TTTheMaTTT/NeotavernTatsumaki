using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Xml;

using GameController;
using Databox;

namespace Dialogue {
    /// <summary>
    /// Класс, управляющий всем процессом диалога
    /// </summary>
    public class CDialogueSystem : IDialogueSystem
    {

        /// <summary>
        /// Конструктор, в котором можно указать датабоксы для баз данных.
        /// Если не указать или указать null для одного из аргументов, то возьмётся значение по умолчанию, которое указано в databases.xml.
        /// </summary>
        /// <param name="dialoguesDatabox">Датабокс с диалогами.</param>
        /// <param name="dialogueTextsDatabox">Датабокс с диалоговыми текстами.</param>
        public CDialogueSystem( DataboxObject dialoguesDatabox = null, DataboxObject dialogueTextsDatabox = null ) 
        {
            _currentStatements = new Queue<CDialogueStatement>();

            _dialogueWindow = CGameUI.Instance().DialogueWindow;
            _dialogueWindow.Initialize( nextStatementCallback );

            // Инициализация базы данных
            if( dialoguesDatabox == null ) {
                tryToInitializeDatabox( DatabaseTools.DialoguesDataboxNodeName, out dialoguesDatabox );
            }
            Assert.IsNotNull( dialoguesDatabox, "Couldn't Initialize databases" );
            _dialogueDatabase = new CDialogueDatabase( dialoguesDatabox );

            if( dialogueTextsDatabox == null ) {
                tryToInitializeDatabox( DatabaseTools.TextsDataboxNodeName, out dialogueTextsDatabox );
            }
            Assert.IsNotNull( dialogueTextsDatabox, "Couldn't Initialize databases" );
            _dialogueTextDatabase = new CDialogueTextDatabase( dialogueTextsDatabox );

        }

        // Инициализация датабокса полем, описанным в xml-файле
        private void tryToInitializeDatabox( string nodename, out DataboxObject databoxObject )
        {
            databoxObject = null;
            try {
                // Прочитаем, какие базы следует использовать, из xml-файла
                TextAsset textAsset = Resources.Load<TextAsset>( DatabaseTools.DatabasesXmlFileName );
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml( textAsset.text );
                XmlElement root = xmlDoc.DocumentElement;
                databoxObject = Resources.Load<DataboxObject>( root.SelectSingleNode( nodename ).InnerText );
            } catch( Exception /*e*/ ) {
            }
        }

        /// <summary>
        /// Реализовать процесс диалога
        /// </summary>
        /// <param name="dialogueId">id диалога в базе данных</param>
        public void StartDialogue( string dialogueId )
        {
            CDialogue dialogue = _dialogueDatabase.GetDialogue( dialogueId );
            Assert.IsNotNull( dialogue, $"dialogue with id {dialogueId} is not found in database" );
            StartDialogue( dialogue );
        }

        /// <summary>
        /// Реализовать процесс диалога
        /// </summary>
        /// <param name="dialogue">Переменная, описывающая диалог</param>
        public void StartDialogue( CDialogue dialogue ) 
        {
            if( dialogue.Statements.Count <= 0 ) {
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
        private CDialogueDatabase _dialogueDatabase;// Доступ к базе данных диалогов
        private CDialogueTextDatabase _dialogueTextDatabase;// Доступ к базе данных диалоговых текстов

    }
}