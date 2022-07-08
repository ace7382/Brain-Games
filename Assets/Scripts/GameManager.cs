using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Doozy.Runtime.Signals;

public class GameManager : MonoBehaviour
{
    #region OLD VARIABLES - TODO: REMOVE ALL OF THESE LOLLLLL

    public LevelBase currentLevelOLD;
    public LevelResultsData currentLevelResults;
    public Minigame currentMinigame;
    public MinigameResultsData currentMinigameResults;

    private SignalReceiver gamemanagement_replaycurrentlevel_receiver;
    private SignalStream gamemanagement_replaycurrentlevel_stream;
    private SignalReceiver gamemanagement_replaycurrentminigame_receiver;
    private SignalStream gamemanagement_replaycurrentminigame_stream;
    private SignalReceiver gamemanagement_playnextlevel_receiver;
    private SignalStream gamemanagement_playnextlevel_stream;

    private SignalReceiver quitconfirmation_popup_receiver;
    private SignalStream quitconfirmation_popup_stream;

    #endregion





    #region Singleton

    public static GameManager   instance = null;

    #endregion

    #region Private Variables

    private NewLevelBase        currentLevel;

    #endregion

    #region Public Properties

    public NewLevelBase         CurrentLevel { get { return currentLevel; } }

    #endregion

    #region Signal Variables

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        gamemanagement_replaycurrentlevel_stream        = SignalStream.Get("GameManagement", "ReplayCurrentLevel");
        gamemanagement_replaycurrentminigame_stream     = SignalStream.Get("GameManagement", "ReplayCurrentMinigame");
        gamemanagement_playnextlevel_stream             = SignalStream.Get("GameManagement", "PlayNextLevel");
        quitconfirmation_popup_stream                   = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_replaycurrentlevel_receiver      = new SignalReceiver().SetOnSignalCallback(PlayCurrentLevel);
        gamemanagement_replaycurrentminigame_receiver   = new SignalReceiver().SetOnSignalCallback(PlayCurrentMinigame);
        gamemanagement_playnextlevel_receiver           = new SignalReceiver().SetOnSignalCallback(PlayNextLevel);
        quitconfirmation_popup_receiver                 = new SignalReceiver().SetOnSignalCallback(ShowExitPopup);
    }

    private void OnEnable()
    {
        gamemanagement_replaycurrentlevel_stream.ConnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_replaycurrentminigame_stream.ConnectReceiver(gamemanagement_replaycurrentminigame_receiver);
        gamemanagement_playnextlevel_stream.ConnectReceiver(gamemanagement_playnextlevel_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_replaycurrentlevel_stream.DisconnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_replaycurrentminigame_stream.DisconnectReceiver(gamemanagement_replaycurrentminigame_receiver);
        gamemanagement_playnextlevel_stream.DisconnectReceiver(gamemanagement_playnextlevel_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    #endregion

    #region Public Functions

    public void SetLevel(NewLevelBase level)
    {
        currentLevel = level;
    }

    //TODO: Probably move the list of things to animate to here vs the wordl map controller
    //      currently world map hides when moving to a game, might have it unload completely
    public void SetWorldMapUnlockLevels(NewLevelBase level)
    {
        //TODO: Re-add this; currently turned off bc world map is unloaded before battle
        FindObjectOfType<WorldMapController>().AddLevelsToUnlock(level);
    }

    #endregion

    #region OLD FUNCTIONS - TODO: REMOVE OR UPDATE

    public void PlayCurrentLevel(Signal signal)
    {
        Signal.Send("GameManagement", "StartLevel");
    }

    public void PlayCurrentMinigame(Signal signal)
    {
        Signal.Send("GameManagement", "StartMinigame");
    }

    public void PlayNextLevel(Signal signal)
    {
        if (currentLevelOLD.levelsUnlockedByThisLevel == null)
        {
            Debug.Log("current level does not have a next level");
            return;
        }

        //TODO: Update this
        //currentLevel = currentLevel.levelsUnlockedByThisLevel;

        PlayCurrentLevel(signal);
    }

    public void SetMinigame(Minigame minigame)
    {
        currentMinigame     = minigame;
        currentLevelOLD        = null;
    }

    public void ShowExitPopup(Signal signal)
    {
        SceneManager.LoadScene("ExitConfirmationScreen", LoadSceneMode.Additive);
    }

    public void SetMinigameResults(MinigameResultsData data)
    {
        currentMinigameResults = data;
    }

    public void ClearMinigameResults()
    {
        currentMinigameResults = null;
    }

    public void SetLevelResults(LevelResultsData data)
    {
        currentLevelResults = data;
    }

    public void ClearLevelResults()
    {
        currentLevelResults = null;
    }

    public void ClearLevelData()
    {
        currentLevelOLD         = null;
        currentMinigame         = null;
        ClearLevelResults();
        ClearMinigameResults();
    }

    #endregion


}
