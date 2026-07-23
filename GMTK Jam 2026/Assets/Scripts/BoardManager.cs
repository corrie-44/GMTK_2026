using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Events; 
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

    public UnityEvent OnLevelReset; 

    [Header("Data")]
    List<Level> _levels; 
    public TextAsset levelData;
    public int levelIndex;

    private int _activeLevelIndex;

    public List<TimeBundle> TimeBundles = new(); 
    //public List<Vector2Int> SpawnPositions = new(); 
    private Board _board; 
    public Board Board => _board;

    GridSquare _hoverSquare;

    Camera mainCam;

    List<GridSquareVisual> gridCells = new();

    bool _drawingPath = false;
    PathInfo _activePath;

    private bool _isPlaying = false;
    public bool IsPlaying => _isPlaying; 

    void Start()
    {
        mainCam = Camera.main;

        _levels = LevelReader.GetLevels(levelData);

        CreateLevel(levelIndex);
    }

    public void CreateLevel(int index)
    {
        _isPlaying = false; 
        _activeLevelIndex = index; 

        //Destroy bundles
        for(int i = TimeBundles.Count - 1; i >= 0; i--)
        {
            Destroy(TimeBundles[i].gameObject);
        }
        TimeBundles.Clear();

        for (int i = gridCells.Count - 1; i >= 0; i--)
        {
            Destroy(gridCells[i].gameObject);
        }
        gridCells.Clear();

        Level activeLevel = _levels[index];

        _board = new Board(activeLevel.Width, activeLevel.Height, cellSize, GenerationScheme.Filled);

        for (int x = 0; x < activeLevel.Width; x++)
        {
            for (int y = 0; y < activeLevel.Height; y++)
            {
                // 0,0 is 0,3 in tile grid coords 
                // 1,0 is 1,3
                // 2,1 is 2,2
                // x stays the same
                // x, 3-y 
                int listIndex = y * activeLevel.Width + x;
                Vector2Int gridCoord = new Vector2Int(x, (activeLevel.Height - 1) - y);
                //Vector2Int gridCoord = new Vector2Int((activeLevel.Width - 1) - x, (activeLevel.Height - 1) - y);
                // Vector2Int gridCoord = new Vector2Int(x, y);

                string element = activeLevel.Elements[listIndex];
                _board.Cells[gridCoord].Element = element;
            }
        }

        foreach (GridSquare square in _board.Cells.Values)
        {
            if (square.Element != "0")
            {
                GridSquareVisual visual = Instantiate(gridTilePrefab, boardParent).GetComponent<GridSquareVisual>();
                visual.transform.position = Board.GridToWorldPosition(square.GridPosition);
                visual.Init(square);
                gridCells.Add(visual);

                if ("roygbiv".Contains(square.Element[0]))
                {
                    TimeBundle tb = Instantiate(timeBundlePrefab, Board.GridToWorldPosition(square.GridPosition), Quaternion.identity);
                    TimeBundles.Add(tb);
                    int.TryParse(square.Element.Substring(1), out int timeValue);
                    int priority = "roygbiv".IndexOf(square.Element[0]);
                    tb.BundleData = new TimeBundleData(timeValue, priority, square.GridPosition);
                    square.Bundles.Add(tb);
                }

                if (square.Element.Contains("*"))
                {
                    square.SetType(GridSqaureType.GOAL);
                }
            }
        }

        OnLevelReset?.Invoke(); 
    }

    public void ResetCurrentLevel()
    {
        StopAllCoroutines();
        CreateLevel(_activeLevelIndex);
    }

    public void NextLevel()
    {
        StopAllCoroutines();
        _activeLevelIndex = _activeLevelIndex + 1;
        if (_activeLevelIndex >= _levels.Count) return; 
        CreateLevel(_activeLevelIndex);
    }

    private void Update()
    {
        UpdateHoveredSquare();

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetCurrentLevel();
        }

        if (_isPlaying) return; 

        if (Input.GetMouseButtonDown(0))
        {
            if(_hoverSquare != null && _hoverSquare.bundle != null)
            {
                _drawingPath = true;
                _activePath = new PathInfo(_hoverSquare.bundle, _hoverSquare);
                _hoverSquare.bundle.pathInfo = _activePath;
                _activePath.PathBundle.UpdatePath(_activePath);
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
        bool valid = Board.Cells.TryGetValue(Board.WorldToGridPosition(intersectPoint), out GridSquare square);
        // GridSquareVisual visual = Board.Cells[Board.WorldToGridPosition(intersectPoint)].Visual;

        if (!valid) return; 
        if(square != _hoverSquare)
        {
            if(_hoverSquare != null && _hoverSquare.Visual != null)
                _hoverSquare.Visual.SetHover(false);

            //if (_hoverSquare == null) return; 
            _hoverSquare = square;

            if (_hoverSquare != null && _hoverSquare.Visual != null)
                _hoverSquare.Visual.SetHover(true);

            HoveredSquareChanged(); 
        }
    }

    void HoveredSquareChanged()
    {
        if (Input.GetMouseButton(0) && _drawingPath && !_isPlaying)
        {
            if (Board.GetOrthogonalNeighbours(_activePath.LastNode).Contains(_hoverSquare))
            {
                
                if (_activePath.Nodes.Contains(_hoverSquare)) return; 
                _activePath.Nodes.Add(_hoverSquare);
                _activePath.PathBundle.UpdatePath(_activePath);
            }
        }
    }

    public void ExecutePaths()
    {
        _isPlaying = true; 
        StartCoroutine(ExecutePathsCoroutine());
    }

    IEnumerator ExecutePathsCoroutine()
    {
        int index = 0;
        float timer = 0;
        float moveTime = stepTime;
        int outcome = 0; 
        while (outcome == 0)
        {
            outcome = GameStatus();

            switch (outcome)
            {
                case 0:
                    break;
                case 1:
                    Debug.Log("WINNER");
                    LevelBeaten(); 
                    yield break;
                case -1:
                    Debug.Log("LOST");
                    ResetCurrentLevel();
                    yield break;  
                default:
                    break;
            }
            foreach (TimeBundle tb in TimeBundles)
            {
                if (!tb.Alive) continue; 
                tb.MovePath(index, moveCurve.Evaluate(timer / moveTime));
                timer += Time.deltaTime;
                yield return null;
            }

            if(timer > moveTime)
            {
                foreach (TimeBundle tb in TimeBundles)
                {
                    if (tb.Alive)
                    {
                        tb.UpdateGridPosition(index);
                        tb.AddTime(-1);
                    }
                }

                MergeCheck();

                timer = 0;
                index++; 
            }


            
        }
    }

    void MergeCheck()
    {
        foreach (GridSquare square in Board.Cells.Values)
        {
            List<TimeBundle> livingBundles = square.Bundles.Where(tb => tb.Alive).ToList();
            if (livingBundles.Count > 1)
            {
                int max = livingBundles.Min(b => b.BundleData.Priority);
                TimeBundle activeBundle = livingBundles.First(b => b.BundleData.Priority == max);
                foreach(TimeBundle t in livingBundles)
                {
                    if (t == activeBundle) continue; 
                    if(t.BundleData.Priority > max)
                    {
                        activeBundle.Eat(); 
                        activeBundle.AddTime(t.BundleData.Time);
                        t.AddTime(-t.BundleData.Time);
                    }
                }
            }
        }
    }

    void LevelBeaten()
    {
        _isPlaying = false;
        StopAllCoroutines();
        StartCoroutine(LevelBeatenCoroutine());
    }

    IEnumerator LevelBeatenCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        NextLevel(); 
    }

    int GameStatus()
    {
        int status = 0; //0: game continues, 1: victory, -1: fail state 
        int numGoalSquares = Board.Cells.Values.Where(s => s.SquareType == GridSqaureType.GOAL).Count();
       
        //If there are more bundles alive than there are goals, the game is still going
        if (TimeBundles.Where(t => t.Alive).Count() > numGoalSquares) return 0;

        //If there are no living bundles, the round is failed
        if (TimeBundles.Where(t => t.Alive).Count() == 0) return -1;

        int numGoalsSatisfied = 0; 
        foreach (GridSquare square in Board.Cells.Values)
        {
            if(square.SquareType == GridSqaureType.GOAL && square.bundle != null)
            {
                if(square.bundle.BundleData.Time == square.GoalValue)
                {
                    Debug.Log(square.GoalValue);
                    Debug.Log(square.bundle.BundleData.Time);
                    numGoalsSatisfied += 1; 
                }
            }
        }

        if (numGoalsSatisfied == numGoalSquares) return 1; 

        return status; 
    }


}
