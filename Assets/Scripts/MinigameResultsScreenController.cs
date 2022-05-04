using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameResultsScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] TextMeshProUGUI    title;
    [SerializeField] TextMeshProUGUI    correctCount;
    [SerializeField] TextMeshProUGUI    incorrectCount;
    [SerializeField] TextMeshProUGUI    difficultyBarText;
    [SerializeField] Image              difficultyBarFill;
    [SerializeField] UIView             uiView;

    #endregion

    #region Signal Variables

    private SignalReceiver              endlevelscreen_setup_receiver;
    private SignalStream                endlevelscreen_setup_stream;

    #endregion

    private void Awake()
    {
        endlevelscreen_setup_stream     = SignalStream.Get("EndLevelScreen", "Setup");
        
        endlevelscreen_setup_receiver   = new SignalReceiver().SetOnSignalCallback(Setup);
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
        SetTitleText();
        SetDifficultyBarText();

        correctCount.text   = GameManager.instance.currentMinigameResults.numCorrect.ToString();
        incorrectCount.text = GameManager.instance.currentMinigameResults.numIncorrect.ToString();

        Signal.Send("EndLevelScreen", "ShowScreen");
    }

    //Called by the Replay Button's OnClick behavior
    public void ReplayMinigame()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "ReplayCurrentMinigame");
    }

    private void SetTitleText()
    {
        //TODO: Add variety based on how well the player did/the overall difficulty etc
        if (GameManager.instance.currentMinigameResults.numCorrect > GameManager.instance.currentMinigameResults.numIncorrect)
            title.text = "~Way to Go~";
        else
            title.text = "~Nice Try~";
    }

    private void SetDifficultyBarText()
    {
        if (GameManager.instance.currentMinigame.currentMaxDifficulty >= GameManager.instance.currentMinigame.maxDifficulty)
        {
            difficultyBarText.text      = "MAX!!";
            difficultyBarText.fontSize  = 100;
        }
        else
        {
            difficultyBarText.text      = GameManager.instance.currentMinigame.currentMaxDifficulty.ToString();
            difficultyBarText.fontSize  = 36;
        }

        float fill =    (float)(GameManager.instance.currentMinigame.currentMaxDifficulty > GameManager.instance.currentMinigame.maxDifficulty ?
                        GameManager.instance.currentMinigame.maxDifficulty : GameManager.instance.currentMinigame.currentMaxDifficulty)
                        / (float)(GameManager.instance.currentMinigame.maxDifficulty);

        SetDifficultyBarFill(fill);
    }

    private void SetDifficultyBarFill(float fill)
    {
        difficultyBarFill.fillAmount = fill;
    }
}
