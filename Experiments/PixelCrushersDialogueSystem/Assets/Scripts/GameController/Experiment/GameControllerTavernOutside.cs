using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEditor;
using PixelCrushers.DialogueSystem;

// Экспериментальный игровой контроллер, использующийся в режиме исследования комнат
public class GameControllerTavernOutside : GameControllerAbstract
{
    private DialogueSystemController _dialogueSystemController;
    private PauseMenu _pauseMenu;
    private EventSystem _eventSystem;

    [SerializeField]
    private RoomLayout roomLayout;// Расположение всех комнат
    [SerializeField]
    private int startRoomId;// Начальная комната
    private int currentRoomId;// id текущей комнаты
    private Dictionary<int, Floor> floors;
    private Dictionary<int, Room> rooms;
    private GameMap _gameMap;// Интерактивная карта уровня

    //private PlayerTavernInside _player;

    // Проинициализировать все составляющие контроллер компоненты.
    protected override void initialize()
    {
        _pauseMenu = FindObjectOfType<PauseMenu>();
        Assert.IsNotNull( _pauseMenu );

        _eventSystem = FindObjectOfType<EventSystem>();
        Assert.IsNotNull( _eventSystem );

        _dialogueSystemController = FindObjectOfType<DialogueSystemController>();

        _gameMap = FindObjectOfType<GameMap>();
        Assert.IsNotNull( _gameMap );
        floors = roomLayout.floors.ToDictionary( x => x.floorId );
        rooms = roomLayout.floors.SelectMany( x => x.rooms ).ToDictionary( x => x.roomId );
        foreach( var idToRoom in rooms ) {
            DialogueLua.SetVariable( GetRoomStateVariableName( idToRoom.Value ), "" );
        }


        //var players = FindObjectsOfType<PlayerTavernInside>();
        //Assert.IsTrue( players.Length == 1, "One and only one player is allowed!" );
        //_player = players[0];


        base.initialize();
    }


    private void Start()
    {
        _pauseMenu.Close();
        _gameMap.SetRoomLayout( roomLayout );

        TryGoInRoom( startRoomId );
        //_player.SetMovability( true );
    }


    private void Update()
    {
        if( Input.GetButtonDown( "Cancel" ) ) {
            if( !_isPaused ) {
                SetOnPause();
            } else {
                SetOnPlay();
            }
        }
    }


    void OnEnable()
    {
        Lua.RegisterFunction( "SetCurrentRoomState", this, SymbolExtensions.GetMethodInfo( () => SetCurrentRoomState( string.Empty ) ) );
    }


    void OnDisable()
    {
        Lua.UnregisterFunction( "SetCurrentRoomState" );
    }


    public override void SetOnPause()
    {
        _isPaused = true;
        Time.timeScale = 0f;
        _pauseMenu.Open();
        _eventSystem.SetSelectedGameObject( null );
        _dialogueSystemController?.SetDialogueSystemInput( false );
    }


    public override void SetOnPlay()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        _pauseMenu.Close();
        _dialogueSystemController?.SetDialogueSystemInput( true );
    }


    // Сохранение (при необходимости) и выход из игры
    public override void ExitGame()
    {
        Application.Quit();
        UnityEditor.EditorApplication.isPlaying = false;
    }


    public void OnConversationStart()
    {
        //_player.SetMovability( false );
    }


    public void OnConversationEnd()
    {
        //_player.SetMovability( true );
    }


    public List<string> GetRoomNames()
    {
        List<string> roomNames = new List<string>();
        foreach( Floor floor in roomLayout.floors ) {
            foreach( Room room in floor.rooms ) {
                roomNames.Add( $"{floor.floorName}/{room.roomName}[{room.roomId}]" );
            }
        }
        return roomNames;
    }


    public void TryGoInRoom( int roomId )
    {
        // Пока что нам ничего не запрещает идти в комнату, если известен её id
        if( roomId == currentRoomId || !rooms.ContainsKey( roomId ) ) {
            return;
        }

        currentRoomId = roomId;
        Room currentRoom = rooms[roomId];
        Debug.Log( rooms[currentRoomId].roomName );

        _gameMap.SetCurrentRoom( rooms[currentRoomId] );

        // Если есть conversation для данной комнаты в текущем состоянии, то начинаем его
        string currentRoomState = DialogueLua.GetVariable( GetRoomStateVariableName( currentRoom ) ).asString;
        int rsIndex = currentRoom.conversations.FindIndex( x => x.RoomState == currentRoomState );
        if( rsIndex != -1 ) {
            _dialogueSystemController.StartConversation( currentRoom.conversations[rsIndex].ConversationId );
        }
    }

    private string GetRoomStateVariableName( Room room )
    {
        Assert.IsTrue( floors.ContainsKey( room.floorId ) );
        return Room.GetRoomFullName( floors[room.floorId], room ) + "_state";
    }


    // Выставить состояние для комнаты, в которой находится игрок
    private void SetCurrentRoomState( string state )
    {
        Assert.IsTrue( rooms.ContainsKey( currentRoomId ) );
        DialogueLua.SetVariable( GetRoomStateVariableName( rooms[currentRoomId] ), state );
    }

}