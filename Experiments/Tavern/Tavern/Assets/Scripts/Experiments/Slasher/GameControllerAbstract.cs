using UnityEngine;

namespace Experiments.Slasher
{
    // Синглтон, контролирующий потоки игровых событий
    public abstract class GameControllerAbstract : MonoBehaviour
    {
        protected static GameControllerAbstract _instance;

        // Был ли проинициализирован контроллер
        protected bool _isInitialized = false;
        // Поставлена ли игра на паузу
        protected bool _isPaused;

        /// <summary>
        /// Доступ к контролеру
        /// </summary>
        /// <returns>Экземпляр игрового контроллера</returns>
        public static GameControllerAbstract Instance()
        {
            if( _instance == null ) {
                _instance = FindObjectOfType<GameControllerAbstract>();
                if( _instance == null ) {
                    _instance = new GameObject().AddComponent<GameControllerAbstract>();
                }
            }

            if( !_instance._isInitialized ) {
                _instance.initialize();
            }

            return _instance;
        }


        private void Awake()
        {
            if( _instance != null && _instance != this ) {
                Destroy( this );
                return;
            }
            _instance = this;

            if( !_isInitialized ) {
                initialize();
            }
        }

        // Проинициализировать все составляющие контроллер компоненты.
        protected virtual void initialize()
        {
            _isInitialized = true;
        }

        public virtual void SetOnPause()
        {
        }


        public virtual void SetOnPlay()
        {
        }

        // Сохранение (при необходимости) и выход из игры
        public virtual void ExitGame()
        {
        }

        // Запустить игровое действие
        public virtual void InitiateGameAction()
        {
        }

        // Получение позиции, привязанной к сетке
        public virtual Vector2 GetPositionOnGrid( Vector2 position )
        {
            return position;
        }

        // Получение позиции, смещённой на offset и привязанной к сетке. Смещение должно задаваться в виде кол-ва ячеек вдоль оси x и y
        public virtual Vector2 GetPositionOnGridWithOffset( Vector2 position, Vector2 offset )
        {
            return position + offset;
        }
    }
}

