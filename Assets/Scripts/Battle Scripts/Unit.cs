using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    #region Inspector Variables

    [SerializeField] private UnitBase battleUnitBase;
    [SerializeField] private Stats stats; //TODO: This doesn't need to be set in the inspector, just exposing to see easily

    #endregion

    #region Private Variables

    private List<Ability> abilities;

    #endregion

    #region Public Properties

    public string Name
    {
        get { return battleUnitBase.name; }
    }

    public int MaxHP
    {
        get { return stats[Helpful.StatTypes.MaxHP]; }
    }

    public int CurrentHP
    {
        get { return stats[Helpful.StatTypes.CurrentHP]; }
        set
        {
            if (stats[Helpful.StatTypes.CurrentHP] == value) return;

            stats[Helpful.StatTypes.CurrentHP] = Mathf.Clamp(value, 0, int.MaxValue);
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
        stats = new Stats();

        abilities   = new List<Ability>();

        for (int i = 0; i < battleUnitBase.abilityNames.Count; i++)
        {
            Ability a = (Ability)Helpful.GetInstance(battleUnitBase.abilityNames[i]);

            abilities.Add(a);

            Debug.Log(abilities[i].GetType());
        }

        for (int i = 0; i < battleUnitBase.baseStats.Length; i++)
        {
            stats[(Helpful.StatTypes) i] = battleUnitBase.baseStats[i];
        }

        stats[Helpful.StatTypes.CurrentHP] = stats[Helpful.StatTypes.MaxHP];
    }

    //TODO: Make each stat have a property?
    //      currentHP and MaxHP do
    public int GetStat(Helpful.StatTypes s)
    {
        return stats[s];
    }

    #endregion
}
