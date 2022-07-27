using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathPuzzleTile : MonoBehaviour
{
    #region Inspector Variables

    [Header("Needed Components")]
    [SerializeField] private RectTransform rectTran;

    [Space]

    [Header("Tile Exit Directions")]
    [SerializeField] private bool           north;
    [SerializeField] private bool           south;
    [SerializeField] private bool           east;
    [SerializeField] private bool           west;

    [Space]

    [Header("Special Tile Indicators")]
    [SerializeField] private bool           start;
    [SerializeField] private bool           finish;
    [SerializeField] private bool           nonpath;

    #endregion

    #region Private Variables

    private IEnumerator                     rotationCoroutine;
    private bool                            partOfPath;
    private Vector2                         gridPosition;

    #endregion

    #region Constants

    private const float                      rotationSpeed = 275f; //TODO: link this to a stat

    #endregion

    #region Public Properties

    public bool Start
    {
        get { return start; }
    }

    public bool Finish
    {
        get { return finish; }
    }

    public bool Nonpath
    {
        get { return nonpath; }
    }

    public bool North
    {
        get { return north; }
    }
    
    public bool South
    {
        get { return south; }
    }

    public bool East
    {
        get { return east; }
    }    

    public bool West
    {
        get { return west; }
    }

    public bool PartOfPath
    {
        get { return partOfPath; }
        set { partOfPath = value; }
    }

    public Vector2 GridPosition
    {
        get { return gridPosition; }
        set { gridPosition = value; }
    }

    #endregion

    #region Public Functions

    //Called by the Tile's OnClick behavior
    public void RotateClockwise()
    {
        if (start || finish || nonpath || rotationCoroutine != null)
        {
            //TODO: add something visual/auditory to show you can't move these pieces

            return;
        }

        rotationCoroutine = AnimateRotation(
           new Vector3(
            rectTran.eulerAngles.x,
            rectTran.eulerAngles.y,
            rectTran.eulerAngles.z - 90 <= -360 ? 0 : rectTran.eulerAngles.z - 90)
        );

        StartCoroutine(rotationCoroutine);
    }

    public void SetInitialRotation(int numberOfClockwiseTurnsFromInitial)
    {
        if (numberOfClockwiseTurnsFromInitial == 1)
        {
            rectTran.eulerAngles = new Vector3(
                rectTran.eulerAngles.x,
                rectTran.eulerAngles.y,
                rectTran.eulerAngles.z - 90);

            bool oldNorth   = north;
            bool oldSouth   = south;
            bool oldEast    = east;

            north           = west;
            east            = oldNorth;
            south           = oldEast;
            west            = oldSouth;
        }
        else if (numberOfClockwiseTurnsFromInitial == 2)
        {
            rectTran.eulerAngles = new Vector3(
                rectTran.eulerAngles.x,
                rectTran.eulerAngles.y,
                rectTran.eulerAngles.z - 180);

            bool oldNorth   = north;
            bool oldWest    = west;
            bool oldEast    = east;

            north           = south;
            east            = oldWest;
            south           = oldNorth;
            west            = oldEast;
        }
        else if (numberOfClockwiseTurnsFromInitial == 3) //Probably can make this just an "else"
        {
            rectTran.eulerAngles = new Vector3(
                rectTran.eulerAngles.x,
                rectTran.eulerAngles.y,
                rectTran.eulerAngles.z - 270);

            bool oldNorth   = north;
            bool oldWest    = west;
            bool oldSouth   = south;

            north           = east;
            east            = oldSouth;
            south           = oldWest;
            west            = oldNorth;
        }
    }

    public void MarkTileAsConnectedToStart()
    {
        if (partOfPath)
            GetComponent<Image>().color = Color.red;
        else
            GetComponent<Image>().color = Color.white;
    }

    #endregion

    #region Private Functions

    private IEnumerator AnimateRotation(Vector3 target)
    {
        Vector3 startMarker     = rectTran.eulerAngles;
        float journeyLength     = Vector3.Distance(startMarker, target);
        float startTime         = Time.time;

        while (rectTran.eulerAngles.z != target.z 
            && rectTran.eulerAngles.z + 360 != target.z 
            && rectTran.eulerAngles.z - 360 != target.z)
        {
            float distCovered       = (Time.time - startTime) * rotationSpeed;
            float fractionOfJourney = distCovered / journeyLength;

            rectTran.eulerAngles    = Vector3.Lerp(startMarker, target, fractionOfJourney);

            yield return null;
        }

        rectTran.eulerAngles = target;

        rotationCoroutine = null;

        bool oldNorth   = north;
        bool oldSouth   = south;
        bool oldEast    = east;

        north           = west;
        east            = oldNorth;
        south           = oldEast;
        west            = oldSouth;

        Signal.Send("PathPuzzle", "TileRotated", gameObject);

        object[] info   = new object[2];
        info[0]         = AbilityCharger.AbilityChargeActions.TILE_ROTATED;
        info[1]         = (Vector2)transform.position;

        Signal.Send("Battle", "AbilityChargeGenerated", info);
    }

    #endregion
}
