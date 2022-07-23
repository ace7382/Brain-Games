using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Attack1 : Ability
{
    #region Private Variables

    private int                                     currentCharges;
    private AbilityCharger.AbilityChargeActions     actionThatChargesThisAbility;
    private AbilityCharger.AbilityChargeActions     actionThatResetsThisAbility;

    #endregion

    #region Signal Variables

    private SignalReceiver                          battle_requestmatchingabilitychargers_receiver;
    private SignalStream                            battle_requestmatchingabilitychargers_stream;

    #endregion

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                                      = AbilityChargeType.NUM_OF_CHARGES;
        numOfCharges                                    = 7;
        abilityName                                     = "Big Attack";
        actionThatChargesThisAbility                    = AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE;
        actionThatResetsThisAbility                     = AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE;

        //

        currentCharges                                  = 0;

        battle_requestmatchingabilitychargers_stream    = SignalStream.Get("Battle", "RequestMatchingAbilityChargers");

        battle_requestmatchingabilitychargers_receiver  = new SignalReceiver().SetOnSignalCallback(HandleChargeRequest);

        battle_requestmatchingabilitychargers_stream.ConnectReceiver(battle_requestmatchingabilitychargers_receiver);
    }

    public override void Deactivate()
    {
        battle_requestmatchingabilitychargers_stream.DisconnectReceiver(battle_requestmatchingabilitychargers_receiver);
    }

    public override void Activate()
    {
        if (currentCharges == 0)
            return;

        object[] info   = new object[2];
        info[0]         = owner.IsPlayer;
        info[1]         = 3 * currentCharges; //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle","UnitTakeDamage", info);

        ResetCharges();
    }

    public void HandleChargeRequest(Signal signal)
    {
        if (currentCharges >= numOfCharges)
            return;

        AbilityCharger.AbilityChargeActions signalType = signal.GetValueUnsafe<AbilityCharger.AbilityChargeActions>();

        if (signalType == actionThatChargesThisAbility)
            AbilityCharger.instance.AddChargeTarget(this, 1, 0); //Standard Charge
        else if (signalType == actionThatResetsThisAbility)
        {
            if (currentCharges == 0)
                return;

            AbilityCharger.instance.AddChargeTarget(this, 1, 1); //Reset Charge
        }
    }

    public override void Charge(int chargeActionID)
    {
        if (chargeActionID == 1)
        { //Reset
            ResetCharges();
            return;
        }

        if (currentCharges < numOfCharges)
        {
            currentCharges      += 1;

            object[] info       = new object[2];
            info[0]             = this; //The ability to charge
            info[1]             = 1;    //The number of charges

            Signal.Send("Battle", "AbilityCharge", info);
        }
    }

    public void ResetCharges()
    {
        currentCharges      = 0;

        object[] info       = new object[1];
        info[0]             = this; //The ability to charge

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    //public void ResetChargesOnMiss(Signal signal)
    //{
    //    ResetCharges();
    //}
}
