using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Unit
{
    #region Inspector Variables

    [SerializeField] private UnitBase battleUnitBase;

    #endregion

    #region Private Variables

    private int currentHP;
    private List<Ability> abilities;

    #endregion

    #region Public Properties

    public string Name
    {
        get { return battleUnitBase.name; }
    }

    public int MaxHP
    {
        get { return battleUnitBase.baseMaxHP; }
    }

    public int CurrentHP
    {
        get { return currentHP; }
        set
        {
            if (currentHP == value) return;

            currentHP = Mathf.Clamp(value, 0, int.MaxValue);
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

    #endregion

    #region Public Functions

    public void Init()
    {
        currentHP   = MaxHP;

        abilities   = new List<Ability>();

        for (int i = 0; i < battleUnitBase.abilityNames.Count; i++)
        {
            Ability a = (Ability)Helpful.GetInstance(battleUnitBase.abilityNames[i]);

            abilities.Add(a);

            Debug.Log(abilities[i].GetType());
        }
    }

    #endregion
}
