using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame", menuName = "New Minigame", order = 55)]
public class Minigame : ScriptableObject
{
    public int                  minigameID;
    public TutorialInfoObject   tutorial;

    [Space]
    [Space]

    public bool                 unlocked;

    [Space]
    [Space]

    public bool                 timed;
    public float                timedStartTime;

    [Space]
    [Space]

    public int                  maxDifficulty;
    public int                  currentMaxDifficulty;
}
