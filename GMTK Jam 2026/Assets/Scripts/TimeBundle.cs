using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using MoreMountains.Feedbacks; 
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
    public Renderer rend;
    public GameObject visual;
    public MMF_Player eatFeedback; 

    public List<Color> priorityColors = new();

    public bool Alive => BundleData.Time > 0; 

    private void Start()
    {
        timeDisplayText.text = BundleData.Time.ToString();
        rend.material.color = priorityColors[BundleData.Priority];
        lineRend.startColor = priorityColors[BundleData.Priority];
        lineRend.gameObject.SetActive(false);
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
        lineRend.gameObject.SetActive(true);
        lineRend.positionCount = pathInfo.Nodes.Count;
        var foo = pathInfo.Nodes.ToArray();
        for (int i = 0; i < foo.Length; i++)
        {
            Vector3 pos = BoardManager.Instance.Board.GridToWorldPosition(foo[i].GridPosition) + Vector3.up * 0.1f;
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
            pathInfo.Nodes[i].Bundles.Remove(this);
            pathInfo.Nodes[i + 1].Bundles.Add(this);
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
        if (pathInfo == null || pathInfo.Nodes.Count == 0) return; 
        if (i >= pathInfo.Nodes.Count - 1) return;
        Vector3 start = pathInfo.Nodes[i].Visual.transform.position;
        Vector3 end = pathInfo.Nodes[i + 1].Visual.transform.position;
        transform.position = Vector3.Lerp(start, end, t);
    }

    public void UpdateGridPosition(int i)
    {
        if (pathInfo == null || pathInfo.Nodes.Count == 0) return;
        if (i >= pathInfo.Nodes.Count - 1) return;
        pathInfo.Nodes[i].Bundles.Remove(this);
        pathInfo.Nodes[i + 1].Bundles.Add(this);
        //pathInfo.Nodes[i].bundle = null;
        //pathInfo.Nodes[i + 1].bundle = this;
        BundleData.GridPosition = pathInfo.Nodes[i + 1].GridPosition; 
    }

    public void AddTime(float time)
    {
        BundleData.Time += time;
        UpdateTime(); 
    }

    private void UpdateTime()
    {
        timeDisplayText.text = BundleData.Time.ToString(); 
        if(BundleData.Time <= 0)
        {
            visual.SetActive(false);
        }
    }

    public void Eat()
    {
        eatFeedback.PlayFeedbacks(); 
    }
}
