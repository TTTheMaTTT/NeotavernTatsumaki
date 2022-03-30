using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CustomEditor( typeof( RoomButton ), true )]
public class RoomButtonEditor : Editor
{
    private const string roomIdPropertyName = "_roomId";
    private const string roomPropertyName = "room";
    private SerializedProperty roomIdProperty;

    private const string currentRoomColorPropertyName = "currentRoomColor";
    private const string visitedRoomColorPropertyName = "visitedRoomColor";
    private const string unvisitedRoomColorPropertyName = "unvisitedRoomColor";
    private SerializedProperty currentRoomColorProperty;
    private SerializedProperty visitedRoomColorProperty;
    private SerializedProperty unvisitedRoomColorProperty;

    private string[] roomNames = null;
    private int roomIndex;

    private void OnEnable()
    {

        GameControllerTavernOutside gameController = FindObjectOfType<GameControllerTavernOutside>();
        var roomNamesList = gameController?.GetRoomNames();
        roomNames = roomNamesList.ToArray();

        roomIdProperty = serializedObject.FindProperty( roomIdPropertyName );
        roomIndex = roomNames != null ? roomNamesList.FindIndex( x => Room.GetIdFromRoomFullName( x ) == roomIdProperty.intValue ) : -1;

        currentRoomColorProperty = serializedObject.FindProperty( currentRoomColorPropertyName );
        visitedRoomColorProperty = serializedObject.FindProperty( visitedRoomColorPropertyName );
        unvisitedRoomColorProperty = serializedObject.FindProperty( unvisitedRoomColorPropertyName );
    }

    /// <summary>
    /// Draws the inspector GUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
#if(UNITY_EDITOR)
        if( roomNames == null ) {
            EditorGUILayout.PropertyField( roomIdProperty );
        } else {
            int newRoomIndex = EditorGUILayout.Popup( roomPropertyName, roomIndex, roomNames );
            if( newRoomIndex != roomIndex ) {
                roomIndex = newRoomIndex;
                roomIdProperty.intValue = Room.GetIdFromRoomFullName( roomNames[roomIndex] );
            }
        }

        EditorGUILayout.PropertyField( currentRoomColorProperty );
        EditorGUILayout.PropertyField( visitedRoomColorProperty );
        EditorGUILayout.PropertyField( unvisitedRoomColorProperty );

        serializedObject.ApplyModifiedProperties();
#endif
    }
}