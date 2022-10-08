using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;

public class MentalHealthBarController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    displayText;
    [SerializeField] private Image              imageMask;

    #endregion

    #region Private Variables

    private Unit                                owner;

    #endregion

    #region Signal Variables

    private SignalReceiver                      partymanagement_unitstatchanged_receiver;
    private SignalStream                        partymanagement_unitstatchanged_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        partymanagement_unitstatchanged_stream      = SignalStream.Get("PartyManagement", "UnitStatChanged");
        partymanagement_unitstatchanged_receiver    = new SignalReceiver().SetOnSignalCallback(UpdateDisplay);
    }

    private void OnEnable()
    {
        partymanagement_unitstatchanged_stream.ConnectReceiver(partymanagement_unitstatchanged_receiver);
    }

    private void OnDisable()
    {
        partymanagement_unitstatchanged_stream.DisconnectReceiver(partymanagement_unitstatchanged_receiver);
    }

    #endregion

    #region Public Functions

    public void Init(Unit u)
    {
        owner = u;
        UpdateDisplay();
    }

    #endregion

    #region Private Functions

    private void UpdateDisplay(Signal signal)
    {
        //Signal info is object[4]
        //info[0] Unit                  - The unit whos stat is changing
        //info[1] Helful.StatType       - The stat changing. type COUNT is current MH
        //info[1] int                   - the amount changing
        //info[2] int                   - the old value

        object[] info = signal.GetValueUnsafe<object[]>();

        if ((Unit)info[0] == owner)
        {
            Helpful.StatTypes stat = (Helpful.StatTypes)info[1];

            if (stat == Helpful.StatTypes.COUNT || stat == Helpful.StatTypes.MaxHP)
            {
                UpdateDisplay();
            }
        }
    }

    private void UpdateDisplay()
    {
        displayText.text = owner.CurrentHP.ToString() + " / " + owner.GetStatWithMods(Helpful.StatTypes.MaxHP).ToString();

        float fillPercent = (float)((float)owner.CurrentHP / (float)owner.GetStatWithMods(Helpful.StatTypes.MaxHP));

        imageMask.fillAmount = fillPercent;
    }

    #endregion
}
