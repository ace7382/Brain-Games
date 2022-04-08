using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Word Scramble Level", menuName = "New Word Scramble Level", order = 53)]
public class WordScrambleLevel : ScriptableObject
{
    public string               title;
    public string               letters;
    public List<string>         hiddenWords;
    public WordScrambleLevel    nextWordScrambleLevel;
    public bool                 unlocked;
}
