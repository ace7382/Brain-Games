using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Word Scramble Level", menuName = "New Word Scramble Level", order = 53)]
public class WordScrambleLevel : ScriptableObject
{
    public string               title;
    public bool                 unlocked;
    public string               letters;

    [Space]
    [Space]

    public int                  goalWordCount;
    public int                  secondGoalWordCount;
    public string               specialWord;
    public List<string>         hiddenWords;
    public List<string>         foundWords;

    [Space]
    [Space]

    public WordScrambleLevel    nextWordScrambleLevel;

}
