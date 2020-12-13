using System.Collections.ObjectModel;
using UnityEngine;
using TMPro;

namespace Dialogue {

    /// <summary>
    /// Интерфейс редактирования диалогового текста
    /// Под редактированием может подразумеваться как простое форматирование текста (цвет, шрифт), так и различные эффекты, показываемые в реальном времени (анимации итп.)
    /// </summary>
    public interface IDialogueEditing
    {
        /// <summary>
        /// Инициализация
        /// </summary>
        /// <param name="editingParams">Параметры редактирования</param>
        /// <param name="textMesh">Обрабатываемый текст</param>
        /// <param name="originVertices">Массив изначальных позиций вершин текста до всех редактирований</param>
        void Initialize ( CDialogueEditingParams editingParams );
        /// <summary>
        /// Произвести начальную обработку
        /// </summary>
        void StartEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices );
        /// <summary>
        /// Обновить состояние редактирования
        /// </summary>
        void UpdateEditing( TMP_TextInfo textInfo, Color[] colors, Vector3[] vertices, ReadOnlyCollection<Vector3> originVertices );
    }
}
