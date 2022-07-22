using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class PlayerPartyManager : MonoBehaviour
{
    #region Singleton

    public static PlayerPartyManager    instance = null;

    #endregion

    #region Inspector Variables

    public List<Unit>                   partyBattleUnits;

    #endregion

    #region Signal Variables

    private SignalReceiver              partymanagement_awardexperience_receiver;
    private SignalStream                partymanagement_awardexperience_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        partymanagement_awardexperience_stream      = SignalStream.Get("PartyManagement", "AwardExperience");

        partymanagement_awardexperience_receiver    = new SignalReceiver().SetOnSignalCallback(ProcessEXP);
    }

    private void OnEnable()
    {
        partymanagement_awardexperience_stream.ConnectReceiver(partymanagement_awardexperience_receiver);
    }

    private void OnDisable()
    {
        partymanagement_awardexperience_stream.DisconnectReceiver(partymanagement_awardexperience_receiver);
    }

    private void Start()
    {
        for (int i = 0; i < partyBattleUnits.Count; i++)
        {
            partyBattleUnits[i].Init();
        }
    }

    #endregion

    #region Public Functions

    public Unit GetFirstLivingUnit()
    {
        return partyBattleUnits.Find(x => x.CurrentHP > 0);
    }

    #endregion

    #region Private Functions

    private void ProcessEXP(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Helpful.StatType  - The stat that earned EXP
        //info[1]   - int               - The amount of EXP earned for the action
        //info[2]   - Unit              - The unit that earns the EXP

        object[] info = signal.GetValueUnsafe<object[]>();

        //TODO: Request Modifiers
        //TODO: Store and Process MOdifiers

        partyBattleUnits.Find(x => x == (Unit)info[2]).AddEXP((Helpful.StatTypes)info[0], (int)info[1]);
    }

    #endregion
}
