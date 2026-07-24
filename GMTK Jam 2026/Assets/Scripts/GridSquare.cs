using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq; 
public enum GridSquareType
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

    private GridSquareType _squareType = GridSquareType.BASIC; 
    public GridSquareType SquareType => _squareType;

    public Action<GridSquare> OnSquareUpdated;

    public Color DebugColor = Color.black;

    public GridSquareVisual Visual;

    public List<TimeBundle> Bundles = new(); 
    public TimeBundle bundle => GetBundle();

    //Special characteristics 
    public string Element;
    public int GoalValue = -1;
    public Vector2Int DiodeDirection = Vector2Int.zero;
    public int SplitterID = -1; 

    TimeBundle GetBundle()
    {
        if (Bundles.Count == 0) return null;
        return Bundles.FirstOrDefault(b => b.Alive);
        //return Bundles[0];
    }

    public GridSquare(Vector2Int gridPosition)
    {
        _gridPosition = gridPosition;
    }

    public void SetType(GridSquareType type)
    {
        _squareType = type;
        
        if (type == GridSquareType.GOAL) int.TryParse(Element.Substring(1), out GoalValue);

        if(type == GridSquareType.DIODE)
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

        if(type == GridSquareType.SPLITTER)
        {
            string splitterIDstring = Element.Substring(2, 1);
            int.TryParse(splitterIDstring, out SplitterID);
        }

        OnSquareUpdated?.Invoke(this);
    }
}
