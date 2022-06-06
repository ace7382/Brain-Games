using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class NewLevelSelectButtonController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private NewLevelBase           level;
    [SerializeField] private CanvasGroup            canvasGroup;

    #endregion

    #region Public Properties

    public bool Unlocked                            { get { return level.unlocked; } }
    public NewLevelBase Level                       { get { return level; } }

    #endregion

    #region Public Functions

    //Called By Level Buttons' PointerClick behavior
    public void OnClick()
    {
        if (!level.unlocked)
            return;

        GameManager.instance.SetLevel(level);

        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "LoadBattleScene");
    }

    public void ShowLockedStatus()
    {
        if (level.unlocked)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }

    public IEnumerator AnimatedUnlocking()
    {
        yield return Helpful.FadeCanvasIn(canvasGroup, 1f);
    }

    #endregion
}
