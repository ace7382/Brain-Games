using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Canvas         canvas;
    [SerializeField] private GameObject     partyMemberCardPrefab;
    [SerializeField] private RectTransform  partyMemberListContainer;

    #endregion

    #region Private Functions

    private List<PartyMemberCardController> partyCards;

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
        canvas.worldCamera      = Camera.main;
        canvas.sortingOrder     = UniversalInspectorVariables.instance.popupScreenOrderInLayer;

        partyCards              = new List<PartyMemberCardController>();

        Setup();
    }

    #endregion

    #region Public Functions

    public void Setup()
    {
        for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
        {
            GameObject go = Instantiate(partyMemberCardPrefab, partyMemberListContainer);

            PartyMemberCardController control = go.GetComponent<PartyMemberCardController>();

            control.Setup(PlayerPartyManager.instance.partyBattleUnits[i]);

            partyCards.Add(control);
        }
    }

    //Called by the Inventory View's OnClose Behavior
    public void OnClose()
    {
        Signal.Send("Inventory", "InventoryClosed");
    }

    #endregion
}
