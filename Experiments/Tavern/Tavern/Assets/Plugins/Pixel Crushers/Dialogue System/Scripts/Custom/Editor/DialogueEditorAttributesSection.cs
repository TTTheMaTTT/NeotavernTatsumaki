using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;


namespace PixelCrushers.DialogueSystem.DialogueEditor
{

    /// <summary>
    /// This part of the Dialogue Editor window handles the Attributes tab.
    /// Attributes are strings, that are assigned to different objects in order to classify them. This objects can be, for example, actors portraits or quests.
    /// </summary>
    public partial class DialogueEditorWindow
    {

        // Current attribute name filter.
        [SerializeField]
        private string attributeFilter = string.Empty;

        // Track list of conflicted attribute names (two or more variables share same name).
        private const double AttributeCheckFrequency = 0.5f;
        private double lastTimeAttributesChecked = 0;
        private HashSet<string> conflictedAttributes = new HashSet<string>();

        private List<AttributeWrapper> attributeWrappers = null;
        public ReorderableList reorderableList = null;
        public int currentAttributeIndex = -1;


        private void DrawAttributesSection()
        {

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField( "Attributes", EditorStyles.boldLabel );
            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            attributeFilter = EditorGUILayout.TextField( GUIContent.none, attributeFilter, "ToolbarSeachTextField" );
            GUILayout.Label( string.Empty, "ToolbarSeachCancelButtonEmpty" );
            if( EditorGUI.EndChangeCheck() ) {
                Refresh();
            }

            DrawAttributesMenu();

            EditorGUILayout.EndHorizontal();

            if( database.syncInfo.syncAttributes ) {
                DrawAttributesSyncDatabase();
            }
            DrawAttributes();
        }


        private void DrawAttributesMenu()
        {
            if( GUILayout.Button( "Menu", "MiniPullDown", GUILayout.Width( 56 ) ) ) {
                GenericMenu menu = new GenericMenu();
                menu.AddItem( new GUIContent( "New Attribute" ), false, CreateNewAttributeAfterCurrent );
                menu.AddItem( new GUIContent( "Sort" ), false, SortAttributesByName );
                menu.AddItem( new GUIContent( "Sync From DB" ), database.syncInfo.syncVariables, ToggleSyncAttributesFromDB );
                menu.ShowAsContext();
            }
        }


        private void CreateNewAttributeAfterCurrent()
        {
            if( reorderableList == null || reorderableList.count == 0 ) {
                CreateNewAttribute();
            } else {
                var attributeWrapper = reorderableList.list[reorderableList.index] as AttributeWrapper;
                var newAttribute = "New Attribute" + database.attributes.Count;
                database.attributes.Insert( attributeWrapper.attributeIndex + 1, newAttribute );
                EditorUtility.SetDirty( database );
                Refresh();
            }
        }


        private void CreateNewAttribute()
        {
            var newAttribute = "New Attribute" + database.attributes.Count;
            database.attributes.Add( newAttribute );
            EditorUtility.SetDirty( database );
            Refresh();
        }


        private void SortAttributesByName()
        {
            database.attributes.Sort( ( x, y ) => x.CompareTo( y ) );
            if( database != null )
                EditorUtility.SetDirty( database );
        }


        private void ToggleSyncAttributesFromDB()
        {
            database.syncInfo.syncAttributes = !database.syncInfo.syncAttributes;
            EditorUtility.SetDirty( database );
        }


        private void DrawAttributesSyncDatabase()
        {
            EditorGUILayout.BeginHorizontal();
            DialogueDatabase newDatabase = EditorGUILayout.ObjectField( new GUIContent( "Sync From", "Database to sync attributes from." ),
                                                                       database.syncInfo.syncAttributesDatabase, typeof( DialogueDatabase ), false ) as DialogueDatabase;
            if( newDatabase != database.syncInfo.syncAttributesDatabase ) {
                database.syncInfo.syncAttributesDatabase = newDatabase;
                database.SyncVariables();
                if( database != null )
                    EditorUtility.SetDirty( database );
            }
            if( GUILayout.Button( new GUIContent( "Sync Now", "Syncs from the database." ), EditorStyles.miniButton, GUILayout.Width( 72 ) ) ) {
                database.SyncAttributes();
                if( database != null )
                    EditorUtility.SetDirty( database );
            }
            EditorGUILayout.EndHorizontal();
        }


