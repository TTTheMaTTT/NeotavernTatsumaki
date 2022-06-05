using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Контролирует покачивания српайтов под музыку + смену спрайтов
public abstract class CharacterRythmVisualAbstract : MonoBehaviour, IBeatResponsive
{
    private string beatSpeedParameterName = "BeatSpeed";
    private string beatOffsetParameterName = "BeatOffset";

    private Animator _animator;

    private const float _beatMomentOffsetProportion = 0.5f;// Величина, используемая для смещения анимации, чтобы момент бита совпадал с моментом удара бита в анимации.

    protected int _moveAnimationLayerId = 1;
    protected virtual string moveAnimationLayerName => "Action Layer";

    protected int _damageAnimationLayerId = 2;
    protected virtual string damageAnimationLayerName => "Damage Layer";

    [SerializeField] private float bpm = 120;// частота битов.
    [SerializeField][Range( 0f, 1f )] private float offset = 0.5f;

    // Start is called before the first frame update
    void Awake()
    {
        _animator = GetComponent<Animator>();
        _moveAnimationLayerId = _animator?.GetLayerIndex( moveAnimationLayerName ) ?? -1;
        _damageAnimationLayerId = _animator?.GetLayerIndex( damageAnimationLayerName ) ?? -1;
    }


    // Выставить анимацию для проигрывания.
    protected void PlayAnimation( string animationName, int layerId )
    {
        _animator?.Play( animationName, layerId );
    }


    public virtual void ConfigureBeats( float bpm, float beatOffset )
    {
        this.bpm = Mathf.Max( bpm, 0f );
        float beatTime = this.bpm > 0 ? 60f / this.bpm : 0f;
        _animator?.SetFloat( beatSpeedParameterName, this.bpm / 60f );
        offset = beatTime > 0f ? Mathf.Repeat( beatOffset, beatTime ) / beatTime : 0f;
        float offsetValue = Mathf.Repeat( offset + _beatMomentOffsetProportion, 1f );
        _animator?.SetFloat( beatOffsetParameterName, offsetValue );
    }


    public virtual void OnBeat()
    {
    }


    // Update is called once per frame
    void Update()
    {
    }
}
