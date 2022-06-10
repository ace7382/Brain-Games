using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimedMinigameController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] protected CountdownProgressBarController countdownProgress;
    [SerializeField] protected CanvasGroup                    gameElementsCanvasGroup;
    [SerializeField] protected CanvasGroup                    exitButtonCanvasGroup;

    #endregion

    #region Private Variables

    protected int                   correctResponses;
    protected int                   incorrectResponses;

    protected MinigameResultsData   results;

    #endregion

    #region Signal Variables

    protected SignalReceiver        gamemanagement_gamesetup_receiver;
    protected SignalStream          gamemanagement_gamesetup_stream;
    protected SignalReceiver        quitconfirmation_exitlevel_receiver;
    protected SignalStream          quitconfirmation_exitlevel_stream;
    protected SignalReceiver        quitconfirmation_backtogame_receiver;
    protected SignalStream          quitconfirmation_backtogame_stream;
    protected SignalReceiver        quitconfirmation_popup_receiver;
    protected SignalStream          quitconfirmation_popup_stream;

    #endregion

    #region Unity Functions

    protected virtual void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        gamemanagement_gamesetup_stream         = SignalStream.Get("GameManagement", "GameSetup");
        quitconfirmation_exitlevel_stream       = SignalStream.Get("QuitConfirmation", "ExitLevel");
        quitconfirmation_backtogame_stream      = SignalStream.Get("QuitConfirmation", "BackToGame");
        quitconfirmation_popup_stream           = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_gamesetup_receiver       = new SignalReceiver().SetOnSignalCallback(Setup);
        quitconfirmation_exitlevel_receiver     = new SignalReceiver().SetOnSignalCallback(EndGameEarly);
        quitconfirmation_backtogame_receiver    = new SignalReceiver().SetOnSignalCallback(Unpause);
        quitconfirmation_popup_receiver         = new SignalReceiver().SetOnSignalCallback(Pause);
    }

    protected virtual void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    protected virtual void OnDisable()
    {
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    #endregion

    #region Public Functions

    //Should be Called by the GamePlay view's Shown callback
    public virtual void StartGame()
    {
        countdownProgress.StartTimer();
    }

    //Should be Invoked by the Countdown Clock's OnOutOfTime Event
    public virtual void EndGame()
    {
        results.numCorrect      = correctResponses;
        results.numIncorrect    = incorrectResponses;

        GameManager.instance.SetMinigameResults(results);

        Signal.Send("GameManagement", "DisableExitLevelButton", false);

        StartCoroutine(EndAnimation());
    }

    #endregion

    #region Protected Functions

    protected virtual void Setup(Signal signal)
    {
        results                         = new MinigameResultsData();
        results.startingDifficulty      = GameManager.instance.currentMinigame.currentDifficultyLevel;

        correctResponses                = 0;
        incorrectResponses              = 0;

        exitButtonCanvasGroup.alpha     = 1;
        gameElementsCanvasGroup.alpha   = 1;

        countdownProgress.SetupTimer(GameManager.instance.currentMinigame.timedStartTime);

        Signal.Send("GameManagement", "DisableExitLevelButton", true);
    }

    protected virtual IEnumerator EndAnimation()
    {
        yield return new WaitForSeconds(.75f);

        StartCoroutine(Helpful.FadePanelOut(exitButtonCanvasGroup, .75f));
        yield return Helpful.FadePanelOut(gameElementsCanvasGroup, .75f);

        Signal.Send("GameManagement", "LevelEnded", 1);
    }

    protected virtual void EndGameEarly(Signal signal)
    {
        countdownProgress.DrainClock();
    }

    protected virtual void Pause(Signal signal)
    {
        countdownProgress.Pause();
    }

    protected virtual void Unpause(Signal signal)
    {
        countdownProgress.Unpause();
    }

    #endregion
}
