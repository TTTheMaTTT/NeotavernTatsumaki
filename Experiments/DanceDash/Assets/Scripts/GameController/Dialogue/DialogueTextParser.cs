using System.Collections.Generic;
using UnityEngine.Assertions;

namespace Dialogue
{
    /// <summary>
    /// Парсер синтаксиса, описывающего диалоговый текст
    /// </summary>
    public class CDialogueTextParser
    {
        public CDialogueTextParser() 
        {
            _foundEdits = new List<CDialogueEditingParams>();
            _formingEdits = new Dictionary<string, CDialogueEditingParams>();
            _openedEditsPerTypes = new Dictionary<string, Stack<string>>();
        }

        /// <summary>
        /// Разбор строки, описывающей диалоговое выражение
        /// </summary>
        /// <param name="inText">Входная строка, которую нужно разобрать</param>
        /// <param name="outText">Выходная строка текста, очищенная от служебных тегов и готовая к отображению</param>
        /// <param name="edits">Набор редактирований, которые нужно провести над текстом при отображении. Задаётся, как список наборов параметров для каждого описанного в inText редактирования </param>
        public void Parse( string inText, out string outText, out List<CDialogueEditingParams> edits )
        {
            initialize();
            doParse( inText );
            outText = _cleanText;
            edits = new List<CDialogueEditingParams>( _foundEdits );
        }

        private void initialize() 
        {
            _cleanText = "";
            _foundEdits.Clear();
            _formingEdits.Clear();
            _openedEditsPerTypes.Clear();
        }

        private void doParse( string inText ) 
        {
            bool startedTag = false;
            string tagText = "";

            // Проходимся посимвольно по тексту, выделяем значимые элементы
            for( int i = 0; i < inText.Length; i++ ) {
                char current = inText[i];
                if( !startedTag ) {
                    Assert.IsTrue( current != TagCloseBracket, "Some close tag bracket is before open bracket" );// Не позволяем быть закрывающей скобке до открывающей
                    if( current == TagOpenBracket ) {
                        startedTag = true;
                        continue;
                    }
                    _cleanText += current;
                } else {
                    Assert.IsTrue( current != TagOpenBracket, "Two open tag brackets in a row" );// Между открывающей скобкой и закрывающей не должно быть другой открывающей.
                    if( current == TagCloseBracket ) {
                        parseTag( tagText, _cleanText.Length );
                        tagText = "";
                        startedTag = false;
                        continue;
                    }
                    tagText += current;
                }
            }

            // Прошлись по тексту, получили информацию, проверим, что не осталось незакрытых тегов
            Assert.IsTrue( _formingEdits.Count == 0, "Some tags are not closed" );
        }

        // Парсинг текста тэга
        void parseTag( string tagText, int position ) 
        {
            tagText = tagText.Replace(" ","").Replace("\t","");
            Assert.IsTrue( tagText.Length > 0, "Empty edit description" );
            
            bool isClosingTag = tagText[0] == CloseTagSymbol;
            if( isClosingTag ) {
                tagText = tagText.Remove( 0, 1 );
            }

            string[] editingsDescriptions = tagText.Split( EditingsSeparator );
            foreach( string editingDescription in editingsDescriptions ) {
                parseEditingDescription( editingDescription, isClosingTag, position );
            }
        }

        // По строке определить все параметры редактирования
        void parseEditingDescription( string editingDescription, bool isClosingTag, int position )
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            string editingName = "";
            string editingTypeString = "";
            
            // Получим все атрибуты и их значения
            string[] attributesDescriptions = editingDescription.Split( AttributesSeparator );

            foreach( string attributeDescription in attributesDescriptions ) {
                string[] keyValue = attributeDescription.Split( KeyValueSeparator );

                if( keyValue.Length == 1 ) {
                    Assert.IsTrue( editingTypeString.Length == 0, "More than one type attributes in editing descriptions" );
                    editingTypeString = keyValue[0].ToLower();
                    Assert.IsTrue( editingTypeString.Length != 0, "Empty type name" );
                    // Считаем, что без ключа можно писать тип редактирования
                } else if( keyValue.Length == 2 ) {
                    string key = keyValue[0].ToLower();
                    string value = keyValue[1].ToLower();
                    Assert.IsTrue( key.Length > 0, "Empty attribute name" );
                    Assert.IsTrue( value.Length > 0, "Empty attribute value" );

                    if( key == TypeAttributeName ) {
                        Assert.IsTrue( editingTypeString.Length == 0, "More than one type attributes in editing descriptions" );
                        editingTypeString = value;
                    } else if( key == NameAttributeName ) {
                        editingName = value;
                    } else if ( !isClosingTag ) {
                        attributes.Add( key, value );
                    }

                }
            }

            if( !isClosingTag ) {
                startEditingDescription( editingDescription, editingTypeString, editingName, attributes, position );
            } else { 
                finishEditingDescription( editingDescription, editingTypeString, editingName, position );
            }
        }

