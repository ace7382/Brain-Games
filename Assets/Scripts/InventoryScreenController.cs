using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class InventoryScreenController : MonoBehaviour
{
    #region Inspector Variables

    [Header("Canvas")]
    [SerializeField] private Canvas             canvas;
    
    [Space]

    [Header("Party Member List")]
    [SerializeField] private GameObject         partyMemberCardPrefab;
    [SerializeField] private RectTransform      partyMemberListContainer;

    [Space]

    [Header("Inventory List")]
    [SerializeField] private GameObject         itemSlotPrefab;
    [SerializeField] private RectTransform      itemDisplayContainer;

    [Space]

    [Header("Item Details View")]
    [SerializeField] private UIView             itemDetailsView;
    [SerializeField] private Image              itemDetailsItemImage;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemNameText;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemDescriptionText;
    [SerializeField] private TextMeshProUGUI    itemDetailsItemCountText;
    [SerializeField] private UIButton           itemDetailsUseItemButton;
    [SerializeField] private TextMeshProUGUI    itemDetailsUseItemButtonText;
    [SerializeField] private UIButton           equipmentDetailsButton;
    [SerializeField] private UIButton           equipItemButton;

    [Space]

    [Header("Item Target View")]
    [SerializeField] private GameObject         itemTargetCardPrefab_Unit;
    [SerializeField] private UIView             itemTargetView;
    [SerializeField] private RectTransform      itemTargetViewListContainer;
    [SerializeField] private TextMeshProUGUI    itemTargetViewHeaderText;
    [SerializeField] private TextMeshProUGUI    itemTargetViewNoTargetsText;

    [Space]

    [Header("Equipment Details View")]
    [SerializeField] private UIView             equipmentDetailsView;
    [SerializeField] private RectTransform      equipmentDetailsCanEquipContainer;
    [SerializeField] private RectTransform      equipmentDetailsStatChangeContainer;
    [SerializeField] private RectTransform      equipmentDetailsAdditionalEffectsContainer;
    [SerializeField] private RectTransform      equipmentDetailsStatRequirementsContainer;
    [SerializeField] private TextMeshProUGUI    equipmentDetailsStatRequirementsHeader;
    [SerializeField] private TextMeshProUGUI    equipmentDetailsStatChangeHeader;
    [SerializeField] private TextMeshProUGUI    equipmentDetailsAdditionalEffectsHeader;
    [SerializeField] private GameObject         equippableUnitIconPrefab;
    [SerializeField] private GameObject         statReqPrefab;
    [SerializeField] private GameObject         statChangePrefab;
    [SerializeField] private GameObject         additionalEffectPrefab;

    #endregion

    #region Private Variables

    private List<PartyMemberCardController>     partyCards;
    private List<ItemSlotController>            itemSlotControllers;
    private Item                                itemDisplayedOnItemDetails;

    #endregion

    #region Private Consts

    private const string                        no_party_members_can_equip_message          = "No Party Members can equip this item!";
    private const string                        no_party_members_available_message          = "No Party Members available!";
    private const string                        no_injured_party_members_available_message  = "No injured Party Members available!";
    private const string                        no_KOed_party_members_available_message     = "No KOed Party Members available!";
    private const string                        no_alive_party_members_available_message    = "All Party Members are KOed!";

    #endregion

    #region Signal Variables

    private SignalReceiver                      inventory_itemused_receiver;
    private SignalStream                        inventory_itemused_stream;
    private SignalReceiver                      inventory_itemequipped_receiver;
    private SignalStream                        inventory_itemequipped_stream;
    private SignalReceiver                      inventory_itemunequipped_receiver;
    private SignalStream                        inventory_itemunequipped_stream;

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
        inventory_itemequipped_stream           = SignalStream.Get("Inventory", "ItemEquipped");
        inventory_itemunequipped_stream         = SignalStream.Get("Inventory", "ItemUnequipped");
        inventory_itemused_receiver             = new SignalReceiver().SetOnSignalCallback(UpdateInventoryPagesOnItemUse);
        inventory_itemequipped_receiver         = new SignalReceiver().SetOnSignalCallback(UpdateInventoryPagesOnItemEquip);
        inventory_itemunequipped_receiver       = new SignalReceiver().SetOnSignalCallback(UpdateInventorySlotForUnequippedItem);

        Setup();
    }

    private void OnEnable()
    {
        inventory_itemused_stream.ConnectReceiver(inventory_itemused_receiver);
        inventory_itemequipped_stream.ConnectReceiver(inventory_itemequipped_receiver);
        inventory_itemunequipped_stream.ConnectReceiver(inventory_itemunequipped_receiver);
    }

    private void OnDisable()
    {
        inventory_itemused_stream.DisconnectReceiver(inventory_itemused_receiver);
        inventory_itemequipped_stream.DisconnectReceiver(inventory_itemequipped_receiver);
        inventory_itemunequipped_stream.DisconnectReceiver(inventory_itemunequipped_receiver);
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
        equipmentDetailsButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();
        equipItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();

        if (itemDisplayedOnItemDetails is Item_Consumable)
        {
            equipItemButton.gameObject.SetActive(false);
            equipmentDetailsButton.gameObject.SetActive(false);
            itemDetailsUseItemButton.gameObject.SetActive(true);

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
                            .Event.AddListener(delegate { ShowItemTargetView_Consumable(); });
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
            equipItemButton.gameObject.SetActive(false);
            equipmentDetailsButton.gameObject.SetActive(false);
            itemDetailsUseItemButton.gameObject.SetActive(true);

            DisableUseItemButton(); //Link to shop?
            itemDetailsUseItemButtonText.text       = "$$$";
        }
        else if (itemDisplayedOnItemDetails is Item_Equipment)
        {
            equipItemButton.gameObject.SetActive(true);
            equipmentDetailsButton.gameObject.SetActive(true);
            itemDetailsUseItemButton.gameObject.SetActive(false);

            equipmentDetailsButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                .Event.AddListener(delegate { ShowEquipmentDetailView((Item_Equipment)itemDisplayedOnItemDetails); });

            equipItemButton.GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                .Event.AddListener(delegate { ShowItemTargetView_Equipment(); });
        }

        itemDetailsView.Show();
    }

    //Called by the Equipment Detail Button's OnClick Behavior (added by code though)
    public void ShowEquipmentDetailView(Item_Equipment eq)
    {
        //Can Equip
        foreach (Transform child in equipmentDetailsCanEquipContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < eq.EquippableUnits.Count; i++)
        {
            int unitIndex = PlayerPartyManager.instance.partyBattleUnits.FindIndex(x => x.Name == eq.EquippableUnits[i]);

            if (eq.EquippableUnits[i] == "ALL")
            {
                //TODO: Display "ALL" icon
                GameObject go = Instantiate(statReqPrefab, equipmentDetailsCanEquipContainer);
                go.GetComponent<TextMeshProUGUI>().text = "ALL";
            }
            else if (unitIndex >= 0)
            {
                //TODO: Add mini icon to unit's definition?
                GameObject go = Instantiate(equippableUnitIconPrefab, equipmentDetailsCanEquipContainer);

                go.GetComponent<Image>().sprite = PlayerPartyManager.instance.partyBattleUnits[unitIndex].MiniSprite;
            }
            else
            {
                //TODO: Add ? icon for any equippable characters not yet found
                GameObject go = Instantiate(statReqPrefab, equipmentDetailsCanEquipContainer);
                go.GetComponent<TextMeshProUGUI>().text = "?";
            }
        }

        //Stat Requirements
        foreach (Transform child in equipmentDetailsStatRequirementsContainer)
            Destroy(child.gameObject);

        equipmentDetailsStatRequirementsHeader.gameObject.SetActive(eq.StatReqs.Count > 0);
        equipmentDetailsStatRequirementsContainer.gameObject.SetActive(eq.StatReqs.Count > 0);

        for (int i = 0; i < eq.StatReqs.Count; i++)
        {
            GameObject go = Instantiate(statReqPrefab, equipmentDetailsStatRequirementsContainer);

            go.GetComponent<TextMeshProUGUI>().text = string.Format("{0}: {1}", eq.StatReqs[i].stat.GetShorthand(), eq.StatReqs[i].amount.ToString());
        }

        //Stat Changes
        foreach (Transform child in equipmentDetailsStatChangeContainer)
            Destroy(child.gameObject);

        equipmentDetailsStatChangeHeader.gameObject.SetActive(eq.StatModifiers.Count > 0);
        equipmentDetailsStatChangeContainer.gameObject.SetActive(eq.StatModifiers.Count > 0);

        for (int i = 0; i < eq.StatModifiers.Count; i++)
        {
            GameObject go = Instantiate(statChangePrefab, equipmentDetailsStatChangeContainer);

            TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
            t.text = string.Format("{0} {1} {2}{3}"
                                            , eq.StatModifiers[i].StatBeingModified.GetStringValue()
                                            , eq.StatModifiers[i].GetStatChangeAmount() > 0 ? "+" : "-"
                                            , eq.StatModifiers[i].GetStatChangeAmount().ToString()
                                            , eq.StatModifiers[i].Percent ? "%" : ""
                                            );
        }

        //Additional Effects
        foreach (Transform child in equipmentDetailsAdditionalEffectsContainer)
            Destroy(child.gameObject);

        equipmentDetailsAdditionalEffectsHeader.gameObject.SetActive(eq.AdditionalEffects.Count > 0);
        equipmentDetailsAdditionalEffectsContainer.gameObject.SetActive(eq.AdditionalEffects.Count > 0);

        for (int i = 0; i < eq.AdditionalEffects.Count; i++)
        {
            GameObject go   = Instantiate(additionalEffectPrefab, equipmentDetailsAdditionalEffectsContainer);

            go.GetComponent<TextMeshProUGUI>().text = eq.AdditionalEffects[i];
        }

        //vvvvv IDFKY but this is needed to make the different sections space out correctly vvvvv
        ContentSizeFitter c = equipmentDetailsAdditionalEffectsContainer.GetComponent<ContentSizeFitter>();
        c.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
        Canvas.ForceUpdateCanvases();
        c.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        equipmentDetailsView.Show();
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

    //Called by the Close Equipment Detail's OnClick Behavior
    public void HideEquipmentDetailView()
    {
        if (!equipmentDetailsView.isVisible)
            return;

        equipmentDetailsView.Hide();
    }

    #endregion

    #region Private Functions

    private void UpdateInventoryPagesOnItemUse(Signal signal)
    {
        //Signal Data - object[2]
        //info[0]   -   Item    -   the item used
        //info[1]   -   Unit    -   the unit that the item was used on. Might be null.
        //TODO: Maybe do this better when more items are implemented

        object[] info   = signal.GetValueUnsafe<object[]>();
        Item usedItem   = (Item)info[0];
        Unit u          = (Unit)info[1];

        StartCoroutine(UpdateInventoryPagesOnItemUseCoroutine(usedItem, u));
    }

    private void UpdateInventoryPagesOnItemEquip(Signal signal)
    {
        //Signal Data - object[2]
        //info[0]   -   Item    -   the item Equipped
        //info[1]   -   Unit    -   the unit that the item was used on. Might be null

        object[] info   = signal.GetValueUnsafe<object[]>();
        Item usedItem   = (Item)info[0];
        Unit u          = (Unit)info[1];

        StartCoroutine(UpdateInventoryPagesOnItemUseCoroutine(usedItem, u));
    }

    private void UpdateInventorySlotForUnequippedItem(Signal signal)
    {
        //Signal Data - object[2]
        //info[0]   -   Item    -   the item Unequipped
        //info[1]   -   Unit    -   the unit that the item was used on. Might be null.

        object[] info   = signal.GetValueUnsafe<object[]>();
        Item usedItem   = (Item)info[0];
        Unit u          = (Unit)info[1];

        ItemSlotController slot = itemSlotControllers.Find(x => x.Item == usedItem);

        if (slot != null)
        {
            slot.UpdateCount();
        }
        else
        {
            //TODO: put this in the correct order after adding it back.
            GameObject go               = Instantiate(itemSlotPrefab, itemDisplayContainer);

            ItemSlotController control  = go.GetComponent<ItemSlotController>();

            control.Setup(usedItem, PlayerPartyManager.instance.PartyItems[usedItem]);

            itemSlotControllers.Add(control);
        }
    }

    private IEnumerator UpdateInventoryPagesOnItemUseCoroutine(Item usedItem, Unit u)
    {
        ItemSlotController slot         = itemSlotControllers.Find(x => x.Item == usedItem);

        slot.UpdateCount();

        if (itemTargetView.isVisible && u != null)
        {
            //1) TODO: block input (probably put up a blank screen). Might make it skip down to part 3 if you click while it's animating

            //-------------------

            //2) Update the Card
            ItemTargetCardController_Unit card = 
                itemTargetViewListContainer.GetComponentsInChildren<ItemTargetCardController_Unit>()
                .ToList<ItemTargetCardController_Unit>().Find(x => x.Unit == u);

            yield return StartCoroutine(card?.UpdateCardOnItemUse()); //We want the animation to finish. 
            //--------------------

            //3) Check Item Count
            if (PlayerPartyManager.instance.GetInventoryCount(usedItem) <= 0)
                itemTargetView.Hide();
            //--------------------

            //4) Check if the updated card is still a target. "setup" again if it is, if not remove it
            else
            {
                bool keepCard = true;

                //Equipment doesn't need to do this check because the only checks for eq don't change on items being equipped/unequipped
                if (usedItem is Item_Consumable)
                {
                    switch (usedItem.Target)
                    {
                        case Item.ItemTarget.PLAYER_UNITS_ALL:
                            itemTargetViewNoTargetsText.text = no_party_members_available_message;
                            keepCard = PlayerPartyManager.instance.partyBattleUnits.Contains(u);
                            break;
                        case Item.ItemTarget.PLAYER_UNITS_INJURED:
                            itemTargetViewNoTargetsText.text = no_injured_party_members_available_message;
                            keepCard = PlayerPartyManager.instance.InjuredPartyMembers.Contains(u);
                            break;
                        case Item.ItemTarget.PLAYER_UNITS_KO:
                            itemTargetViewNoTargetsText.text = no_KOed_party_members_available_message;
                            keepCard = PlayerPartyManager.instance.KOedPartyMembers.Contains(u);
                            break;
                        case Item.ItemTarget.PLAYER_UNITS_ALIVE:
                            itemTargetViewNoTargetsText.text = no_alive_party_members_available_message;
                            keepCard = PlayerPartyManager.instance.AlivePartyMembers.Contains(u);
                            break;
                    }
                }

                if (keepCard)
                    card.Setup(card.Unit, usedItem);
                else
                {
                    Destroy(card.gameObject); //TODO: Animate the removal

                    if (itemTargetViewListContainer.childCount == 1) //This should be the card destroyed above, bc it's not removed until end of frame
                        itemTargetViewListContainer.DetachChildren();
                }
            }
            //-------------------------------

            //5) Put text up to show no target
            itemTargetViewNoTargetsText.gameObject.SetActive(itemTargetViewListContainer.childCount == 0);
            //
        }

        if (itemDetailsView.isVisible)
        {
            itemDetailsItemCountText.text = PlayerPartyManager.instance.GetInventoryCount(itemDisplayedOnItemDetails).ToString();

            if (itemDisplayedOnItemDetails == usedItem)
            {
                if (PlayerPartyManager.instance.GetInventoryCount(itemDisplayedOnItemDetails) <= 0)
                {
                    //if (itemTargetView.isVisible) //Moved above
                    //    itemTargetView.Hide();

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

    private void ShowItemTargetView_Equipment()
    {
        itemTargetViewHeaderText.text   = "Choose " + itemDisplayedOnItemDetails.name + "'s Target";

        Item_Equipment equipment        = (Item_Equipment)itemDisplayedOnItemDetails;

        foreach (Transform child in itemTargetViewListContainer.transform)
            Destroy(child.gameObject);

        itemTargetViewListContainer.DetachChildren();

        bool all = equipment.EquippableUnits.Contains("ALL");

        for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
        {
            if (all || equipment.EquippableUnits.Contains(PlayerPartyManager.instance.partyBattleUnits[i].Name))
            {
                bool passesStatReqCheck = true;

                foreach (Item_Equipment.StatIntCombo req in equipment.StatReqs)
                {
                    if (PlayerPartyManager.instance.partyBattleUnits[i].GetStat(req.stat) < req.amount)
                    {
                        passesStatReqCheck = false;
                        break;
                    }
                }

                if (passesStatReqCheck)
                {
                    GameObject go                           = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                    go.transform.localScale                 = Vector3.one;

                    ItemTargetCardController_Unit control   = go.GetComponent<ItemTargetCardController_Unit>();

                    control.Setup(PlayerPartyManager.instance.partyBattleUnits[i], equipment);
                }
            }
        }

        itemTargetViewNoTargetsText.text = no_party_members_can_equip_message;
        itemTargetViewNoTargetsText.gameObject.SetActive(itemTargetViewListContainer.childCount == 0);

        itemTargetView.Show();
    }

    private void ShowItemTargetView_Consumable()
    {
        itemTargetViewHeaderText.text   = "Choose " + itemDisplayedOnItemDetails.name + "'s Target";

        Item_Consumable consumable      = (Item_Consumable)itemDisplayedOnItemDetails;

        foreach (Transform child in itemTargetViewListContainer.transform)
            Destroy(child.gameObject);

        itemTargetViewListContainer.DetachChildren();

        switch (consumable.Target)
        {
            case Item.ItemTarget.no_target:
                return;
            case Item.ItemTarget.PLAYER_UNITS_KO:
                {
                    itemTargetViewNoTargetsText.text = no_KOed_party_members_available_message;

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
                    itemTargetViewNoTargetsText.text = no_injured_party_members_available_message;

                    for (int i = 0; i < PlayerPartyManager.instance.InjuredPartyMembers.Count; i++)
                    {
                        GameObject go = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                        go.transform.localScale = Vector3.one;

                        ItemTargetCardController_Unit control = go.GetComponent<ItemTargetCardController_Unit>();

                        control.Setup(PlayerPartyManager.instance.InjuredPartyMembers[i], consumable);
                    }

                    break;
                }
            case Item.ItemTarget.PLAYER_UNITS_ALIVE:
                {
                    itemTargetViewNoTargetsText.text = no_alive_party_members_available_message;

                    for (int i = 0; i < PlayerPartyManager.instance.AlivePartyMembers.Count; i++)
                    {
                        GameObject go = Instantiate(itemTargetCardPrefab_Unit, itemTargetViewListContainer);

                        go.transform.localScale = Vector3.one;

                        ItemTargetCardController_Unit control = go.GetComponent<ItemTargetCardController_Unit>();

                        control.Setup(PlayerPartyManager.instance.AlivePartyMembers[i], consumable);
                    }

                    break;
                }
            default: //All Player units
                {
                    itemTargetViewNoTargetsText.text = no_party_members_available_message;

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

        itemTargetViewNoTargetsText.gameObject.SetActive(itemTargetViewListContainer.childCount == 0);

        itemTargetView.Show();
    }

    #endregion
}
