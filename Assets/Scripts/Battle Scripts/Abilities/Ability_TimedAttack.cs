using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_TimedAttack : Ability
{
    #region Private Variables

    private int                 activeCharges;

    #endregion

    #region Signal Variables

    private SignalReceiver      battle_abilitytimerbarfilled_receiver;
    private SignalStream        battle_abilitytimerbarfilled_stream;

    #endregion

    #region Public Functions

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                              = AbilityChargeType.TIMED;
        numOfCharges                            = 2;
        abilityName                             = "Timed Attack";
        secondsToChargeOneBar                   = 2f;

        //

        activeCharges                           = 0;

        battle_abilitytimerbarfilled_stream     = SignalStream.Get("Ability", "TimerBarFilled");
        battle_abilitytimerbarfilled_receiver   = new SignalReceiver().SetOnSignalCallback(ChargeAbility);
        battle_abilitytimerbarfilled_stream.ConnectReceiver(battle_abilitytimerbarfilled_receiver);
    }

    public override void Deactivate()
    {
        //ResetCharges();

        battle_abilitytimerbarfilled_stream.DisconnectReceiver(battle_abilitytimerbarfilled_receiver);

        Debug.Log(string.Format("{0}'s ability {1} was deactivated", owner.UnitInfo.Name, this.abilityName));
    }

    public override void Activate()
    {
        if (activeCharges < 1)
            return;

        int chargesToAttackWith = activeCharges;

        ResetCharges();

        object[] info   = new object[2];
        info[0]         = owner.IsPlayer;
        info[1]         = (100 * chargesToAttackWith); //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle", "UnitTakeDamage", info);
    }

    public override void Charge(int chargeActionID)
    {
        throw new System.NotImplementedException();
    }

    #endregion

    #region Private functions

    private void ChargeAbility(Signal signal)
    {
        //Signal info   - object[1]
        //info[0]       - Ability       - the ability to receive 1 charge

        object[] info = signal.GetValueUnsafe<object[]>();

        if (this == (Ability)info[0])
        {
            Debug.Log(string.Format("{0}'s ability {1} received a charge", owner.UnitInfo.Name, this.abilityName));

            activeCharges = Mathf.Clamp(activeCharges + 1, 0, numOfCharges);

            Signal.Send("Battle", "AbilityCharge", info);
        }
    }

    private void ResetCharges()
    {
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        activeCharges       = 0;

        object[] info       = new object[1];
        info[0]             = this; //The ability to reset

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    #endregion
}
