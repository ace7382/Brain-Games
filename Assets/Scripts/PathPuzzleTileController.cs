using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Signals;

[System.Serializable]
public class PathPuzzleTileController : MonoBehaviour
{
    public Vector2                              gridPosition;

    public float                                rotationSpeed;
    public float                                shakeTime;
    public float                                shakeMagnitude;

    public bool                                 north;
    public bool                                 south;
    public bool                                 east;
    public bool                                 west;

    public bool                                 start;
    public bool                                 finish;
    public bool                                 nonpath;
    public bool                                 partOfPath;

    [SerializeField] private RectTransform      rectTran;

    private IEnumerator                         rotCo;
    private bool                                queueRotation = false;

    public void RotateClockwise()
    {
        if (start || finish || nonpath)
        {
            if (rotCo == null)
            {
                rotCo = Shake();

                StartCoroutine(rotCo);
            }

            return;
        }

        if (rotCo != null)
        {
            queueRotation = true;
            return;
        }

        rotCo = AnimateRotation(
           new Vector3(
            rectTran.eulerAngles.x,
            rectTran.eulerAngles.y,
            rectTran.eulerAngles.z - 90 <= -360 ? 0 : rectTran.eulerAngles.z - 90)
        );

        StartCoroutine(rotCo);

        bool oldNorth   = north;
        bool oldSouth   = south;
        bool oldEast    = east;

        north   = west; //west hasn't changed yet so we don't need an "old" tracker
        east    = oldNorth;
        south   = oldEast;
        west    = oldSouth;
    }

    public void SetInitialRotation(int numberOfClockwiseTurnsFromInitial)
    {
        //if (start || finish || nonpath || numberOfClockwiseTurnsFromInitial == 0)
        //    return;

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
        if(partOfPath)
            GetComponent<Image>().color = Color.red;
        else
            GetComponent<Image>().color = Color.white;
    }

    private IEnumerator AnimateRotation(Vector3 target)
    {
        Vector3 startMarker = rectTran.eulerAngles;
        float journeyLength = Vector3.Distance(startMarker, target);
        float startTime = Time.time;

        while (rectTran.eulerAngles.z != target.z && rectTran.eulerAngles.z + 360 != target.z && rectTran.eulerAngles.z - 360 != target.z)
        {
            float distCovered = (Time.time - startTime) * rotationSpeed;
            float fractionOfJourney = distCovered / journeyLength;

            rectTran.eulerAngles = Vector3.Lerp(startMarker, target, fractionOfJourney);

            yield return null;
        }

        rectTran.eulerAngles = target;

        rotCo = null;

        Signal.Send("PathPuzzle", "TileRotated");

        yield return new WaitForSeconds(.1f);

        if (queueRotation)
        {
            queueRotation = false;
            RotateClockwise();
        }
    }

    private IEnumerator Shake()
    {
        float startingZ = rectTran.eulerAngles.z;
        float tempShakeTime = shakeTime;
        bool direction = false; //Up

        while (tempShakeTime > 0)
        {
            if (direction)
            {
                rectTran.eulerAngles = new Vector3(0, 0, Mathf.Clamp(rectTran.eulerAngles.z + rotationSpeed, startingZ - shakeMagnitude, startingZ + shakeMagnitude));
            }
            else
            {
                rectTran.eulerAngles = new Vector3(0, 0, Mathf.Clamp(rectTran.eulerAngles.z - rotationSpeed, startingZ - shakeMagnitude, startingZ + shakeMagnitude));
            }

            if (Mathf.Abs(rectTran.eulerAngles.z) >= Mathf.Abs(startingZ) + shakeMagnitude 
                || Mathf.Abs(rectTran.eulerAngles.z) <= Mathf.Abs(startingZ) - shakeMagnitude)
                direction = !direction;

            tempShakeTime -= Time.deltaTime;

            yield return null;
        }

        rectTran.eulerAngles = new Vector3(0, 0, startingZ);

        rotCo = null;
    }
}
