using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Кнопка, соответствующая какой-то комнате.
public class RoomButton : MonoBehaviour
{
    [System.Serializable]
    public class RoomButtonEvent : UnityEvent<int>
    {
    }

    [HideInInspector]
    public UnityEvent<int> roomButtonEvent = new RoomButtonEvent();

    [SerializeField]
    private int _roomId;

    public int roomId { get { return _roomId; } }

    private Button button;
    private Image buttonImage;

    [SerializeField]
    private Color currentRoomColor = new Color( 1, 1, 1, 1 );
    [SerializeField]
    private Color visitedRoomColor = new Color( 0.7f, 0.7f, 0.7f, 1 );
    [SerializeField]
    private Color unvisitedRoomColor = new Color( 0.3f, 0.3f, 0.3f, 1 );


    private void Awake()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        if( button != null ) {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener( buttonAction );
        }
    }


    public void SetAsCurrentRoom()
    {
        buttonImage.color = currentRoomColor;
    }


    public void SetAsVisitedRoom()
    {
        buttonImage.color = visitedRoomColor;
    }


    public void SetAsUnvisitedRoom()
    {
        buttonImage.color = unvisitedRoomColor;
    }


    private void buttonAction()
    {
        roomButtonEvent.Invoke( _roomId );
    }

}
