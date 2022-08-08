using Doozy.Runtime.Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "New Item - Consumable", order = 42)]
public class Item_Consumable : Item
{
    #region Inspector Variables

    [SerializeField] private bool                       useFromInventory;
    [SerializeField] private bool                       useFromBattle;

    [Space]

    [SerializeField] private List<string>               onUseActions;

    #endregion

    #region Public Properties

    public bool CanUseFromInventory                     { get { return useFromInventory; } }
    public bool CanUseFromBattle                        { get { return useFromBattle; } }

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

        object[] info = new object[1];
        info[0] = this;

        Signal.Send("Inventory", "ItemUsed", info);
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

    private void Heal25Percent(Unit u)
    {
        int amountToHeal = (int)(u.MaxHP * .25f);

        u.CurrentHP += amountToHeal;

        Debug.Log(string.Format("{0} healed {1} HP", u.Name, amountToHeal.ToString()));
    }

    private void RemoveItemWithUnitTargetOnUse(Unit u)
    {
        RemoveItemOnUse();
    }

    #endregion
}
