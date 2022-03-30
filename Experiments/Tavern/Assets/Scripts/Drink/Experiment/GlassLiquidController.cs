using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// Рассчитывает свойства жидоксти внутри стакана
public class GlassLiquidController : MonoBehaviour
{

    [SerializeField] private int _liquidAmount = 0;
    private int _prevLiquidAmount = 0;
    [SerializeField] private int maxLiquidAmount = 2400;
    private int maxOverflowAmount = 100;
    private float _liquidLevel;

    private Color liquidColor = Color.blue;

    // Эффекты выливания жидкости с разных краёв
    [SerializeField] private ParticleSystem leftPourEffect;
    [SerializeField] private ParticleSystem rightPourEffect;
    [SerializeField] private ParticleSystem overflowEffect;
    // Компонент, придающий объекту свойство переместимости
    private DraggableMonobehaviour _draggableMb;

    // Отношение ширины стакана к высоте
    private const float _widthHeightProportion = 1f;
    // Диагональ стакана, если считать высоту за 1
    private float _diagonal;
    // Угол диагонали стакана
    private float _diagonalAngle;

    // Углы, при которых происходит выливание жидкости
    private float _minPourAngle = 90f;
    private float _maxPourAngle = 90f;
    private const float _pourAnglesDistance = 5f;
    private const float _maxPossiblePourAngle = 90f;
    private const float _minPossiblePourAngle = 2f;

    // Управление эффектом выливания жидкости из стакана
    [SerializeField] private int maxPourEffectEmission = 1500;
    [SerializeField] private int overflowEmission = 20000;
    private int _pourEffectEmission = 0;

    // Управление изменением кол-ва жидкости в стакане
    [SerializeField] private float maxLiquidAmountLossSpeed = 200f;
    [SerializeField] private float overflowAmountLossSpeed = 1000f;
    private float _liquidAmountLossSpeed = 0f;

    private Vector3 _lastPos;
    private Vector3 _lastRot;
    // Колебание жидкости
    [SerializeField] private float maxWobble = 0.15f;
    [SerializeField] private float wobbleSensitivity = 0.05f;
    [SerializeField] private float wobbleFrequence = 2f;
    [SerializeField] private float wobbleRecovery = 2f;
    private float _minWobbleLiquidLevel = 0.15f;
    private float _maxWobbleLiquidLevel = 0.9f;
    private float _wobbleAmplitude;
    private float _wobbleAmount;
    private float _prevWobbleAmount;

    // Управление материалом и цветом
    private Material _liquidMaterial = null;
    private bool _shouldUpdateMaterial = false;
    private HashSet<Liquid> _mixtureLiquids = new HashSet<Liquid>();
    private Dictionary<Liquid, float> _mixtureParts = new Dictionary<Liquid, float>();
    

    private const string _liquidColorProperty = "_Color";
    private const string _liquidLevelProperty = "_LiquidLevel";
    private const string _wobbleProperty = "_Wobble";


    private void Start()
    {
        _liquidMaterial = GetComponent<Renderer>().material;
        _draggableMb = GetComponent<DraggableMonobehaviour>();
        if( _draggableMb == null && transform.parent != null ) {
            _draggableMb = transform.parent.GetComponent<DraggableMonobehaviour>();
        }
        _prevLiquidAmount = -1;
        _liquidLevel = 0f;
        _prevWobbleAmount = 0f;

        Assert.IsTrue( _widthHeightProportion >= 0f );
        _diagonal = Mathf.Sqrt( 1f + _widthHeightProportion * _widthHeightProportion );// Диагональ стакана, если считать высоту за 1
        _diagonalAngle = Mathf.Asin( 1 / _diagonal ) * Mathf.Rad2Deg;// Угол диагонали стакана

        _minWobbleLiquidLevel = Mathf.Max( _minWobbleLiquidLevel, 0f );
        _maxWobbleLiquidLevel = Mathf.Min( _maxWobbleLiquidLevel, 1f );
    }


