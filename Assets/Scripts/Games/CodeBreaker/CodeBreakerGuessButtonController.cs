using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Signals;

public class CodeBreakerGuessButtonController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image                      guessIcon;

    #endregion

    #region Public Variables

    public CodeBreakerChoicesInfo.CodebreakerChoices    currentGuess;
    public int                                          buttonID;

    #endregion

    #region Public Functions

    public void Clear()
    {
        currentGuess = CodeBreakerChoicesInfo.CodebreakerChoices.NULL;

        guessIcon.gameObject.SetActive(false);
    }

    public void SetIcon(CodeBreakerChoicesInfo.CodebreakerChoices choice)
    {
        currentGuess = choice;

        guessIcon.gameObject.SetActive(true);

        CodeBreakerChoicesInfo.GetChoiceImage(currentGuess, ref guessIcon);
    }

    //Called by the Button's OnClick Behavior
    public void OnClick()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("CodeBreaker", "GuessButtonClicked", buttonID);
    }

    #endregion
}
