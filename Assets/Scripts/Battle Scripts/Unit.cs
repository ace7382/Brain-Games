using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    #region Inspector Variables

    [SerializeField] private UnitBase   battleUnitBase;

    #endregion

    #region Private Variables

    private List<Ability>               abilities;

    //TODO: This doesn't need to be set in the inspector, just exposing to see easily
    [SerializeField] private Stats      stats;     
    [SerializeField] private Stats      EXPstats;     
    [SerializeField] private Stats      EXPNextLevelValues;     
    [SerializeField] private Stats      growthRates;     
    [SerializeField] private int        currentHP;

    #endregion

    #region Public Properties

    public string Name
    {
        get { return battleUnitBase.name; }
    }

    public Sprite MiniSprite
    {
        get { return battleUnitBase.miniSprite; }
    }

    public int MaxHP
    {
        get { return stats[Helpful.StatTypes.MaxHP]; }
    }

    public int CurrentHP
    {
        get { return currentHP; }
        set
        {
            if (currentHP == value) return;

            currentHP = Mathf.Clamp(value, 0, stats[Helpful.StatTypes.MaxHP]);
        }
    }

    public List<Ability> Abilities
    {
        get { return abilities; }
    }

    public Sprite InBattleSprite
    {
        get { return battleUnitBase.inBattleSprite; }
    }

    public Helpful.BattleGameTypes BattleGame
    {
        get { return battleUnitBase.battleGame; }
    }

    public string BattleGameName
    {
        get { return Helpful.GetStringFromBattleGameType(battleUnitBase.battleGame); }
    }

    public List<TriviaQuestion> TriviaQuestions
    {
        get { return battleUnitBase.triviaQuestions; }
    }

    public List<PathPuzzleBoard> PathPuzzleBoards
    {
        get { return battleUnitBase.pathPuzzleBoards; }
    }

    public List<ShadowShapePuzzle> ShadowShapePuzzles
    {
        get { return battleUnitBase.shadowShapePuzzles; }
    }

    #endregion

    #region Public Functions

    public void Init()
    {
        stats               = new Stats();
        EXPstats            = new Stats();
        EXPNextLevelValues  = new Stats();
        growthRates         = new Stats();

        abilities           = new List<Ability>();

        for (int i = 0; i < battleUnitBase.abilityNames.Count; i++)
        {
            Ability a = (Ability)Helpful.GetInstance(battleUnitBase.abilityNames[i]);

            abilities.Add(a);

            Debug.Log(abilities[i].GetType());
        }

        for (int i = 0; i < battleUnitBase.baseStats.Length; i++)
        {
            Helpful.StatTypes currentStat   = (Helpful.StatTypes) i;
            stats[currentStat]              = battleUnitBase.baseStats[i];
            growthRates[currentStat]        = (int)battleUnitBase.statGrowthRates[i];
            EXPNextLevelValues[currentStat] = Formulas.GetNextLevelEXP((Helpful.StatGrowthRates)growthRates[currentStat], stats[currentStat]);
        }

        currentHP = stats[Helpful.StatTypes.MaxHP];
    }

    //TODO: Make each stat have a property?
    public int GetStat(Helpful.StatTypes s)
    {
        return stats[s];
    }

    public int GetExpForStat(Helpful.StatTypes s)
    {
        return EXPstats[s];
    }

    public int GetEXPNextLevelValue(Helpful.StatTypes s)
    {
        return EXPNextLevelValues[s];
    }

    public int GetGrowthRate (Helpful.StatTypes s)
    {
        return growthRates[s];
    }

    public void AddEXP(Helpful.StatTypes s, int amount)
    {
        EXPstats[s] += amount;

        while (EXPstats[s] >= EXPNextLevelValues[s])
        {
            LevelUpStat(s);
        }
    }

    public void LevelUpStat(Helpful.StatTypes statToLevelUp)
    {
        stats[statToLevelUp]                += 1;

        EXPstats[statToLevelUp]             -= EXPNextLevelValues[statToLevelUp];

        EXPNextLevelValues[statToLevelUp]   = Formulas.GetNextLevelEXP(
                                              (Helpful.StatGrowthRates)growthRates[statToLevelUp]
                                              , stats[statToLevelUp]);
    }

    #endregion
}
