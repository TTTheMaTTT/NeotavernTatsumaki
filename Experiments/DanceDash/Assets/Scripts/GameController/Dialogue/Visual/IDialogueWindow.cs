using System;
using System.Xml;

namespace Dialogue { 

    /// <summary>
    /// Интерфейс диалогового окна
    /// </summary>
    public interface IDialogueWindow
    {
        /// <summary>
        /// Инициализация окна. Просто указывается функция, вызываемая при переходе к следующему выражению.
        /// </summary>
        /// <param name="nextStatementCallback">Коллбэк при переходе к следующему выражению</param>
        void Initialize( Action nextStatementCallback );
        void Open();
        void Close();
        /// <summary>
        /// Отобразить одно выражение из диалога
        /// </summary>
        /// <param name="statement">Выражение</param>
        void ShowDialogueStatement( CDialogueStatement statement );
    }

}
