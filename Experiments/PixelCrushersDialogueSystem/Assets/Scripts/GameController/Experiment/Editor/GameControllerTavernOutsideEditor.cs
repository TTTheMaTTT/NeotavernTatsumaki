using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using PixelCrushers.DialogueSystem;

[CustomEditor( typeof( GameControllerTavernOutside ), true )]
public class GameControllerTavernOutsideEditor : Editor
{

    private SerializedObject prevSerializedObject = null;
    private DialogueDatabase _dialogueDatabase;
    private List<string> _locationNames = new List<string>();
    private List<string> _conversationNames = new List<string>();

    private int maxRoomId = -1;
    private int maxFloorId = -1;
    private Dictionary<int, string> floorIdToFloorName = new Dictionary<int, string>();
    private Dictionary<int, string> roomIdToRoomName = new Dictionary<int, string>();
    private Dictionary<int, int> roomIdToFloorId = new Dictionary<int, int>();
    private List<string> roomFullNames = new List<string>();
    private Dictionary<int, HashSet<int>> roomIdToNeighborIds = new Dictionary<int, HashSet<int>>();

    private Dictionary<int, bool> floorIdToFoldout = new Dictionary<int, bool>();
    private Dictionary<int, int> roomIdToLocationIndex = new Dictionary<int, int>();
    private Dictionary<int, Dictionary<string, int>> roomIdToConversationsIndices = new Dictionary<int, Dictionary<string, int>>();
    private Dictionary<int, List<int>> roomIdToNeighborIndices = new Dictionary<int, List<int>>();

    private SerializedProperty roomLayoutProperty;
    private SerializedProperty floorsProperty;
    private Dictionary<int, SerializedProperty> roomIdToProperty = new Dictionary<int, SerializedProperty>();

    private SerializedProperty startRoomIdProperty;
    private int startRoomIndex;

    private const string roomLayoutPropertyName = "roomLayout";
    private const string floorsPropertyName = "floors";
    private const string roomsPropertyName = "rooms";
    private const string floorIdPropertyName = "floorId";
    private const string floorNamePropertyName = "floorName";
    private const string roomIdPropertyName = "roomId";
    private const string roomNamePropertyName = "roomName";
    private const string locationIdPropertyName = "locationId";
    private const string locationPropertyName = "location";
    private const string conversationsPropertyName = "conversations";
    private const string conversationIdPropertyName = "ConversationId";
    private const string conversationPropertyName = "conversation";
    private const string roomStatePropertyName = "RoomState";
    private const string neighborsPropertyName = "neigbourRooms";
    private const string startRoomIdPropertyName = "startRoomId";
    private const string startRoomPropertyName = "startRoom";


    private void OnEnable()
    {
#if(UNITY_EDITOR)
        CheckDatabase( out bool hasDatabaseChanges );

        if( prevSerializedObject == serializedObject && !hasDatabaseChanges ) {
            return;
        }
        BuildStructures();

#endif
    }


    private void CheckDatabase( out bool hasDatabaseChanges )
    {
        hasDatabaseChanges = false;
        var gameController = target as GameControllerTavernOutside;
        var dialogueSystemControllers = FindObjectsOfType<DialogueSystemController>();
        DialogueSystemController dialogueSystemController = null;

        if( dialogueSystemControllers != null && dialogueSystemControllers.Length == 1 ) {
            dialogueSystemController = dialogueSystemControllers[0];
            _dialogueDatabase = dialogueSystemController?.initialDatabase;
            if( !(_dialogueDatabase is null) ) {
                List<string> newLocationNames = _dialogueDatabase.locations.Select( x => x.Name ).ToList();
                if( !newLocationNames.SequenceEqual( _locationNames ) ) {
                    hasDatabaseChanges = true;
                    _locationNames = newLocationNames;
                }
                List<string> newConversationNames = _dialogueDatabase.conversations.Select( x => x.Title ).ToList();
                if( !newConversationNames.SequenceEqual( _conversationNames ) ) {
                    hasDatabaseChanges = true;
                    _conversationNames = newConversationNames;
                }
            }
        }
    }

    private void Clear()
    {
        maxRoomId = 0;
        maxFloorId = 0;
        floorIdToFloorName.Clear();
        roomIdToProperty.Clear();
        roomIdToRoomName.Clear();
        roomIdToFloorId.Clear();
        roomFullNames.Clear();
        roomIdToLocationIndex.Clear();
        roomIdToConversationsIndices.Clear();
        floorIdToFoldout.Clear();
        roomIdToNeighborIds.Clear();
        roomIdToNeighborIndices.Clear();
        startRoomIndex = -1;
    }



