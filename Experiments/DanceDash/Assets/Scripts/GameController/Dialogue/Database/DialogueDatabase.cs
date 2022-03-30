using System;
using UnityEngine;
using UnityEngine.Assertions;

using Databox;

namespace Dialogue
{
    /// <summary>
    /// Доступ к базе диалогов
    /// </summary>
    public class CDialogueDatabase
    {
        private const string DefaultDialogueTableName = "Dialogues";

        public CDialogueDatabase( DataboxObject databox )
        {
            _databox = databox;
            Assert.IsNotNull( _databox );

            _tableName = DefaultDialogueTableName;
            
        }

        public CDialogueDatabase( DataboxObject databox, string tableName )
        {
            _databox = databox;
            Assert.IsNotNull( _databox );

            _tableName = tableName;
        }

        /// <summary>
        /// Возвращает диалог из базы по его названию. Если диалога нет, то вернёт null
        /// </summary>
        /// <param name="dialogueId"></param>
        /// <returns></returns>
        public CDialogue GetDialogue( string dialogueId )
        {
            CDialogue dialogue = null;
            try {
                // Считаем, что у Entry в данной таблице есть ровно одно Value. Иначе ничего не возвращаем, так как непонятно что возвращать
                var keys = _databox.GetValuesFromEntry( _tableName, dialogueId ).Keys;
                if( keys.Count == 1 ) {
                    var enumerator = keys.GetEnumerator();
                    Assert.IsTrue( enumerator.MoveNext() );
                    _databox.TryGetData<CDialogue>( _tableName, dialogueId, enumerator.Current, out dialogue );
                }
            } catch( Exception /*e*/) { }
            return dialogue;
        }

        private DataboxObject _databox;
        private string _tableName;
    }
}