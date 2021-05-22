using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.Assertions;

namespace PixelCrushers.DialogueSystem
{
    /// <summary>
    /// This is a parser, that reads tags from text and defines which effects to implement
    /// </summary>
    public class TextEffectsParser
    {
        public TextEffectsParser() 
        {
            _foundEffects = new List<TextEffectParams>();
            _formingEffects = new Dictionary<string, TextEffectParams>();
            _openedEffectsPerTypes = new Dictionary<string, Stack<string>>();
        }

        /// <summary>
        /// Parse of text string with stripping all tag effects
        /// </summary>
        /// <param name="inText">Parsed text</param>
        /// <param name="textWithoutEffects">Text without effect tags. which can be correctly shown by text mesh pro</param>
        /// <param name="textWithoutTags">Text without all tags. Effect ranges are appliable to this text.</param> 
        /// <param name="effects">Effect parameters, stripped from text</param>
        /// <return>true if parsing succeeded</return>
        public bool Parse( string inText, out string textWithoutEffects, out string textWithoutTags, 
            out List<TextEffectParams> effects )
        {
            initialize();
            bool success = doParse( inText );
            if( success ) {
                textWithoutEffects = _textWithoutEffects;
                textWithoutTags = _textWithoutTags;
                effects = new List<TextEffectParams>( _foundEffects );
            } else {
                textWithoutEffects = _textWithoutEffects;
                textWithoutTags = _textWithoutTags;
                effects = new List<TextEffectParams>();
            }
            return success;

        }

        private void initialize() 
        {
            _textWithoutEffects = "";
            _textWithoutTags = "";
            _foundEffects.Clear();
            _formingEffects.Clear();
            _openedEffectsPerTypes.Clear();
            haveErrors = false;
        }

        private bool doParse( string inText ) 
        {
            int textPos = 0;
            var tagMatches = Tools.GetTagMatches( inText );
            foreach( Match match in tagMatches ) {
                string textPart = Tools.UnescapeTagBrackets( inText.Substring( textPos, match.Index - textPos ) );
                _textWithoutTags += textPart;
                if( Tools.IsTextMeshProTag( match.Value ) || Tools.IsRichTextCode( match.Value ) || !tryParseEffectTag( match.Value, _textWithoutTags.Length ) ) {
                    _textWithoutEffects += textPart + inText.Substring( match.Index, match.Length );
                } else {
                    _textWithoutEffects += textPart;
                }
                textPos = match.Index + match.Length;
            }
            _textWithoutTags += inText.Substring( textPos, inText.Length - textPos );
            _textWithoutEffects += inText.Substring( textPos, inText.Length - textPos );
            return _formingEffects.Count == 0 && !haveErrors;
        }

        // returns true if at least one effect was parsed
        bool tryParseEffectTag( string tagText, int position ) 
        {
            tagText = tagText.Replace(" ","").Replace("\t","");
            Assert.IsTrue( tagText.Length >= 2 && tagText[0] == TagOpenBracket && tagText[tagText.Length - 1] == TagCloseBracket, "Tag borders should be correct in this place." );
            tagText = tagText.Substring( 1, tagText.Length - 2 );// Remove borders
            if( tagText.Length == 0 ) {
                return false;
            }
            
            bool isClosingTag = tagText[0] == CloseTagSymbol;
            if( isClosingTag ) {
                tagText = tagText.Remove( 0, 1 );
            }

            string[] effectsDescriptions = tagText.Split( EffectsSeparator );// More than one effect can be described in one tag

            bool success = false;
            foreach( string effectDescription in effectsDescriptions ) {
                if( parseEffectDescription( effectDescription, isClosingTag, position ) ) {
                    success = true;
                } else {
                    haveErrors = true;
                }
            }
            return success;
        }

        // Define all effect parameters from string
        bool parseEffectDescription( string effectDescription, bool isClosingTag, int position )
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();
            string effectName = "";
            string effectTypeString = "";
            
            string[] attributesDescriptions = effectDescription.Split( AttributesSeparator );

            foreach( string attributeDescription in attributesDescriptions ) {
                string[] keyValue = attributeDescription.Split( KeyValueSeparator );

                if( keyValue.Length == 1 ) {
                    // only type can be described by 1 word
                    if( effectTypeString.Length > 0 ){//More than one type attributes in effect description
                        return false;
                    }
                    effectTypeString = keyValue[0].ToLower();
                    if( effectTypeString.Length == 0 ) {// Empty type name
                        return false;
                    }
                } else if( keyValue.Length == 2 ) {
                    string key = keyValue[0].ToLower();
                    string value = keyValue[1].ToLower();
                    if( key.Length == 0 || value.Length == 0 ) {// either attribute name or value is empty
                        return false;
                    }

                    if( key == TypeAttributeName ) {
                        if( effectTypeString.Length > 0 ) {//More than one type attributes in effect description
                            return false;
                        }
                        effectTypeString = value;
                    } else if( key == NameAttributeName ) {
                        effectName = value;
                    } else if ( !isClosingTag ) {
                        attributes.Add( key, value );
                    }

                }
            }

