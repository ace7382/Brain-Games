using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "New Level", order = 31)]
public class NewLevelBase : ScriptableObject
{
    public string               levelName;
    public bool                 unlocked;
    public List<NewLevelBase>   levelsUnlockedByThisLevel;
    public List<Unit>           enemyUnits;
}
