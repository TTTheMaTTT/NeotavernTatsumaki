using System.Collections.Generic;
using UnityEngine;

// Информация об одной комнате
[System.Serializable]
public class Room
{
    // Отображаемое имя комнаты.
    public string roomName;

    // Id комнаты
    public int roomId;

    // Id этажа, которому принадлежит комната
    public int floorId;

    // Id локации, для отображения комнаты. Может быть пусто.
    public int locationId;

    // Диалоги, которые могут произойти в комнате
    public List<RoomStateWithConversation> conversations;

    // Соседние комнаты
    public List<int> neigbourRooms;

    // Составление полного имени комнаты
    public static string GetRoomFullName( Floor floor, Room room )
    {
        return $"{floor.floorName}/{room.roomName}[{room.roomId}]";
    }

    // Составление полного имени комнаты
    public static string GetRoomFullName( string floorName, string roomName, int roomId )
    {
        return $"{floorName}/{roomName}[{roomId}]";
    }

    // Извлечение id комнаты из полного имени. Возвращает -1, если извлечь id не удаётся
    public static int GetIdFromRoomFullName( string roomFullName )
    {
        int left = roomFullName.LastIndexOf( '[' );
        int right = roomFullName.LastIndexOf( ']' );
        if( left == -1 || right < left ) {
            return -1;
        }
        return int.TryParse( roomFullName.Substring( left + 1, right - left - 1 ), out var result ) ? result : -1;
    }
}

// Структура, состоящая из строки, содержащей информацию о состоянии комнаты, и id conversation'а
// Используется для вызова нужного conversation'а в зависимости от состояния комнаты
[System.Serializable]
public struct RoomStateWithConversation
{
    // Название состояния комнаты. Может быть пусто.
    public string RoomState;

    // Id conversation'а, соответствующего данному состоянию
    public int ConversationId;

}