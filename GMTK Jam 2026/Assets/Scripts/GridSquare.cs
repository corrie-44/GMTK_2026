using UnityEngine;
using System;
using System.Collections.Generic; 
public enum GridSqaureType
{
    EMPTY,
    BASIC,
    START,
    GOAL,
    DIODE,
    GATE,
    SPLITTER,
    COLORCHANGER,
    STRAYTIME
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
    public Vector2Int DiodeDirection = Vector2Int.zero; 

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
        
        if (type == GridSqaureType.GOAL) int.TryParse(Element.Substring(1), out GoalValue);

        if(type == GridSqaureType.DIODE)
        {
            string compassDirection = Element.Substring(2, 1);
            switch (compassDirection)
            {
                case "N":
                    DiodeDirection = new Vector2Int(0,1); 
                    break;
                case "E":
                    DiodeDirection = new Vector2Int(-1, 0);
                    break;
                case "S":
                    DiodeDirection = new Vector2Int(0, -1);
                    break;
                case "W":
                    DiodeDirection = new Vector2Int(1, 0);
                    break;
                default:
                    break;
            }
        }

        OnSquareUpdated?.Invoke(this);
    }
}
