using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Databox;

using LanguageTools;

namespace Dialogue
{
    /// <summary>
    /// Доступ к базе текстов диалогов
    /// </summary>
    public class CDialogueTextDatabase
    {
        // Используем 2 таблицы с предопределёнными именами.
        private const string TextsTableName = "Texts";
        private const string EditedTextsTableName = "EditedTexts";

        public CDialogueTextDatabase( DataboxObject databox )
        {
            _databox = databox;
            Assert.IsNotNull( _databox );
        }

        /// <summary>
        /// Возвращает текст из базы по его id. Если текста нет, то вернёт null
        /// </summary>
        /// <param name="textId"></param>
        /// <returns></returns>
        public StringType GetText( string textId, TLanguage language )
        {
            return GetValue( TextsTableName, textId, language );
        }

        /// <summary>
        /// Возвращает текст с редактированиями из базы по его id. Если текста нет, то вернёт null
        /// </summary>
        /// <param name="textId"></param>
        /// <returns></returns>
        public StringType GetEditedText( string textId, TLanguage language )
        {
            return GetValue( EditedTextsTableName, textId, language );
        }

        private StringType GetValue( string tableName, string entryId, TLanguage language )
        {
            Assert.IsTrue( LanguageValues.LanguageNamesMap.ContainsKey( language ) );
            _databox.TryGetData<StringType>( tableName, entryId, LanguageValues.LanguageNamesMap[language], out StringType value );
            return value;
        }

        private DataboxObject _databox;
    }
}