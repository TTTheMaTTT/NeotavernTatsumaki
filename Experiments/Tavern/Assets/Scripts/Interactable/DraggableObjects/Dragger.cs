using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Объект, ответственный за перетаскивание объектов. Точка захвата объектов. захваченный объект следует за драггером, драггер следует за курсором. 
// Почему бы не сделать так, чтобы захваченный объект следовал за курсором. Чтобы можно было, отдельно от курсора, вращать драгер с объектом как-то расположенным относительно него.
public class Dragger : MonoBehaviour
{
    private CursorAbstract _cursor;
    private bool _moveToCursor = true;


    private class DraggedObject
    {
        public Transform transform;
        public Transform prevParent;
        public DraggableMonobehaviour mb;
        public Rigidbody2D rigidbody;

        public DraggedObject( DraggableMonobehaviour draggable )
        {
            transform = draggable.transform;
            prevParent = draggable.transform.parent;
            mb = draggable;
            rigidbody = draggable.GetComponent<Rigidbody2D>();
        }
    }

    // Объект, с которым взаимодействует драггер
    private DraggedObject _draggedObject;

    private Vector2 _prevPosition;
    // Константы и переменные для симуляции углового инерционного движения
    private bool _imitateInertia = false;
    private float _centerMassPositionCoeff = 0f;
    private float _centerMassDelta = 1f;
    private float _inertionDelta = 10f;
    private float _inertionMaxAngle = 60f;
    private float _inertionMaxSpeed = 30f;
    private float _inertionAngularSpeed = 10f;

    // Константы для сохранения движения при детаче
    private bool _haveMomentumOnDetach = false;
    private float _speedReductionCoef = 0.5f;
    private Vector2 _velocity;
    private Quaternion _prevRotation;
    private float _angularVelocity;

    // Константы и переменные для режима вращения
    bool _isRotationMode;// Находимся ли сейчас в режиме вращения?
    private float _positionToRotateDelta = 1f;// Как далеко должен находиться курсор, чтобы произвести вращение
    private float _rotationAngularSpeed = 10f;


    public void Initialize( CursorAbstract cursor, IDraggable draggedObject, bool saveHierarchy = true )
    {
        // Инициализируем своё положение курсором
        _cursor = cursor;
        transform.position = _cursor.transform.position;

        _imitateInertia = false;
        _haveMomentumOnDetach = false;

        // Прикрепляемся к объекту
        var draggedMB = draggedObject as DraggableMonobehaviour;
        if( draggedMB != null ) {
            _draggedObject = new DraggedObject( draggedMB );
            transform.rotation = _draggedObject.transform.rotation;
            if( saveHierarchy ) {
                transform.SetParent( _draggedObject.prevParent );
            }
            draggedMB.transform.SetParent( transform );

            _imitateInertia = draggedObject.ImitateInertia();
            _haveMomentumOnDetach = draggedObject.HaveMomentumOnDetach() && _draggedObject.rigidbody != null;

            if( _imitateInertia ) {
                _prevPosition = transform.position;
                Vector2 centerMassPos = Vector2.zero;
                if( _draggedObject.rigidbody != null ) {
                    // Рассчитываем положение точки захвата относительно центра масс
                    // В зависимости от взаимного положения этих точек будем по разному раскачивать объект
                    centerMassPos = _draggedObject.rigidbody.worldCenterOfMass;

                } else {
                    centerMassPos = _draggedObject.transform.position;
                }
                Vector2 dragPointRelativePos = (Vector2)transform.position - centerMassPos;
                Vector2 dragPointRelativePosNorm = dragPointRelativePos.normalized;
                float dragPointRelativePosUpProjection = dragPointRelativePosNorm.x * transform.up.x + dragPointRelativePosNorm.y * transform.up.y;
                _centerMassPositionCoeff = dragPointRelativePos.magnitude < _centerMassDelta ? 0f : Mathf.Sign( dragPointRelativePosUpProjection );
            }
            if( _haveMomentumOnDetach ) {
                _prevRotation = transform.rotation;
            }

            // Выключаем перемещение объекта
            if( _draggedObject.rigidbody != null ) {
                _draggedObject.rigidbody.angularVelocity = 0f;
                _draggedObject.rigidbody.velocity = Vector2.zero;
                _draggedObject.rigidbody.isKinematic = true;
            }
        }
    }


