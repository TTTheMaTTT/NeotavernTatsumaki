using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���� �������� ����������
public enum CharacterAction
{
    None = 0,// �������� ������ �� ������
    Attack = 1,// �������� �������
    Defend = 2// �������� ����������
}


// ������� ��������� ���������
public struct CharacterState
{
    public int Health;// �������� ���������
    public CharacterAction Action;// ������� �������� ���������
}


public class CharacterController : MonoBehaviour
{
    protected CharacterState _state;
    public CharacterState State { get { return _state; } set { _state = value; } }
}