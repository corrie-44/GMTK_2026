using UnityEngine;
using System; 

public enum GridSqaureType
{
    EMPTY,
    BASIC,
    START,
    GOAL,
    FEATURE
}

public class GridSquare
{
    private Vector2Int _gridPosition;
    public Vector2Int GridPosition => _gridPosition;

    private GridSqaureType _squareType = GridSqaureType.BASIC; 
    public GridSqaureType SquareType => _squareType;

    public Action<GridSquare> OnSquareUpdated;

    public Color DebugColor = Color.black;

    public GridSquareVisual Visual;

    public TimeBundle bundle; 

    public GridSquare(Vector2Int gridPosition)
    {
        _gridPosition = gridPosition;
    }

    public void SetType(GridSqaureType type)
    {
        _squareType = type;
        OnSquareUpdated?.Invoke(this);
    }
}