    void BuildStructures()
    {
        Clear();
        prevSerializedObject = serializedObject;
        roomLayoutProperty = serializedObject.FindProperty( roomLayoutPropertyName );
        floorsProperty = roomLayoutProperty.FindPropertyRelative( floorsPropertyName );
        List<int> floorsToDelete = new List<int>();
        for( int i = 0; i < floorsProperty.arraySize; i++ ) {
            var floorProperty = floorsProperty.GetArrayElementAtIndex( i );
            int floorId = floorProperty.FindPropertyRelative( floorIdPropertyName ).intValue;
            if( floorIdToFloorName.ContainsKey( floorId ) ) {
                floorsToDelete.Add( i );
                continue;
            }
            string floorName = floorProperty.FindPropertyRelative( floorNamePropertyName ).stringValue;
            maxFloorId = Mathf.Max( floorId, maxFloorId );
            floorIdToFloorName.Add( floorId, floorName );
            floorIdToFoldout.Add( floorId, false );
            // Rooms
            List<int> roomsToDelete = new List<int>();
            var roomsProperty = floorProperty.FindPropertyRelative( roomsPropertyName );
            for( int j = 0; j < roomsProperty.arraySize; j++ ) {
                var roomProperty = roomsProperty.GetArrayElementAtIndex( j );
                int roomId = roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue;
                if( roomIdToRoomName.ContainsKey( roomId ) ) {
                    roomsToDelete.Add( roomId );
                    continue;
                }
                roomIdToFloorId.Add( roomId, floorId );
                roomProperty.FindPropertyRelative( floorIdPropertyName ).intValue = floorId;
                string roomName = roomProperty.FindPropertyRelative( roomNamePropertyName ).stringValue;
                roomIdToRoomName.Add( roomId, roomName );
                roomFullNames.Add( CreateRoomFullName( roomId ) );
                maxRoomId = Mathf.Max( roomId, maxRoomId );
                roomIdToProperty.Add( roomId, roomProperty );
                // Location
                FillRoomLocationStructures( roomProperty, roomId );
                // Conversations
                FillRoomConversationStructures( roomProperty, roomId );
            }
            for( int j = roomsToDelete.Count - 1; j >= 0; j-- ) {
                roomsProperty.DeleteArrayElementAtIndex( roomsToDelete[j] );
            }
        }
        for( int i = floorsToDelete.Count - 1; i >= 0; i-- ) {
            floorsProperty.DeleteArrayElementAtIndex( floorsToDelete[i] );
        }
        FillRoomNeighborsStructures();

        // Start Room Id
        startRoomIdProperty = serializedObject.FindProperty( startRoomIdPropertyName );
        startRoomIndex = roomFullNames.FindIndex( x => Room.GetIdFromRoomFullName( x ) == startRoomIdProperty.intValue );
    }


    private void FillRoomLocationStructures( SerializedProperty roomProperty, int roomId )
    {
        int locationId = roomProperty.FindPropertyRelative( locationIdPropertyName ).intValue;
        int locationIndex = -1;
        if( !(_dialogueDatabase is null) ) {
            locationIndex = _dialogueDatabase.locations.FindIndex( x => x.id == locationId );
        }
        roomIdToLocationIndex.Add( roomId, locationIndex );
    }


    private void FillRoomConversationStructures( SerializedProperty roomProperty, int roomId )
    {
        roomIdToConversationsIndices.Add( roomId, new Dictionary<string, int>() );
        var stateToConversationIndex = roomIdToConversationsIndices[roomId];
        var conversationsProperty = roomProperty.FindPropertyRelative( conversationsPropertyName );
        for( int k = conversationsProperty.arraySize - 1; k >= 0; k-- ) {
            var converstationWithStateProperty = conversationsProperty.GetArrayElementAtIndex( k );
            int conversationId = converstationWithStateProperty.FindPropertyRelative( conversationIdPropertyName ).intValue;
            string stateName = converstationWithStateProperty.FindPropertyRelative( roomStatePropertyName ).stringValue;
            if( stateToConversationIndex.ContainsKey( stateName ) ) {
                conversationsProperty.DeleteArrayElementAtIndex( k );
                continue;
            }
            int conversationIndex = -1;
            if( !(_dialogueDatabase is null) ) {
                conversationIndex = _dialogueDatabase.conversations.FindIndex( x => x.id == conversationId );
            }
            stateToConversationIndex.Add( stateName, conversationIndex );
        }
    }


