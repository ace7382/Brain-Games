using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public List<TriviaModeLevelButtonController>    triviaLevelButtons;
    public List<WordScrambleLevelButtonController>  wordScrambleLevelButtons;

    //Screen - Main Menu's OnShowCallback calls this
    public void Setup()
    {
        for (int i = 0; i < triviaLevelButtons.Count; i++)
        {
            triviaLevelButtons[i].ShowLockedStatus();
        }

        for (int i = 0; i < wordScrambleLevelButtons.Count; i++)
        {
            wordScrambleLevelButtons[i].ShowLockedStatus();
        }
    }
}
