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

    #region Structs

    [System.Serializable]
    private struct MultiArrowFormation
    {
        public List<Vector2> points;
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private Image              arrowImage;
    [SerializeField] private RectTransform      extraArrowContainer;
    [SerializeField] private GameObject         trailRenderer;
    [SerializeField] private Sprite[]           arrowImages; //The shapes that the arrow can be
    [Tooltip("Center Position needs to be index 0")]
    [SerializeField] List<MultiArrowFormation>  multiArrowPatterns;

    #endregion

    #region Private Variables

    //private bool reverse;
    private Direction                           arrowDirection;
    private bool                                upCorrect;
    private bool                                rightCorrect;
    private bool                                downCorrect;
    private bool                                leftCorrect;

    private Vector2                             startSwipePosition;
    private float                               startSwipeTime;
    private bool                                swiping;

    private IEnumerator                         trailCoroutine;

    private float                               boardXMin;
    private float                               boardXMax;

    private float                               performanceIndex;
    private float                               performanceTimer;

    private float                               keyArrowSize;

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

    private void Update()
    {
        performanceTimer += Time.deltaTime;
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        keyArrowSize                        = arrowImage.GetComponent<RectTransform>().sizeDelta.x;
        performanceTimer                    = 0f;
        performanceIndex                    = 0f;

        swiping                             = false;

        RectTransform rt                    = GetComponent<RectTransform>();

        boardXMin                           = rt.TransformPoint(rt.rect.min).x;
        boardXMax                           = rt.TransformPoint(rt.rect.max).x;

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
        swiping = false;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        InputManager.instance.OnStartTouch -= SwipeStart;
        InputManager.instance.OnEndTouch -= SwipeEnd;

        base.EndGame();
    }

    public override void Pause()
    {
        swiping = false;

        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
            trailCoroutine = null;
        }

        InputManager.instance.OnStartTouch -= SwipeStart;
        InputManager.instance.OnEndTouch -= SwipeEnd;
    }

    public override void Unpause()
    {
        InputManager.instance.OnStartTouch += SwipeStart;
        InputManager.instance.OnEndTouch += SwipeEnd;
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

        swiping = true;
        startSwipePosition = position;
        startSwipeTime = time;

        trailRenderer.SetActive(true);

        trailRenderer.transform.position = position;
        trailCoroutine = trailRenderer.GetComponent<SwipeTrailController>().Trail();
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

        if (upCorrect && Vector2.Dot(Vector2.up, swipeDirection) > directionThreshold)
            CorrectSwipe(position);
        else if (rightCorrect && Vector2.Dot(Vector2.right, swipeDirection) > directionThreshold)
            CorrectSwipe(position);
        else if (downCorrect && Vector2.Dot(Vector2.down, swipeDirection) > directionThreshold)
            CorrectSwipe(position);
        else if (leftCorrect && Vector2.Dot(Vector2.left, swipeDirection) > directionThreshold)
            CorrectSwipe(position);
        else
            IncorrectSwipe(position);
    }

    private void NextArrow()
    {
        //TODO: Move single arrows around
        //TODO: Add moving arrows

        foreach (Transform child in extraArrowContainer)
            Destroy(child.gameObject);

        int difficultyLevel = Formulas.GetDifficulty(BattleManager.instance.CurrentPlayerUnit.UnitInfo
                                                    , BattleManager.instance.CurrentEnemy.UnitInfo
                                                    , Helpful.BattleGameTypes.ArrowSwipe
                                                    , performanceIndex);
        
        Debug.Log("Difficulty Index: " + difficultyLevel.ToString());

        Color[] arrowColors = new Color[3] { Color.green, Color.red, Color.blue };

        int color = 0;

        if (difficultyLevel > 300)
        {
            color = Random.Range(0, 3);
        }
        else if (difficultyLevel > 50)
        {
            color = Random.Range(0, 2);
        }

        int shape = 0; //shape list defined in the arrowImages array from the inspector

        if (difficultyLevel > 400)
        {
            shape = Random.Range(0, 3);
        }
        else if (difficultyLevel > 200)
        {
            shape = Random.Range(0, 2);
        }
        //More shapes if square and triangle arent enough lol

        //0 == single arrow
        //1 == multiple arrow, one is different and the rest are the same. Diff arrow is key
        //2 == multiple arrow, all are random, center arrow is key

        int multiArrowBoard = 0;

        if (difficultyLevel > 700)
        {
            multiArrowBoard = Random.Range(0, 3);
        }
        else if (difficultyLevel > 500)
        {
            multiArrowBoard = Random.Range(0, 2);
        }

        MultiArrowFormation arrowPattern = new MultiArrowFormation();

        if (multiArrowBoard != 0)
        {
            //TODO: Maybe start with just a couple patterns and allow more to be chosen from as diff goes up
            arrowPattern = multiArrowPatterns[Random.Range(0, multiArrowPatterns.Count)];
        }

        int direct = Random.Range(0, 4);

        //Construct board
        arrowImage.color = arrowColors[color];
        arrowImage.sprite = arrowImages[shape];
        SetRandomArrowRotation(arrowImage.transform, direct, true); //Set the key arrows rotation
        SetSolution(color);

        //Create Multi Arrow board
        if (multiArrowBoard == 0) //Not multi arrow
        {
            arrowImage.transform.localPosition = Vector3.zero;
        }
        else if (multiArrowBoard == 1) //One different key
        {
            int eColor  = Random.Range(0, arrowColors.Length); //TODO: limit the colors/shapes if multi arrows
            int eImage  = Random.Range(0, arrowImages.Length); //      are avaiable at diff levels lower than some colors/shapes
            int eRot    = Random.Range(0, 4);

            //Ensure the extra arrows aren't the same as the key
            if (eColor == color && eImage == shape && eRot == direct)
                eRot = eRot + 1 > 3 ? 0 : eRot + 1;

            int keyIndex = Random.Range(0, arrowPattern.points.Count);

            arrowImage.transform.localPosition = arrowPattern.points[keyIndex];

            for (int i = 0; i < arrowPattern.points.Count; i++)
            {
                //Skip the key's position
                if (i == keyIndex)
                    continue;

                //Make the extra arrows
                GameObject a                = new GameObject();

                Image im                    = a.AddComponent<Image>();

                im.color                    = arrowColors[eColor];
                im.sprite                   = arrowImages[eImage];
                SetRandomArrowRotation(a.transform, eRot);

                a.transform.SetParent(extraArrowContainer);
                ((RectTransform)a.transform).sizeDelta  = new Vector2(keyArrowSize, keyArrowSize);
                a.transform.localPosition               = arrowPattern.points[i];
                a.transform.localScale                  = Vector3.one;
            }
        }
        else if (multiArrowBoard == 2) //Random, center is key
        {
            arrowImage.transform.localPosition = arrowPattern.points[0];

            int prevRot = direct;
            
            for (int i = 1; i < arrowPattern.points.Count; i++)
            {
                GameObject a                = new GameObject();

                Image im                    = a.AddComponent<Image>();

                im.color                    = arrowColors[Random.Range(0, arrowColors.Length)];
                im.sprite                   = arrowImages[Random.Range(0, arrowImages.Length)];
                int r                       = Random.Range(0, 4);

                if (prevRot == r) //This will ensure that all of the extra arrows arent the same
                    r = r + 1 > 3 ? 0 : r + 1;

                SetRandomArrowRotation(a.transform, r);

                a.transform.SetParent(extraArrowContainer);
                ((RectTransform)a.transform).sizeDelta = new Vector2(keyArrowSize, keyArrowSize);
                a.transform.localPosition = arrowPattern.points[i];
                a.transform.localScale = Vector3.one;
            }
        }
    }

    private void CorrectSwipe(Vector2 responseChargeOrigin)
    {
        //Faster responses will make it more difficult more quickly. Might have to tweak these values
        performanceIndex += 1f - performanceTimer > 1f ? 0f: (performanceTimer * .5f);
        performanceTimer = 0f;

        AudioManager.instance.Play("Go");

        object[] info = new object[2];
        info[0] = AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE;
        info[1] = responseChargeOrigin;

        Signal.Send("Battle", "AbilityChargeGenerated", info);

        info = new object[3];
        info[0] = Helpful.StatTypes.Speed;
        info[1] = 1;
        info[2] = BattleManager.instance.CurrentPlayerUnit.UnitInfo;

        Signal.Send("PartyManagement", "AwardExperience", info);

        info = new object[3];
        info[0] = Helpful.StatTypes.Observation;
        info[1] = 12;
        info[2] = BattleManager.instance.CurrentPlayerUnit.UnitInfo;

        Signal.Send("PartyManagement", "AwardExperience", info);

        NextArrow();
    }

    private void IncorrectSwipe(Vector2 responseChargeOrigin)
    {
        performanceIndex -= .25f;
        performanceTimer = 0f;

        AudioManager.instance.Play("No");

        object[] info = new object[2];
        info[0] = AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE;
        info[1] = responseChargeOrigin;

        Signal.Send("Battle", "AbilityChargeGenerated", info);

        NextArrow();
    }

    private void SetRandomArrowRotation(Transform tran, int direction, bool keyArrow = false)
    {
        if (direction == 0) //Up
        {
            tran.eulerAngles = new Vector3(0, 0, 90);
            if (keyArrow) arrowDirection = Direction.Up;
        }
        else if (direction == 1) //Default - Right
        {
            tran.eulerAngles = new Vector3(0, 0, 0);
            if (keyArrow) arrowDirection = Direction.Right;
        }
        else if (direction == 2) //Down
        {
            tran.eulerAngles = new Vector3(0, 0, -90);
            if (keyArrow) arrowDirection = Direction.Down;
        }
        else if (direction == 3) //Left
        {
            tran.eulerAngles = new Vector3(0, 0, 180);
            if (keyArrow) arrowDirection = Direction.Left;
        }
    }

    //0 - Green, same direction
    //1 - Red, opposite direction
    //2 - Blue, perpendicular direction
    private void SetSolution(int color)
    {
        if (color == 0) //Green
        {
            switch(arrowDirection)
            {
                case Direction.Up:
                    upCorrect       = true;
                    rightCorrect    = false;
                    downCorrect     = false;
                    leftCorrect     = false;
                    break;
                case Direction.Right:
                    upCorrect       = false;
                    rightCorrect    = true;
                    downCorrect     = false;
                    leftCorrect     = false;
                    break;
                case Direction.Down:
                    upCorrect       = false;
                    rightCorrect    = false;
                    downCorrect     = true;
                    leftCorrect     = false;
                    break;
                case Direction.Left:
                    upCorrect       = false;
                    rightCorrect    = false;
                    downCorrect     = false;
                    leftCorrect     = true;
                    break;
            }
        }
        else if (color == 1) //Red
        {
            switch(arrowDirection)
            {
                case Direction.Up:
                    upCorrect       = false;
                    rightCorrect    = false;
                    downCorrect     = true;
                    leftCorrect     = false;
                    break;
                case Direction.Right:
                    upCorrect       = false;
                    rightCorrect    = false;
                    downCorrect     = false;
                    leftCorrect     = true;
                    break;
                case Direction.Down:
                    upCorrect       = true;
                    rightCorrect    = false;
                    downCorrect     = false;
                    leftCorrect     = false;
                    break;
                case Direction.Left:
                    upCorrect       = false;
                    rightCorrect    = true;
                    downCorrect     = false;
                    leftCorrect     = false;
                    break;
            }
        }
        else if (color == 2) //Blue
        {
            switch(arrowDirection)
            {
                case Direction.Up:
                    upCorrect       = false;
                    rightCorrect    = true;
                    downCorrect     = false;
                    leftCorrect     = true;
                    break;
                case Direction.Right:
                    upCorrect       = true;
                    rightCorrect    = false;
                    downCorrect     = true;
                    leftCorrect     = false;
                    break;
                case Direction.Down:
                    upCorrect       = false;
                    rightCorrect    = true;
                    downCorrect     = false;
                    leftCorrect     = true;
                    break;
                case Direction.Left:
                    upCorrect       = true;
                    rightCorrect    = false;
                    downCorrect     = true;
                    leftCorrect     = false;
                    break;
            }
        }
    }

    #endregion
}
