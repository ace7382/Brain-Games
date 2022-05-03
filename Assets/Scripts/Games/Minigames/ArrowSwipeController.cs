using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowSwipeController : MonoBehaviour
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

    [SerializeField] private Image                      arrowImage;
    [SerializeField] private CountdownClockController   countdownClock;

    #endregion

    #region Private Variables

    private bool                reverse;
    private Direction           arrowDirection;
    private int                 correctSwipes;
    private int                 incorrectSwipes;

    private Vector2             startSwipePosition;
    private float               startSwipeTime;

    #endregion

    #region Signal Variables

    private SignalReceiver      gamemanagement_gamesetup_receiver;
    private SignalStream        gamemanagement_gamesetup_stream;
    private SignalReceiver      quitconfirmation_exitlevel_receiver;
    private SignalStream        quitconfirmation_exitlevel_stream;
    private SignalReceiver      quitconfirmation_backtogame_receiver;
    private SignalStream        quitconfirmation_backtogame_stream;
    private SignalReceiver      quitconfirmation_popup_receiver;
    private SignalStream        quitconfirmation_popup_stream;

    #endregion

    private void Awake()
    {
        gamemanagement_gamesetup_stream     = SignalStream.Get("GameManagement", "GameSetup");
        quitconfirmation_exitlevel_stream   = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream  = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream       = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_gamesetup_receiver   = new SignalReceiver().SetOnSignalCallback(Setup);
    }

    private void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    public void Setup(Signal signal)
    {
        correctSwipes   = 0;
        incorrectSwipes = 0;

        //countdownClock.SetupTimer(30f, 0f, false);

        NextArrow();
    }

    //Called by the GamePlay view's Shown callback
    public void StartGame()
    {
        InputManager.instance.OnStartTouch += SwipeStart;
        InputManager.instance.OnEndTouch += SwipeEnd;

        //countdownClock.StartTimer();
    }

    private void SwipeStart(Vector2 position, float time)
    {
        startSwipePosition = position;
        startSwipeTime = time;
    }

    private void SwipeEnd(Vector2 position, float time)
    {
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
        correctSwipes++;

        AudioManager.instance.Play("Go");

        NextArrow();
    }

    private void IncorrectSwipe()
    {
        incorrectSwipes++;

        AudioManager.instance.Play("No");

        NextArrow();
    }

    private void SetRandomArrowRotation(int direction)
    {
        if (direction == 0) //Default, up
        {
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, 0);
            arrowDirection                      = Direction.Up;
        }
        else if (direction == 1) //Right
        {
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, -90);
            arrowDirection                      = Direction.Right;
        }
        else if (direction == 2) //Down
        {
            arrowImage.transform.eulerAngles    = new Vector3(0, 0, 180);
            arrowDirection                      = Direction.Down;
        }
        else if (direction == 3) //Left
        {
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
}
