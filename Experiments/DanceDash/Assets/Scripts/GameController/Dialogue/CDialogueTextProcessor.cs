using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using TMPro;

namespace Dialogue {

    /// <summary>
    /// Обработчик текста для показа в диалоговом окне.
    /// Занимается анимированием, окрашиванием, добавлением эффектов в тексте, используя TextMeshPro
    /// </summary>
    public class CDialogueTextProcessor : MonoBehaviour
    {

        public TextMeshProUGUI TextGUI { set { _textGUI = value; } }// Соответствующий обработчику элемент GUI, занимающийся отображением текста

        /// <summary>
        /// Обработать и выставить текст в соответствующее обработчику окно.
        /// </summary>
        /// <param name="text">Выставляемый текст</param>
        public void SetText( string text ) 
        {
            // Парсинг
            Assert.IsTrue( _parser != null );
            _parser.Parse( text, out var cleanText, out var editingParamsList );

            // Выставление текста
            _textGUI.SetText( cleanText );
            _textGUI.ForceMeshUpdate();
            _originVertices = _textGUI.mesh.vertices;
            Assert.IsTrue( cleanText.Length == _textGUI.textInfo.characterCount );

            // Инициализация редактирований
            Assert.IsTrue( _editingFactory != null );
            _editings.Clear();
            foreach( CDialogueEditingParams editingParams in editingParamsList ) {
                _editings.Add( _editingFactory.CreateEditing( editingParams ) );
                _editings[_editings.Count - 1].Initialize( editingParams );
            }

            Color[] colors = _textGUI.mesh.colors;
            Vector3[] vertices = _textGUI.mesh.vertices;
            TMP_TextInfo textInfo = _textGUI.textInfo;
            // Начальное редактирование
            foreach( IDialogueEditing editing in _editings ) {
                editing.StartEditing( textInfo, colors, vertices, Array.AsReadOnly( _originVertices ));
            }
            _textGUI.mesh.colors = colors;
            _textGUI.mesh.vertices = vertices;
            _textGUI.canvasRenderer.SetMesh( _textGUI.mesh );
        }

        private void Awake()
        {
            _textGUI = GetComponent<TextMeshProUGUI>();
            _parser = new CDialogueTextParser();
            _editingFactory = new CDialogueEditingFactory();
            _editings = new List<IDialogueEditing>();
        }

        private void Update()
        {
            Color[] colors = _textGUI.mesh.colors;
            Vector3[] vertices = _textGUI.mesh.vertices;
            TMP_TextInfo textInfo = _textGUI.textInfo;
            foreach( IDialogueEditing editing in _editings ) {
                editing.UpdateEditing( textInfo, colors, vertices, Array.AsReadOnly( _originVertices ));
            }
            _textGUI.mesh.colors = colors;
            _textGUI.mesh.vertices = vertices;
            _textGUI.canvasRenderer.SetMesh( _textGUI.mesh );
        }

        private TextMeshProUGUI _textGUI;// Соответствующий обработчику элемент GUI, занимающийся отображением текста
        private CDialogueTextParser _parser;// Разбирает входной текст, определяя, что нужно вывести и как это редактировать
        private CDialogueEditingFactory _editingFactory;// Фабрика редактирований

        private Vector3[] _originVertices;// Изначальные вершины меша до всех редактирований
        private List<IDialogueEditing> _editings;// Набор применяемых в данный момент редактирований

    }
}