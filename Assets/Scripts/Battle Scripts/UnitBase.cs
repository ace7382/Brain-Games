using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit Base", menuName = "New Unit Base", order = 30)]
public class UnitBase : ScriptableObject
{
    public int                          baseMaxHP;
    public List<string>                 abilityNames;
    public Helpful.BattleGameTypes      battleGame;
    public Sprite                       inBattleSprite;
    public List<TriviaQuestion>         triviaQuestions;
}