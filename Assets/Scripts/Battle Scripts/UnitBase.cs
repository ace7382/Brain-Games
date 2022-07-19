using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Base", menuName = "New Unit Base", order = 30)]
public class UnitBase : ScriptableObject
{
    public List<string>                 abilityNames;
    public Helpful.BattleGameTypes      battleGame;
    public Sprite                       inBattleSprite;
    public List<TriviaQuestion>         triviaQuestions;
    public List<PathPuzzleBoard>        pathPuzzleBoards;
    public List<ShadowShapePuzzle>      shadowShapePuzzles;

    public int[]                        baseStats = new int[(int)Helpful.StatTypes.COUNT];
}