using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public List<TriviaModeLevelButtonController>    triviaLevelButtons;

    public void Setup()
    {
        for (int i = 0; i < triviaLevelButtons.Count; i++)
        {
            triviaLevelButtons[i].ShowLockedStatus();
        }
    }
}
