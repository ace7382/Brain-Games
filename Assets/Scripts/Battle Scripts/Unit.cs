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

    #endregion

    #region Public Properties

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

    #endregion

    #region Public Functions

    public void Init()
    {
        currentHP = MaxHP;
    }

    #endregion
}
