using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;

public class Ability_EnemyTimedAttack : Ability
{
    #region Private Variables

    private int                                         currentCharges;
    private List<AbilityCharger.AbilityChargeActions>   actionsThatResetThisAbility;

    #endregion

    #region Signal Variables

    private SignalReceiver      battle_abilitytimerbarfilled_receiver;
    private SignalStream        battle_abilitytimerbarfilled_stream;
    private SignalReceiver      battle_requestmatchingabilitychargers_receiver;
    private SignalStream        battle_requestmatchingabilitychargers_stream;

    #endregion

    #region Public Functions

    public override void Init(BattleUnitController owner)
    {
        base.Init(owner);

        //HARD CODED SHIT

        chargeType                                          = AbilityChargeType.TIMED;
        numOfCharges                                        = 5;
        abilityName                                         = "Enemy Timed Attack";
        secondsToChargeOneBar                               = 2.5f;
        autocast                                            = true;

        actionsThatResetThisAbility                         = new List<AbilityCharger.AbilityChargeActions>();

        actionsThatResetThisAbility.Add(AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE);
        actionsThatResetThisAbility.Add(AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE);

        //

        currentCharges = 0;

        battle_abilitytimerbarfilled_stream                 = SignalStream.Get("Ability", "TimerBarFilled");
        battle_requestmatchingabilitychargers_stream        = SignalStream.Get("Battle", "RequestMatchingAbilityChargers");

        battle_abilitytimerbarfilled_receiver               = new SignalReceiver().SetOnSignalCallback(ChargeAbility);
        battle_requestmatchingabilitychargers_receiver      = new SignalReceiver().SetOnSignalCallback(HandleChargeRequest);

        battle_abilitytimerbarfilled_stream.ConnectReceiver(battle_abilitytimerbarfilled_receiver);
        battle_requestmatchingabilitychargers_stream.ConnectReceiver(battle_requestmatchingabilitychargers_receiver);
    }

    public override void Deactivate()
    {
        battle_abilitytimerbarfilled_stream.DisconnectReceiver(battle_abilitytimerbarfilled_receiver);
        battle_requestmatchingabilitychargers_stream.DisconnectReceiver(battle_requestmatchingabilitychargers_receiver);

        Debug.Log(string.Format("{0}'s ability {1} was deactivated", owner.UnitInfo.Name, this.abilityName));
    }

    public override void Activate()
    {
        if (currentCharges == 0)
            return;

        int chargesToAttackWith = currentCharges;

        ResetCharges(); //TODO: Should this go before the calls at the bottom? Other abilities just reset at the end???

        object[] info   = new object[2];
        info[0]         = owner;
        info[1]         = (1 * chargesToAttackWith); //owner.UnitInfo.{whatever stat}

        Signal.Send("Battle", "UnitAttacked", info);
        Signal.Send("Battle", "BoardReset");
    }

    public override void Charge(int chargeActionID)
    {
        //This function actually just resets this ability on recipient of the appropriate "charge"
        //Timers charge this via ChargeAbility(), but game responses reset the ability

        ResetCharges();
    }

    #endregion

    #region Private functions

    private void HandleChargeRequest(Signal signal)
    {
        //Don't check for charges bc thefirst timer bar may be partially filled and it should still reset

        AbilityCharger.AbilityChargeActions signalType = signal.GetValueUnsafe<AbilityCharger.AbilityChargeActions>();

        if (actionsThatResetThisAbility.Contains(signalType))
            AbilityCharger.instance.AddChargeTarget(this, 1, 1); //reset charge
    }

    private void ChargeAbility(Signal signal)
    {
        //Signal info   - object[1]
        //info[0]       - Ability       - the ability to receive 1 charge

        object[] info = signal.GetValueUnsafe<object[]>();

        if (this == (Ability)info[0])
        {
            Debug.Log(string.Format("{0}'s ability {1} received a charge", owner.UnitInfo.Name, this.abilityName));

            currentCharges = Mathf.Clamp(currentCharges + 1, 0, numOfCharges);

            Signal.Send("Battle", "AbilityCharge", info);
        }

        if (currentCharges == numOfCharges && autocast)
            Activate();
    }

    private void ResetCharges()
    {
        Debug.Log(string.Format("{0}'s ability {1} was reset", owner.UnitInfo.Name, this.abilityName));

        currentCharges   = 0;

        object[] info   = new object[1];
        info[0]         = this; //The ability to reset

        Signal.Send("Battle", "ResetAbilityCharges", info);
    }

    #endregion
}
