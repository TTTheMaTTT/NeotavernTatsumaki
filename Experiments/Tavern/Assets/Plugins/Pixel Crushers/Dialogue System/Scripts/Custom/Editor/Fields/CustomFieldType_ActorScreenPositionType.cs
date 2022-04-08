using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{
    [CustomFieldTypeService.Name( "Actor Position Type" )]
    public class CustomFieldType_ActorPositionType : CustomFieldType
    {
        public override string Draw( string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentPositionType( currentValue );
            return EditorGUILayout.EnumPopup( enumValue ).ToString();
        }

        public override string Draw( Rect rect, string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentPositionType( currentValue );
            return EditorGUI.EnumPopup( rect, enumValue ).ToString();
        }

        private ActorScreenPositionType GetCurrentPositionType( string currentValue )
        {
            if( string.IsNullOrEmpty( currentValue ) )
                currentValue = ActorScreenPositionType.Center.ToString();
            try {
                return (ActorScreenPositionType)Enum.Parse( typeof( ActorScreenPositionType ), currentValue, true );
            } catch( Exception ) {
                return ActorScreenPositionType.Center;
            }
        }

    }
}