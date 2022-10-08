using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitAnimationEventController : MonoBehaviour
{
    public void DeathAnimationComplete()
    {
        BattleManager.instance.OnEnemyKOAnimationFinish();
    }

    public void TakeDamageAninmationComplete()
    {
        Animator a = GetComponent<Animator>();

        if (a.GetInteger("State") != 9)
            a.SetInteger("State", 0);
    }
}