    private void FillRoomNeighborsStructures()
    {
        // Устанавливаем связи между комнатами
        for( int i = 0; i < floorsProperty.arraySize; i++ ) {
            var floorProperty = floorsProperty.GetArrayElementAtIndex( i );
            var roomsProperty = floorProperty.FindPropertyRelative( roomsPropertyName );
            for( int j = 0; j < roomsProperty.arraySize; j++ ) {
                var roomProperty = roomsProperty.GetArrayElementAtIndex( j );
                int roomId = roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue;
                if( !roomIdToNeighborIds.ContainsKey( roomId ) ) {
                    roomIdToNeighborIds.Add( roomId, new HashSet<int>() );
                }
                HashSet<int> neighborIdsForRepeatCheck = new HashSet<int>();
                var neighborsProperty = roomProperty.FindPropertyRelative( neighborsPropertyName );
                for( int k = neighborsProperty.arraySize - 1; k >= 0; k-- ) {
                    int neighborId = neighborsProperty.GetArrayElementAtIndex( k ).intValue;
                    if( !roomIdToRoomName.ContainsKey( neighborId ) || neighborIdsForRepeatCheck.Contains( neighborId ) ) {
                        neighborsProperty.DeleteArrayElementAtIndex( k );
                        continue;
                    }
                    neighborIdsForRepeatCheck.Add( neighborId );
                    roomIdToNeighborIds[roomId].Add( neighborId );
                    if( !roomIdToNeighborIds.ContainsKey( neighborId ) ) {
                        roomIdToNeighborIds.Add( neighborId, new HashSet<int>() );
                    }
                    roomIdToNeighborIds[neighborId].Add( roomId );
                }
            }
        }
        // Заполняем, каким индексам в списке всех комнат соответствуют связи.
        for( int i = 0; i < floorsProperty.arraySize; i++ ) {
            var floorProperty = floorsProperty.GetArrayElementAtIndex( i );
            int floorId = floorProperty.FindPropertyRelative( floorIdPropertyName ).intValue;
            var roomsProperty = floorProperty.FindPropertyRelative( roomsPropertyName );
            for( int j = 0; j < roomsProperty.arraySize; j++ ) {
                var roomProperty = roomsProperty.GetArrayElementAtIndex( j );
                int roomId = roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue;
                roomIdToNeighborIndices.Add( roomId, new List<int>() );
                HashSet<int> remainingNeighbors = new HashSet<int>( roomIdToNeighborIds[roomId] );
                var neighborsProperty = roomProperty.FindPropertyRelative( neighborsPropertyName );
                for( int k = 0; k < neighborsProperty.arraySize; k++ ) {
                    int neighborId = neighborsProperty.GetArrayElementAtIndex( k ).intValue;
                    remainingNeighbors.Remove( neighborId );
                    int index = roomFullNames.FindIndex( x => x == CreateRoomFullName( neighborId ) );
                    Assert.IsTrue( index != -1 );
                    roomIdToNeighborIndices[roomId].Add( index );
                }
                // Если по какой-то причине соседи не имели ссылку друг на друга (и имел только один из них), то здесь мы это исправим
                foreach( int neighborId in remainingNeighbors ) {
                    int index = roomFullNames.FindIndex( x => x == CreateRoomFullName( neighborId ) );
                    Assert.IsTrue( index != -1 );
                    roomIdToNeighborIndices[roomId].Add( index );
                    neighborsProperty.InsertArrayElementAtIndex( neighborsProperty.arraySize );
                    neighborsProperty.GetArrayElementAtIndex( neighborsProperty.arraySize - 1 ).intValue = neighborId;
                }
            }
        }
    }


    void ResetRooms()
    {
        roomIdToProperty.Clear();
        for( int i = 0; i < floorsProperty.arraySize; i++ ) {
            var roomsProperty = floorsProperty.GetArrayElementAtIndex( i ).FindPropertyRelative( roomsPropertyName );
            for( int j = 0; j < roomsProperty.arraySize; j++ ) {
                var roomProperty = roomsProperty.GetArrayElementAtIndex( j );
                roomIdToProperty.Add( roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue, roomProperty );
            }
        }

        Dictionary<int, int> roomIdToNameIndex = new Dictionary<int, int>();
        roomFullNames = new List<string>();
        int index = 0;
        foreach( int roomId in roomIdToRoomName.Keys ) {
            roomFullNames.Add( CreateRoomFullName( roomId ) );
            roomIdToNameIndex.Add( roomId, index );
            index++;
        }
        foreach( var roomIdWithNeighborIndices in roomIdToNeighborIndices ) {
            int roomId = roomIdWithNeighborIndices.Key;
            var neighborsProperty = roomIdToProperty[roomId].FindPropertyRelative( neighborsPropertyName );
            if( neighborsProperty is null ) {
                Assert.IsTrue( roomIdWithNeighborIndices.Value.Count == 0 );
                continue;
            }
            Assert.IsTrue( roomIdWithNeighborIndices.Value.Count == neighborsProperty.arraySize );
            for( int i = 0; i < roomIdWithNeighborIndices.Value.Count; i++ ) {
                roomIdWithNeighborIndices.Value[i] = roomIdToNameIndex[neighborsProperty.GetArrayElementAtIndex( i ).intValue];
            }
        }
    }


