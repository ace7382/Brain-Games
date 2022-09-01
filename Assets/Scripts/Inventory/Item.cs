using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Item : ScriptableObject
{
    #region Public Enums

    public enum ItemTarget
    {
        no_target,
        PLAYER_UNITS_ALL,
        PLAYER_UNITS_INJURED,
        PLAYER_UNITS_KO,
        PLAYER_UNITS_ALIVE
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private Sprite             itemSprite;

    [TextArea(3, 5)]
    [SerializeField] private string             itemDescription;
    
    [Space]
    
    [SerializeField] private bool               alwaysInInventory;

    [SerializeField] private ItemTarget         target;

    #endregion

    #region Public Properties

    public Sprite                       ItemSprite          { get { return itemSprite; } }
    public string                       ItemDescription     { get { return itemDescription; } }
    public bool                         AlwaysInInventory   { get { return alwaysInInventory; } }
    public ItemTarget                   Target              { get { return target; } }

    #endregion
}