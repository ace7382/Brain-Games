using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    //list of party/enemies
    //the current battle member
    //the current game

    /* 
     * Player Starts Level
     * 
     * Step 1: Load Player 1
     * Step 2: Load Enemy 1
     * Step 3: Initialize the BattleGameControllerBase that's linked to the enemy/level
     *          These will be Prefabs? that get plopped into the play area.
     * Step 4: Countdown
     * Step 5: Start the Game
     * Step 6: Play
     * Step 7: Process Player/Enemy Death
     * 
     */

    #region Inspector Variables

    public GameObject battleGamePrefab;
    public Unit testEnemy;

    [Space]

    [SerializeField] private RectTransform          gameBoardRectTrans;
    [SerializeField] private BattleUnitController   currentPlayerUnitController;
    [SerializeField] private BattleUnitController   currentEnemyUnitController;

    #endregion

    #region Private Variables

    private BattleGameControllerBase        currentGameController;

    #endregion

    #region Unity Functions

    private void Start()
    {
        SetupPlayerUnit();
        SetupEnemyUnit();

        InitHPBars();

        SetupGame();
    }

    #endregion

    #region Public Functions

    public void SetupPlayerUnit()
    {
        currentPlayerUnitController.Setup(PlayerPartyManager.instance.partyBattleUnits[0]);
    }

    public void SetupEnemyUnit()
    {
        testEnemy.Init();

        currentEnemyUnitController.Setup(testEnemy);
    }

    public void SetupGame()
    {
        GameObject go = Instantiate(battleGamePrefab, gameBoardRectTrans);

        go.transform.localPosition  = Vector3.zero;
        go.transform.localScale     = Vector3.one;

        currentGameController       = go.GetComponent<BattleGameControllerBase>();

        currentGameController.StartGame();
    }

    #endregion

    #region Private Functions

    private void InitHPBars()
    {
        Signal.Send("Battle", "PlayerMaxHPUpdate", currentPlayerUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "PlayerCurrentHPUpdate", currentPlayerUnitController.UnitInfo.CurrentHP);

        Signal.Send("Battle", "EnemyMaxHPUpdate", currentEnemyUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "EnemyCurrentHPUpdate", currentEnemyUnitController.UnitInfo.CurrentHP);
    }

    #endregion
}
