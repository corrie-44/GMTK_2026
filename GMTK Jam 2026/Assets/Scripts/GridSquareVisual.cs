using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;
public class GridSquareVisual : MonoBehaviour
{
    private GridSquare _gridSqaure;
    public GridSquare GridSquare => _gridSqaure;

    [SerializeField] GameObject hoverVisual; 

    public void Init(GridSquare gridSquare)
    {
        _gridSqaure = gridSquare;
        _gridSqaure.OnSquareUpdated += UpdateSquareVisual;
        _gridSqaure.Visual = this; 
    }


    public void UpdateSquareVisual(GridSquare square)
    {
    }

    public void SetHover(bool hover)
    {
        hoverVisual.SetActive(hover);
    }
}
