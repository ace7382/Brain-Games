using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Attack : Ability
{
    #region Private Variables

    private int                                     currentCharges;
    private AbilityCharger.AbilityChargeActions     actionThatChargesThisAbility;

    #endregion

    #region Signal Variables

    private SignalReceiver                          battle_requestmatchingabilitychargers_receiver;
    private SignalStream                            battle_requestmatchingabilitychargers_stream;

    #endregion

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT
        chargeType                      = AbilityChargeType.NUM_OF_CHARGES;
        numOfCharges                    = 3;
        abilityName                     = "Attack";
        actionThatChargesThisAbility    = AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE;
        //

        currentCharges                  = 0;

        battle_requestmatchingabilitychargers_stream    = SignalStream.Get("Battle", "RequestMatchingAbilityChargers");
        battle_requestmatchingabilitychargers_receiver  = new SignalReceiver().SetOnSignalCallback(HandleChargeRequest);

        battle_requestmatchingabilitychargers_stream.ConnectReceiver(battle_requestmatchingabilitychargers_receiver);
    }

    public override void Deactivate()
    {
        battle_requestmatchingabilitychargers_stream.DisconnectReceiver(battle_requestmatchingabilitychargers_receiver);

        Debug.Log(string.Format("{0}'s ability {1} was deactivated", owner.UnitInfo.Name, this.abilityName));
    }

    public override void Activate()
    {
        if (currentCharges == 0)
            return;

        object[] info   = new object[2];
        info[0]         = owner.IsPlayer;
        info[1]         = CalculateDamage() * currentCharges;

        Signal.Send("Battle","UnitTakeDamage", info);

        ResetCharges();
    }

    public override string GetDetails()
    {
        return string.Format("Basic Attack\n" +
            "To Charge: Respond correctly to Minigames\n" +
            "{0} damage per charge", CalculateDamage().ToString());
    }

    public void HandleChargeRequest(Signal signal)
    {
        //Signal:
        //AbilityCharger.AbilityChargeAction    - the type of charge action that was completed

        if (currentCharges >= numOfCharges)
            return;

        AbilityCharger.AbilityChargeActions signalType = signal.GetValueUnsafe<AbilityCharger.AbilityChargeActions>();

        if (signalType == actionThatChargesThisAbility)
            AbilityCharger.instance.AddChargeTarget(this, 1, 0); //Standard Charge
    }

    public override void Charge(int notUsedByThisAbility)
    {
        Debug.Log(string.Format("{0}'s ability {1} received a charge", owner.UnitInfo.Name, this.abilityName));

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
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        currentCharges      = 0;

        object[] info       = new object[1];
        info[0]             = this; //The ability to charge

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    private int CalculateDamage()
    {
        return Mathf.Clamp(owner.UnitInfo.GetStat(Helpful.StatTypes.Level) / 10, 1, 100);
    }
}
