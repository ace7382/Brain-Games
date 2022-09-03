using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Equipment", menuName = "New Item - Equipment", order = 4)]
public class Item_Equipment : Item
{
    #region Public Classes

    [System.Serializable]
    public struct StatIntCombo
    {
        public int                                      amount;
        public Helpful.StatTypes                        stat;
    }

    [System.Serializable]
    public struct EquipmentStatChange
    {
        [System.Serializable]
        public enum EquipmentStatChangeType
        {
            Percent,
            Value
        }

        public StatIntCombo                             statAndAmount;
        public EquipmentStatChangeType                  type;
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private List<string>               equippableUnits;
    [SerializeField] private List<StatIntCombo>         statReqs;
    //[SerializeField] private List<EquipmentStatChange>  statChanges;
    [SerializeField] private List<StatModifier>         statModifiers;
    [SerializeField] private List<string>               additionalEffects;

    #endregion

    #region Public Properties

    public List<string>                 EquippableUnits         { get { return equippableUnits; } }
    public List<StatIntCombo>           StatReqs                { get { return statReqs; } }
    public List<StatModifier>           StatModifiers           { get { return statModifiers; } }
    public List<string>                 AdditionalEffects       { get { return additionalEffects; } }

    #endregion

    #region Public Functions

    public int GetStatChangeAmount(Helpful.StatTypes stat, int statBase)
    {
        int ind = StatModifiers.FindIndex(x => x.StatBeingModified == stat);

        if (StatModifiers.Count <= 0 || ind < 0)
            return 0;

        return StatModifiers[ind].GetStatChangeAmount(statBase);
    }

    public void OnEquip(Unit u)
    {
        Debug.Log(u.Name + " Equipping " + name);

        for (int i = 0; i < statModifiers.Count;i++)
        {
            u.AddStatModifier(statModifiers[i]);
        }

        PlayerPartyManager.instance.RemoveItemFromInventory(this, 1);

        object[] info = new object[2];
        info[0] = this;
        info[1] = u;

        Signal.Send("Inventory", "ItemEquipped", info);
    }

    public void OnUnequip(Unit u)
    {
        Debug.Log(u.Name + " UNequipping " + name);

        for (int i = 0; i < statModifiers.Count; i++)
        {
            u.RemoveStatModifier(statModifiers[i]);
        }

        PlayerPartyManager.instance.AddItemToInventory(this, 1);

        object[] info   = new object[2];
        info[0]         = this;
        info[1]         = u;

        Signal.Send("Inventory", "ItemUnequipped", info);
    }

    #endregion
}
