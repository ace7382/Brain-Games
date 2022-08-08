using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;

public class EndLevelScreenContoller : MonoBehaviour
{
    public TextMeshProUGUI      failSuccessDisplay;
    public TextMeshProUGUI      subtitleText;

    public CanvasGroup          objective1Group;
    public TextMeshProUGUI      objective1Title;
    public GameObject           objective1Dot;

    public CanvasGroup          objective2Group;
    public TextMeshProUGUI      objective2Title;
    public GameObject           objective2Dot;

    public CanvasGroup          objective3Group;
    public TextMeshProUGUI      objective3Title;
    public GameObject           objective3Dot;

    public GameObject           buttonContainer;
    public GameObject           nextLevelButton;
    public GameObject           skipMenuAnimationButton;

    private SignalReceiver      endlevelscreen_setup_receiver;
    private SignalStream        endlevelscreen_setup_stream;

    private IEnumerator         menuLoading;

    private LevelBase           levelCompleted;
    private LevelResultsData    levelResults;

    private void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.popupScreenOrderInLayer;

        endlevelscreen_setup_stream = SignalStream.Get("EndLevelScreen", "Setup");

        endlevelscreen_setup_receiver = new SignalReceiver().SetOnSignalCallback(Setup);
    }

    private void OnEnable()
    {
        endlevelscreen_setup_stream.ConnectReceiver(endlevelscreen_setup_receiver);
    }

    private void OnDisable()
    {
        endlevelscreen_setup_stream.DisconnectReceiver(endlevelscreen_setup_receiver);
    }

    public void Setup(Signal signal)
    {
        //levelCompleted  = GameManager.instance.currentLevelOLD;
        //levelResults    = GameManager.instance.currentLevelResults;

        if (levelResults.successIndicator)
        {
            failSuccessDisplay.text = "~Success~";
            failSuccessDisplay.color = Color.green;
        }
        else
        {
            failSuccessDisplay.text = "Oh No :(";
            failSuccessDisplay.color = Color.red;
        }

        subtitleText.text = levelResults.subtitleText;

        //TODO: Update this
        //nextLevelButton.SetActive(levelCompleted.levelsUnlockedByThisLevel == null ? false : levelCompleted.levelsUnlockedByThisLevel.unlocked);
        nextLevelButton.SetActive(true); //This replaces the above line for now

        objective1Group.alpha = 0;
        objective2Group.alpha = 0;
        objective3Group.alpha = 0;

        objective1Title.text = Helpful.GetLevelObjectiveTitles(levelCompleted, 1);
        objective2Title.text = Helpful.GetLevelObjectiveTitles(levelCompleted, 2);
        objective3Title.text = Helpful.GetLevelObjectiveTitles(levelCompleted, 3);

        buttonContainer.GetComponent<CanvasGroup>().alpha = 0;

        objective1Dot.SetActive(levelCompleted.objective1);
        objective2Dot.SetActive(levelCompleted.objective2);
        objective3Dot.SetActive(levelCompleted.objective3);

        skipMenuAnimationButton.SetActive(true);

        Signal.Send("EndLevelScreen", "ShowScreen");
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
    }

    //Called by the Next Level button's Click
    public void NextLevel()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "PlayNextLevel");
    }

    private IEnumerator MenuFadeIn()
    {
        WaitForSeconds w = new WaitForSeconds(.25f);

        yield return w;

        yield return Helpful.FadeCanvasIn(objective1Group, .75f);

        yield return w;

        yield return Helpful.FadeCanvasIn(objective2Group, .75f);

        yield return w;

        yield return Helpful.FadeCanvasIn(objective3Group, .75f);

        yield return w;

        yield return Helpful.FadeCanvasIn(buttonContainer.GetComponent<CanvasGroup>(), .75f);

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
}
