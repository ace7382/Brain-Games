using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBase : ScriptableObject
{
    public enum DifficultyLevel
    {
        VeryEasy = 5,
        Easy = 10,
        Medium = 15,
        Hard = 20,
        VeryHard = 25,
        Insane = 30
    };

    public string               levelName;
    public bool                 unlocked;
    public TutorialInfoObject   tutorial;
    public LevelBase            nextLevel;
    public DifficultyLevel      difficulty;

    [Space]
    [Space]

    public bool                 objective1;
    public bool                 objective2;
    public bool                 objective3;

    public void ResetObjectives()
    {
        objective1 = false;
        objective2 = false;
        objective3 = false;
    }
}