    private void Update()
    {
        if( _shouldUpdateMaterial ) {
            if( _liquidMaterial != null ) {
                _liquidMaterial.SetColor( _liquidColorProperty, liquidColor );
                _liquidMaterial.SetFloat( _liquidLevelProperty, _liquidLevel );
                _liquidMaterial.SetFloat( _wobbleProperty, _wobbleAmount );
                _shouldUpdateMaterial = false;
            }
            if( leftPourEffect != null ) {
                leftPourEffect.startColor = liquidColor;
            }
            if( rightPourEffect != null ) {
                rightPourEffect.startColor = liquidColor;
            }
            if( overflowEffect != null ) {
                overflowEffect.startColor = liquidColor;
            }
        }
    }


    private void FixedUpdate()
    {
        _liquidLevel = Mathf.Clamp01( (float)_liquidAmount / (float)maxLiquidAmount );

        if( _liquidAmount != _prevLiquidAmount ) {
            // Пересчитываем углы, при которых происходит выливание жидкости
            CalculatePourAngle();
            if( _draggableMb != null ) {
                _draggableMb.AngularLimitations = new DraggableAbstract.AngleLimitations( true, _maxPourAngle );
            }

            if( _liquidAmount == 0 ) {
                _mixtureParts.Clear();
                _mixtureLiquids.Clear();
                if( _draggableMb != null ) {
                    _draggableMb.AngularLimitations = new DraggableAbstract.AngleLimitations( false, 0f );
                }
            }
        }

        _prevLiquidAmount = _liquidAmount;

        float currentAngle = Mathf.Repeat( transform.eulerAngles.z + 180f, 360f ) - 180f; // маппим на (-180; 180)
        bool shoulStopLiquidLoss = true;

        if( _liquidAmount > 0 ) {
            if( Mathf.Abs( currentAngle ) >= _minPourAngle ) {
                float absAngle = Mathf.Max( _maxPourAngle, Mathf.Abs( currentAngle ) );
                float pourValue = (absAngle - _minPourAngle) / (_maxPourAngle - _minPourAngle);
                _pourEffectEmission = Mathf.RoundToInt( maxPourEffectEmission * pourValue );
                _liquidAmountLossSpeed = maxLiquidAmountLossSpeed * pourValue;
                if( leftPourEffect != null && rightPourEffect != null ) {
                    ParticleSystem otherSystem, currentSystem;
                    (currentSystem, otherSystem) = currentAngle > 0 ? (leftPourEffect, rightPourEffect) : (rightPourEffect, leftPourEffect);
                    otherSystem.emissionRate = 0f;
                    currentSystem.emissionRate = _pourEffectEmission;
                }
                CalculateLiquidLoss();
                shoulStopLiquidLoss = false;
            } else if( _liquidAmount > maxLiquidAmount ) {
                if( overflowEffect != null ) {
                    overflowEffect.emissionRate = overflowEmission;
                }
                _liquidAmountLossSpeed = overflowAmountLossSpeed;
                CalculateLiquidLoss();
                shoulStopLiquidLoss = false;
            }
        }

        if( shoulStopLiquidLoss ) {
            _liquidAmountLossSpeed = 0f;
            _pourEffectEmission = 0;
            if( leftPourEffect != null ) {
                leftPourEffect.emissionRate = 0f;
            }
            if( rightPourEffect != null ) {
                rightPourEffect.emissionRate = 0f;
            }
            if( overflowEffect != null ) {
                overflowEffect.emissionRate = 0f;
            }
        }

        if( _liquidLevel >= _minWobbleLiquidLevel && _liquidLevel <= _maxWobbleLiquidLevel ) {
            CalculateWobble();
        } else {
            _wobbleAmplitude = 0f;
            _wobbleAmount = 0f;
        }
    }


