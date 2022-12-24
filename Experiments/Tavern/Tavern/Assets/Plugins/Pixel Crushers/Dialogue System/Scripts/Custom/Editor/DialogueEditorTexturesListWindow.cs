using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    // Owner of modal texturesListWondow. Used to invoke an action when window is closed.
    public class TexturesListWindowOwner : IModalWindowOwner
    {
        Action<int> _actionOnOk;

        public TexturesListWindowOwner( Action<int> actionOnOk )
        {
            _actionOnOk = actionOnOk;
        }

        // Change portrait index after portrait was chosen
        public void ModalClosed( ModalWindow window )
        {
            TexturesListWindow texturesListWindow = window as TexturesListWindow;
            if( texturesListWindow == null ) {
                return;
            }
            if( window.Result == ModalWindowResult.Ok ) {
                _actionOnOk.Invoke( texturesListWindow.ChosenIndex );
            }
        }
    }

    // Window which shows you a list of textures to choose from
    public class TexturesListWindow : ModalWindow
    {
        const int Width = 400;
        const int Height = 400;
        Vector2 scrollPos;
        const int TextureSize = 128;
        const int TexturesInRow = 3;

        public int ChosenIndex { get; set; } = -1;
        private List<Texture> _textures;
        public List<Texture> Textures
        {
            set 
            {
                _textures = value;
            }
        }

        public static void ShowWindow( List<Texture> textures, Vector2 position, Action<int> actionOnOk )
        {
            TexturesListWindowOwner windowOwner = new TexturesListWindowOwner( actionOnOk );
            TexturesListWindow window = ScriptableObject.CreateInstance( typeof( TexturesListWindow ) ) as TexturesListWindow;
            window.Textures = textures;
            window.owner = windowOwner;
            window.ShowModalUtility();

            float halfWidth = Width / 2;
            float x = position.x - halfWidth;
            float y = position.y;
            Rect rect = new Rect( x, y, 0, 0 );
            window.position = rect;
            window.ShowAsDropDown( rect, new Vector2( Width, Height ) );

        }

        void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Width( Width ), GUILayout.Height( Height ) );
            for( int i = 0; i < _textures.Count; i++ ) {
                if( i % TexturesInRow == 0 ) {
                    GUILayout.BeginHorizontal();
                }

                if( GUILayout.Button( new GUIContent( _textures[i] ), GUILayout.Height( TextureSize ), GUILayout.Width( TextureSize ) ) ) {
                    ChosenIndex = i;
                    result = ModalWindowResult.Ok;
                    Ok();
                }

                if( i % TexturesInRow == TexturesInRow - 1 ) {
                    GUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();
        }

    }
}