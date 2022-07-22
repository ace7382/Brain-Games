using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCharger : MonoBehaviour
{
    //Accept any signal used to charge "Charge Type" abilities
    //Request information from abilities to see if they should charge
    //      store positive requests in a list to generate animated particles to fly to the move being charged

    #region Private Variables



    #endregion

    #region Signal Variables

    private SignalReceiver      battle_correctresponse_receiver;
    private SignalStream        battle_correctresponse_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        battle_correctresponse_stream       = SignalStream.Get("Battle", "CorrectResponse");

        battle_correctresponse_receiver     = new SignalReceiver().SetOnSignalCallback(GenerateCharge);
    }

    private void OnEnable()
    {
        battle_correctresponse_stream.ConnectReceiver(battle_correctresponse_receiver);
    }

    private void OnDisable()
    {
        battle_correctresponse_stream.DisconnectReceiver(battle_correctresponse_receiver);
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    #endregion

    #region Private Functions

    private void GenerateCharge(Signal signal)
    {
        //Game actions will need to send signal with
        //  charge type - correct, incorrect, etc

        //This sends signal - Request Charge
        //  signal info will be charge typetype
        //
        //  All abilities/ability buttons will need to listen for this request
        //      if charge type matches AND there is an empty charge slot on the ability
        //      ability will return it's charge box's position

        //For each position received, generate a charge particle
    }

    #endregion
}
