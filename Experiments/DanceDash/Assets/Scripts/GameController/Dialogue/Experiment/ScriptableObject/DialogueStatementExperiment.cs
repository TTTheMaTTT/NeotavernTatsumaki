using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Класс, описывающий одну диалоговую сессию
/// </summary>
[System.Serializable]
public class CDialogueStatementExperiment
{
    [SerializeField]
    public string Name;// Отображаемое имя говорящего
    [SerializeField]
    public string ActorName;// Имя говорящего действующего лица. Может отличаться от Name. Нужно для соотнесения с ActorsArrangements и ActorsImages
}

