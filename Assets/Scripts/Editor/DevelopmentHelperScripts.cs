using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class DevelopmentHelperScripts : MonoBehaviour
{
    [MenuItem("Dev Commands/Reset Trivia Levels")]
    public static void CreateSecretShop()
    {
        List<TriviaSet> triviasets = new List<TriviaSet>(Resources.LoadAll<TriviaSet>("Scriptable Objects/Trivia Sets"));

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

    [MenuItem("Dev Commands/Link all Trivia Level Buttons to Main Menu Controller")]
    public static void LinkTriviaLevelButtonsToMainMenuController()
    {
        MainMenuController m = GameObject.FindObjectOfType<MainMenuController>();
        TriviaModeLevelButtonController[] buttons = GameObject.FindObjectsOfType<TriviaModeLevelButtonController>();

        m.triviaLevelButtons = buttons.ToList<TriviaModeLevelButtonController>();
    }
}