            if( !isClosingTag ) {
                startEffectDescription( effectDescription, effectTypeString, effectName, attributes, position );
            } else { 
                finishEffectDescription( effectDescription, effectTypeString, effectName, position );
            }

            return true;
        }

        // Create entry about effect.
        // return true if description is correct
        private bool startEffectDescription( string effectDescription, string effectTypeString, string effectName, 
            Dictionary<string, string> attributes, int position ) 
        {
            TextEffectParams newParams = new TextEffectParams();

            if( !TextEffectTables.StringToEffectType.ContainsKey( effectTypeString ) ) {//Non-existing type of editing
                return false;
            }
            newParams.EffectType = TextEffectTables.StringToEffectType[effectTypeString];
            newParams.Range = new TextRange( position, position );// Пока заполняем так, затем изменим, когда найдётся закрывающий тег
            newParams.Attributes = attributes;

            // Create name for description in order to correctly find closing tag
            string key = effectName.Length == 0 ? effectDescription : "name=" + effectName;
            if( _formingEffects.ContainsKey( key ) ) { // Have two tags with the same name before their closing tags. This leads to ambiguity. 
                return false;
            }
            _formingEffects.Add( key, newParams );
            if( !_openedEffectsPerTypes.ContainsKey( effectTypeString ) ) {
                _openedEffectsPerTypes.Add( effectTypeString, new Stack<string>() );
            }
            _openedEffectsPerTypes[effectTypeString].Push( key );
            return true;
        }

        // Close effect entry: define right border of effect text range
        // return true if description is correct and opened entry was found with it
        private bool finishEffectDescription( string effectDescription, string effectTypeString, string effectName, int position ) 
        {
            // Find opened entry of effect appropriate to the description
            TextEffectParams closedParams = null;
            string closedParamsKey = "";
            if( effectName.Length != 0 ) {
                closedParamsKey = "name=" + effectName;
                if( !_formingEffects.ContainsKey( closedParamsKey ) ){// Closing tag has name, which does't fit to any opening tag 
                    return false;
                }
                closedParams = _formingEffects[closedParamsKey];
            } else if( _formingEffects.ContainsKey( effectDescription ) ) {
                closedParamsKey = effectDescription;
                closedParams = _formingEffects[effectDescription];
            } else if ( effectTypeString.Length != 0 ) {
                // Указали тип редактирования. Попробуем закрыть последнюю запись с соответствующим типом
                if( _openedEffectsPerTypes.ContainsKey( effectTypeString ) ) {
                    while( _openedEffectsPerTypes[effectTypeString].Count > 0 ) {
                        string formingParamKey = _openedEffectsPerTypes[effectTypeString].Pop();
                        // _formingEffects may not have a key, it's allowed: 
                        // entry may be deleted from _formingEffects erlier
                        if( _formingEffects.ContainsKey( formingParamKey ) ) {
                            closedParamsKey = formingParamKey;
                            closedParams = _formingEffects[formingParamKey];
                        }
                    }
                }
            }

            if( closedParams == null || closedParamsKey.Length == 0 ) {// Some closing effect tag doesn't have appropriate opening tag. Parser can't define, what to close. 
                return false;
            }

            // Finish entry
            closedParams.Range = new TextRange( closedParams.Range.From, position );
            _foundEffects.Add( closedParams );
            _formingEffects.Remove( closedParamsKey );
            return true;
        }

        private const char TagOpenBracket = '<';
        private const char TagCloseBracket = '>';
        private const char CloseTagSymbol = '/';
        private const char EffectsSeparator = ';';
        private const char AttributesSeparator = ',';
        private const char KeyValueSeparator = '=';

        private const string TypeAttributeName = "type";
        private const string NameAttributeName = "name";

        private string _textWithoutEffects;// Text without text effects tags.
        private string _textWithoutTags;// Text without tags. Effect ranges are appliable to this text
        private List<TextEffectParams> _foundEffects;// Parsed effects parameters
        private Dictionary<string, TextEffectParams> _formingEffects;// Effects with yet unknown range
        // Service variable for correct closing of opened effect tags.
        // Key is effect type, value - stack of keys to _formingEffects.
        private Dictionary<string, Stack<string>> _openedEffectsPerTypes;

        private bool haveErrors;
        
    }
}