using System;
using System.Xml;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Assertions;

namespace Dialogue {

    /// <summary>
    /// Реализация окна диалога
    /// </summary>
    public class CDialogueWindow : MonoBehaviour, IDialogueWindow
    {

        //IDialogueWindow
        public void Initialize( Action nextStatementCallback ) 
        {
            _nextStatementCallback = nextStatementCallback.Invoke;

            if( !_areChildrenWindowsDefined ) {
                defineChildrenWindows();
            }
        }

        public void Open()
        {
            _dialoguePane.SetActive( true );
            _statementTextProcessor.SetText( "" );
            _nameText.SetText( "" );
            _actorsPanel.OnStartDialogue();
        }

        public void Close()
        {
            _dialoguePane.SetActive( false );
        }

        public void ShowDialogueStatement( CDialogueStatement statement ) 
        {
            _nameText.SetText( statement.Name );
            _statementTextProcessor.SetText( statement.Statement );

            _actorsPanel.ShowActors( statement.ActorsArrangements, statement.ActorsImages, statement.ActorName );
        }

        /// <summary>
        /// Переход к следующему выражению
        /// </summary>
        public void GoToNextStatement() 
        {
            if( _nextStatementCallback != null ) {
                _nextStatementCallback.Invoke();
            }
        }

        private void Awake()
        {
            if( !_areChildrenWindowsDefined ) {
                defineChildrenWindows();
            }
            _dialoguePane.SetActive( false );
        }

        // Инициализируем дочерние окна.
        private void defineChildrenWindows() 
        {
            if( _dialoguePane == null ) {
                _dialoguePane = transform.Find( "DialoguePane" ).gameObject;
            }
            Assert.IsTrue( _dialoguePane != null );

            if( _statementText == null ) {
                _statementText = _dialoguePane.transform.Find( "StatementText" ).GetComponent<TextMeshProUGUI>();
            }
            Assert.IsTrue( _statementText != null );

            _statementTextProcessor = _statementText.gameObject.GetComponent<CDialogueTextProcessor>();
            if( _statementTextProcessor == null ) {
                _statementTextProcessor = _statementText.gameObject.AddComponent<CDialogueTextProcessor>();
            }
            Assert.IsTrue( _statementTextProcessor != null );

            if( _nameText == null ) {
                _nameText = _dialoguePane.transform.Find( "NameText" ).GetComponent<TextMeshProUGUI>();
            }
            Assert.IsTrue( _nameText != null );

            if( _nextStatementButton == null ) {
                _nextStatementButton = _dialoguePane.transform.Find( "NextStatementButton" ).GetComponent<Button>();
            }
            Assert.IsTrue( _nextStatementButton != null );

            if( _actorsPanel == null ) {
                _actorsPanel = _dialoguePane.transform.Find( "ActorsPanel" ).GetComponent<CDialogueActorsPanel>();
            }
            Assert.IsTrue( _actorsPanel != null );

            _areChildrenWindowsDefined = true;
        }

        [SerializeField] private GameObject _dialoguePane;// Панель со всеми диалоговыми вьюшками
        [SerializeField] private TextMeshProUGUI _statementText;// Окно с текстом диалогового выражения
        [SerializeField] private TextMeshProUGUI _nameText;// Окно с текстом имени говорящего
        [SerializeField] private Button _nextStatementButton;// Кнопка для перехода к следующему выражению.
        [SerializeField] private CDialogueActorsPanel _actorsPanel;// Окно отображения персонажей диалога

        private bool _areChildrenWindowsDefined = false;// Определены ли дочерние окна?
        private CDialogueTextProcessor _statementTextProcessor;// Обработчик текста, используемый перед выставлением текста в UI.

        private delegate void NextStatementDelegate();
        private NextStatementDelegate _nextStatementCallback;// Функция, вызываемая при переходе к следующему выражению

    }
}