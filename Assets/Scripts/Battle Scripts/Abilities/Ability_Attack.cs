using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Attack : Ability
{
    #region Private Variables

    private float               chargePercentage;

    #endregion

    #region Signal Variables

    private SignalReceiver      battle_correctresponse_receiver;
    private SignalStream        battle_correctresponse_stream;

    #endregion

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                      = AbilityChargeType.NUM_OF_CHARGES;
        numOfCharges                    = 3;
        abilityName                     = "Attack";

        //

        battle_correctresponse_stream   = SignalStream.Get("Battle", "CorrectResponse");

        battle_correctresponse_receiver = new SignalReceiver().SetOnSignalCallback(ChargeAbility);

        battle_correctresponse_stream.ConnectReceiver(battle_correctresponse_receiver);
    }

    public override void Deactivate()
    {
        battle_correctresponse_stream.DisconnectReceiver(battle_correctresponse_receiver);
    }

    public override void Activate()
    {
        object[] info   = new object[2];
        info[0]         = owner.IsPlayer;
        info[1]         = (int)(3 * chargePercentage); //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle","UnitTakeDamage", info);

        ResetCharges();
    }

    public void ChargeAbility(Signal signal)
    {
        if (chargePercentage < 1f)
        {
            chargePercentage    += 1f / numOfCharges;

            object[] info       = new object[2];
            info[0]             = this; //The ability to charge
            info[1]             = 1;    //The number of charges

            Signal.Send("Battle", "AbilityCharge", info);
        }
    }

    public void ResetCharges()
    {
        chargePercentage    = 0f;

        object[] info       = new object[1];
        info[0]             = this; //The ability to charge

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }
}
