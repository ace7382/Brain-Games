using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    #region TEST VARIABLES TODO: REMOVE

    public GameObject battleGamePrefab;
    public List<Unit> enemyParty;

    public List<AbilityButtonController> playerAbilityButtons;
    public List<AbilityButtonController> enemyAbilityButtons;

    #endregion

    #region Inspector Variables

    [Space]

    [Header("Prefabs")]
    [SerializeField] private GameObject             abilityButtonPrefab;
    [SerializeField] private GameObject             abilityButtonChargeMarkerPrefab;

    [Space]

    [Header("Variables")]
    [SerializeField] private RectTransform          gameBoardRectTrans;
    [SerializeField] private BattleUnitController   currentPlayerUnitController;
    [SerializeField] private BattleUnitController   currentEnemyUnitController;
    [SerializeField] private RectTransform          playerAbilityButtonPanel;
    [SerializeField] private RectTransform          enemyAbilityButtonPanel;

    #endregion

    #region Private Variables

    private BattleGameControllerBase                currentGameController;

    #endregion

    #region Signal Variables

    private SignalReceiver                          battle_playerkoed_receiver;
    private SignalStream                            battle_playerkoed_stream;
    private SignalReceiver                          battle_enemykoed_receiver;
    private SignalStream                            battle_enemykoed_stream;

    #endregion

    #region Public Properties

    public GameObject AbilityButtonPrefab   { get { return abilityButtonPrefab; } }
    public GameObject ChargeMarkerPrefab    { get { return abilityButtonChargeMarkerPrefab; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        battle_playerkoed_stream    = SignalStream.Get("Battle", "PlayerKOed");
        battle_enemykoed_stream     = SignalStream.Get("Battle", "EnemyKOed");

        battle_playerkoed_receiver  = new SignalReceiver().SetOnSignalCallback(PlayerKO);
        battle_enemykoed_receiver   = new SignalReceiver().SetOnSignalCallback(EnemyKO);
    }

    private void OnEnable()
    {
        battle_playerkoed_stream.ConnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.ConnectReceiver(battle_enemykoed_receiver);
    }

    private void OnDisable()
    {
        battle_playerkoed_stream.DisconnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.DisconnectReceiver(battle_enemykoed_receiver);
    }

    private void Start()
    {
        SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());

        InitializeEnemyUnits();

        SetupGame();
    }

    #endregion

    #region Public Functions

    public void InitializeEnemyUnits()
    {
        for (int i = 0; i < enemyParty.Count; i++)
        {
            enemyParty[i].Init();
        }

        SetupEnemyBattleUnit(enemyParty[0]);
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

    private void SetupPlayerBattleUnit(Unit playerUnit)
    {
        currentPlayerUnitController.Setup(playerUnit);

        foreach (Transform child in playerAbilityButtonPanel)
            Destroy(child.gameObject);

        for (int i = 0; i < currentPlayerUnitController.UnitInfo.Abilities.Count; i++)
        {
            GameObject go                   = Instantiate(abilityButtonPrefab, playerAbilityButtonPanel);
            go.transform.localPosition      = Vector3.zero;
            go.transform.localScale         = Vector3.one;

            AbilityButtonController control = go.GetComponent<AbilityButtonController>();
            control.SetupButton(currentPlayerUnitController.UnitInfo.Abilities[i]);

            playerAbilityButtons.Add(control);
        }

        Signal.Send("Battle", "PlayerMaxHPUpdate", currentPlayerUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "PlayerCurrentHPUpdate", currentPlayerUnitController.UnitInfo.CurrentHP);
    }

    private void SetupEnemyBattleUnit(Unit enemyUnit)
    {
        currentEnemyUnitController.Setup(enemyUnit);

        foreach (Transform child in enemyAbilityButtonPanel)
            Destroy(child.gameObject);

        for (int i = 0; i < currentEnemyUnitController.UnitInfo.Abilities.Count; i++)
        {
            GameObject go                   = Instantiate(abilityButtonPrefab, enemyAbilityButtonPanel);
            go.transform.localPosition      = Vector3.zero;
            go.transform.localScale         = Vector3.one;

            AbilityButtonController control = go.GetComponent<AbilityButtonController>();
            control.SetupButton(currentEnemyUnitController.UnitInfo.Abilities[i]);

            go.GetComponentInChildren<UIButton>().enabled = false;

            enemyAbilityButtons.Add(control);
        }

        Signal.Send("Battle", "EnemyMaxHPUpdate", currentEnemyUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "EnemyCurrentHPUpdate", currentEnemyUnitController.UnitInfo.CurrentHP);
    }

    private void EnemyKO(Signal signal)
    {
        int nextEnemyIndex = enemyParty.FindIndex(x => x.CurrentHP > 0);

        if (nextEnemyIndex >= 0)
        {
            SetupEnemyBattleUnit(enemyParty[nextEnemyIndex]);
        }
        else
        {
            Debug.Log("All Enemies Defeated. Battle Won!");
        }
    }

    private void PlayerKO(Signal signal)
    {
        if (PlayerPartyManager.instance.GetFirstLivingUnit() != null)
        {
            SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());
        }
        else
        {
            Debug.Log("All Players Defeated. Battle Lost!");
        }
    }

    #endregion
}
