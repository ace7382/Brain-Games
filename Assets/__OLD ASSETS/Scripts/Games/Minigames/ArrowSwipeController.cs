using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowSwipeController : TimedMinigameController
{
    #region Enums

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private Image                              arrowImage;

    [SerializeField] private GameObject                         trailRenderer;

    #endregion

    #region Private Variables

    private bool                reverse;
    private Direction           arrowDirection;

    private Vector2             startSwipePosition;
    private float               startSwipeTime;

    private IEnumerator         trailCoroutine;

    #endregion

    #region Unity Functions

    protected override void OnDisable()
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        base.OnDisable();
    }

    #endregion

    #region Public Functions

    //Called by the GamePlay view's Shown callback
    public override void StartGame()
    {
        InputManager.instance.OnStartTouch  += SwipeStart;
        InputManager.instance.OnEndTouch    += SwipeEnd;

        base.StartGame();
    }

    //Invoked by the Countdown Clock's OnOutOfTime Event
    public override void EndGame()
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        base.EndGame();
    }

    #endregion

    #region Override Functions

    protected override void Setup(Signal signal)
    {
        base.Setup(signal);

        NextArrow();
    }

    protected override void Pause(Signal signal)
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        base.Pause(signal);
    }

    protected override void Unpause(Signal signal)
    {
        InputManager.instance.OnStartTouch  += SwipeStart;
        InputManager.instance.OnEndTouch    += SwipeEnd;

        base.Unpause(signal);
    }

    protected override IEnumerator EndAnimation()
    {
        arrowImage.color = Color.grey;

        return base.EndAnimation();
    }

    #endregion

    #region Private Functions

    private void SwipeStart(Vector2 position, float time)
    {
        startSwipePosition = position;
        startSwipeTime = time;

        trailRenderer.SetActive(true);
        trailRenderer.transform.position = position;
        trailCoroutine = trailRenderer.GetComponent<SwipeTrailController>().Trail();
        StartCoroutine(trailCoroutine);
    }

    private void SwipeEnd(Vector2 position, float time)
    {
        trailRenderer.SetActive(false);

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        if (Vector3.Distance(startSwipePosition, position) >= InputManager.instance.minimumSwipeDistance
            && (time - startSwipeTime) <= InputManager.instance.maximumSwipeTime)
        {
            Debug.DrawLine(startSwipePosition, position, Color.red, 3f);

            Vector3 direction = position - startSwipePosition;
            Vector2 direction2D = new Vector2(direction.x, direction.y).normalized;

            ProcessSwipe(direction2D);
        }
    }

    private void ProcessSwipe(Vector2 swipeDirection)
    {
        float directionThreshold = .9f; //Can be between 0 and 1. Controls how close to the desired value you need to be swiping
        Vector2 correctDirection;

        if (arrowDirection == Direction.Up)
            correctDirection = reverse ? Vector2.down : Vector2.up;
        else if (arrowDirection == Direction.Down)
            correctDirection = reverse ? Vector2.up : Vector2.down;
        else if (arrowDirection == Direction.Left)
            correctDirection = reverse ? Vector2.right : Vector2.left;
        else
            correctDirection = reverse ? Vector2.left : Vector2.right;

        if (Vector2.Dot(correctDirection, swipeDirection) > directionThreshold)
            CorrectSwipe();
        else
            IncorrectSwipe();
    }

    private void NextArrow()
    {
        SetReverse();

        SetRandomArrowRotation(Random.Range(0, 4));
    }

    private void CorrectSwipe()
    {
        correctResponses++;

        AudioManager.instance.Play("Go");

        NextArrow();
    }

    private void IncorrectSwipe()
    {
        incorrectResponses++;

        AudioManager.instance.Play("No");

        NextArrow();
    }

    private void SetRandomArrowRotation(int direction)
    {
        if (direction == 0) //Default, up
        {
            Debug.Log("Up");
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, 0);
            arrowDirection                      = Direction.Up;
        }
        else if (direction == 1) //Right
        {
            Debug.Log("Right");
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, -90);
            arrowDirection                      = Direction.Right;
        }
        else if (direction == 2) //Down
        {
            Debug.Log("Down");
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, 180);
            arrowDirection                      = Direction.Down;
        }
        else if (direction == 3) //Left
        {
            Debug.Log("Left");
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, 90);
            arrowDirection                      = Direction.Left;
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

    #endregion
}
