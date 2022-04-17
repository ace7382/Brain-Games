using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;

public class WordScrambleEndScreenController : MonoBehaviour
{
    public TextMeshProUGUI      wordsFoundText;

    public CanvasGroup          goal1DotGroup;
    public TextMeshProUGUI      goal1DotTitle;
    public GameObject           goal1Dot;
    public CanvasGroup          goal2DotGroup;
    public TextMeshProUGUI      goal2DotTitle;
    public GameObject           goal2Dot;
    public CanvasGroup          specialWordDotGroup;
    public GameObject           specialWordDot;

    public GameObject           buttonContainer;
    public GameObject           nextLevelButton;
    public GameObject           skipMenuAnimationButton;

    private SignalReceiver      wordscramble_endgame_receiver;
    private SignalStream        wordscramble_endgame_stream;

    private IEnumerator         menuLoading;

    private void Awake()
    {
        wordscramble_endgame_stream = SignalStream.Get("WordScramble", "EndGame");

        wordscramble_endgame_receiver = new SignalReceiver().SetOnSignalCallback(SetUp);
    }

    private void OnEnable()
    {
        wordscramble_endgame_stream.ConnectReceiver(wordscramble_endgame_receiver);
    }

    private void OnDisable()
    {
        wordscramble_endgame_stream.DisconnectReceiver(wordscramble_endgame_receiver);
    }

    public void SetUp(Signal signal)
    {
        //Signal Data should be object[1]
        //  index 0 =>  WordScrambleLevel   - the level that was just played

        object[] data = signal.GetValueUnsafe<object[]>();
        WordScrambleLevel level = (WordScrambleLevel)data[0];

        wordsFoundText.text     = string.Format("{0} words found", level.foundWords.Count);
        goal1DotTitle.text      = string.Format("{0} words", level.goalWordCount);
        goal2DotTitle.text      = string.Format("{0} words", level.secondGoalWordCount);

        //You shouldn't be able to get to the end screen without meeting at least the first goal of the level
        //TODO: probably just make the button active in the scene and take this line out
        nextLevelButton.SetActive(level.nextLevel != null); 

        wordsFoundText.alpha                                = 0;
        goal1DotGroup.alpha                                 = 0;
        goal2DotGroup.alpha                                 = 0;
        specialWordDotGroup.alpha                           = 0;
        buttonContainer.GetComponent<CanvasGroup>().alpha   = 0;

        goal1Dot.SetActive(level.foundWords.Count >= level.goalWordCount);
        goal2Dot.SetActive(level.foundWords.Count >= level.secondGoalWordCount);
        specialWordDot.SetActive(level.foundWords.Contains(level.specialWord));

        skipMenuAnimationButton.SetActive(true);
    }

    //Called from the View - Screen WordScramble End's Show Animation's end animation callback
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

        wordsFoundText.alpha                                = 1;
        goal1DotGroup.alpha                                 = 1;
        goal2DotGroup.alpha                                 = 1;
        specialWordDotGroup.alpha                           = 1;
        buttonContainer.GetComponent<CanvasGroup>().alpha   = 1;
    }
    public void ReplayLevel()
    {
        object[] data = new object[1];
        data[0] = 1;

        Signal.Send("WordScramble", "WordScrambleSetup", data);
    }

    public void NextLevel()
    {
        object[] data = new object[1];
        data[0] = 2;

        Signal.Send("WordScramble", "WordScrambleSetup", data);
    }

    private IEnumerator MenuFadeIn()
    {
        yield return new WaitForSeconds(.25f);

        yield return FadeTextIn(wordsFoundText);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(goal1DotGroup);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(goal2DotGroup);

        yield return new WaitForSeconds(.25f);

        yield return FadeCanvasIn(specialWordDotGroup);

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
