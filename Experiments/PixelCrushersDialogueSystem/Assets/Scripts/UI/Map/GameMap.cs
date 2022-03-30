using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameMap : MonoBehaviour
{

    private Dictionary<int, RoomButton> roomIdToRoomButtons = new Dictionary<int, RoomButton>();
    private HashSet<int> visitedRooms = new HashSet<int>();
    private HashSet<int> unvisitedRooms = new HashSet<int>();
    private int currentRoomId = -1;


    private const string roomButtonsParentPath = "Canvas/Panel";


    private void Awake()
    {
    }


    private void Start()
    {
    }


    // Выставить схему расположений комнат
    public void SetRoomLayout( RoomLayout layout )
    {
        // Пока что считаем, что схема устанавливается через уже существующие кнопки, на сам layout не смотрим
        var roomButtonsParent = transform.Find( roomButtonsParentPath );
        roomIdToRoomButtons.Clear();
        var roomButtons = new List<RoomButton>( roomButtonsParent.GetComponentsInChildren<RoomButton>() );
        unvisitedRooms.Clear();
        visitedRooms.Clear();

        foreach( var roomButton in roomButtons ) {
            if( roomIdToRoomButtons.ContainsKey( roomButton.roomId ) ) {
                Debug.LogWarning( $"GameMap: Repeated room id [{roomButton.roomId}] in game map" );
            }
            roomIdToRoomButtons.Add( roomButton.roomId, roomButton );
            roomButton.roomButtonEvent.RemoveAllListeners();
            roomButton.roomButtonEvent.AddListener( OnChooseRoom );

            // Пока что считаем, что все комнаты являются не посещёнными
            unvisitedRooms.Add( roomButton.roomId );
            roomButton.SetAsUnvisitedRoom();
            roomButton.gameObject.SetActive( false );
        }
    }


    // Выставить указанную комнату текущей
    public void SetCurrentRoom( Room room )
    {
        // Предыдущая посещённая комната помечается, как "посещённая"
        if( currentRoomId != -1 ) {
            roomIdToRoomButtons[currentRoomId].SetAsVisitedRoom();
        }
        currentRoomId = room.roomId;
        if( !roomIdToRoomButtons.ContainsKey( currentRoomId ) ) {
            Debug.LogWarning( $"GameMap: Unknown room id [{currentRoomId}]" );
        } else {
            // Помечаем данную комнату, как посещённую
            var currentRoomButton = roomIdToRoomButtons[currentRoomId];
            currentRoomButton.gameObject.SetActive( true );
            currentRoomButton.SetAsCurrentRoom();
            if( unvisitedRooms.Contains( currentRoomId ) ) {
                visitedRooms.Add( currentRoomId );
                unvisitedRooms.Remove( currentRoomId );
            }
        }

        // Показываем соседей данной комнаты, если они ещё не были показаны
        foreach( var neighbor in room.neigbourRooms ) {
            if( !roomIdToRoomButtons.ContainsKey( neighbor ) ) {
                Debug.LogWarning( $"GameMap: Unknown room id [{neighbor}]" );
                continue;
            }
            roomIdToRoomButtons[neighbor].gameObject.SetActive( true );
        }

    }


    void Update()
    {
        
    }


    public void OnChooseRoom( int roomId )
    {
        GameControllerTavernOutside gameController = GameController.Instance() as GameControllerTavernOutside;
        gameController.TryGoInRoom( roomId );
    }


}
