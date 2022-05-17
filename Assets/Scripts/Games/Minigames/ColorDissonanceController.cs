using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ColorDissonanceController : MonoBehaviour
{
    #region Classes

    [System.Serializable]
    private class WordColorCombo
    {
        [SerializeField] private string word;
        [SerializeField] private Color  color;

        public string Word { get { return word; } }
        public Color Color { get { return color; } }
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI        leftPanelText;
    [SerializeField] private TextMeshProUGUI        rightPanelText;
    [SerializeField] private List<WordColorCombo>   colorsAnswerKey;

    #endregion

    #region Signal Variables

    private SignalReceiver      gamemanagement_gamesetup_receiver;
    private SignalStream        gamemanagement_gamesetup_stream;
    private SignalReceiver      quitconfirmation_exitlevel_receiver;
    private SignalStream        quitconfirmation_exitlevel_stream;
    private SignalReceiver      quitconfirmation_backtogame_receiver;
    private SignalStream        quitconfirmation_backtogame_stream;
    private SignalReceiver      quitconfirmation_popup_receiver;
    private SignalStream        quitconfirmation_popup_stream;

    #endregion

    #region Unity Functions

    private void Awake()
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

    private void OnEnable()
    {
        gamemanagement_gamesetup_stream.ConnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.ConnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.ConnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.ConnectReceiver(quitconfirmation_popup_receiver);
    }

    private void OnDisable()
    {
        gamemanagement_gamesetup_stream.DisconnectReceiver(gamemanagement_gamesetup_receiver);
        quitconfirmation_exitlevel_stream.DisconnectReceiver(quitconfirmation_exitlevel_receiver);
        quitconfirmation_backtogame_stream.DisconnectReceiver(quitconfirmation_backtogame_receiver);
        quitconfirmation_popup_stream.DisconnectReceiver(quitconfirmation_popup_receiver);
    }

    #endregion

    #region Public Functions

    public void Setup(Signal s)
    {
        NextSet();
    }

    //Called by the Does Match Button's OnClick Behavior
    public void CheckYes()
    {
        if (rightPanelText.color == colorsAnswerKey.Find(x => x.Word == leftPanelText.text).Color)
            Debug.Log("Correct");
        else
            Debug.Log("Incorrect");

        NextSet();
    }

    //Called by the Does NOT Match Button's OnClick Behavior
    public void CheckNo()
    {
        if (rightPanelText.color != colorsAnswerKey.Find(x => x.Word == leftPanelText.text).Color)
            Debug.Log("Correct");
        else
            Debug.Log("Incorrect");

        NextSet();
    }

    #endregion

    #region Private Functions

    private void NextSet()
    {
        int leftPanelColorIndex     = Random.Range(0, colorsAnswerKey.Count);
        int leftPanelWordIndex      = Random.Range(0, colorsAnswerKey.Count);

        int rightPanelColorIndex    = Random.Range(0, colorsAnswerKey.Count);
        int rightPanelWordIndex     = Random.Range(0, colorsAnswerKey.Count);

        leftPanelText.color         = colorsAnswerKey[leftPanelColorIndex].Color;
        leftPanelText.text          = colorsAnswerKey[leftPanelWordIndex].Word;

        rightPanelText.color        = colorsAnswerKey[rightPanelColorIndex].Color;
        rightPanelText.text         = colorsAnswerKey[rightPanelWordIndex].Word;
    }

    private void EndGameEarly(Signal signal)
    {

    }

    private void Pause(Signal signal)
    {

    }

    private void Unpause(Signal signal)
    {

    }

    #endregion
}