        // Завести запись о редактировании, которая будет ожидать своего завершения при закрытии соответствующим тегом
        private void startEditingDescription( string editingDescription, string editingTypeString, string editingName, 
            Dictionary<string, string> attributes, int position ) 
        {
            CDialogueEditingParams newParams = new CDialogueEditingParams();

            // Заполняем параметры редактирования
            Assert.IsTrue( _stringToEditingType.ContainsKey( editingTypeString ), $"Non-existing type of editing : { editingTypeString }" );
            newParams.EditingType = _stringToEditingType[editingTypeString];
            newParams.Range = new CTextRange( position, position );// Пока заполняем так, затем изменим, когда найдётся закрывающий тег
            newParams.Attributes = attributes;

            // Подбираем, под каким ключом запомним данную запись
            string key = editingName.Length == 0 ? editingDescription : "name=" + editingName;
            Assert.IsTrue( !_formingEdits.ContainsKey( key ), "Have two tags with the same name before their closing tags. This lead to ambiguity." );
            _formingEdits.Add( key, newParams );
            if( !_openedEditsPerTypes.ContainsKey( editingTypeString ) ) {
                _openedEditsPerTypes.Add( editingTypeString, new Stack<string>() );
            }
            _openedEditsPerTypes[editingTypeString].Push( key );
        }

        // Завершение записи о редактировании, заключающаяся в определении правого границы области редактирования
        private void finishEditingDescription( string editingDescription, string editingTypeString, string editingName, int position ) 
        {
            // Сначала найдём заведённую незавершённую запись о редактировании, соответствующую описанию
            CDialogueEditingParams closedParams = null;
            string closedParamsKey = "";
            if( editingName.Length != 0 ) {
                closedParamsKey = "name=" + editingName;
                Assert.IsTrue( _formingEdits.ContainsKey( closedParamsKey ), $"Closing tag has name ${ editingName }, which does't fit to any opening tag" );
                closedParams = _formingEdits[closedParamsKey];
            } else if( _formingEdits.ContainsKey( editingDescription ) ) {
                closedParamsKey = editingDescription;
                closedParams = _formingEdits[editingDescription];
            } else if ( editingTypeString.Length != 0 ) {
                // Указали тип редактирования. Попробуем закрыть последнюю запись с соответствующим типом
                if( _openedEditsPerTypes.ContainsKey( editingTypeString ) ) {
                    while( _openedEditsPerTypes[editingTypeString].Count > 0 ) {
                        string formingParamKey = _openedEditsPerTypes[editingTypeString].Pop();
                        // Теоретически словарь _formingEdits может не содержать полученный ключ, ну и фиг с ним, это допустимая ситуация, 
                        // просто из словаря уже была получена и удалена запись с этим ключом, только другим способом
                        if( _formingEdits.ContainsKey( formingParamKey ) ) {
                            closedParamsKey = formingParamKey;
                            closedParams = _formingEdits[formingParamKey];
                        }
                    }
                }
            }

            // Дозаполняем запись, заносим куда нужно, удаляем откуда нужно
            Assert.IsTrue( closedParams != null && closedParamsKey.Length > 0, "Some closing tag doesn't have appropriate opening tag. Parser can't define, what to close." );
            closedParams.Range = new CTextRange( closedParams.Range.From, position );
            _foundEdits.Add( closedParams );
            _formingEdits.Remove( closedParamsKey );
        }

        private const char TagOpenBracket = '<';
        private const char TagCloseBracket = '>';
        private const char CloseTagSymbol = '/';
        private const char EditingsSeparator = ';';
        private const char AttributesSeparator = ',';
        private const char KeyValueSeparator = '=';

        private const string TypeAttributeName = "type";
        private const string NameAttributeName = "name";

        private readonly Dictionary<string, TEditingType> _stringToEditingType = new Dictionary<string, TEditingType> { 
            { "color", TEditingType.Color },
            { "change_color", TEditingType.Color },
            { "color_transition", TEditingType.Color_Transition },
            { "color_blink", TEditingType.Color_Blink },
            { "color_rainbow", TEditingType.Color_Rainbow },
            { "rainbow", TEditingType.Color_Rainbow },
            { "animation_wave", TEditingType.Animation_Wave },
            { "wave", TEditingType.Animation_Wave },
            { "animation_wobble", TEditingType.Animation_Wobble },
            { "wobble", TEditingType.Animation_Wobble },
            { "animation_shake", TEditingType.Animation_Shake },
            { "shake", TEditingType.Animation_Shake },
            { "size_transition", TEditingType.Animation_SizeTransition },
            { "animation_size_transition", TEditingType.Animation_SizeTransition }
        };

        private string _cleanText;// Сформированный чистый текст без тегов.
        private List<CDialogueEditingParams> _foundEdits;// Список найденных  при парсинге текста описаний редактирований
        private Dictionary<string, CDialogueEditingParams> _formingEdits;// Словарь из формирующихся описаний редактирований
        // Словарь, используемый для корректного завершения формирования редактирования, когда закрывающий тег даёт лишь информацию о типе редактирования
        // С его помощью закрываем последний открывающий тег того же типа.
        // Ключом является название типа, значением - стек из ключей к _foundEdits.
        private Dictionary<string, Stack<string>> _openedEditsPerTypes;
        
    }
}