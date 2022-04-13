using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathPuzzleTileController : MonoBehaviour
{
    public Vector2  gridPosition;

    public bool     north;
    public bool     south;
    public bool     east;
    public bool     west;

    public bool     start;
    public bool     finish;
    public bool     partOfPath;

    [SerializeField] private RectTransform rectTran;

    public void RotateClockwise()
    {
        rectTran.eulerAngles = new Vector3(
            rectTran.eulerAngles.x,
            rectTran.eulerAngles.y,
            rectTran.eulerAngles.z - 90 <= -360 ? 0 : rectTran.eulerAngles.z - 90
            ) ;

        bool oldNorth   = north;
        bool oldSouth   = south;
        bool oldEast    = east;
        //bool oldWest    = west;

        //north   = oldWest;
        north   = west;
        east    = oldNorth;
        south   = oldEast;
        west    = oldSouth;
    }
}
