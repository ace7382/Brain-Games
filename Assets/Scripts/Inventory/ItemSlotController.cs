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

    #endregion

    #region Public Properties

    public Item                                 Item { get { return item; } }

    #endregion

    #region Public Functions

    public void Setup(Item i, int count)
    {
        item                = i;
        itemImage.sprite    = item.ItemSprite;

        itemCountText.text  = count.ToString();
    }

    public void UpdateCount()
    {
        if (item == null)
            return;

        itemCountText.text = PlayerPartyManager.instance.GetInventoryCount(item).ToString();
    }

    //Called by the ItemSlotController's OnClick Behavior
    public void OnClick()
    {
        FindObjectOfType<InventoryScreenController>().ShowItemDetailsView(item);
    }

    #endregion
}
