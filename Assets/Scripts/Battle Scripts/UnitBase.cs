using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Base", menuName = "New Unit Base", order = 30)]
public class UnitBase : ScriptableObject
{
    public List<string>                 abilityNames;
    public Helpful.BattleGameTypes      battleGame;
    public Sprite                       inBattleSprite;
    public Sprite                       miniSprite;
    public List<TriviaQuestion>         triviaQuestions;
    public List<PathPuzzleBoard>        pathPuzzleBoards;
    public List<ShadowShapePuzzle>      shadowShapePuzzles;

    [Header("Enemy things")] //TODO: Make an enemy class lolllllllllllllllllllllll
    public int                          difficultyBase;
    public List<Vector2Int>             expAwardedOnDefeat; //x - the int aligned with the stat, y - the amount given
    public List<ItemReward>             itemRewards;

    public int[]                        baseStats       = new int[(int)Helpful.StatTypes.COUNT];
    public Helpful.StatGrowthRates[]    statGrowthRates = new Helpful.StatGrowthRates[(int)Helpful.StatTypes.COUNT];

    public GameObject                   unitModel;
}