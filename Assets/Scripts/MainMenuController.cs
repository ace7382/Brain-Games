using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public List<LevelSelectButtonController>        levelButtons;

    //Screen - Main Menu's OnShowCallback calls this
    public void Setup()
    {
        for (int i = 0; i < levelButtons.Count; i++)
            levelButtons[i].ShowLockedStatus();
    }
}
