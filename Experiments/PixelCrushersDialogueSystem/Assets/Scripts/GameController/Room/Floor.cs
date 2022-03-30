using System;
using System.Collections.Generic;
using UnityEngine;

// Информация об одном этаже
[System.Serializable]
public class Floor
{
    // Отображаемое имя этажа.
    public string floorName;
    // Id этажа
    public int floorId;
    // Комнаты на этаже
    public List<Room> rooms;
}
