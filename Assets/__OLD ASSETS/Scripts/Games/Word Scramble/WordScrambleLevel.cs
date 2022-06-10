using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Word Scramble Level", menuName = "New Word Scramble Level", order = 53)]
public class WordScrambleLevel : LevelBase
{
    //Objective 1 - low word count (goalWordCount)
    //Objective 2 - high word count (secondGoalWordCount)
    //Objective 3 - Find special word (specialWord)

    public string               letters;

    [Space]
    [Space]

    public int                  goalWordCount;
    public int                  secondGoalWordCount;
    public string               specialWord;
    public List<string>         hiddenWords;
    public List<string>         foundWords;
}
