using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;

public class TriviaModeEndScreenController : MonoBehaviour
{
    public TextMeshProUGUI  failSuccessDisplay;
    public TextMeshProUGUI  questionsCorrectDisplay;
    public TextMeshProUGUI  timeRemainingDisplay;
    public CanvasGroup      completedDotGroup;
    public GameObject       completedDot;
    public CanvasGroup      parTimeGroup;
    public GameObject       parTimeDot;
    public CanvasGroup      allQuestionsGroup;
    public GameObject       allQuestionsDot;

    public GameObject       buttonContainer;
    public GameObject       nextLevelButton;
    public GameObject       skipMenuAnimationButton;

    private SignalReceiver  trivia_endgame_receiver;
    private SignalStream    trivia_endgame_stream;

    private IEnumerator     menuLoading;

    private void Awake()
    {
        trivia_endgame_stream = SignalStream.Get("Trivia", "EndGame");

        trivia_endgame_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
    }

    private void OnEnable()
    {
        trivia_endgame_stream.ConnectReceiver(trivia_endgame_receiver);
    }

    private void OnDisable()
    {
        trivia_endgame_stream.DisconnectReceiver(trivia_endgame_receiver);
    }

    public void SetUp(Signal signal)
    {
        //Signal Data should be object[9]
        //  index 0 =>  bool    - true if won, false if timedout
        //  index 1 =>  int     - num of questions correct
        //  index 2 =>  int     - total num of querstions
        //  index 3 =>  float   - seconds remaining
        //  index 4 =>  bool    - whether there's a next level available
        //  index 5 =>  bool    - the completed dot's status (can appear even on a loss if the level was beaten previously)
        //  index 6 =>  bool    - the par time dot's status
        //  index 7 =>  bool    - the all questions dot's status
        //  index 8 =>  int     - level's par time

        object[] data = signal.GetValueUnsafe<object[]>();

        questionsCorrectDisplay.text = string.Format("Questions Correct - {0} / {1}", (int)data[1], (int)data[2]);

        if ((bool)data[0])
        {
            failSuccessDisplay.text = "~Success~";
            failSuccessDisplay.color = Color.green;
            
            System.TimeSpan ts = System.TimeSpan.FromSeconds((float)data[3]);
            System.TimeSpan ts2 = System.TimeSpan.FromSeconds((int)data[8]);
            timeRemainingDisplay.text = string.Format("Remaining - {0}:{1} ~ Par Time  - {2}:{3}"
                , ts.Minutes, ts.Seconds.ToString("00")
                , ts2.Minutes, ts2.Seconds.ToString("00"));
        }
        else
        {
            failSuccessDisplay.text = "Oh No :(";
            failSuccessDisplay.color = Color.red;

            timeRemainingDisplay.text = "Time Remaining - 0:00";
        }

        nextLevelButton.SetActive((bool)data[4]);

        questionsCorrectDisplay.alpha                       = 0;
        timeRemainingDisplay.alpha                          = 0;
        completedDotGroup.alpha                             = 0;
        parTimeGroup.alpha                                  = 0;
        allQuestionsGroup.alpha                             = 0;
        buttonContainer.GetComponent<CanvasGroup>().alpha   = 0;

        completedDot.SetActive((bool)data[5]);
        parTimeDot.SetActive((bool)data[6]);
        allQuestionsDot.SetActive((bool)data[7]);

        skipMenuAnimationButton.SetActive(true);
    }

    //Called from the View - Screen Trivia End's Show Animation's end animation callback
    public void StartMenuFadeIn()
    {
        menuLoading = MenuFadeIn();

        StartCoroutine(menuLoading);
    }

    public void SkipMenuAnimation()
    {
        skipMenuAnimationButton.SetActive(false);

        if (menuLoading != null)
        {
            StopCoroutine(menuLoading);
            menuLoading = null;
        }

        questionsCorrectDisplay.alpha                       = 1;
        timeRemainingDisplay.alpha                          = 1;
        completedDotGroup.alpha                             = 1;
        parTimeGroup.alpha                                  = 1;
        allQuestionsGroup.alpha                             = 1;
        buttonContainer.GetComponent<CanvasGroup>().alpha   = 1;
    }

    public void ReplayLevel()
    {
        object[] data = new object[1];
        data[0] = 1;

        Signal.Send("Trivia", "TriviaSetup", data);
    }

    public void NextLevel()
    {
        object[] data = new object[1];
        data[0] = 2;

        Signal.Send("Trivia", "TriviaSetup", data);
    }

    private IEnumerator MenuFadeIn()
    {
        yield return new WaitForSeconds(.25f);

        yield return FadeTextIn(questionsCorrectDisplay);

        yield return new WaitForSeconds(.25f);

        yield return FadeTextIn(timeRemainingDisplay);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(completedDotGroup);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(parTimeGroup);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(allQuestionsGroup);

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
