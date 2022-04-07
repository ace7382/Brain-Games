using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;

public class TriviaModeLevelButtonController : MonoBehaviour
{
    public TriviaSet triviaSet;

    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject levelNumber;
    [SerializeField] private UIButton   button;
    [SerializeField] private GameObject completedDot;
    [SerializeField] private GameObject underParDot;
    [SerializeField] private GameObject allQuestionsCorrectDot;

    //Called By TriviaLevel Button's PointerClick behavior
    public void Click()
    {
        if (!triviaSet.unlocked)
            return;

        object[] data = new object[2];

        data[0] = 0;
        data[1] = triviaSet;

        Signal.Send("Trivia", "TriviaSetup", data);
    }

    public void ShowLockedStatus()
    {
        if (triviaSet.unlocked)
        {
            lockIcon.SetActive(false);
            levelNumber.SetActive(true);
            button.interactable = true;

            completedDot.SetActive(triviaSet.completed);
            underParDot.SetActive(triviaSet.underParTime);
            allQuestionsCorrectDot.SetActive(triviaSet.allQuestionsCorrect);
        }
        else
        {
            lockIcon.SetActive(true);
            levelNumber.SetActive(false);
            button.interactable = false;
        }
    }
}
