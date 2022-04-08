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
    [SerializeField] private GameObject completedDot;
    [SerializeField] private GameObject underParDot;
    [SerializeField] private GameObject allQuestionsCorrectDot;

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

    }
}
