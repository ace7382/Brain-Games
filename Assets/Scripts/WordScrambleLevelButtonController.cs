using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;

public class WordScrambleLevelButtonController : MonoBehaviour
{
    public WordScrambleLevel            wordScrambleLevel;

    [SerializeField] private GameObject lockIcon;
    [SerializeField] private GameObject levelNumber;
    [SerializeField] private UIButton   button;
    [SerializeField] private GameObject goal1Dot;
    [SerializeField] private GameObject goal2Dot;
    [SerializeField] private GameObject specialWordDot;

    //Called By Word Scramble Level Buttons' PointerClick behavior
    public void Click()
    {
        if (!wordScrambleLevel.unlocked)
            return;

        object[] data = new object[2];

        data[0] = 0;
        data[1] = wordScrambleLevel;

        Signal.Send("WordScramble", "WordScrambleSetup", data);
    }

    public void ShowLockedStatus()
    {
        if (wordScrambleLevel.unlocked)
        {
            lockIcon.SetActive(false);
            levelNumber.SetActive(true);
            button.interactable = true;

            goal1Dot.SetActive(wordScrambleLevel.foundWords.Count >= wordScrambleLevel.goalWordCount);
            goal2Dot.SetActive(wordScrambleLevel.foundWords.Count >= wordScrambleLevel.secondGoalWordCount);
            specialWordDot.SetActive(wordScrambleLevel.foundWords.Contains(wordScrambleLevel.specialWord));
        }
        else
        {
            lockIcon.SetActive(true);
            levelNumber.SetActive(false);
            button.interactable = false;
        }
    }
}