    /// <summary>
    /// Draws the inspector GUI.
    /// </summary>
    public override void OnInspectorGUI()
    {
#if(UNITY_EDITOR)
        EditorGUILayout.LabelField( "Room Layout", EditorStyles.boldLabel );

        int indexToRemove = -1;
        for( int i = 0; i < floorsProperty.arraySize; i++ ) {
            // Отображение информации о каждом этаже
            EditorWindowTools.EditorGUILayoutBeginGroup();
            DrawFloor( i, out bool shouldRemove );
            if( shouldRemove ) {
                indexToRemove = i;
            }
            EditorWindowTools.EditorGUILayoutEndGroup();
        }
        if( indexToRemove != -1 ) {
            // Удаление этажа
            RemoveFloor( indexToRemove );
        }

        // Добавление нового этажа
        if( GUILayout.Button( "Add new floor" ) ) {
            AddFloor();
        }

        // Начальная комната
        int newRoomIndex = EditorGUILayout.Popup( startRoomPropertyName, startRoomIndex, roomFullNames.ToArray() );
        if( newRoomIndex != startRoomIndex ) {
            startRoomIndex = newRoomIndex;
            startRoomIdProperty.intValue = Room.GetIdFromRoomFullName( roomFullNames[startRoomIndex] );
        }

        serializedObject.ApplyModifiedProperties();
#endif
    }


    // Отображение информации об этаже
    private void DrawFloor( int floorIndex, out bool shouldRemoveFloor )
    {
        Assert.IsTrue( floorIndex >= 0 && floorIndex < floorsProperty.arraySize );
        shouldRemoveFloor = false;
        var floorProperty = floorsProperty.GetArrayElementAtIndex( floorIndex );
        var floorNameProperty = floorProperty.FindPropertyRelative( floorNamePropertyName );

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Floor " + floorNameProperty.stringValue, EditorStyles.boldLabel );
        if( GUILayout.Button( new GUIContent( " ", "Remove floor." ), "OL Minus", GUILayout.Width( 16 ) ) ) {
            shouldRemoveFloor = true;
        }
        EditorGUILayout.EndHorizontal();

        int floorId = floorProperty.FindPropertyRelative( floorIdPropertyName ).intValue;
        floorIdToFoldout[floorId] = EditorGUILayout.Foldout( floorIdToFoldout[floorId], "Floor Properties" );
        if( floorIdToFoldout[floorId] ) {
            string oldName = floorNameProperty.stringValue;
            string newName = EditorGUILayout.DelayedTextField( floorNamePropertyName, oldName );
            if( !string.Equals( oldName, newName ) ) {
                floorNameProperty.stringValue = newName;
                floorIdToFloorName[floorId] = newName;
                // Необходимо переименовать все записи в roomFullNames
                for( int i = 0; i < roomFullNames.Count; i++ ) {
                    int roomId = GetRoomIdFromName( roomFullNames[i] );
                    if( roomIdToFloorId[roomId] == floorId ) {
                        roomFullNames[i] = CreateRoomFullName( roomId );
                    }
                }
            }

            var roomsProperty = floorProperty.FindPropertyRelative( roomsPropertyName );
            int indexToRemove = -1;
            for( int i = 0; i < roomsProperty.arraySize; i++ ) {
                // Отображение информации о каждой комнате на этаже
                EditorWindowTools.EditorGUILayoutBeginGroup();
                DrawRoom( roomsProperty, i, out bool shouldRemoveRoom );
                if( shouldRemoveRoom ) {
                    indexToRemove = i;
                }
                EditorWindowTools.EditorGUILayoutEndGroup();
            }

            if( indexToRemove != -1 ) {
                // Удаление комнаты из этажа
                RemoveRoom( roomsProperty, indexToRemove, false );
            }

            // Добавление новой комнаты
            if( GUILayout.Button( "Add new room" ) ) {
                AddRoom( roomsProperty, floorId );
            }
        }

    }


