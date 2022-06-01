using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;

public class AbilityButtonController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    abilityNameText;
    [SerializeField] private RectTransform      chargeMarkerPanelTrans;

    #endregion

    #region Private Variables

    private int                                         currentChargeMarkerIndex;
    private Ability                                     ability;
    private List<AbilityButtonChargeMarkerController>   chargeMarkers;
    private Color                                       chargeMarkerChargedColor;
    private Color                                       chargeMarkerUnchargedColor;

    #endregion

    #region Signal Variables

    private SignalReceiver                              battle_abilitycharge_receiver;
    private SignalStream                                battle_abilitycharge_stream;
    private SignalReceiver                              battle_resetabilitycharges_receiver;
    private SignalStream                                battle_resetabilitycharges_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        battle_abilitycharge_stream         = SignalStream.Get("Battle", "AbilityCharge");
        battle_resetabilitycharges_stream   = SignalStream.Get("Battle", "ResetAbilityCharges");

        battle_abilitycharge_receiver       = new SignalReceiver().SetOnSignalCallback(UpdateChargeButtons);
        battle_resetabilitycharges_receiver = new SignalReceiver().SetOnSignalCallback(ClearCharges);
    }

    private void OnEnable()
    {
        battle_abilitycharge_stream.ConnectReceiver(battle_abilitycharge_receiver);
        battle_resetabilitycharges_stream.ConnectReceiver(battle_resetabilitycharges_receiver);
    }

    private void OnDisable()
    {
        battle_abilitycharge_stream.DisconnectReceiver(battle_abilitycharge_receiver);
        battle_resetabilitycharges_stream.DisconnectReceiver(battle_resetabilitycharges_receiver);
    }

    #endregion

    #region Public Functions

    public void SetupButton(Ability a)
    {
        ability                     = a;
        currentChargeMarkerIndex    = 0;
        chargeMarkers               = new List<AbilityButtonChargeMarkerController>();

        chargeMarkerUnchargedColor  = Color.black;
        chargeMarkerChargedColor    = Color.green;

        abilityNameText.text        = ability.abilityName;

        if (ability.chargeType == Ability.AbilityChargeType.NUM_OF_CHARGES)
        {
            for (int i = 0; i < ability.numOfCharges; i++)
            {
                BattleManager bm                                = FindObjectOfType<BattleManager>();

                GameObject go                                   = Instantiate(bm.ChargeMarkerPrefab, chargeMarkerPanelTrans);
                go.transform.localPosition                      = Vector3.zero;
                go.transform.localScale                         = Vector3.one;

                AbilityButtonChargeMarkerController control     = go.GetComponent<AbilityButtonChargeMarkerController>();
                control.SetFillColor(chargeMarkerUnchargedColor);

                chargeMarkers.Add(control);
            }
        }
    }

    public void Activate()
    {
        ability.Activate();
    }

    #endregion

    #region Private Functions

    private void UpdateChargeButtons(Signal signal)
    {
        //Signal is object[2]
        //info[1]   - Ability   - The ability to charge
        //info[2]   - int       - the number of charges

        object[] info = signal.GetValueUnsafe<object[]>();

        if ((Ability)info[0] == ability)
        {
            int chargesToSet = (int)info[1];

            while (chargesToSet > 0)
            {
                SetNextCharge();

                chargesToSet--;
            }
        }
    }

    private void SetNextCharge()
    {
        if (currentChargeMarkerIndex >= ability.numOfCharges)
            return;

        chargeMarkers[currentChargeMarkerIndex].SetFillColor(chargeMarkerChargedColor);

        currentChargeMarkerIndex++;
    }

    private void ClearCharges(Signal signal)
    {
        //Signal info   - object[1]
        //info[0]       - Ability       - The ability that needs to be cleared

        object[] info   = signal.GetValueUnsafe<object[]>(); 

        if (ability != (Ability)info[0])
            return;

        for (int i = 0; i < chargeMarkers.Count; i++)
        {
            chargeMarkers[i].SetFillColor(chargeMarkerUnchargedColor);
        }

        currentChargeMarkerIndex = 0;
    }

    #endregion
}
