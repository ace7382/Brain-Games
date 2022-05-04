using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public List<LevelSelectButtonController>        levelButtons;
    public List<MinigameSelectButtonController>     minigameButtons;

    //Screen - Main Menu's OnShowCallback calls this
    public void Setup()
    {
        for (int i = 0; i < levelButtons.Count; i++)
            levelButtons[i].ShowLockedStatus();

        for (int i = 0; i < minigameButtons.Count; i++)
            minigameButtons[i].ShowLockedStatus();

        GameManager.instance.ClearLevelData();
    }
}
