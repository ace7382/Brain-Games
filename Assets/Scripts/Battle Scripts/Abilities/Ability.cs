using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public abstract class Ability
{
    #region Enums

    public enum AbilityChargeType
    {
        NUM_OF_CHARGES  = 100,
        TIMED           = 200,
    }

    #endregion

    #region

    public BattleUnitController owner;
    public AbilityChargeType    chargeType;
    public int                  numOfCharges;
    public string               abilityName;
    public bool                 autocast = false;

    #endregion

    public virtual void Init(BattleUnitController owner)
    {
        this.owner = owner;
    }

    public abstract void Activate();

    public virtual void Deactivate()
    {

    }
}
