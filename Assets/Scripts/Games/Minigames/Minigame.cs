using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Minigame", menuName = "New Minigame", order = 55)]
public class Minigame : ScriptableObject
{
    public int                  minigameID;
    public bool                 unlocked;
    public bool                 timed;
    public TutorialInfoObject   tutorial;
}
