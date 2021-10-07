using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Синглтон, контролирующий потоки игровых событий
public abstract class GameControllerAbstract: MonoBehaviour
{
    protected static GameControllerAbstract _instance;

    // Был ли проинициализирован контроллер
    protected bool _isInitialized = false;

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
        if( _instance != null ) {
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
}
