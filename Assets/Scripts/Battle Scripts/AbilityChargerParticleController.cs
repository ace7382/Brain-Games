using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityChargerParticleController : MonoBehaviour
{
    public Ability  ability;
    public int      chargeActionID;

    public void ChargeAbility()
    {
        if (ability == null)
            return;

        ability.Charge(chargeActionID);
    }
}
