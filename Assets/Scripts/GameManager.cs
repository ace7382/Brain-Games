using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Doozy.Runtime.Signals;

public class GameManager : MonoBehaviour
{
    public static GameManager   instance = null;

    public LevelBase            currentLevel;
    public Minigame             currentMinigame;

    private SignalReceiver      gamemanagement_replaycurrentlevel_receiver;
    private SignalStream        gamemanagement_replaycurrentlevel_stream;
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

        gamemanagement_replaycurrentlevel_stream    = SignalStream.Get("GameManagement", "ReplayCurrentLevel");
        gamemanagement_playnextlevel_stream         = SignalStream.Get("GameManagement", "PlayNextLevel");
        gamemanagement_unloadgamescenes_stream      = SignalStream.Get("GameManagement", "UnloadGameScenes");
        quitconfirmation_popup_stream               = SignalStream.Get("QuitConfirmation", "Popup");

        gamemanagement_replaycurrentlevel_receiver  = new SignalReceiver().SetOnSignalCallback(PlayCurrentLevel);
        gamemanagement_playnextlevel_receiver       = new SignalReceiver().SetOnSignalCallback(PlayNextLevel);
        gamemanagement_unloadgamescenes_receiver    = new SignalReceiver().SetOnSignalCallback(UnloadAllGameScenes);
        quitconfirmation_popup_receiver             = new SignalReceiver().SetOnSignalCallback(ShowExitPopup);
    }

    private void OnEnable()
    {
        gamemanagement_replaycurrentlevel_stream.ConnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_playnextlevel_stream.ConnectReceiver(gamemanagement_playnextlevel_receiver);
        gamemanagement_unloadgamescenes_stream.ConnectReceiver(gamemanagement_unloadgamescenes_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_replaycurrentlevel_stream.DisconnectReceiver(gamemanagement_replaycurrentlevel_receiver);
        gamemanagement_playnextlevel_stream.DisconnectReceiver(gamemanagement_playnextlevel_receiver);
        gamemanagement_unloadgamescenes_stream.DisconnectReceiver(gamemanagement_unloadgamescenes_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    public void PlayCurrentLevel(Signal signal)
    {
        Signal.Send("GameManagement", "StartLevel");
    }

    public void PlayNextLevel(Signal signal)
    {
        if (currentLevel.nextLevel == null)
        {
            Debug.Log("current level does not have a next level");
            return;
        }

        currentLevel = currentLevel.nextLevel;

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

}
