using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

using Databox;
using Databox.Ed;

namespace Dialogue
{
    [System.Serializable]
    [DataboxTypeAttribute( Name = "Dialogue" )]
    public class CDialogue : DataboxType
    {
        [SerializeField]
        public List<CDialogueStatement> Statements;

        public override void DrawEditor()
        {
            using( new GUILayout.VerticalScope() ) {

                if( Statements == null ) {
                    Statements = new List<CDialogueStatement>();
                }

                GUILayout.Label( "Dialogue Statements: " );
                using( new GUILayout.HorizontalScope() ) {
                    GUILayout.Label( "Count" );
                    int newCount = EditorGUILayout.DelayedIntField( Statements.Count );
                    if( newCount != Statements.Count && newCount >= 0 ) {
                        while( Statements.Count < newCount ) {
                            Statements.Add( new CDialogueStatement() );
                        }
                        while( Statements.Count > newCount ) {
                            Statements.RemoveAt( Statements.Count - 1 );
                        }
                    }
                }
                for( int i = 0; i < Statements.Count; i++ ) {
                    DrawEditorForStatement( i );
                }
            }
        }

        private void DrawEditorForStatement( int index )
        {
            GUIStyle style = new GUIStyle();
            style.normal.background = new Texture2D(1,1);
            Color prevColor = GUI.backgroundColor;
            int tintPart = index % 2;
            GUI.backgroundColor = (8 * prevColor + tintPart * Color.blue) / ( 8 + tintPart );
            using( new GUILayout.VerticalScope( style ) ) {
                GUILayout.Label( "Element" + index.ToString() + ":" );
                using( new GUILayout.HorizontalScope() ) {
                    GUILayout.Label( "Name:" );
                    Statements[index].Name = EditorGUILayout.TextField( Statements[index].Name );
                }
                using( new GUILayout.HorizontalScope() ) {
                    GUILayout.Label( "ActorName:" );
                    Statements[index].ActorName = EditorGUILayout.TextField( Statements[index].ActorName );
                }
            }
            GUI.backgroundColor = prevColor;
        }

    }
}