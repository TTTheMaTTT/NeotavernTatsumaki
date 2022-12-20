using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Experiments.TurnBaseSimultaniousCombat
{
    public class EnemyController : CharacterController
    {
        private EnemyModel _model;
        private Transform _visual;

        public override void Analyze()
        {
            _model.DecideAction();
        }

        public override void Action()
        {
            _model.Action();

            if( _visual != null ) {
                // ѕоворот персонажа в соответствии с направлением.
                int sign = _model.CurrentOrientation == CharacterOrientation.Right ? 1 : -1;
                Vector3 scale = _visual.transform.localScale;
                scale.x *= Mathf.Sign( scale.x ) * sign;
                _visual.transform.localScale = scale;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            _model = new EnemyModel();
            _visual = GetComponentInChildren<SpriteRenderer>()?.transform;
            _model.Initialize( transform, _visual?.localScale.x > 0 ? CharacterOrientation.Right : CharacterOrientation.Left ) ;
        }


        private void Update()
        {
            transform.position = Vector2.Lerp( transform.position, _model.CurrentPosition, Settings.MovementSpeed * Time.deltaTime );
        }

    }
}