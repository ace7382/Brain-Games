using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPartyManager : MonoBehaviour
{
    #region Singleton

    public static PlayerPartyManager instance = null;

    #endregion

    #region Inspector Variables

    public List<Unit> partyBattleUnits;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < partyBattleUnits.Count; i++)
        {
            partyBattleUnits[i].Init();
        }
    }

    #endregion
}
