using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private bool isPlayer;

    #endregion

    #region Private Variables

    [SerializeField] private Unit unit; //TODO: Remove the SF tag, just for testing

    #endregion

    #region Public Properties

    public Unit UnitInfo { get { return unit; } }

    #endregion

    #region Signal Variables

    protected SignalReceiver    battle_unittakedamage_receiver;
    protected SignalStream      battle_unittakedamage_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        battle_unittakedamage_stream    = SignalStream.Get("Battle", "UnitTakeDamage");

        battle_unittakedamage_receiver  = new SignalReceiver().SetOnSignalCallback(TakeDamage);
    }

    protected virtual void OnEnable()
    {
        battle_unittakedamage_stream.ConnectReceiver(battle_unittakedamage_receiver);
    }

    protected virtual void OnDisable()
    {
        battle_unittakedamage_stream.DisconnectReceiver(battle_unittakedamage_receiver);
    }

    #endregion

    #region Public Functions

    public void Setup(Unit unit)
    {
        this.unit = unit;
    }

    #endregion

    #region Private Functions

    private void TakeDamage(Signal signal)
    {
        //Signal info is object[2]
        //info[0] bool                  - true = player, false = enemy
        //info[1] int                   - The amount of damage to take

        object[] info = signal.GetValueUnsafe<object[]>();

        if ((bool)info[0] == isPlayer)
        {
            unit.CurrentHP -= (int)info[1];

            Signal.Send("Battle", isPlayer ? "PlayerCurrentHPUpdate" : "EnemyCurrentHPUpdate", unit.CurrentHP);
        }
    }

    #endregion
}
