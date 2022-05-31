using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatStroke : MonoBehaviour
{

    private GameObject _reactionAreaObject;
    private string _reactionAreaObjectName = "ReactionArea";

    private void Awake()
    {
        _reactionAreaObject = transform.Find( _reactionAreaObjectName )?.gameObject;
    }

    public void SetReactionAreaWidth( float width )
    {
        if( _reactionAreaObject != null ) {
            var rectTransform = _reactionAreaObject.GetComponent<RectTransform>();
            if( rectTransform == null ) {
                return;
            }
            Vector2 size = rectTransform.sizeDelta;
            size.x = width;
            rectTransform.sizeDelta = size;
        }
    }

    // Update is called once per frame
    private void Update()
    {
        
    }
}
