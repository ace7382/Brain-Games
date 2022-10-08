using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnitController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private bool   isPlayer;

    #endregion

    #region Private Variables

    [SerializeField] private Unit   unit; //TODO: Remove the SF tag, just for testing
    private GameObject              model;

    #endregion

    #region Public Properties

    public Unit UnitInfo            { get { return unit; } }
    public bool IsPlayer            { get { return isPlayer; } }
    public GameObject Model         { get { return model; } set { model = value; } }

    #endregion

    #region Signal Variables

    #endregion

    #region Unity Functions

    private void Awake()
    {
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    #endregion

    #region Public Functions

    public void Setup(Unit unit)
    {
        this.unit = unit;

        for (int i = 0; i < this.unit.Abilities.Count; i++)
        {
            this.unit.Abilities[i].Init(this);
        }
    }

    public void ResetBattleUnitController()
    {
        if (unit == null || unit.Abilities == null)
            return;

        for (int i = 0; i < this.unit.Abilities.Count; i++)
        {
            unit.Abilities[i].Deactivate();
        }
    }

    public void TakeDamage(int dam)
    {
        unit.CurrentHP -= dam;
    }

    #endregion

    #region Private Functions

    #endregion
}
