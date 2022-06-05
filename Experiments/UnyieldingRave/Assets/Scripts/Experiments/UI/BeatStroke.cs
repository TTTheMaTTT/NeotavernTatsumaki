using System;
using UnityEngine;

public class BeatStroke : MonoBehaviour
{

    private GameObject _reactionAreaObject;
    private string _reactionAreaObjectName = "ReactionArea";

    private void Awake()
    {
        _reactionAreaObject = transform.Find( _reactionAreaObjectName )?.gameObject;
    }


    public void SetReactionArea( float width, BeatReactionType reactionType )
    {
        if( _reactionAreaObject != null ) {
            var rectTransform = _reactionAreaObject.GetComponent<RectTransform>();
            if( rectTransform == null ) {
                return;
            }
            Vector2 size = rectTransform.sizeDelta;
            size.x = width;
            rectTransform.sizeDelta = size;
            Vector2 position = rectTransform.anchoredPosition;
            switch( reactionType ) {
                case BeatReactionType.OnlyBeforeBeat:
                    position.x = -size.x / 2;
                    break;
                case BeatReactionType.BeforeAndAfterBeat:
                    position.x = 0f;
                    break;
                default:
                    throw new Exception( $"Wrong reaction type {reactionType}" );
                    break;
            }
            rectTransform.anchoredPosition = position;
        }
    }


    // Update is called once per frame
    private void Update()
    {
        
    }
}
