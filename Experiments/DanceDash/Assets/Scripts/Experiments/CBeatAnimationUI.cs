using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CBeatAnimationUI : MonoBehaviour
{
    private void Awake() {
        _beatPeriod = 60f / _bitRate;
        _rectTransform = GetComponent<RectTransform>();
        Assert.IsTrue( _rectTransform != null );
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _beatPeriod = 60f / _bitRate;
        float beatMoment = Mathf.Repeat( Time.unscaledTime, _beatPeriod );
        float beatProportion = beatMoment / _beatPeriod;
        float value = _movementCurve.Evaluate( beatProportion );
        _rectTransform.anchoredPosition =  Vector2.Lerp( _fromPosition, _toPosition, value );
        _rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Horizontal, Mathf.Lerp( _fromSize.x, _toSize.x, value ) );
        _rectTransform.SetSizeWithCurrentAnchors( RectTransform.Axis.Vertical, Mathf.Lerp( _fromSize.y, _toSize.y, value ));
    }

    [SerializeField] private Vector2 _fromPosition;
    [SerializeField] private Vector2 _toPosition;
    [SerializeField] private Vector2 _fromSize;
    [SerializeField] private Vector2 _toSize;
    [SerializeField] private AnimationCurve _movementCurve;

    [SerializeField] private float _bitRate = 60f;
    private float _beatPeriod;

    private RectTransform _rectTransform;

}
