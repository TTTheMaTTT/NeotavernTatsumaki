using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Experiments.TurnBaseSimultaniousCombat
{
    public class GameController : GameControllerAbstract
    {
        public const string PlayerTag = "Player";

        private Grid _grid;
        private List<CharacterController> _actors;

        protected override void initialize()
        {
            var grids = FindObjectsOfType<Grid>();
            Assert.IsTrue( grids.Length > 0, "Can't find any grid" );
            if( grids.Length > 1 ) {
                Debug.LogWarning( "Found more than one grid" );
            }
            _grid = grids[0];

            _actors = FindObjectsOfType<CharacterController>().Select( x=>x as CharacterController ).ToList();
        }

        public override void InitiateGameAction()
        {
            _actors.ForEach( x => { x.Analyze(); x.Action();} );
        }


        public override Vector2 GetPositionOnGrid( Vector2 position )
        {
            return SnapPositionToGrid( position );
        }


        public override Vector2 GetPositionOnGridWithOffset( Vector2 position, Vector2 offset )
        {
            position.x += (_grid.cellSize.x + _grid.cellGap.x) * Mathf.Sign( offset.x ) * Mathf.Floor( Mathf.Abs( offset.x ) );
            position.y += (_grid.cellSize.y + _grid.cellGap.y) * Mathf.Sign( offset.y ) * Mathf.Floor( Mathf.Abs( offset.y ) );
            return SnapPositionToGrid( position );

        }


        private Vector2 SnapPositionToGrid( Vector2 position )
        {
            Vector3 relPosition = position - (Vector2)_grid.transform.position;
            relPosition.x = relPosition.x - Mathf.Repeat( relPosition.x, _grid.cellSize.x + _grid.cellGap.x ) + _grid.cellSize.x / 2;
            relPosition.y = relPosition.y - Mathf.Repeat( relPosition.y, _grid.cellSize.y + _grid.cellGap.y ) + _grid.cellSize.y / 2;
            return _grid.transform.position + relPosition;
        }
    }
}
