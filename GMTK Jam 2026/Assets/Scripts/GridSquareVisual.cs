using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;
using TMPro; 
public class GridSquareVisual : MonoBehaviour
{
    private GridSquare _gridSqaure;
    public GridSquare GridSquare => _gridSqaure;

    [SerializeField] GameObject hoverVisual;
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private GameObject goalParent; 

    public void Init(GridSquare gridSquare)
    {
        _gridSqaure = gridSquare;
        _gridSqaure.OnSquareUpdated += UpdateSquareVisual;
        _gridSqaure.Visual = this;
        goalParent.SetActive(false);
    }


    public void UpdateSquareVisual(GridSquare square)
    {
        if(GridSquare.SquareType == GridSqaureType.GOAL)
        {
            goalText.text = square.Element.Substring(1);
            goalParent.SetActive(true);
        }
    }

    public void SetHover(bool hover)
    {
        hoverVisual.SetActive(hover);
    }
}