    // Добавление этажа
    private void AddFloor()
    {
        Assert.IsTrue( maxFloorId + 1 > maxFloorId );
        floorsProperty.InsertArrayElementAtIndex( floorsProperty.arraySize );
        var newFloorProperty = floorsProperty.GetArrayElementAtIndex( floorsProperty.arraySize - 1 );

        maxFloorId++;
        int floorId = maxFloorId;
        string floorName = $"F{floorId}";
        newFloorProperty.FindPropertyRelative( floorIdPropertyName ).intValue = floorId;
        newFloorProperty.FindPropertyRelative( floorNamePropertyName ).stringValue = floorName;
        newFloorProperty.FindPropertyRelative( roomsPropertyName ).arraySize = 0;
        floorIdToFloorName.Add( floorId, floorName );
        floorIdToFoldout.Add( floorId, false );
    }


    // Удаление этажа
    private void RemoveFloor( int floorIndex )
    {
        Assert.IsTrue( floorIndex >= 0 && floorIndex < floorsProperty.arraySize );
        var floorProperty = floorsProperty.GetArrayElementAtIndex( floorIndex );

        // Сначала удалим все комнаты из этажа
        var roomsProperty = floorProperty.FindPropertyRelative( roomsPropertyName );
        for( int i = roomsProperty.arraySize - 1; i >= 0; i-- ) {
            RemoveRoom( roomsProperty, i, true );
        }

        int floorId = floorProperty.FindPropertyRelative( floorIdPropertyName ).intValue;
        // Чтобы избежать квадратичности, удалим из roomFullNames записи, соответствующие этажу в этом методе, а не в RemoveRoom
        string floorPrefix = $"{floorIdToFloorName[floorId]}/";
        roomFullNames.RemoveAll( x => x.StartsWith( floorPrefix ) );

        // Удалим сам этаж
        floorIdToFloorName.Remove( floorId );
        floorIdToFoldout.Remove( floorId );
        floorsProperty.DeleteArrayElementAtIndex( floorIndex );

        ResetRooms();
    }


    // Отрисовка комнаты
    private void DrawRoom( SerializedProperty roomsProperty, int roomIndex, out bool shouldRemoveRoom )
    {
        Assert.IsTrue( roomIndex >= 0 && roomIndex < roomsProperty.arraySize );
        shouldRemoveRoom = false;
        var roomProperty = roomsProperty.GetArrayElementAtIndex( roomIndex );

        var roomNameProperty = roomProperty.FindPropertyRelative( roomNamePropertyName );

        int roomId = roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Room " + roomNameProperty.stringValue, EditorStyles.boldLabel );
        if( GUILayout.Button( new GUIContent( " ", "Remove room." ), "OL Minus", GUILayout.Width( 16 ) ) ) {
            shouldRemoveRoom = true;
        }
        EditorGUILayout.EndHorizontal();

        string oldName = roomNameProperty.stringValue;
        string newName = EditorGUILayout.DelayedTextField( roomNamePropertyName, oldName );
        if( !string.Equals( oldName, newName ) ) {
            roomNameProperty.stringValue = newName;
            roomIdToRoomName[roomId] = newName;
            int index = roomFullNames.FindIndex( x => GetRoomIdFromName( x ) == roomId );
            Assert.IsTrue( index != -1 );
            roomFullNames[index] = CreateRoomFullName( roomId );
        }

        // Location
        DrawRoomLocation( roomProperty, roomId );

        // Conversations
        DrawRoomConversations( roomProperty, roomId );

        // Neighbors
        DrawRoomNeighbors( roomProperty, roomId );
    }


    private void AddRoom( SerializedProperty roomsProperty, int floorId )
    {
        roomsProperty.InsertArrayElementAtIndex( roomsProperty.arraySize );
        var roomProperty = roomsProperty.GetArrayElementAtIndex( roomsProperty.arraySize - 1 );
        maxRoomId++;
        int roomId = maxRoomId;
        string roomName = $"R{roomId}";
        roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue = roomId;
        roomProperty.FindPropertyRelative( roomNamePropertyName ).stringValue = roomName;
        roomProperty.FindPropertyRelative( floorIdPropertyName ).intValue = floorId;
        roomProperty.FindPropertyRelative( locationIdPropertyName ).intValue = 0;
        roomProperty.FindPropertyRelative( conversationsPropertyName ).arraySize = 0;
        roomProperty.FindPropertyRelative( neighborsPropertyName ).arraySize = 0;

        roomIdToRoomName.Add( roomId, roomName );
        roomIdToFloorId.Add( roomId, floorId );
        roomIdToLocationIndex.Add( roomId, -1 );
        roomIdToConversationsIndices.Add( roomId, new Dictionary<string, int>() );
        roomIdToNeighborIds.Add( roomId, new HashSet<int>() );
        roomIdToNeighborIndices.Add( roomId, new List<int>() );
        roomFullNames.Add( CreateRoomFullName( roomId ) );
        roomIdToProperty.Add( roomId, roomProperty );
    }


