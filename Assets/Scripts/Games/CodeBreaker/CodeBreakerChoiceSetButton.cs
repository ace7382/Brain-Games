using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Signals;

public class CodeBreakerChoiceSetButton : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image                      icon;

    #endregion

    #region Public Variables

    public CodeBreakerChoicesInfo.CodebreakerChoices    choice;
    public int                                          buttonID;

    #endregion

    #region Public Funtions

    public void Setup(CodeBreakerChoicesInfo.CodebreakerChoices c, int id)
    {
        choice      = c;
        buttonID    = id;
        
        CodeBreakerChoicesInfo.GetChoiceImage(choice, ref icon);
    }

    //Called by the UIButton's OnClick Behavior
    public void OnClick()
    {
        Signal.Send("CodeBreaker", "AssignChoice", buttonID);
    }

    #endregion
}
