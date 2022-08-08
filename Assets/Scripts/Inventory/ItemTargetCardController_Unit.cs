using Doozy.Runtime.UIManager.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemTargetCardController_Unit : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image      characterPortrait;
    [SerializeField] private UIButton   button;

    #endregion

    #region Private Variables

    private Unit                        unit;

    #endregion

    #region Public Functions

    public void Setup(Unit u, Item i)
    {
        unit                        = u;
        characterPortrait.sprite    = unit.InBattleSprite;

        button.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event
            .AddListener(delegate { OnClick(i); });
    }

    #endregion

    #region Private Functions

    private void OnClick(Item i)
    {
        ((Item_Consumable)i).UseItemFromInventory(unit);
    }

    #endregion


}
