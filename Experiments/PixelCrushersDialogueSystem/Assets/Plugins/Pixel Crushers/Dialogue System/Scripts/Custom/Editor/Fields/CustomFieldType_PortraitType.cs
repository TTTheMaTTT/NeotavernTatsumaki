using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{
    [CustomFieldTypeService.Name( "Portrait Type" )]
    public class CustomFieldType_PortraitType : CustomFieldType
    {
        public override string Draw( string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentPortraitType( currentValue );
            return EditorGUILayout.EnumPopup( enumValue ).ToString();
        }

        public override string Draw( Rect rect, string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentPortraitType( currentValue );
            return EditorGUI.EnumPopup( rect, enumValue ).ToString();
        }

        private PortraitType GetCurrentPortraitType( string currentValue )
        {
            if( string.IsNullOrEmpty( currentValue ) )
                currentValue = PortraitType.Sprite.ToString();
            try {
                return (PortraitType)Enum.Parse( typeof( PortraitType ), currentValue, true );
            } catch( Exception ) {
                return PortraitType.Sprite;
            }
        }
    }
}
