using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Canvas             canvas;
    
    [Space]

    [SerializeField] private GameObject         partyMemberCardPrefab;
    [SerializeField] private RectTransform      partyMemberListContainer;

    [Space]

    [SerializeField] private GameObject         itemSlotPrefab;
    [SerializeField] private RectTransform      itemDisplayContainer;

    [Space]

    [SerializeField] private UIView             itemDetailsView;
    [SerializeField] private Image              itemDetailsItemImage;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemNameText;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemDescriptionText;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemCountText;
    [SerializeField] private UIButton           itemDetailsUseItemButton;
    [SerializeField] private TextMeshProUGUI    itemDetailsUseItemButtonText;

    [Space]

    [SerializeField] private GameObject         itemTargetCardPrefab_Unit;
    [SerializeField] private UIView             itemTargetView;
    [SerializeField] private RectTransform      itemTargetViewListContainer;
    [SerializeField] private TextMeshProUGUI    itemTargetViewHeaderText;

    #endregion

    #region Private Variables

    private List<PartyMemberCardController>     partyCards;
    private List<ItemSlotController>            itemSlotControllers;
    private Item                                itemDisplayedOnItemDetails;

    #endregion

    #region Signal Variables

    private SignalReceiver                      inventory_itemused_receiver;
    private SignalStream                        inventory_itemused_stream;

    #endregion

    #region Public Properties

    public List<PartyMemberCardController> PartyCards
    {
        get { return partyCards; }
    }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        canvas.worldCamera                      = Camera.main;
        canvas.sortingOrder                     = UniversalInspectorVariables.instance.popupScreenOrderInLayer;

        partyCards                              = new List<PartyMemberCardController>();
        itemSlotControllers                     = new List<ItemSlotController>();

        inventory_itemused_stream               = SignalStream.Get("Inventory", "ItemUsed");
        inventory_itemused_receiver             = new SignalReceiver().SetOnSignalCallback(UpdateInventoryPagesOnItemUse);

        Setup();
    }

    private void OnEnable()
    {
        inventory_itemused_stream.ConnectReceiver(inventory_itemused_receiver);
    }

    private void OnDisable()
    {
        inventory_itemused_stream.DisconnectReceiver(inventory_itemused_receiver);
    }

    #endregion

    #region Public Functions

    public void Setup()
    {
        for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
        {
            GameObject go                       = Instantiate(partyMemberCardPrefab, partyMemberListContainer);

            PartyMemberCardController control   = go.GetComponent<PartyMemberCardController>();

            control.Setup(PlayerPartyManager.instance.partyBattleUnits[i]);

            partyCards.Add(control);
        }

        foreach (KeyValuePair<Item, int> pair in PlayerPartyManager.instance.PartyItems)
        {
            GameObject go                       = Instantiate(itemSlotPrefab, itemDisplayContainer);

            ItemSlotController control          = go.GetComponent<ItemSlotController>();

            control.Setup(pair.Key, pair.Value);

            itemSlotControllers.Add(control);
        }
    }

    //Called by the Inventory View's OnClose Behavior
    public void OnClose()
    {
        Signal.Send("Inventory", "InventoryClosed");
    }

    public void ShowItemDetailsView(Item itemToShowDetailsFor)
    {
        if (itemDetailsView.isShowing)
            return;

        itemDisplayedOnItemDetails          = itemToShowDetailsFor;

        itemDetailsItemImage.sprite         = itemDisplayedOnItemDetails.ItemSprite;
        itemDetailsItemNameText.text        = itemDisplayedOnItemDetails.name;
        itemDetailsItemDescriptionText.text = itemDisplayedOnItemDetails.ItemDescription;
        itemDetailsItemCountText.text       = PlayerPartyManager.instance.PartyItems[itemDisplayedOnItemDetails].ToString();

        itemDetailsUseItemButton.gameObject.SetActive(true);
        itemDetailsUseItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();

        if (itemDisplayedOnItemDetails is Item_Consumable)
        {
            Item_Consumable consumable      = (Item_Consumable)itemDisplayedOnItemDetails;

            if (consumable.CanUseFromInventory)
            {
                if (PlayerPartyManager.instance.GetInventoryCount(itemDisplayedOnItemDetails) > 0)
                {
                    if (consumable.Target == Item.ItemTarget.no_target)
                    {
                        itemDetailsUseItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                            .Event.AddListener(delegate { consumable.UseItemFromInventory(); });
                    }
                    else
                    {
                        //Make button Pull up target choice screen
                        itemDetailsUseItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                            .Event.AddListener(delegate { ShowItemTargetView(); });
                    }

                    EnableUseItemButton();
                }
                else
                {
                    DisableUseItemButton();
                }
            }
            else
            {
                DisableUseItemButton();
                itemDetailsUseItemButtonText.text   = "Use in Battle";
            }
        }
        else if (itemDisplayedOnItemDetails is Item_Currency)
        {
            DisableUseItemButton(); //Link to shop?
            itemDetailsUseItemButtonText.text       = "$$$";
        }

        itemDetailsView.Show();
    }

    //Called by the HideItemDetailsView Button's OnClick behavior
    public void HideItemDetailsView()
    {
        if (!itemDetailsView.isVisible)
            return;

        itemDetailsView.Hide();
    }

    //Called by the Hide Item Target View's Button's OnClick behavior
    public void HideItemTargetView()
    {
        if (!itemTargetView.isVisible)
            return;

        itemTargetView.Hide();
    }

    #endregion

    #region Private Functions

    private void UpdateInventoryPagesOnItemUse(Signal signal)
    {
        object[] info                   = signal.GetValueUnsafe<object[]>();
        Item usedItem                   = (Item)info[0];

        ItemSlotController slot         = itemSlotControllers.Find(x => x.Item == usedItem);

        slot.UpdateCount();

        if (itemDetailsView.isVisible)
        {
            itemDetailsItemCountText.text = PlayerPartyManager.instance.GetInventoryCount(itemDisplayedOnItemDetails).ToString();

            if (itemDisplayedOnItemDetails == usedItem)
            {
                if (PlayerPartyManager.instance.GetInventoryCount(itemDisplayedOnItemDetails) <= 0)
                {
                    if (itemTargetView.isVisible)
                        itemTargetView.Hide();

                    if (!itemDisplayedOnItemDetails.AlwaysInInventory)
                        itemDetailsView.Hide();
                    else
                    {
                        DisableUseItemButton();
                    }
                }
            }
        }

        if (PlayerPartyManager.instance.GetInventoryCount(slot.Item) <= 0)
        {
            if (!slot.Item.AlwaysInInventory)
            {
                itemSlotControllers.Remove(slot);
                Destroy(slot.gameObject);
            }
        }
    }

    private void DisableUseItemButton()
    {
        itemDetailsUseItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();
        itemDetailsUseItemButton.interactable   = false;
        itemDetailsUseItemButtonText.text       = "Out of Item";
    }

    private void EnableUseItemButton()
    {
        itemDetailsUseItemButton.interactable   = true;
        itemDetailsUseItemButtonText.text       = "Use Item";
    }

    private void ShowItemTargetView()
    {
        itemTargetViewHeaderText.text   = "Choose " + itemDisplayedOnItemDetails.name + "'s Target";

        Item_Consumable consumable      = (Item_Consumable)itemDisplayedOnItemDetails;

        foreach (Transform child in itemTargetViewListContainer.transform)
            Destroy(child.gameObject);

        switch (consumable.Target)
        {
            case Item.ItemTarget.no_target:
                return;
            case Item.ItemTarget.PLAYER_UNITS_KO:
                {
                    for (int i = 0; i < PlayerPartyManager.instance.KOedPartyMembers.Count; i++)
                    {
                        GameObject go = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                        go.transform.localScale = Vector3.one;

                        ItemTargetCardController_Unit control = go.GetComponent<ItemTargetCardController_Unit>();

                        control.Setup(PlayerPartyManager.instance.KOedPartyMembers[i], consumable);
                    }

                    break;
                }
            case Item.ItemTarget.PLAYER_UNITS_INJURED:
                {
                    for (int i = 0; i < PlayerPartyManager.instance.InjuredPartyMembers.Count; i++)
                    {
                        GameObject go = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                        go.transform.localScale = Vector3.one;

                        ItemTargetCardController_Unit control = go.GetComponent<ItemTargetCardController_Unit>();

                        control.Setup(PlayerPartyManager.instance.InjuredPartyMembers[i], consumable);
                    }

                    break;
                }
            default: //All Player units
                {
                    for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
                    {
                        GameObject go = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                        go.transform.localScale = Vector3.one;

                        ItemTargetCardController_Unit control = go.GetComponent<ItemTargetCardController_Unit>();

                        control.Setup(PlayerPartyManager.instance.partyBattleUnits[i], consumable);
                    }

                    break;
                }
        }

        itemTargetView.Show();
    }

    #endregion
}
