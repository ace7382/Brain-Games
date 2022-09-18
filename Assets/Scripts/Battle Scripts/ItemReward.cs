using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemReward
{
    #region Inspector Variables

    [SerializeField] private Item   item;
    [SerializeField] private float  chance;

    #endregion

    #region Public Properties

    public Item                     Item        { get { return item; } }
    public float                    Chance      { get { return chance; } }

    #endregion
}
