using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RythmUIController : MonoBehaviour
{
    [SerializeField] private GameObject beatStrokePrefab;// ������, ������� ������������ ��������� ����

    [SerializeField] private float deltaTime;// ��� ����� ��������� ����
    [SerializeField] private float speed;// �������� ����������� �����

    [SerializeField] private float instantiatePosX;// �� ����� ������� ��������� ����
    [SerializeField] private float deletePosX;// �� ����� ������� ������� ����


    private GameObject _rythmLineObject;
    private const string rythmLineObjectName = "RythmLine";
    private Stack<GameObject> _beatStrokeObjects = new Stack<GameObject>();// ������������ �������

    private float _timer = 0f;

    private void Start()
    {
        _rythmLineObject = transform.Find( rythmLineObjectName );
    }

    private void Update()
    {
        if( _timer >= deltaTime ) {
            
        }
    }

}
