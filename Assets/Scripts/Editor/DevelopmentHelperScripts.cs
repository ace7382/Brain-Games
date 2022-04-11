using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class DevelopmentHelperScripts : MonoBehaviour
{
    [MenuItem("Dev Commands/Link all Level Buttons to Main Menu Controller")]
    public static void LinkLevelButtonsToMainMenuController()
    {
        MainMenuController m = GameObject.FindObjectOfType<MainMenuController>();

        TriviaModeLevelButtonController[] triviaButtons = FindObjectsOfType<TriviaModeLevelButtonController>();
        WordScrambleLevelButtonController[] wordScrambleButtons = FindObjectsOfType<WordScrambleLevelButtonController>();

        m.triviaLevelButtons = triviaButtons.ToList();
        m.wordScrambleLevelButtons = wordScrambleButtons.ToList();
    }

    [MenuItem("Dev Commands/Reset all Levels")]
    public static void ResetAllLevels()
    {
        ResetTriviaLevels();
        ResetWordScrambleLevels();
    }

    [MenuItem("Dev Commands/Reset Trivia Levels")]
    public static void ResetTriviaLevels()
    {
        List<TriviaSet> triviasets = new List<TriviaSet>(Resources.LoadAll<TriviaSet>("Scriptable Objects/Trivia Sets"));

        if (triviasets == null)
            return;

        foreach (TriviaSet a in triviasets)
        {
            if (a.name.Contains("Level 1"))
            {
                a.unlocked = true;
            }
            else
            {
                a.unlocked = false;
            }

            a.completed = false;
            a.allQuestionsCorrect = false;
            a.underParTime = false;
        }
    }

    [MenuItem("Dev Commands/Reset Word Scramble Levels")]
    public static void ResetWordScrambleLevels()
    {
        List<WordScrambleLevel> levels = 
            new List<WordScrambleLevel>(Resources.LoadAll<WordScrambleLevel>("Scriptable Objects/Word Scramble Levels"));

        //Get all of the words from the text file
        TextAsset wordList = Resources.Load<TextAsset>("Full Word List");
        string[] a = wordList.text.Split('\n');
        List<string> allWords = new List<string>();

        for (int i = 0; i < a.Length; i++)
        {
            string word = a[i].TrimEnd('\r', '\n');

            if (word.Length >= 2 && !string.IsNullOrEmpty(word))
            {
                allWords.Add(word);
            }
        }
        ////------------

        if (levels == null || wordList == null || allWords.Count <= 0)
        {
            Debug.Log("word list or levels did not load correctly");
            return;
        }

        foreach (WordScrambleLevel level in levels)
        {
            level.foundWords = new List<string>();

            level.hiddenWords = new List<string>();

            for (int i = 0; i < allWords.Count; i++)
            {
                if (CheckWord(allWords[i], level.letters))
                {
                    level.hiddenWords.Add(allWords[i]);
                }
            }

            if (level.name.Contains("Level 1"))
                level.unlocked = true;
            else
                level.unlocked = false;
        }
    }

    private static bool CheckWord(string wordToCheck, string availableLetters)
    {
        List<string> localLetters = new List<string>();

        for (int i = 0; i < availableLetters.Length; i++)
        {
            localLetters.Add(availableLetters[i].ToString());
        }

        //Debug.Log(string.Format("Checking {0} with {1} letters", wordToCheck, availableLetters));

        for (int i = 0; i < wordToCheck.Length; i++)
        {
            //Debug.Log(string.Format("{0} more letters availble", localLetters.Count));

            int index = localLetters.IndexOf(wordToCheck[i].ToString().ToLower());

            //Debug.Log(string.Format("{0} found at index {1}", wordToCheck[i].ToString().ToLower(), index));

            if (index < 0)
                return false;
            else
            {
                localLetters.RemoveAt(index);
            }
        }

        return true;
    }
}
