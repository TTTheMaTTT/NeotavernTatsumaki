using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dialogue {

    public static class DialogueStatementDefaultValues
    {
        public const int MaxActorsOnScreen = 10;// Максимальное кол-во действующих лиц, которое может одновременно присутствовать на экране
    }

    // Описывает, какое действие будет произведено с лицом диалога на экране
    public enum TDialogueActorArrangementAction
    {
        Appear,// Появление актёра на экране
        Leave,// Уход актёра с экрана
        ChangePosition// Смена позиции
    }

    // Описывает, на какой части экрана нужно расположить лицо диалога
    public enum TDialogueActorArrangementPosition
    {
        // Диалоговая система сама определяет точные координаты, но пытается сделать это в соответствие с указанным значением енама
        Left,
        Center,
        Right,
        Custom// Пользователь сам указывает координаты (в системе отсчёта UI)
    }

    /// <summary>
    ///  Описывает, как нужно поменять расположение для одного действующего лица диалога
    /// </summary>
    [System.Serializable]
    public class CDialogueActorArrangement
    {
        public string ActorName;// Имя действующего лица. Уникально и является идентификатором лица.
        public TDialogueActorArrangementAction ArrangementAction;// Действие, которое произведётся с лицом, будь то добавление или удаление
        public TDialogueActorArrangementPosition Position;// Где расположить лицо. Не имеет смысла при ArrangementAction == Leave
        public Vector2 ExactPosistion;// Точные координаты (в системе отсчёта UI), на которых нужно расположить лицо. Принимается в расчёт при ArrangementPosition == Custom.
        public bool UseAutoSize = true;// Автоматически рассчитывать размер спрайта лица
        public bool UseCustomScale;// Использовать значение поля Scale для задавания изображения
        public float Scale;// Во сколько раз нужно увеличить изображение на экране относительно оригинального размера изображения. Имеет смысл, если UseAutoSize == false и UseCustomScale == true.
        public Vector2 Size;// Выставляемый размер изображению. Имеет смысл только если UseAutoSize и UseCustomScale выставлены в false.
    }

    /// <summary>
    /// Класс, описывающий, как нужно расположить действующих лиц для данного высказывания. 
    /// Считаем, что этот класс описывает не само расположение, а как нужно его поменять, относительно предыдущего расположения
    /// Также считаем, что если экземпляр класса для данного выражения ничего не описывает (т.е. пусто), то ничего и не меняем в текущем расположении.
    /// </summary>
    [System.Serializable]
    public class CDialogueActorsArrangements
    {
        public bool UseDefaultMaxActorsOnScreen = true;// Если true, то считаем, что MaxActorsOnScreen = DialogueStatementDefaultValues.MaxActorsOnScreen, иначе берём заданное извне значение.
        public int MaxActorsOnScreen = 0;// Максимальное кол-во лиц диалога на экране. При превышении кол-во лиц, отображаться будут только последние MaxActorsOnScreen говоривших.

        public CDialogueActorArrangement[] Arrangements;// Описание всех изменений в расположении для всех лиц
    }

    /// <summary>
    /// Описывает, как нужно отобразить действующее лицо
    /// </summary>
    [System.Serializable]
    public class CDialogueActorImage
    {
        public string ActorName;// Имя действующего лица. Уникально и является идентификатором лица. Лицо должно присутствовать в диалоге, чтобы имело смысл
        // Можно указать изображение двумя способами. Либо непосредственно указать изображение, либо путь к нему
        public Sprite Image;// Само изображение. Предпочтительный способ указать изображение, используется диалоговой системой в первую очередь, если возможно.
        // Путь к папке с изображением. Можно указать полный путь, а можно указать уникальное имя папки с изображениями. 
        // Тогда система попытается найти её папку (рекурсивно) в общей папке для всех персонажей (поле CharactersFolder DialogueSystem.ini).
        // Можно совсем ничего не указывать, тогда поиск будет происходить по имени ActorName
        public string ImagesDir;
        public string ImageName;// Имя изображения из папки ImagesDir
    }

    /// <summary>
    /// Класс, описывающий одно высказывание в диалоге
    /// </summary>
    [System.Serializable]
    public class CDialogueStatement
    {
        public string Name;// Имя отображаемого имени говорящего
        public string ActorName;// Имя говорящего действующего лица. Может отличаться от Name. Нужно для соотнесения с ActorsArrangements и ActorsImages

        public CDialogueActorsArrangements ActorsArrangements;// Как нужно поменять расположение лиц диалога при данном высказывании
        public CDialogueActorImage[] ActorsImages;// Как нужно отобразить лиц диалога.

        [TextArea]
        public string Statement;// Само высказывание
    }

}
