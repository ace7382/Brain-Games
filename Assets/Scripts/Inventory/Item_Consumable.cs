using Doozy.Runtime.Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "New Item - Consumable", order = 42)]
public class Item_Consumable : Item
{
    #region Public Structs

    [System.Serializable]
    public struct ConsumableStatChange
    {
        public bool                 permanentChange;
        public Helpful.StatTypes    statToChange; //If COUNT then change current hp. TODO: Don't do this lolllllllll
        public bool                 percentChange;
        public float                amount;
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private bool                       useFromInventory;
    [SerializeField] private bool                       useFromBattle;

    [Space]

    [SerializeField] private List<string>               onUseActions;
    [SerializeField] private List<ConsumableStatChange> onUseStatChanges;

    #endregion

    #region Public Properties

    public bool                                         CanUseFromInventory         { get { return useFromInventory; } }
    public bool                                         CanUseFromBattle            { get { return useFromBattle; } }
    public List<ConsumableStatChange>                   OnUseStatChanges            { get { return onUseStatChanges; } }

    #endregion

    #region Public Functions

    public void UseItemFromInventory()
    {
        if (!useFromInventory)
            return;

        for (int i = 0; i < onUseActions.Count; i++)
        {
            Type        thisType = GetType();
            MethodInfo  theMethod = thisType.GetMethod(onUseActions[i], BindingFlags.NonPublic | BindingFlags.Instance);

            if (theMethod != null)
                theMethod.Invoke(this, null);
            else
                Debug.Log("Item Use Function not found:" + onUseActions[i]);
        }

        object[] info   = new object[2];
        info[0]         = this;
        info[1]         = null; //The unit target, which is null here

        Signal.Send("Inventory", "ItemUsed", info);
    }

    public void UseItemInBattle()
    {
        if (!useFromBattle)
            return;
    }

    public void UseItemFromInventory(Unit u)
    {
        if (!useFromInventory)
            return;

        Debug.Log(name + ": unit target item used on " + u.Name);

        //Do everything with unit as parameter
        for (int i = 0; i < onUseActions.Count; i++)
        {
            Type thisType = GetType();
            MethodInfo theMethod = thisType.GetMethod(onUseActions[i], BindingFlags.NonPublic | BindingFlags.Instance);

            if (theMethod != null)
                theMethod.Invoke(this, new object[] { u });
            else
                Debug.Log("Item Use Function not found:" + onUseActions[i]);
        }

        object[] info = new object[2];
        info[0] = this;
        info[1] = u;

        Signal.Send("Inventory", "ItemUsed", info);
    }

    public int GetStatChangeAmount(Helpful.StatTypes stat, int statBase)
    {
        int ind = OnUseStatChanges.FindIndex(x => x.statToChange == stat);

        if (OnUseStatChanges.Count <= 0 || ind < 0)
            return 0;

        if (!OnUseStatChanges[ind].percentChange)
            return (int)OnUseStatChanges[ind].amount;
        else
            return Formulas.MultiplyIntByPercentAndTruncate(statBase, OnUseStatChanges[ind].amount);
    }

    #endregion

    #region On Use Action Functions - No Target

    private void TestFunctionOne()
    { 
        Debug.Log("Test1"); 
    }

    private void RemoveItemOnUse()
    {
        PlayerPartyManager.instance.RemoveItemFromInventory(this, 1);
    }

    #endregion

    #region On Use Action Functions - Unit Target

    private void ApplyStatChanges(Unit u)
    {
        bool levelChanging = false;

        for (int i = 0; i < onUseStatChanges.Count; i++)
        {
            if (onUseStatChanges[i].statToChange == Helpful.StatTypes.Level)
            {
                levelChanging = true;
                continue;
            }

            //TODO: Make current hp a stat probably
            int b = onUseStatChanges[i].statToChange == Helpful.StatTypes.COUNT ? 
                u.GetStatWithMods(Helpful.StatTypes.MaxHP)
                : u.GetStat(onUseStatChanges[i].statToChange);

            int amountToChangeBy = GetStatChangeAmount(OnUseStatChanges[i].statToChange, b);

            if (onUseStatChanges[i].permanentChange)
            {
                if (onUseStatChanges[i].statToChange == Helpful.StatTypes.COUNT)
                {
                    u.CurrentHP += amountToChangeBy;
                }
                else
                {
                    //Permanent changes to stats are handled by giving the unit EXP equal
                    //  to the number of stat points that are being awarded

                    if(amountToChangeBy != 0)
                        if (amountToChangeBy > 0)
                            u.PermanentlyRaiseStat(onUseStatChanges[i].statToChange, amountToChangeBy);
                        else
                            u.PermanentlyLowerStat(onUseStatChanges[i].statToChange, -1 * amountToChangeBy); //needs a positive number to reduce by
                }
            }
            else
            {
                //TODO: Allow for temporary/timed/per battle changes
            }
        }

        //We want level changes to come last because they give exp to other stats, and it could result in inconsistencies if items
        //aren't declared with the stats in the correct order.

        //EX:
        //Gives 2 LVL, and + 1 to a stat.
        //Stat is level 0, needs 3, 5, 10, 15, 25 for levels 0 - 5.
        //Lvl up gives 5 exp to the stat per LVL, going from lvl 0 >> 2
        //Stat boost first:
        //  stat:   0 >> 1  :   +3 EXP
        //  LVL:    0 >> 1  :   +5 EXP
        //  LVL:    1 >> 2  :   +10 EXP
        //  ----------------------------
        //                      +18EXP
        //  Stat:   3,  0 exp
        //
        //Leveling first:
        //  LVL:    0 >> 1  :   +5 EXP
        //  LVL:    1 >> 2  :   +10 EXP
        //  ----------------------------
        //                      +15 EXP
        //  Stat:   2,  7 exp
        //  stat:   2 >> 3      +10 EXP
        //  ----------------------------
        //  Stat:   3, 7 exp    (+25 total EXP)
        //
        //Applying levels last does end up giving a lower amount to the player in these cases, but it will probably be easier
        //to deal with when building.

        if (levelChanging)
        {
            ConsumableStatChange c  = onUseStatChanges.Find(x => x.statToChange == Helpful.StatTypes.Level);

            int b                   = u.GetStat(Helpful.StatTypes.Level);
            int amountToChangeBy    = GetStatChangeAmount(Helpful.StatTypes.Level, b);

            if (c.permanentChange)
            {
                if (amountToChangeBy != 0)
                    if (amountToChangeBy > 0)
                        u.PermanentlyRaiseStat(Helpful.StatTypes.Level, amountToChangeBy);
                    else
                        u.PermanentlyLowerStat(Helpful.StatTypes.Level, -1 * amountToChangeBy);
            }
            else
            {
                //TODO: Allow for temporary/timed/per battle changes (might not be relevant
                //      here; not sure what a temp reduced level would do if leveling just gives exp to
                //      each stat. Might block abilities/skills etc?
            }
        }
    }

    private void ReviveUnit(Unit u)
    {
        //TODO: Not sure i even need a fully separate function if i target the items appropriately
        //      might change if KO is a full status though
        ApplyStatChanges(u);
    }

    private void RemoveItemWithUnitTargetOnUse(Unit u)
    {
        RemoveItemOnUse();
    }

    #endregion
}
