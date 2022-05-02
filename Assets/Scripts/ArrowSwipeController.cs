using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowSwipeController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image                      arrowImage;
    [SerializeField] private CountdownClockController   countdownClock;

    #endregion

    #region Private Variables

    private bool    reverse;
    private int     correctSwipes;
    private int     incorrectSwipes;

    #endregion

    public void Setup()
    {
        correctSwipes   = 0;
        incorrectSwipes = 0;

        countdownClock.SetupTimer(30f, 0f, false);

        NextArrow();
    }

    public void StartGame()
    {
        countdownClock.StartTimer();
    }

    private void NextArrow()
    {
        SetReverse();
        SetRandomArrowRotation(Random.Range(0, 4));
    }

    private void SetRandomArrowRotation(int direction)
    {
        if (direction == 0) //Default, up
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (direction == 1) //Right
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, -90);
        }
        else if (direction == 2) //Down
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 180);
        }
        else if (direction == 3) //Up
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 90);
        }
    }

    private void SetReverse()
    {
        int r = Random.Range(0, 2);

        reverse = (r == 0);

        if (reverse)
        {
            arrowImage.color = Color.red;
        }
        else
        {
            arrowImage.color = Color.green;
        }    
    }
}
