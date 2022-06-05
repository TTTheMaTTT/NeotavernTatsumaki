using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Виды действий персонажей
public enum CharacterAction
{
    None = 0,// Персонаж ничего не делает
    Attack = 1,// Персонаж атакует
    Defend = 2// Персонаж защищается
}


// Текущее состояние персонажа
public struct CharacterState
{
    public int Health;// Здоровье персонажа
    public CharacterAction Action;// Текущее действие персонажа
}


public class CharacterController : MonoBehaviour
{
    protected CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }
}