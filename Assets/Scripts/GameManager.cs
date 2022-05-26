using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Doozy.Runtime.Signals;

public class GameManager : MonoBehaviour
{
    public static GameManager   instance = null;

    public LevelBase            currentLevel;
    public LevelResultsData     currentLevelResults;
    public Minigame             currentMinigame;
    public MinigameResultsData  currentMinigameResults;

    private SignalReceiver      gamemanagement_replaycurrentlevel_receiver;
    private SignalStream        gamemanagement_replaycurrentlevel_stream;
    private SignalReceiver      gamemanagement_replaycurrentminigame_receiver;
    private SignalStream        gamemanagement_replaycurrentminigame_stream;
    private SignalReceiver      gamemanagement_playnextlevel_receiver;
    private SignalStream        gamemanagement_playnextlevel_stream;
    private SignalReceiver      gamemanagement_unloadgamescenes_receiver;
    private SignalStream        gamemanagement_unloadgamescenes_stream;

    private SignalReceiver      quitconfirmation_popup_receiver;
    private SignalStream        quitconfirmation_popup_stream;

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
        gamemanagement_unloadgamescenes_stream          = SignalStream.Get("GameManagement", "UnloadGameScenes");
        quitconfirmation_popup_stream                   = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_replaycurrentlevel_receiver      = new SignalReceiver().SetOnSignalCallback(PlayCurrentLevel);
        gamemanagement_replaycurrentminigame_receiver   = new SignalReceiver().SetOnSignalCallback(PlayCurrentMinigame);
        gamemanagement_playnextlevel_receiver           = new SignalReceiver().SetOnSignalCallback(PlayNextLevel);
        gamemanagement_unloadgamescenes_receiver        = new SignalReceiver().SetOnSignalCallback(UnloadAllGameScenes);
        quitconfirmation_popup_receiver                 = new SignalReceiver().SetOnSignalCallback(ShowExitPopup);
    }

    private void OnEnable()
    {
        gamemanagement_replaycurrentlevel_stream.ConnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_replaycurrentminigame_stream.ConnectReceiver(gamemanagement_replaycurrentminigame_receiver);
        gamemanagement_playnextlevel_stream.ConnectReceiver(gamemanagement_playnextlevel_receiver);
        gamemanagement_unloadgamescenes_stream.ConnectReceiver(gamemanagement_unloadgamescenes_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_replaycurrentlevel_stream.DisconnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_replaycurrentminigame_stream.DisconnectReceiver(gamemanagement_replaycurrentminigame_receiver);
        gamemanagement_playnextlevel_stream.DisconnectReceiver(gamemanagement_playnextlevel_receiver);
        gamemanagement_unloadgamescenes_stream.DisconnectReceiver(gamemanagement_unloadgamescenes_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

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
        if (currentLevel.levelsUnlockedByThisLevel == null)
        {
            Debug.Log("current level does not have a next level");
            return;
        }

        //TODO: Update this
        //currentLevel = currentLevel.levelsUnlockedByThisLevel;

        PlayCurrentLevel(signal);
    }

    public void SetLevel(LevelBase level)
    {
        currentLevel        = level;
        currentMinigame     = null;
    }

    public void SetMinigame(Minigame minigame)
    {
        currentMinigame     = minigame;
        currentLevel        = null;
    }

    public void UnloadAllGameScenes(Signal signal)
    {
        //TODO - evaluate whether I need this
        //      right now there are unload scene nodes for all games on return to main menu
        //      not super simple to track and unload all available game scenes (there should only be one but not
        //      trusting that right not). Might be better to do it this way though as more modes are added
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
        currentLevel        = null;
        currentMinigame     = null;
        ClearLevelResults();
        ClearMinigameResults();
    }

    //TODO: Probably move the list of things to animate to here vs the wordl map controller
    //      currently world map hides when moving to a game, might have it unload completely
    public void SetWorldMapUnlockLevels(LevelBase level)
    {
        FindObjectOfType<WorldMapController>().AddLevelsToUnlock(level);
    }
}
