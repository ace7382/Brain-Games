using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatModifier
{
    #region Private Variables

    [SerializeField] private Helpful.StatTypes   statBeingModified;
    [SerializeField] private bool                percent;
    [SerializeField] private int                 amount;

    //TODO: Add Source?

    #endregion

    #region Public Properties

    public Helpful.StatTypes    StatBeingModified   { get { return statBeingModified; } }
    public bool                 Percent             { get { return percent; } }

    #endregion

    #region Constructor

    public StatModifier(Helpful.StatTypes statBeingModified, int amount, bool percent, Item sourceItem)
    {
        this.statBeingModified  = statBeingModified;
        this.amount             = amount;
        this.percent            = percent;
    }

    #endregion

    #region Public Functions

    public int GetStatChangeAmount(int statBase)
    {
        if (percent)
            return Formulas.MultiplyIntByPercentAndTruncate(statBase, amount / 100f);
        else
            return amount;
    }

    /// <summary>
    /// This override should be used for displays only, not calculations
    /// </summary>
    /// <returns></returns>
    public int GetStatChangeAmount()
    {
        return amount;
    }

    #endregion
}
