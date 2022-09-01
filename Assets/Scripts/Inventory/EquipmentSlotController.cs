using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;

public class EquipmentSlotController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image          itemImage;
    [SerializeField] private Image          highlight;
    [SerializeField] private UIButton       uiButton;

    #endregion

    #region Private Variables

    private Item_Equipment                  item;

    #endregion

    #region Public Properties

    public Item_Equipment                   Item { get { return item; } }
    public UIButton                         UIButton { get { return uiButton; } }

    #endregion

    #region Public Functions

    public void Setup(Item_Equipment i)
    {
        item                    = i;

        if (item == null)
        {
            itemImage.sprite    = null;
            itemImage.color     = Color.clear;
            highlight.color     = Color.clear;
        }
        else
        {
            itemImage.sprite    = item.ItemSprite;
            itemImage.color     = Color.white;
            highlight.color     = Color.green;
        }
    }

    //Called by the EquipmentSlotConroller's OnClick Behavior
    public void OnClick()
    {
        //TODO: Probably remove this
    }

    #endregion
}