        private void DrawAttributes()
        {
            if( EditorApplication.timeSinceStartup - lastTimeAttributesChecked >= AttributeCheckFrequency ) {
                lastTimeAttributesChecked = EditorApplication.timeSinceStartup;
                CheckAttributesForConflicts();
            }
            bool draggable = string.IsNullOrEmpty( attributeFilter );
            if( attributeWrappers == null ) {
                attributeWrappers = new List<AttributeWrapper>();
                string lowerAttributeFilter = attributeFilter.ToLower();
                for( int i = 0; i < database.attributes.Count(); i++ ) {
                    if( database.attributes[i].ToLower().Contains( lowerAttributeFilter ) ) {
                        attributeWrappers.Add( new AttributeWrapper( database.attributes, i ) );
                    }
                }
                reorderableList = new ReorderableList( attributeWrappers, typeof( AttributeWrapper ), draggable, false, true, true );
                reorderableList.drawHeaderCallback = OnDrawAttributeHeader;
                reorderableList.drawElementCallback = OnDrawAttributeElement;
                reorderableList.onAddDropdownCallback = OnAddAttributeDropdown;
                reorderableList.onRemoveCallback = OnRemoveAttribute;
                reorderableList.onReorderCallbackWithDetails = OnReorderAttributes;
            }
            reorderableList.draggable = draggable;
            EditorWindowTools.StartIndentedSection();
            reorderableList.DoLayoutList();
            EditorWindowTools.EndIndentedSection();
        }


        private void CheckAttributesForConflicts()
        {
            if( database == null )
                return;
            conflictedAttributes.Clear();
            var attributesSet = new HashSet<string>();
            foreach( string attribute in database.attributes ) {
                if( attributesSet.Contains( attribute ) )
                    conflictedAttributes.Add( attribute );
                attributesSet.Add( attribute );
            }
        }


        private void OnDrawAttributeHeader( Rect rect )
        {
            var handleWidth = 16f;
            var wholeWidth = rect.width - 6f - handleWidth;
            rect.x += handleWidth;
            EditorGUI.LabelField( new Rect( rect.x, rect.y, wholeWidth, EditorGUIUtility.singleLineHeight ), "Name" );
        }


        private void OnDrawAttributeElement( Rect rect, int index, bool isActive, bool isFocused )
        {
            if( !(reorderableList != null && 0 <= index && index < reorderableList.count) )
                return;
            var attributeWrapper = reorderableList.list[index] as AttributeWrapper;
            if( attributeWrapper == null )
                return;
            var wholeWidth = rect.width - 6f;
            var originalColor = GUI.backgroundColor;
            var conflicted = conflictedAttributes.Contains( attributeWrapper.attribute );
            if( conflicted )
                GUI.backgroundColor = Color.red;
            EditorGUI.BeginChangeCheck();

            string newAttribute = EditorGUI.TextField( new Rect( rect.x, rect.y + 2, wholeWidth, EditorGUIUtility.singleLineHeight ), attributeWrapper.attribute );
            if( EditorGUI.EndChangeCheck() ) {
                attributeWrapper.attribute = newAttribute;
                Refresh();
                EditorUtility.SetDirty( database );
            }
            if( conflicted )
                GUI.backgroundColor = originalColor;
        }


        private void OnRemoveAttribute( ReorderableList list )
        {
            if( !(reorderableList != null && 0 <= list.index && list.index < reorderableList.count) )
                return;
            var attributeWrapper = reorderableList.list[list.index] as AttributeWrapper;
            if( attributeWrapper == null )
                return;
            if( EditorUtility.DisplayDialog( string.Format( "Delete '{0}'?", attributeWrapper.attribute ), "Are you sure you want to delete this?", "Delete", "Cancel" ) ) {
                attributeWrapper.OnDelete();
                Refresh();
                EditorUtility.SetDirty( database );          
            }
        }


        private void OnAddAttributeDropdown( Rect buttonRect, ReorderableList list )
        {
            CreateNewAttribute();
        }


        private void OnReorderAttributes( ReorderableList list, int oldIndex, int newIndex )
        {
            if( !string.IsNullOrEmpty( attributeFilter ) || database.attributes.Count != list.count ) {
                return;
            }

            string movedAttribute = database.attributes[oldIndex];
            database.attributes.RemoveAt( oldIndex );
            database.attributes.Insert( newIndex, movedAttribute );
            Refresh();
        }


        private void Refresh()
        {
            foreach( var attributeWrapper in attributeWrappers ) {
                attributeWrapper.Disable();
            }
            attributeWrappers = null;
            reorderableList = null;
            currentAttributeIndex = -1;
        }


        private class AttributeWrapper
        {
            private int _attributeIndex;
            public int attributeIndex { get { return _attributeIndex; } }
            private List<string> attributesList = null;
            private bool hasAttribute;

            public string attribute { get { return attributesList[_attributeIndex]; } set { attributesList[_attributeIndex] = value; } }

            public AttributeWrapper( List<string> attributesList, int attributeIndex )
            {
                this.attributesList = attributesList;
                this._attributeIndex = attributeIndex;
                this.hasAttribute = true;
            }


            public void OnDelete()
            {
                if( !hasAttribute ) {
                    return;
                }
                attributesList.RemoveAt( _attributeIndex );
                Disable();
            }


            public void Disable()
            {
                hasAttribute = false;
            }
        }
    }

}