using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Enemy_TooManyTurns : Ability
{
    #region Private Variables

    private int                                     currentCharges;
    private AbilityCharger.AbilityChargeActions     actionThatChargesThisAbility;

    #endregion

    #region Signal Variables

    private SignalReceiver                          battle_requestmatchingabilitychargers_receiver;
    private SignalStream                            battle_requestmatchingabilitychargers_stream;

    #endregion

    #region Public Functions
    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                                      = AbilityChargeType.NUM_OF_CHARGES;
        numOfCharges                                    = 15;
        abilityName                                     = "Too Many Turns";
        autocast                                        = true;
        actionThatChargesThisAbility                    = AbilityCharger.AbilityChargeActions.TILE_ROTATED;

        //

        currentCharges                                  = 0;

        battle_requestmatchingabilitychargers_stream    = SignalStream.Get("Battle", "RequestMatchingAbilityChargers");

        battle_requestmatchingabilitychargers_receiver  = new SignalReceiver().SetOnSignalCallback(HandleChargeRequest);

        battle_requestmatchingabilitychargers_stream.ConnectReceiver(battle_requestmatchingabilitychargers_receiver);
    }

    public override void Activate()
    {
        object[] info   = new object[2];
        info[0]         = owner;
        info[1]         = 4 * currentCharges;

        Signal.Send("Battle","UnitAttacked", info);

        ResetCharges();
    }

    public override void Charge(int chargeActionID)
    {
        if (currentCharges < numOfCharges)
        {
            currentCharges  += 1;

            object[] info   = new object[2];
            info[0]         = this;     //The ability to charge
            info[1]         = 1;        //The number of charges

            Signal.Send("Battle", "AbilityCharge", info);
        }

        if (currentCharges == numOfCharges && autocast)
            Activate();
    }

    #endregion

    #region Private Functions

    private void ResetCharges()
    {
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        currentCharges      = 0;

        object[] info       = new object[1];
        info[0]             = this; //The ability to charge

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    private void HandleChargeRequest(Signal signal)
    {
        if (currentCharges >= numOfCharges)
            return;

        AbilityCharger.AbilityChargeActions signalType = signal.GetValueUnsafe<AbilityCharger.AbilityChargeActions>();

        if (signalType == actionThatChargesThisAbility)
            AbilityCharger.instance.AddChargeTarget(this, 1, 0); //Standard Charge
    }

    #endregion
}
