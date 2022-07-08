using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyManagementScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Canvas         canvas;
    [SerializeField] private GameObject     partyMemberCardPrefab;
    [SerializeField] private RectTransform  partyMemberListContainer;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        canvas.worldCamera      = Camera.main;
        canvas.sortingOrder     = UniversalInspectorVariables.instance.popupScreenOrderInLayer;

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
        }
    }

    //Called by the Party Management View's OnShow Callback
    public void OnShow()
    {

    }

    #endregion
}
