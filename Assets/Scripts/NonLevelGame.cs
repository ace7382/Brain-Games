using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Non-Level Game 'Level'", menuName = "New Non-Level Game 'Level'", order = 58)]
public class NonLevelGame : LevelBase
{
    public enum GameType
    {
        ArrowSwipe
    }

    public NonLevelGame.GameType gameType;
}
