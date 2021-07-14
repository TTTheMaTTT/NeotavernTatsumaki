using UnityEngine;
using UnityEditor;
using System;

namespace PixelCrushers.DialogueSystem
{
    [CustomFieldTypeService.Name( "Actor Arrangement Action Type" )]
    public class CustomFieldType_ActorArrangementActionType : CustomFieldType
    {
        public override string Draw( string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentArrangementActionType( currentValue );
            return EditorGUILayout.EnumPopup( enumValue ).ToString();
        }

        public override string Draw( Rect rect, string currentValue, DialogueDatabase dataBase )
        {
            var enumValue = GetCurrentArrangementActionType( currentValue );
            return EditorGUI.EnumPopup( rect, enumValue ).ToString();
        }

        private ActorArrangementActionType GetCurrentArrangementActionType( string currentValue )
        {
            if( string.IsNullOrEmpty( currentValue ) )
                currentValue = ActorArrangementActionType.ChangePosition.ToString();
            try {
                return (ActorArrangementActionType)Enum.Parse( typeof( ActorArrangementActionType ), currentValue, true );
            } catch( Exception ) {
                return ActorArrangementActionType.ChangePosition;
            }
        }
    }
}