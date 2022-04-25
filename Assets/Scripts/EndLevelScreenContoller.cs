using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;

public class EndLevelScreenContoller : MonoBehaviour
{
    public TextMeshProUGUI  failSuccessDisplay;
    public TextMeshProUGUI  subtitleText;

    public CanvasGroup      objective1Group;
    public TextMeshProUGUI  objective1Title;
    public GameObject       objective1Dot;

    public CanvasGroup      objective2Group;
    public TextMeshProUGUI  objective2Title;
    public GameObject       objective2Dot;

    public CanvasGroup      objective3Group;
    public TextMeshProUGUI  objective3Title;
    public GameObject       objective3Dot;

    public GameObject       buttonContainer;
    public GameObject       nextLevelButton;
    public GameObject       skipMenuAnimationButton;

    private SignalReceiver  gamemanagement_levelended_receiver;
    private SignalStream    gamemanagement_levelended_stream;

    private IEnumerator     menuLoading;

    private LevelBase       levelCompleted;

    private void Awake()
    {
        gamemanagement_levelended_stream = SignalStream.Get("GameManagement", "LevelEnded");

        gamemanagement_levelended_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
    }

    private void OnEnable()
    {
        gamemanagement_levelended_stream.ConnectReceiver(gamemanagement_levelended_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_levelended_stream.DisconnectReceiver(gamemanagement_levelended_receiver);
    }

    public void SetUp(Signal signal)
    {
        //Signal Data should be object[3]
        //  index 0 =>  LevelBase       - The level that was just completed/exited
        //  index 1 =>  bool            - true = success, false = exit early/fail
        //  index 2 =>  string          - subtitle text

        object[] data = signal.GetValueUnsafe<object[]>();

        levelCompleted = (LevelBase)data[0];

        if ((bool)data[1])
        {
            failSuccessDisplay.text = "~Success~";
            failSuccessDisplay.color = Color.green;
        }
        else
        {
            failSuccessDisplay.text = "Oh No :(";
            failSuccessDisplay.color = Color.red;
        }

        subtitleText.text = (string)data[2];

        nextLevelButton.SetActive(levelCompleted.nextLevel == null ? false : levelCompleted.nextLevel.unlocked);

        objective1Group.alpha   = 0;
        objective2Group.alpha   = 0;
        objective3Group.alpha   = 0;

        objective1Title.text    = Helpful.GetLevelObjectiveTitles(levelCompleted, 1);
        objective2Title.text    = Helpful.GetLevelObjectiveTitles(levelCompleted, 2);
        objective3Title.text    = Helpful.GetLevelObjectiveTitles(levelCompleted, 3);

        buttonContainer.GetComponent<CanvasGroup>().alpha = 0;

        objective1Dot.SetActive(levelCompleted.objective1);
        objective2Dot.SetActive(levelCompleted.objective2);
        objective3Dot.SetActive(levelCompleted.objective3);

        skipMenuAnimationButton.SetActive(true);
    }

    //Called from the View - Screen End of Level's Show Animation's end animation callback
    public void StartMenuFadeIn()
    {
        menuLoading = MenuFadeIn();

        StartCoroutine(menuLoading);
    }

    //Called by the invisible button's click
    public void SkipMenuAnimation()
    {
        skipMenuAnimationButton.SetActive(false);

        if (menuLoading != null)
        {
            StopCoroutine(menuLoading);
            menuLoading = null;
        }

        objective1Group.alpha = 1;
        objective2Group.alpha = 1;
        objective3Group.alpha = 1;

        buttonContainer.GetComponent<CanvasGroup>().alpha = 1;
    }

    //Called by the Reply Level button's Click
    public void ReplayLevel()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "ReplayCurrentLevel");
        //object[] data = new object[1];
        //data[0] = 1;

        //System.Type t = levelCompleted.GetType();

        //if (t == typeof(PathPuzzleLevel))
        //{
        //    Signal.Send("PathPuzzle", "PathPuzzleSetup", data);
        //}
        //else if (t == typeof(TimedTriviaLevel))
        //{
        //    Signal.Send("Trivia", "TriviaSetup", data);
        //}
        //else if (t == typeof(WordScrambleLevel))
        //{
        //    Signal.Send("WordScramble", "WordScrambleSetup", data);
        //}
    }

    //Called by the Next Level button's Click
    public void NextLevel()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "PlayNextLevel");

        //object[] data = new object[1];
        //data[0] = 2;

        //System.Type t = levelCompleted.GetType();

        //if (t == typeof(PathPuzzleLevel))
        //{
        //    Signal.Send("PathPuzzle", "PathPuzzleSetup", data);
        //}
        //else if (t == typeof(TimedTriviaLevel))
        //{
        //    Signal.Send("Trivia", "TriviaSetup", data);
        //}
        //else if (t == typeof(WordScrambleLevel))
        //{
        //    Signal.Send("WordScramble", "WordScrambleSetup", data);
        //}
    }

    private IEnumerator MenuFadeIn()
    {
        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(objective1Group);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(objective2Group);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(objective3Group);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(buttonContainer.GetComponent<CanvasGroup>());

        skipMenuAnimationButton.SetActive(false);
    }

    private IEnumerator FadeTextIn(TextMeshProUGUI t)
    {
        while (t.color.a < 1)
        {
            float newAlpha = Mathf.MoveTowards(t.color.a, 1, .75f * Time.deltaTime);
            t.color = new Color(t.color.r, t.color.g, t.color.b, newAlpha);

            yield return null;
        }
    }

    private IEnumerator FadeCanvasIn(CanvasGroup c)
    {
        while (c.alpha < 1)
        {
            c.alpha = Mathf.MoveTowards(c.alpha, 1, .75f * Time.deltaTime);

            yield return null;
        }
    }
}
