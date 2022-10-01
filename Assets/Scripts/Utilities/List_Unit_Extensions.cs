using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class List_Unit_Extensions
{
    public static Unit GetFirstLivingUnit(this List<Unit> value)
    {
        return value.Find(x => x.CurrentHP > 0);
    }
}