    private void RemoveRoom( SerializedProperty roomsProperty, int index, bool isRemovingFloor = false )
    {
        Assert.IsTrue( index >= 0 && index < roomsProperty.arraySize );
        var roomProperty = roomsProperty.GetArrayElementAtIndex( index );

        int roomId = roomProperty.FindPropertyRelative( roomIdPropertyName ).intValue;

        HashSet<int> neighborIds = new HashSet<int>( roomIdToNeighborIds[roomId] );
        // Удалим соседей из комнаты и комнату из соседей
        foreach( int neighborId in neighborIds ) {
            RemoveNeighborFromRoom( roomId, neighborId );
            RemoveNeighborFromRoom( neighborId, roomId );
        }

        // если выставлен isRemovingFloor в true, то потом уберём комнату из roomFullNames
        if( !isRemovingFloor ) {
            roomFullNames.Remove( CreateRoomFullName( roomId ) );
        }

        roomIdToRoomName.Remove( roomId );
        roomIdToFloorId.Remove( roomId );
        roomIdToLocationIndex.Remove( roomId );
        roomIdToConversationsIndices.Remove( roomId );
        roomIdToNeighborIds.Remove( roomId );
        roomIdToNeighborIndices.Remove( roomId );
        roomIdToProperty.Remove( roomId );

        roomsProperty.DeleteArrayElementAtIndex( index );

        if( !isRemovingFloor ) {
            ResetRooms();
        }
    }


    private void DrawRoomLocation( SerializedProperty roomProperty, int roomId )
    {
        if( _dialogueDatabase is null ) {
            EditorGUI.BeginDisabledGroup( true );
            EditorGUILayout.PropertyField( roomProperty.FindPropertyRelative( locationIdPropertyName ) );
            EditorGUI.EndDisabledGroup();
        } else {
            int newIndex = EditorGUILayout.Popup( locationPropertyName, roomIdToLocationIndex[roomId], _locationNames.ToArray() );
            if( newIndex != roomIdToLocationIndex[roomId] ) {
                roomIdToLocationIndex[roomId] = newIndex;
                roomProperty.FindPropertyRelative( locationIdPropertyName ).intValue = _dialogueDatabase.locations[newIndex].id;
            }
        }
    }


    private void DrawRoomConversations( SerializedProperty roomProperty, int roomId )
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Conversations", EditorStyles.boldLabel );
        var conversationsProperty = roomProperty.FindPropertyRelative( conversationsPropertyName );
        if( GUILayout.Button( new GUIContent( " ", "Add conversation with state." ), "OL Plus", GUILayout.Width( 16 ) ) ) {
            AddConversationState( conversationsProperty, roomId );
        }

