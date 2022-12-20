using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

// —одержит информацию о том, где можно перемещатьс€, а где нельз€.
[System.Serializable]
public class MapLayout
{
    public MapLayout( int height, int width )
    {
        Width = width;
        Height = height;
        _mapCells = new MapCell[height][];
        for( int i = 0; i < _mapCells.Length; i++ ) {
            _mapCells[i] = new MapCell[width];
        }
    }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public Vector2 CellSize { get; set; } = new Vector2( 1f, 1f );
    public Vector2 OriginPoint { get; set; } = new Vector2( 0f, 0f );
    
    private MapCell[][] _mapCells;

    public MapCell GetCell( int i, int j )
    {
        Assert.IsTrue( j >= 0 && j < Width && i >= 0 && i < Height );
        return _mapCells[i][j];
    }

    public void SetCell( int i, int j, MapCell cell )
    {
        Assert.IsTrue( j >= 0 && j < Width && i >= 0 && i < Height );
        _mapCells[i][j] = cell;
    }
}

public enum CellState
{
    Free,
    Impassable
}

// »нформаци€ о €чейке
[System.Serializable]
public struct MapCell
{
    public CellState state;

    public MapCell( CellState _state )
    {
        state = _state;
    }
}