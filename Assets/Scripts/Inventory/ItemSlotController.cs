using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image              itemImage;
    [SerializeField] private GameObject         itemCountPanel;
    [SerializeField] private TextMeshProUGUI    itemCountText;

    #endregion

    #region Private Variables

    private Item                                item;
    private int                                 count;

    #endregion

    #region Public Properties

    public Item                                 Item { get { return item; } }

    #endregion

    #region Public Functions

    public void Setup(Item i, int cou)
    {
        item                = i;
        itemImage.sprite    = item.ItemSprite;

        count               = cou;  
        itemCountText.text  = count.ToString();
    }

    public void UpdateCountFromInventory()
    {
        if (item == null)
            return;

        count              = PlayerPartyManager.instance.GetInventoryCount(item);
        itemCountText.text = count.ToString();
    }

    public void AddToCount(int addition)
    {
        count               += addition;
        itemCountText.text  = count.ToString();
    }

    //Called by the ItemSlotController's OnClick Behavior
    public void OnClick()
    {
        FindObjectOfType<InventoryScreenController>().ShowItemDetailsView(item);
    }

    #endregion
}
