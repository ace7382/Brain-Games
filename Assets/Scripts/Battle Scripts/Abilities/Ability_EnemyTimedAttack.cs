using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class Ability_EnemyTimedAttack : Ability
{
    #region Private Variables

    private int                 activeCharges;

    #endregion

    #region Signal Variables

    private SignalReceiver      battle_abilitytimerbarfilled_receiver;
    private SignalStream        battle_abilitytimerbarfilled_stream;
    private SignalReceiver      battle_correctresponse_receiver;
    private SignalStream        battle_correctresponse_stream;
    private SignalReceiver      battle_incorrectresponse_receiver;
    private SignalStream        battle_incorrectresponse_stream;

    #endregion

    #region Public Functions

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                              = AbilityChargeType.TIMED;
        numOfCharges                            = 1;
        abilityName                             = "Enemy Timed Attack";
        secondsToChargeOneBar                   = 10f;
        autocast                                = true;

        //

        activeCharges = 0;

        battle_abilitytimerbarfilled_stream     = SignalStream.Get("Ability", "TimerBarFilled");
        battle_correctresponse_stream           = SignalStream.Get("Battle", "CorrectResponse");
        battle_incorrectresponse_stream         = SignalStream.Get("Battle", "IncorrectResponse");

        battle_abilitytimerbarfilled_receiver   = new SignalReceiver().SetOnSignalCallback(ChargeAbility);
        battle_correctresponse_receiver         = new SignalReceiver().SetOnSignalCallback(ResetChargesOnPlayerResponse);
        battle_incorrectresponse_receiver       = new SignalReceiver().SetOnSignalCallback(ResetChargesOnPlayerResponse);

        battle_abilitytimerbarfilled_stream.ConnectReceiver(battle_abilitytimerbarfilled_receiver);
        battle_correctresponse_stream.ConnectReceiver(battle_correctresponse_receiver);
        battle_incorrectresponse_stream.ConnectReceiver(battle_incorrectresponse_receiver);
    }

    public override void Deactivate()
    {
        //ResetCharges();

        battle_abilitytimerbarfilled_stream.DisconnectReceiver(battle_abilitytimerbarfilled_receiver);
        battle_correctresponse_stream.DisconnectReceiver(battle_correctresponse_receiver);
        battle_incorrectresponse_stream.DisconnectReceiver(battle_incorrectresponse_receiver);

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
        info[1]         = (1 * chargesToAttackWith); //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle", "UnitTakeDamage", info);
        Signal.Send("Battle", "BoardReset");
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

        if (activeCharges == numOfCharges && autocast)
            Activate();
    }

    private void ResetChargesOnPlayerResponse(Signal signal)
    {
        ResetCharges();
    }

    private void ResetCharges()
    {
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        activeCharges   = 0;

        object[] info   = new object[1];
        info[0]         = this; //The ability to reset

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    #endregion
}
