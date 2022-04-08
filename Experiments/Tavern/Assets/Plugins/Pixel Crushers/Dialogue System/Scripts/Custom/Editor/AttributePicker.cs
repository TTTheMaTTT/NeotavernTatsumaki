using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PixelCrushers.DialogueSystem
{
    public class AttributePicker
    {
        public DialogueDatabase database = null;
        public string currentAttribute = string.Empty;
        public bool usePicker = false;

        private List<string> attributes = null;
        private int currentIndex = -1;

        public AttributePicker( DialogueDatabase database, string currentAttribute, bool usePicker )
        {
            this.database = database;
            this.currentAttribute = currentAttribute;
            this.usePicker = usePicker;
            UpdateTitles();
            bool currentAttributeIsInDatabase = (database != null) || (currentIndex >= 0);
            /*
            if( usePicker && !string.IsNullOrEmpty( currentAttribute ) && !currentConversationIsInDatabase ) {
                this.usePicker = false;
            }
            */
        }


        public void UpdateTitles()
        {
            /*
            currentIndex = -1;
            if( database == null || database.attributes == null ) {
                attributes = new List<string>();
            } else {
                attributes = database.attributes;
                currentIndex = attributes.FindIndex( x => x == currentAttribute );
                for( int i = 0; i < database.conversations.Count; i++ ) {
                    titles[i] = database.conversations[i].Title;
                    if( string.Equals( currentConversation, titles[i] ) ) {
                        currentIndex = i;
                    }
                }
            }
            */
        }

    }
}