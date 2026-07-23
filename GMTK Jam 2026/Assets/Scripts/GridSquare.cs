using UnityEngine;
using System;
using System.Collections.Generic; 
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

    public List<TimeBundle> Bundles = new(); 
    public TimeBundle bundle => GetBundle();

    public string Element;

    public int GoalValue = -1; 

    TimeBundle GetBundle()
    {
        if (Bundles.Count == 0) return null;
        return Bundles[0];
    }

    public GridSquare(Vector2Int gridPosition)
    {
        _gridPosition = gridPosition;
    }

    public void SetType(GridSqaureType type)
    {
        _squareType = type;
        OnSquareUpdated?.Invoke(this);
        if (type == GridSqaureType.GOAL) int.TryParse(Element.Substring(1), out GoalValue);
    }
}