        EditorGUILayout.EndHorizontal();
        int indexToRemove = -1;
        for( int i = 0; i < conversationsProperty.arraySize; i++ ) {
            var conversationWithStateProperty = conversationsProperty.GetArrayElementAtIndex( i );
            DrawConversationState( conversationWithStateProperty, roomId, out bool shouldRemoveConversationWithState );
            if( shouldRemoveConversationWithState ) {
                indexToRemove = i;
            }
        }
        if( indexToRemove != -1 ) {
            RemoveConversationState( conversationsProperty, roomId, indexToRemove );
        }
    }


    private void AddConversationState( SerializedProperty conversationsProperty, int roomId )
    {
        conversationsProperty.InsertArrayElementAtIndex( conversationsProperty.arraySize );
        var conversationWithStateProperty = conversationsProperty.GetArrayElementAtIndex( conversationsProperty.arraySize - 1 );
        Assert.IsTrue( roomIdToConversationsIndices.ContainsKey( roomId ) );
        var roomStatesToConversations = roomIdToConversationsIndices[roomId];
        string newStateName = "";
        int conversationId = 0;
        int index = 0;
        while( roomStatesToConversations.ContainsKey( newStateName ) ) {
            newStateName = $"state{index}";
            index++;
        }
        conversationWithStateProperty.FindPropertyRelative( roomStatePropertyName ).stringValue = newStateName;
        conversationWithStateProperty.FindPropertyRelative( conversationIdPropertyName ).intValue = conversationId;
        roomIdToConversationsIndices[roomId].Add( newStateName, conversationId );
    }


    private void DrawConversationState( SerializedProperty conversationWithStateProperty, int roomId, out bool shouldRemoveConversationWithState )
    {
        shouldRemoveConversationWithState = false;
        var roomStateProperty = conversationWithStateProperty.FindPropertyRelative( roomStatePropertyName );
        var conversationIdProperty = conversationWithStateProperty.FindPropertyRelative( conversationIdPropertyName );
        EditorGUILayout.BeginHorizontal();

        string oldState = roomStateProperty.stringValue;
        EditorGUILayout.LabelField( roomStatePropertyName, GUILayout.Width( 100 ) );
        string newState = EditorGUILayout.DelayedTextField( oldState );
        if( !string.Equals( newState, oldState ) ) {
            Assert.IsTrue( roomIdToConversationsIndices.ContainsKey( roomId ) );
            Assert.IsTrue( roomIdToConversationsIndices[roomId].ContainsKey( oldState )  );
            var roomStatesToConversations = roomIdToConversationsIndices[roomId];
            int index = 0;
            string searchedStateName = newState;
            while( roomStatesToConversations.ContainsKey( searchedStateName ) ) {
                searchedStateName = $"{newState}{index}";
                index++;
            }
            newState = searchedStateName;
            int oldConversationIndex = roomIdToConversationsIndices[roomId][oldState];
            roomIdToConversationsIndices[roomId].Remove( oldState );
            roomIdToConversationsIndices[roomId].Add( newState, oldConversationIndex );

            roomStateProperty.stringValue = newState;
        }

        int conversationIndex = roomIdToConversationsIndices[roomId][newState];
        EditorGUILayout.LabelField( conversationPropertyName, GUILayout.Width( 100 ) );
        int newIndex = EditorGUILayout.Popup( conversationIndex, _conversationNames.ToArray() );
        if( newIndex != conversationIndex ) {
            roomIdToConversationsIndices[roomId][newState] = newIndex;
            conversationIdProperty.intValue = _dialogueDatabase.conversations[newIndex].id;
        }

        if( GUILayout.Button( new GUIContent( " ", "Remove conversation with state." ), "OL Minus", GUILayout.Width( 16 ) ) ) {
            shouldRemoveConversationWithState = true;
        }

        EditorGUILayout.EndHorizontal();
    }


    private void RemoveConversationState( SerializedProperty conversationsProperty, int roomId, int indexToRemove )
    {
        Assert.IsTrue( indexToRemove >= 0 && indexToRemove < conversationsProperty.arraySize );
        string roomState = conversationsProperty.GetArrayElementAtIndex( indexToRemove ).FindPropertyRelative( roomStatePropertyName ).stringValue;
        if( roomIdToConversationsIndices.ContainsKey( roomId ) ) {
            roomIdToConversationsIndices[roomId].Remove( roomState );
        }
        conversationsProperty.DeleteArrayElementAtIndex( indexToRemove );
    }


    private void DrawRoomNeighbors( SerializedProperty roomProperty, int roomId )
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField( "Neighbors", EditorStyles.boldLabel );
        var neighborsProperty = roomProperty.FindPropertyRelative( neighborsPropertyName );
        if( GUILayout.Button( new GUIContent( " ", "Add neighbor." ), "OL Plus", GUILayout.Width( 16 ) ) ) {
            AddRoomNeighbor( neighborsProperty, roomId );
        }

        EditorGUILayout.EndHorizontal();
        int indexToRemove = -1;
        for( int i = 0; i < neighborsProperty.arraySize; i++ ) {
            var neighborProperty = neighborsProperty.GetArrayElementAtIndex( i );
            DrawRoomNeighbor( neighborProperty, i, roomId, out bool shouldRemoveNeighbor );
            if( shouldRemoveNeighbor ) {
                indexToRemove = i;
            }
        }
        if( indexToRemove != -1 ) {
            RemoveNeighbor( neighborsProperty, roomId, indexToRemove );
        }
    }


    private void AddRoomNeighbor( SerializedProperty neighborsProperty, int roomId )
    {
        if( neighborsProperty.arraySize >= roomFullNames.Count - 1 ) {
            return;// Уже не осталось новых комнат, чтобы им быть соседями
        }
        Assert.IsTrue( roomIdToNeighborIds.ContainsKey( roomId ) );
        int neighborId = -1;
        foreach( var roomIdToName in roomIdToRoomName ) {
            int newNeighborId = roomIdToName.Key;
            if( roomId != newNeighborId && !roomIdToNeighborIds[roomId].Contains( newNeighborId ) ) {
                neighborId = newNeighborId;
                break;
            }
        }
        Assert.IsTrue( neighborId != -1 );
        AddNeighborToRoom( roomId, neighborId );
        AddNeighborToRoom( neighborId, roomId );
    }


    private void DrawRoomNeighbor( SerializedProperty neighborProperty, int index, int roomId, out bool shouldRemoveNeighbor )
    {
        shouldRemoveNeighbor = false;
        EditorGUILayout.BeginHorizontal();

        Assert.IsTrue( roomIdToNeighborIndices.ContainsKey( roomId ) );
        Assert.IsTrue( index >=0 && index < roomIdToNeighborIndices[roomId].Count );
        int neighborIndex = roomIdToNeighborIndices[roomId][index];
        int newNeighborIndex = EditorGUILayout.Popup( $"neighbor {index}", neighborIndex, roomFullNames.ToArray() );
        if( newNeighborIndex != neighborIndex ) {
            int newNeighborId = GetRoomIdFromName( roomFullNames[newNeighborIndex] );
            int oldNeighborId = neighborProperty.intValue;
            if( newNeighborId != roomId && !roomIdToNeighborIds[roomId].Contains( newNeighborId ) ) {
                RemoveNeighborFromRoom( roomId, oldNeighborId );
                RemoveNeighborFromRoom( oldNeighborId, roomId );
                AddNeighborToRoom( roomId, newNeighborId );
                AddNeighborToRoom( newNeighborId, roomId );
            }
        }

        if( GUILayout.Button( new GUIContent( " ", "Remove neighbor." ), "OL Minus", GUILayout.Width( 16 ) ) ) {
            shouldRemoveNeighbor = true;
        }
        EditorGUILayout.EndHorizontal();
    }


    private void RemoveNeighbor( SerializedProperty neighborsProperty, int roomId, int indexToRemove )
    {
        Assert.IsTrue( indexToRemove >= 0 && neighborsProperty.arraySize > indexToRemove );
        int neighborId = neighborsProperty.GetArrayElementAtIndex( indexToRemove ).intValue;
        RemoveNeighborFromRoom( roomId, neighborId );
        RemoveNeighborFromRoom( neighborId, roomId );
    }


    private void AddNeighborToRoom( int roomId, int neighborId )
    {
        var roomProperty = roomIdToProperty[roomId];
        var neighborsProperty = roomProperty.FindPropertyRelative( neighborsPropertyName );
        neighborsProperty.InsertArrayElementAtIndex( neighborsProperty.arraySize );
        neighborsProperty.GetArrayElementAtIndex( neighborsProperty.arraySize - 1 ).intValue = neighborId;
        roomIdToNeighborIds[roomId].Add( neighborId );
        string neighborRoomName = CreateRoomFullName( neighborId );
        int neighborIndex = roomFullNames.FindIndex( x => x == neighborRoomName );
        Assert.IsTrue( neighborIndex != -1 );
        roomIdToNeighborIndices[roomId].Add( neighborIndex );
    }


    private void RemoveNeighborFromRoom( int roomId, int neighborId )
    {
        var roomProperty = roomIdToProperty[roomId];
        var neighborsProperty = roomProperty.FindPropertyRelative( neighborsPropertyName );
        int index = -1;
        for( int i = 0; i < neighborsProperty.arraySize; i++ ) {
            if( neighborsProperty.GetArrayElementAtIndex( i ).intValue == neighborId ) {
                index = i;
                break;
            }
        }
        if( index == -1 ) {
            return;
        }

        neighborsProperty.DeleteArrayElementAtIndex( index );
        roomIdToNeighborIds[roomId].Remove( neighborId );
        string neighborRoomName = CreateRoomFullName( neighborId );
        int neighborIndex = roomFullNames.FindIndex( x => x == neighborRoomName );
        Assert.IsTrue( neighborIndex != -1 );
        roomIdToNeighborIndices[roomId].Remove( neighborIndex );
    }


    private string CreateRoomFullName( int roomId )
    {
        int floorId = roomIdToFloorId[roomId];
        return Room.GetRoomFullName( floorIdToFloorName[floorId], roomIdToRoomName[roomId], roomId );
    }


    private int GetRoomIdFromName( string roomName )
    {
        int roomId = Room.GetIdFromRoomFullName( roomName );
        Assert.IsTrue( roomId != -1 );
        return roomId;
    }

}
