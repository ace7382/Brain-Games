using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBase : ScriptableObject
{
    public string               levelName;
    public bool                 unlocked;
    public bool                 timedLevel;
    public TutorialInfoObject   tutorial;
    public LevelBase            nextLevel;

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
