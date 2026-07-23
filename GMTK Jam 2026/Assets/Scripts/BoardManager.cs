using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq; 
public class PathInfo
{
    public TimeBundle PathBundle;
    public List<GridSquare> Nodes;
    public GridSquare LastNode => Nodes[Nodes.Count - 1];

    public PathInfo(TimeBundle bundle, GridSquare startSquare)
    {
        PathBundle = bundle;
        Nodes = new();
        Nodes.Add(startSquare);
    }
}

public class BoardManager : Singleton<BoardManager>
{
    [SerializeField] private Transform boardParent;
    [SerializeField] private GameObject gridTilePrefab;
    [SerializeField] private TimeBundle timeBundlePrefab; 
    [SerializeField] private float cellSize = 1;
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] private float stepTime = 1; 
    public List<TimeBundle> TimeBundles = new(); 
    public List<Vector2Int> SpawnPositions = new(); 
    private Board _board; 
    public Board Board => _board;

    GridSquareVisual _hoverSquare;

    Camera mainCam;

    List<GridSquareVisual> gridCells = new();

    bool _drawingPath = false;
    PathInfo _activePath; 

    void Start()
    {
        mainCam = Camera.main; 
        _board = new Board(4, 4, cellSize, GenerationScheme.Filled);

        foreach (GridSquare square in _board.Cells.Values)
        {
            if (square.SquareType != GridSqaureType.EMPTY)
            {
                GridSquareVisual visual = Instantiate(gridTilePrefab, boardParent).GetComponent<GridSquareVisual>();
                visual.transform.position = Board.GridToWorldPosition(square.GridPosition);
                visual.Init(square);
                gridCells.Add(visual);

                if (SpawnPositions.Contains(square.GridPosition)){
                    TimeBundle tb = Instantiate(timeBundlePrefab, Board.GridToWorldPosition(square.GridPosition), Quaternion.identity);
                    TimeBundles.Add(tb);
                    tb.BundleData = new TimeBundleData(10, 1, square.GridPosition);
                    square.bundle = tb; 
                }
            }
        }
    }

    private void Update()
    {
        UpdateHoveredSquare();

        if (Input.GetMouseButtonDown(0))
        {
            if(_hoverSquare && _hoverSquare.GridSquare.bundle)
            {
                _drawingPath = true;
                _activePath = new PathInfo(_hoverSquare.GridSquare.bundle, _hoverSquare.GridSquare);
                _hoverSquare.GridSquare.bundle.pathInfo = _activePath; 
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            _drawingPath = false; 
        }
    }

    void UpdateHoveredSquare()
    {
        Ray mouseRay = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, 0);
        groundPlane.Raycast(mouseRay, out float distance);
        Vector3 intersectPoint = mouseRay.origin + mouseRay.direction * distance;
        GridSquareVisual visual = Board.Cells[Board.WorldToGridPosition(intersectPoint)].Visual;
        if(visual != _hoverSquare)
        {
            if(_hoverSquare != null)
                _hoverSquare.SetHover(false);
            _hoverSquare = visual;
            _hoverSquare.SetHover(true);
            HoveredSquareChanged(); 
        }
    }

    void HoveredSquareChanged()
    {
        if (Input.GetMouseButton(0) && _drawingPath)
        {
            if (Board.GetOrthogonalNeighbours(_activePath.LastNode).Contains(_hoverSquare.GridSquare))
            {
                _activePath.Nodes.Add(_hoverSquare.GridSquare);
                _activePath.PathBundle.UpdatePath(_activePath);
            }
        }
    }

    public void ExecutePaths()
    {
        StartCoroutine(ExecutePathsCoroutine());
    }

    IEnumerator ExecutePathsCoroutine()
    {
        int index = 0;
        float timer = 0;
        float moveTime = stepTime;
        while (index < 10)
        {
            foreach (TimeBundle tb in TimeBundles)
            {
                tb.MovePath(index, moveCurve.Evaluate(timer / moveTime));
                timer += Time.deltaTime;
                yield return null;
            }

            if(timer > moveTime)
            {
                foreach (TimeBundle tb in TimeBundles)
                {
                    tb.UpdateGridPosition(index);
                    tb.UpdateTime(); 

                    //Merge bundles on the same tile
                }
                timer = 0;
                index++; 
            }
        }
    }

    void MergeCheck()
    {

    }


}
