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
    [SerializeField] private List<EquipmentStatChange>  statChanges;
    [SerializeField] private List<string>               additionalEffects;

    #endregion

    #region Public Properties

    public List<string>                 EquippableUnits         { get { return equippableUnits; } }
    public List<StatIntCombo>           StatReqs                { get { return statReqs; } }
    public List<EquipmentStatChange>    StatChanges             { get { return statChanges; } }
    public List<string>                 AdditionalEffects       { get { return additionalEffects; } }

    #endregion
}