    private void CalculatePourAngle()
    {
        float h = (float)_liquidLevel;// заполненность стакана, от 0 до 1
        float beta = Mathf.Asin( (h - 0.5f) * 2 / _diagonal ) * Mathf.Rad2Deg;// Угол, относительно горизонтальной линиий, проходящей через центр стакана, под которым должен находится край стакана, чтобы началось выливание

        _minPourAngle = _diagonalAngle - beta;
        _maxPourAngle = _minPourAngle + _pourAnglesDistance;
        _minPourAngle = Mathf.Max( _minPossiblePourAngle, Mathf.Min( _maxPossiblePourAngle, _minPourAngle ) );
        _maxPourAngle = Mathf.Max( _minPossiblePourAngle, Mathf.Min( _maxPossiblePourAngle, _maxPourAngle ) );
        if( _draggableMb != null ) {
            _draggableMb.AngularLimitations = new DraggableAbstract.AngleLimitations( true, _maxPourAngle );
        }
    }


    private void CalculateLiquidLoss()
    {
        _liquidAmount -= Mathf.RoundToInt( _liquidAmountLossSpeed * Time.fixedDeltaTime );
        _liquidAmount = Mathf.Max( 0, _liquidAmount );
        CalculateMixture();
        _shouldUpdateMaterial = true;
    }


    // Расчёт пропорции и цвета жидкости
    private void CalculateMixture() 
    {
        float mixtureAmount = _mixtureParts.Sum( x => x.Value );
        if( mixtureAmount != _liquidAmount ) {
            float normalizationValue = _liquidAmount / mixtureAmount;
            var keys = _mixtureParts.Keys;
            foreach( var key in _mixtureLiquids ) {
                _mixtureParts[key] *= normalizationValue;
            }
        }
        // Расчёт цвета жидкости
        liquidColor = Color.white;
        mixtureAmount = 0f;
        foreach( var mixturePart in _mixtureParts ) {
            mixtureAmount += mixturePart.Value;
            liquidColor = Color.Lerp( liquidColor, mixturePart.Key.LiquidColor, (float)mixturePart.Value / (float)mixtureAmount );
        }
        _shouldUpdateMaterial = true;
    }


    private void CalculateWobble()
    {
        _prevWobbleAmount = 1f;
        // Затухание движения
        _wobbleAmplitude = Mathf.Lerp( _wobbleAmplitude, 0, Time.fixedDeltaTime * (wobbleRecovery) );
        float pulse = 2 * Mathf.PI * wobbleFrequence;
        Vector3 velocity = (_lastPos - transform.position) / Time.fixedDeltaTime;
        Vector3 angularVelocity = transform.rotation.eulerAngles - _lastRot;
        // Учёт движения самого стакана
        float movement = ( velocity.x + 0.2f * angularVelocity.z ) * wobbleSensitivity;

        // Колебательное движение, с учётом поступательного движения
        // Если сам стакан слишком быстро движется, то колебаний не видно
        float wobbleDirection = Mathf.Clamp( Mathf.Sin( pulse * Time.fixedTime ) - movement, -1f, 1f );
        float wobbleValue = _wobbleAmplitude * wobbleDirection;
        // Перекос в сторону против движения
        wobbleValue -= movement * wobbleSensitivity;
        wobbleValue = Mathf.Clamp( wobbleValue, -maxWobble, maxWobble );
        // Амплитуда колебаний увеличивается от движения
        _wobbleAmplitude = Mathf.Max( Mathf.Abs( wobbleValue ), _wobbleAmplitude );
        _wobbleAmount = Mathf.Lerp( _wobbleAmount, wobbleValue, Time.fixedDeltaTime * 20f );
        
        _lastPos = transform.position;
        _lastRot = transform.eulerAngles;

        if( _prevWobbleAmount != _wobbleAmount ) {
            _shouldUpdateMaterial = true;
        }
    }

    
    // Попадание частицы жидкости в стакан
    public void AddLiquidParticle( Liquid liquid )
    {
        _liquidAmount = Mathf.Min( _liquidAmount + 1, maxLiquidAmount + maxOverflowAmount );
        if( !_mixtureParts.ContainsKey( liquid ) ) {
            _mixtureParts.Add( liquid, 0 );
            _mixtureLiquids.Add( liquid );
        }
        _mixtureParts[liquid] += 1f;
        CalculateMixture();

        _shouldUpdateMaterial = true;
    }
}
