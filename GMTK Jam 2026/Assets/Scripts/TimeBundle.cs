using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro; 
public class TimeBundleData
{
    public float Time;
    public int Priority;
    public Vector2Int GridPosition;
    
    public TimeBundleData(float time, int priority, Vector2Int gridPosition)
    {
        Time = time;
        Priority = priority;
        GridPosition = gridPosition; 
    }
}

public class TimeBundle : MonoBehaviour
{
    public TimeBundleData BundleData;

    public List<Vector2Int> Moves;

    public LineRenderer lineRend;

    public PathInfo pathInfo;
    public TMP_Text timeDisplayText;

    private void Start()
    {
        timeDisplayText.text = BundleData.Time.ToString();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Move(); 
        }
    }

    public void UpdatePath(PathInfo pathInfo)
    {
       // Vector3[] positions = new Vector3[pathInfo.Nodes.Count]; 
        lineRend.positionCount = pathInfo.Nodes.Count;
        var foo = pathInfo.Nodes.ToArray();
        for (int i = 0; i < foo.Length; i++)
        {
            Vector3 pos = BoardManager.Instance.Board.GridToWorldPosition(foo[i].GridPosition);
            lineRend.SetPosition(i, pos);
        }
    }

    public void Move()
    {
        StartCoroutine(TakePath());
    }

    IEnumerator TakePath()
    {
        for (int i = 0; i < pathInfo.Nodes.Count - 1; i++)
        {
            yield return StartCoroutine(MoveCoroutine(
                pathInfo.Nodes[i].Visual.transform.position,
                pathInfo.Nodes[i + 1].Visual.transform.position));
            pathInfo.Nodes[i].bundle = null; 
            pathInfo.Nodes[i + 1].bundle = this; 
        }
    }

    public IEnumerator MoveCoroutine(Vector3 start, Vector3 end)
    {
        float timer = 0; 
        while(timer < 1)
        {
            float t = timer / 1;
            transform.position = Vector3.Lerp(start, end, t);
            timer += Time.deltaTime;
            yield return null; 
        }
    }

    public void MovePath(int i, float t)
    {
        if (i >= pathInfo.Nodes.Count - 1) return;
        Vector3 start = pathInfo.Nodes[i].Visual.transform.position;
        Vector3 end = pathInfo.Nodes[i + 1].Visual.transform.position;
        transform.position = Vector3.Lerp(start, end, t);
    }

    public void UpdateGridPosition(int i)
    {
        if (i >= pathInfo.Nodes.Count - 1) return;
        pathInfo.Nodes[i].bundle = null;
        pathInfo.Nodes[i + 1].bundle = this;
        BundleData.GridPosition = pathInfo.Nodes[i + 1].GridPosition; 
    }

    public void UpdateTime()
    {
        BundleData.Time += -1;
        timeDisplayText.text = BundleData.Time.ToString(); 
    }
}
