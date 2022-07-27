using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleGame_ArrowSwipe : BattleGameControllerBase
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

    [SerializeField] private Image          arrowImage;
    [SerializeField] private GameObject     trailRenderer;

    #endregion

    #region Private Variables

    private bool                            reverse;
    private Direction                       arrowDirection;

    private Vector2                         startSwipePosition;
    private float                           startSwipeTime;
    private bool                            swiping;

    private IEnumerator                     trailCoroutine;

    private float                           boardXMin;
    private float                           boardXMax;

    #endregion

    #region Unity Functions

    protected override void OnDisable()
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        base.OnDisable();
    }

    protected void OnDestroy()
    {
        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        swiping             = false;

        RectTransform rt    = GetComponent<RectTransform>();

        boardXMin           = rt.TransformPoint(rt.rect.min).x;
        boardXMax           = rt.TransformPoint(rt.rect.max).x;

        InputManager.instance.OnStartTouch  += SwipeStart;
        InputManager.instance.OnEndTouch    += SwipeEnd;

        NextArrow();
    }

    public override void BoardReset()
    {
        NextArrow();
    }

    public override void EndGame()
    {
        swiping                             = false;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd;

        base.EndGame();
    }

    public override void Pause()
    {
        swiping                             = false;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        InputManager.instance.OnStartTouch  -= SwipeStart;
        InputManager.instance.OnEndTouch    -= SwipeEnd; 
    }

    public override void Unpause()
    {
        InputManager.instance.OnStartTouch  += SwipeStart;
        InputManager.instance.OnEndTouch    += SwipeEnd;
    }

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.ArrowSwipe);
    }

    #endregion

    #region Private Functions

    private void SwipeStart(Vector2 position, float time)
    {
        if (position.x < boardXMin || position.x > boardXMax)
            return;

        swiping                             = true;
        startSwipePosition                  = position;
        startSwipeTime                      = time;

        trailRenderer.SetActive(true);

        trailRenderer.transform.position    = position;
        trailCoroutine                      = trailRenderer.GetComponent<SwipeTrailController>().Trail();
        StartCoroutine(trailCoroutine);

    }

    private void SwipeEnd(Vector2 position, float time)
    {
        if (!swiping)
            return;

        swiping = false;

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

            ProcessSwipe(direction2D, position);
        }
    }

    private void ProcessSwipe(Vector2 swipeDirection, Vector2 position)
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
            CorrectSwipe(position);
        else
            IncorrectSwipe(position);
    }

    private void NextArrow()
    {
        SetReverse();

        SetRandomArrowRotation(Random.Range(0, 4));
    }

    private void CorrectSwipe(Vector2 responseChargeOrigin)
    {
        AudioManager.instance.Play("Go");

        object[] info   = new object[2];
        info[0]         = AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE;
        info[1]         = responseChargeOrigin;

        Signal.Send("Battle", "AbilityChargeGenerated", info);

        info            = new object[3];
        info[0]         = Helpful.StatTypes.Responsiveness;
        info[1]         = 1;
        info[2]         = BattleManager.instance.CurrentPlayerUnit.UnitInfo;

        Signal.Send("PartyManagement", "AwardExperience", info);

        NextArrow();
    }

    private void IncorrectSwipe(Vector2 responseChargeOrigin)
    {
        AudioManager.instance.Play("No");

        //Signal.Send("Battle", "IncorrectResponse");

        object[] info   = new object[2];
        info[0]         = AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE;
        info[1]         = responseChargeOrigin;

        Signal.Send("Battle", "AbilityChargeGenerated", info);

        NextArrow();
    }

    private void SetRandomArrowRotation(int direction)
    {
        if (direction == 0) //Default, up
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 0);
            arrowDirection = Direction.Up;
        }
        else if (direction == 1) //Right
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, -90);
            arrowDirection = Direction.Right;
        }
        else if (direction == 2) //Down
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 180);
            arrowDirection = Direction.Down;
        }
        else if (direction == 3) //Left
        {
            arrowImage.transform.eulerAngles = new Vector3(0, 0, 90);
            arrowDirection = Direction.Left;
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
