using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    #region Inspector Variables

    [SerializeField] private UnitBase           battleUnitBase;

    #endregion

    #region Private Variables

    private List<Ability>                       abilities;

    //TODO: These don't need to be set in the inspector, just exposing to see easily
    [SerializeField] private Stats              stats;
    [SerializeField] private Stats              EXPstats;   
    [SerializeField] private Stats              EXPNextLevelValues;
    [SerializeField] private Stats              growthRates;
    [SerializeField] private int                currentHP;
    [SerializeField] private List<StatModifier> statModifiers;
    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

    private List<Item_Equipment>                equipment;


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

    public int CurrentHP
    {
        get { return currentHP; }
        set
        {
            if (currentHP == value) return;

            currentHP = Mathf.Clamp(value, 0, GetStatWithMods(Helpful.StatTypes.MaxHP));
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

    public List<Item_Equipment> Equipment
    {
        get { return equipment; }
    }

    public List<StatModifier> StatModifiers
    {
        get { return statModifiers; }
    }

    public int BaseDifficulty
    { 
        get { return battleUnitBase.difficultyBase; } 
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
        statModifiers       = new List<StatModifier>();

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

        //TODO: Maybe do this better/somewhere else?
        equipment = new List<Item_Equipment>();

        equipment.Add(null); //Always have 1 slot.

        if (Random.Range(0, 2) > 0) { equipment.Add(null); }
        if (Random.Range(0, 2) > 0) { equipment.Add(null); }
        if (Random.Range(0, 2) > 0) { equipment.Add(null); }
        //------------------------------------------
    }

    //TODO: Make each stat have a property?
    public int GetStat(Helpful.StatTypes s)
    {
        return stats[s];
    }

    public int GetStatWithMods(Helpful.StatTypes s)
    {
        //TODO: Remove this if statement for final versions or put the in-editor only tags on it
        if (s == Helpful.StatTypes.Level || s == Helpful.StatTypes.COUNT)
            Debug.Log(Name + "'s " + s.GetShorthand() + " was trying to get a With Mods value");

        int ret = stats[s];

        for (int i = 0; i < statModifiers.Count; i++)
        {
            if (statModifiers[i].StatBeingModified == s)
            {
                ret += statModifiers[i].GetStatChangeAmount(stats[s]);
            }
        }

        return ret;
    }

    public int GetExpForStat(Helpful.StatTypes s)
    {
        return EXPstats[s];
    }

    public int GetEXPNextLevelValue(Helpful.StatTypes s)
    {
        return EXPNextLevelValues[s];
    }

    public int GetEXPNextLevelValue(Helpful.StatTypes s, int numberOfLevelsToGetEXPFor)
    {
        int ret = EXPNextLevelValues[s]; //Start with the current level up

        for (int i = 1; i < numberOfLevelsToGetEXPFor; i++)
        {
            ret += Formulas.GetNextLevelEXP((Helpful.StatGrowthRates)growthRates[s], stats[s] + i);
        }

        return ret;
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

        //Leveling up the unit's actual level gives a bunch of exp to all of their stats
        //TODO: call a popup that has their stat changes maybe?
        if (statToLevelUp == Helpful.StatTypes.Level)
        {
            AddEXP(Helpful.StatTypes.MaxHP, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.MaxHP));
            AddEXP(Helpful.StatTypes.Memory, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.Memory));
            AddEXP(Helpful.StatTypes.Observation, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.Observation));
            AddEXP(Helpful.StatTypes.ProblemSolving, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.ProblemSolving));
            AddEXP(Helpful.StatTypes.Speed, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.Speed));
            AddEXP(Helpful.StatTypes.Math, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.Math));
            AddEXP(Helpful.StatTypes.Language, 10 * GetStat(Helpful.StatTypes.Level) * GetGrowthRate(Helpful.StatTypes.Language));
        }
    }

    public void PermanentlyRaiseStat(Helpful.StatTypes statToRaise, int amountToRaiseBy)
    {
        //TODO: Not sure if i want raising stats with items to give a full level's EXP or to just bump it up
        //      The " - EXPStats[statToRaise]" below will make it only give the amount needed to get to 0 EXP after
        //      the increase
        int expToAward = GetEXPNextLevelValue(statToRaise, amountToRaiseBy); //- EXPstats[statToRaise];

        AddEXP(statToRaise, expToAward);

        if (statToRaise == Helpful.StatTypes.MaxHP)
            currentHP += amountToRaiseBy;
    }

    public void PermanentlyLowerStat(Helpful.StatTypes statToLower, int amountToLowerBy)
    {

        if (statToLower == Helpful.StatTypes.MaxHP)
        {
            //TODO: Clamp Max M.Health to a different value than zero
            stats[statToLower] = Mathf.Clamp(stats[statToLower] - amountToLowerBy, 5, int.MaxValue);

            if (currentHP > GetStatWithMods(Helpful.StatTypes.MaxHP))
                currentHP = GetStatWithMods(Helpful.StatTypes.MaxHP);
        }
        else
            stats[statToLower] = Mathf.Clamp(stats[statToLower] - amountToLowerBy, 0, int.MaxValue);

        EXPstats[statToLower] = 0;

        EXPNextLevelValues[statToLower] = Formulas.GetNextLevelEXP((Helpful.StatGrowthRates)growthRates[statToLower], stats[statToLower]);
    }

    public void EquipItem(int equipmentSlot, Item_Equipment itemToEquip)
    {
        if (equipmentSlot >= equipment.Count)
        {
            Debug.Log("Trying to equip item to slot: " + equipmentSlot.ToString()
                + " but only have " + (equipment.Count).ToString() + " slots");

            return;
        }

        if (equipment[equipmentSlot] != null)
        {
            equipment[equipmentSlot].OnUnequip(this);
        }

        equipment[equipmentSlot] = itemToEquip;

        equipment[equipmentSlot].OnEquip(this);
    }

    public void AddStatModifier(StatModifier sm)
    {
        statModifiers.Add(sm);

        CheckCurrentHPOnMaxHPChange(sm, true);
    }
 
    public void RemoveStatModifier(StatModifier sm)
    {
        statModifiers.Remove(sm);
    }

    #endregion

    #region Private Functions

    private void CheckCurrentHPOnMaxHPChange(StatModifier sm, bool addingSM)
    {
        //TODO: Enforce minimum max hp here?
        //      maybe at the GetStatWithMods function instead?
        if (sm.StatBeingModified == Helpful.StatTypes.MaxHP)
        {
            //If adding a stat mod will raise MaxHP, raise current HP by the same amount
            if (addingSM && sm.GetStatChangeAmount(GetStat(Helpful.StatTypes.MaxHP)) > 0)
            {
                CurrentHP += sm.GetStatChangeAmount(GetStat(Helpful.StatTypes.MaxHP));
            }
            //If Removing a stat mod will raise MaxHP, raise current HP by the same amount
            else if (!addingSM && sm.GetStatChangeAmount(GetStat(Helpful.StatTypes.MaxHP)) < 0)
            {
                CurrentHP += sm.GetStatChangeAmount(GetStat(Helpful.StatTypes.MaxHP)) * -1; //It's a negative num so add it's inverse
            }

            if (CurrentHP >= GetStatWithMods(Helpful.StatTypes.MaxHP))
                CurrentHP = GetStatWithMods(Helpful.StatTypes.MaxHP);
        }
    }

    #endregion
}
