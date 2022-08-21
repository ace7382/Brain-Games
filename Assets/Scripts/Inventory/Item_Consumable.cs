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

        object[] info   = new object[1];
        info[0]         = this;

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
        //DO everything with unit as parameter

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
        for (int i = 0; i < onUseStatChanges.Count; i++)
        {
            //TODO: Make current hp a stat probably
            int b = onUseStatChanges[i].statToChange == Helpful.StatTypes.COUNT ? u.MaxHP : u.GetStat(onUseStatChanges[i].statToChange);

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
