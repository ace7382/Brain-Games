using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability_Enemy_TooManyTurns : Ability
{
    #region Private Variables

    private float           chargePercentage;

    #endregion

    #region Signal Variables

    private SignalReceiver  pathpuzzle_tilerotated_receiver;
    private SignalStream    pathpuzzle_tilerotated_stream;

    #endregion

    #region Public Functions
    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType      = AbilityChargeType.NUM_OF_CHARGES;
        numOfCharges    = 15;
        abilityName     = "Too Many Turns";
        autocast        = true;

        //

        chargePercentage = 0f;

        pathpuzzle_tilerotated_stream   = SignalStream.Get("PathPuzzle", "TileRotated");

        pathpuzzle_tilerotated_receiver = new SignalReceiver().SetOnSignalCallback(ChargeAbility);

        pathpuzzle_tilerotated_stream.ConnectReceiver(pathpuzzle_tilerotated_receiver);
    }

    public override void Activate()
    {
        object[] info   = new object[2];
        info[0]         = owner.IsPlayer;
        info[1]         = (int)(10 * chargePercentage); //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle","UnitTakeDamage", info);

        ResetCharges();
    }

    #endregion

    #region Private Functions

    public void ChargeAbility(Signal signal)
    {
        Debug.Log(string.Format("{0}'s ability {1} received a charge", owner.UnitInfo.Name, this.abilityName));

        if (chargePercentage < 1f)
        {
            chargePercentage    += 1f / numOfCharges;

            object[] info       = new object[2];
            info[0]             = this; //The ability to charge
            info[1]             = 1;    //The number of charges

            Signal.Send("Battle", "AbilityCharge", info);
        }

        if (chargePercentage >= 1f && autocast)
            Activate();
    }

    public void ResetCharges()
    {
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        chargePercentage = 0f;

        object[] info       = new object[1];
        info[0]             = this; //The ability to charge

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    #endregion
}
