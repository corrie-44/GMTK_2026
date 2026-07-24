using UnityEngine;
using MoreMountains.Feedbacks;
using UnityEngine.EventSystems;
using TMPro;
using MoreMountains.Feedbacks; 
public class GridSquareVisual : MonoBehaviour
{
    private GridSquare _gridSqaure;
    public GridSquare GridSquare => _gridSqaure;

    [SerializeField] GameObject hoverVisual;
    [SerializeField] private TMP_Text goalText;
    [SerializeField] private GameObject goalParent;
    [SerializeField] private Transform diodeVisual;
    [SerializeField] private MMF_Player diodeFeedbacks; 

    public void Init(GridSquare gridSquare)
    {
        _gridSqaure = gridSquare;
        _gridSqaure.OnSquareUpdated += UpdateSquareVisual;
        _gridSqaure.Visual = this;
        goalParent.SetActive(false);
        diodeVisual.gameObject.SetActive(false);
    }


    public void UpdateSquareVisual(GridSquare square)
    {
        if(GridSquare.SquareType == GridSqaureType.GOAL)
        {
            goalText.text = square.Element.Substring(1);
            goalParent.SetActive(true);
        }

        if(GridSquare.SquareType == GridSqaureType.DIODE)
        {
            //Quaternion forwardRot = Quaternion.LookRotation(new Vector3(GridSquare.DiodeDirection.x, 0, GridSquare.DiodeDirection.y), Vector3.down);
            Vector3 dir = new Vector3(GridSquare.DiodeDirection.x, 0, GridSquare.DiodeDirection.y);
            if (dir.x != 0) dir *= -1; //Don't know why, just go with it
            Quaternion fromTo = Quaternion.FromToRotation(diodeVisual.forward, dir);
            diodeVisual.rotation *= fromTo; 
            diodeVisual.gameObject.SetActive(true);
        }
    }

    public void SetHover(bool hover)
    {
        hoverVisual.SetActive(hover);
    }

    public void DiodeBump()
    {
        diodeFeedbacks.PlayFeedbacks(); 
    }
}
