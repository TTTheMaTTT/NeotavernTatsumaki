using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue
{ 

    /// <summary>
    /// Класс, описывающий одну диалоговую сессию
    /// </summary>
    [CreateAssetMenu( fileName = "Dialogue", menuName = "ScriptableObjects/Create Dialogue", order = 1 )]
    [System.Serializable]
    public class CDialogue : ScriptableObject
    {
        public CDialogueStatement[] Statements;
    }

}