    // Освобождение объекта из захвата
    public void Detach()
    {
        if( _draggedObject == null ) {
            return;
        }

        // Возвращаем прежнего родителя объекта
        if( _draggedObject.prevParent != null && _draggedObject.prevParent.gameObject != null ) {
            _draggedObject.transform.SetParent( _draggedObject.prevParent );
        } else {
            _draggedObject.transform.SetParent( null );
        }

        // Возвращаем объекту подвижность
        if( _draggedObject.rigidbody != null ) {
            _draggedObject.rigidbody.isKinematic = false;
            if( _haveMomentumOnDetach ) {
                _draggedObject.rigidbody.velocity = _velocity;
                _draggedObject.rigidbody.angularVelocity = _angularVelocity;
            }
        }

        _draggedObject = null;
    }


    public void SetRotationMode( bool isRotationMode )
    {
        _isRotationMode = isRotationMode;

        if( _draggedObject == null ) {
            return;
        }

        if( _isRotationMode ) {
            if( _cursor != null ) {
                transform.position = _cursor.transform.position;
            }
            _prevPosition = transform.position;

            if( _haveMomentumOnDetach ) {
                _velocity = Vector2.zero;
                _angularVelocity = 0f;
            }
        }

    }


    private void FixedUpdate()
    {
        if( _isRotationMode ) {
            RotationMovement();
        } else {
            UsualMovement();
        }
    }


    private void RotationMovement()
    {
        // Следим за курсором и вращаемся в зависимости от его положения
        Vector2 mousePos = Camera.main.ScreenToWorldPoint( Input.mousePosition );
        Vector2 direction = mousePos - (Vector2)transform.position;
        if( direction.magnitude < _positionToRotateDelta ) {
            return;
        }
        float angle = Mathf.Atan2( direction.y, direction.x ) * Mathf.Rad2Deg + 90;
        Quaternion rotation = Quaternion.AngleAxis( angle, Vector3.forward );
        transform.rotation = Quaternion.Slerp( transform.rotation, rotation, _rotationAngularSpeed * Time.fixedDeltaTime );

        // Ограничения поворота
        if( _draggedObject == null || !_draggedObject.mb.AngularLimitations.HasLimitations ) {
            return;
        }

        float currentAngle = Mathf.Repeat( transform.eulerAngles.z + 180f, 360f ) - 180f; // маппим на (-180; 180)
        Vector3 eulerAngles = transform.eulerAngles;
        float angleLimit = Mathf.Abs( _draggedObject.mb.AngularLimitations.Limitations );
        if( currentAngle > angleLimit ) {
            transform.eulerAngles = new Vector3( eulerAngles.x, eulerAngles.y, angleLimit );
        } else if( currentAngle < -angleLimit ) {
            transform.eulerAngles = new Vector3( eulerAngles.x, eulerAngles.y, -angleLimit );
        }
        
    }


    private void UsualMovement()
    {
        if( _cursor == null ) {
            return;
        }

        // Положение
        transform.position = _cursor.transform.position;

        if( _draggedObject == null ) {
            return;
        }

        float inertionAngularValue = 0f;
        if( _imitateInertia ) {
            // Поворот в следствие инерции
            float deltaX = _prevPosition.x - transform.position.x;
            inertionAngularValue = Mathf.Clamp( (Mathf.Abs( deltaX )) <= _inertionDelta * Time.fixedDeltaTime ? 0f : deltaX / (_inertionMaxSpeed * Time.fixedDeltaTime), -1f, 1f );
            inertionAngularValue *= _centerMassPositionCoeff;
            
        }
        Quaternion rotation = Quaternion.AngleAxis( inertionAngularValue * _inertionMaxAngle, Vector3.forward );
        transform.rotation = Quaternion.Slerp( transform.rotation, rotation, _inertionAngularSpeed * Time.fixedDeltaTime );

        if( _haveMomentumOnDetach ) {
            // Учитываем перемещение
            _velocity = _speedReductionCoef * ((Vector2)transform.position - _prevPosition) / Time.fixedDeltaTime;
            Quaternion delta = transform.rotation * Quaternion.Inverse( _prevRotation );
            _angularVelocity = delta.z * Mathf.Rad2Deg * 2.0f / Time.fixedDeltaTime;
            _prevRotation = transform.rotation;
        }
        _prevPosition = (Vector2)transform.position;
    }


    private void OnDestroy()
    {
        Detach();
    }
}
