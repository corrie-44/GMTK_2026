using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum GenerationScheme
{
    Filled,
    RandomWalk
}

public class Board
{
    
    public Dictionary<Vector2Int, GridSquare> Cells = new();
    private int _width, _height;
    private float _cellSize;
    public int Width => _width;
    public int Height => _height;
    public float CellSize => _cellSize; 

    public Board(int width, int height, float cellSize, GenerationScheme scheme)
    {
        _width = width; _height = height;
        _cellSize = cellSize; 
        switch (scheme)
        {
            case GenerationScheme.Filled:
                Filled();
                break;
            case GenerationScheme.RandomWalk:
                //RandomWalk();
                break;
            default:
                break;
        }

    }

    void Filled()
    {
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                
                Vector2Int position = new Vector2Int(x, y);
                Cells.Add(position, new GridSquare(position));
                Cells[position].SetType(GridSquareType.BASIC);
            }
        }

        /*
        Cells[new Vector2Int(0, 0)].SetType(GridSqaureType.START);
        List<GridSquare> goalSquares = new();
        while (goalSquares.Count < 1)
        {
            GridSquare candidate = GetRandomSquareOnBorder();
            if (candidate.SquareType != GridSqaureType.BASIC) continue;
            goalSquares.Add(candidate);
            candidate.SetType(GridSqaureType.GOAL);
        }
        */
    }

    public List<GridSquare> GetNeighbours(GridSquare cell, int range = 1)
    {
        List<GridSquare> neighbours = new List<GridSquare>();

        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector2Int testPos = cell.GridPosition + new Vector2Int(x, y);
                if (Cells.TryGetValue(testPos, out GridSquare neighbour)) neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }

    public List<GridSquare> GetOrthogonalNeighbours(GridSquare cell)
    {
        List<GridSquare> neighbours = new List<GridSquare>();
        if (Cells.TryGetValue(cell.GridPosition + Vector2Int.up, out GridSquare north)) neighbours.Add(north);
        else neighbours.Add(null);

        if (Cells.TryGetValue(cell.GridPosition + Vector2Int.right, out GridSquare east)) neighbours.Add(east);
        else neighbours.Add(null);

        if (Cells.TryGetValue(cell.GridPosition - Vector2Int.up, out GridSquare south)) neighbours.Add(south);
        else neighbours.Add(null);

        if (Cells.TryGetValue(cell.GridPosition - Vector2Int.right, out GridSquare west)) neighbours.Add(west);
        else neighbours.Add(null);

        return neighbours;
    }

    public GridSquare GetRandomSquareOnBorder()
    {
        List<GridSquare> LRedgeSquares = Cells.Values.Where(s => s.GridPosition.x == 0 || s.GridPosition.x == Width - 1).ToList();
        List<GridSquare> TBedgeSquares = Cells.Values.Where(s => s.GridPosition.y == 0 || s.GridPosition.y == Height - 1).ToList();
        List<GridSquare> edgeSquares = LRedgeSquares.Union(TBedgeSquares).ToList();
        return edgeSquares[Random.Range(0, edgeSquares.Count)];
    }

    public Vector3 GridToWorldPosition(Vector2Int gridPosition)
    {
        float x = _cellSize * gridPosition.x - (_cellSize * Width * 0.5f) + _cellSize * 0.5f;
        float y = _cellSize * gridPosition.y - (_cellSize * Height * 0.5f) + _cellSize * 0.5f;
        
        return new Vector3(x, 0, y);
    }

    public Vector2Int WorldToGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt(worldPosition.x + (_cellSize * Width * 0.5f) / _cellSize);
        int y = Mathf.FloorToInt(worldPosition.z + (_cellSize * Height * 0.5f) / _cellSize);
        //x = Mathf.Clamp(x, 0, Width - 1);
       // y = Mathf.Clamp(y, 0, Height -1);
        return new Vector2Int(x, y);
    }
}
