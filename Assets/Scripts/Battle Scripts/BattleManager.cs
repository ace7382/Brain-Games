using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    #region Singleton

    public static BattleManager instance = null; //Does NOT persist between scenes currnetly though

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
    [SerializeField] private Image                  playerSprite;
    [SerializeField] private Image                  enemySprite;

    [Space]

    [Header("Pre Battle Screen")]
    [SerializeField] private GameObject             preBattleScreen;
    [SerializeField] private TextMeshProUGUI        enemyCountText;
    [SerializeField] private TextMeshProUGUI        enemyNameText;
    [SerializeField] private GameObject             previousArrow;
    [SerializeField] private GameObject             nextArrow;

    [Space]
    [Header("Pause Screen")]
    [SerializeField] private GameObject             pauseButton;
    [SerializeField] private GameObject             pauseScreen;

    #endregion

    #region Private Variables

    private GameObject                              battleBoard;
    private BattleGameControllerBase                currentGameController;
    private List<AbilityButtonController>           playerAbilityButtons;
    private List<AbilityButtonController>           enemyAbilityButtons;
    private List<Unit>                              enemyParty;
    private int                                     prePanelEnemyDisplayedIndex;

    #endregion

    #region Signal Variables

    private SignalReceiver                          battle_playerkoed_receiver;
    private SignalStream                            battle_playerkoed_stream;
    private SignalReceiver                          battle_enemykoed_receiver;
    private SignalStream                            battle_enemykoed_stream;
    private SignalReceiver                          battle_pause_receiver;
    private SignalStream                            battle_pause_stream;
    private SignalReceiver                          battle_unpause_receiver;
    private SignalStream                            battle_unpause_stream;
    private SignalReceiver                          battle_countdownended_receiver;
    private SignalStream                            battle_countdownended_stream;

    #endregion

    #region Public Properties

    public GameObject AbilityButtonPrefab   { get { return abilityButtonPrefab; } }
    public GameObject ChargeMarkerPrefab    { get { return abilityButtonChargeMarkerPrefab; } }
    public bool IsPaused                    { get { return pauseScreen.activeInHierarchy; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        Canvas c                        = GameObject.Find("Battle Canvas").GetComponent<Canvas>();
        c.worldCamera                   = Camera.main;
        c.sortingOrder                  = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        battle_playerkoed_stream        = SignalStream.Get("Battle", "PlayerKOed");
        battle_enemykoed_stream         = SignalStream.Get("Battle", "EnemyKOed");
        battle_pause_stream             = SignalStream.Get("Battle", "Pause");
        battle_unpause_stream           = SignalStream.Get("Battle", "Unpause");
        battle_countdownended_stream    = SignalStream.Get("Battle", "CountdownEnded");

        battle_playerkoed_receiver      = new SignalReceiver().SetOnSignalCallback(PlayerKO);
        battle_enemykoed_receiver       = new SignalReceiver().SetOnSignalCallback(EnemyKO);
        battle_pause_receiver           = new SignalReceiver().SetOnSignalCallback(Pause);
        battle_unpause_receiver         = new SignalReceiver().SetOnSignalCallback(Unpause);
        battle_countdownended_receiver  = new SignalReceiver().SetOnSignalCallback(CountdownEndedBattleBegin);

        enemyParty                      = new List<Unit>(GameManager.instance.CurrentLevel.enemyUnits);
        prePanelEnemyDisplayedIndex     = 0;

        playerSprite.color              = Color.clear;
        enemySprite.color               = Color.clear;

        SetupPreBattleScreen();
    }

    private void OnEnable()
    {
        battle_playerkoed_stream.ConnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.ConnectReceiver(battle_enemykoed_receiver);
        battle_pause_stream.ConnectReceiver(battle_pause_receiver);
        battle_unpause_stream.ConnectReceiver(battle_unpause_receiver);
        battle_countdownended_stream.ConnectReceiver(battle_countdownended_receiver);
    }

    private void OnDisable()
    {
        battle_playerkoed_stream.DisconnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.DisconnectReceiver(battle_enemykoed_receiver);
        battle_pause_stream.DisconnectReceiver(battle_pause_receiver);
        battle_unpause_stream.DisconnectReceiver(battle_unpause_receiver);
        battle_countdownended_stream.DisconnectReceiver(battle_countdownended_receiver);
    }

    #endregion

    #region Public Functions

    //Called by the Begin Battle Button's OnClick Behavior
    public void BeginBattle()
    {
        preBattleScreen.SetActive(false);

        SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());

        InitializeEnemyUnits();

        SetupGame();
    }

    //Called by the Previous Button and Next Button's OnClick Behaviors
    public void ChangePreBattleDisplay(int upDown)
    {
        prePanelEnemyDisplayedIndex = Mathf.Clamp(prePanelEnemyDisplayedIndex + upDown, 0, enemyParty.Count - 1);

        SetupPreBattleScreen();
    }

    public void InitializeEnemyUnits()
    {
        for (int i = 0; i < enemyParty.Count; i++)
        {
            enemyParty[i].Init();
        }

        SetupEnemyBattleUnit(enemyParty[0]);
    }

    #endregion

    #region Private Functions

    private void SetupPreBattleScreen()
    {
        preBattleScreen.SetActive(true);

        pauseButton.SetActive(false);
        pauseScreen.SetActive(false);

        enemyCountText.text     = string.Format("# {0} / {1}", (prePanelEnemyDisplayedIndex + 1).ToString(), enemyParty.Count.ToString());
        enemyNameText.text      = enemyParty[prePanelEnemyDisplayedIndex].Name;

        nextArrow.SetActive(prePanelEnemyDisplayedIndex != enemyParty.Count - 1);
        previousArrow.SetActive(prePanelEnemyDisplayedIndex > 0);
    }

    private void SetupPlayerBattleUnit(Unit playerUnit)
    {
        playerAbilityButtons = new List<AbilityButtonController>();

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

        playerSprite.sprite = currentPlayerUnitController.UnitInfo.InBattleSprite;
        playerSprite.color  = Color.white;

        Signal.Send("Battle", "PlayerMaxHPUpdate", currentPlayerUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "PlayerCurrentHPUpdate", currentPlayerUnitController.UnitInfo.CurrentHP);
    }

    private void SetupEnemyBattleUnit(Unit enemyUnit)
    {
        enemyAbilityButtons = new List<AbilityButtonController>();

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

            enemyAbilityButtons.Add(control);
        }

        SetEnemyAbilityButtonsInteractability(false);

        enemySprite.sprite  = currentEnemyUnitController.UnitInfo.InBattleSprite;
        enemySprite.color   = Color.white;

        Signal.Send("Battle", "EnemyMaxHPUpdate", currentEnemyUnitController.UnitInfo.MaxHP);
        Signal.Send("Battle", "EnemyCurrentHPUpdate", currentEnemyUnitController.UnitInfo.CurrentHP);
    }

    private void SetupGame()
    {
        pauseScreen.SetActive(false);

        GameObject.Find("Player HP Bar").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Enemy HP Bar").GetComponent<CanvasGroup>().alpha = 1;

        ChangeGame();

        //battleBoard = Instantiate(
        //    Resources.Load<GameObject>(Helpful.GetBattleGameboardLoadingPath(currentEnemyUnitController.UnitInfo.BattleGame))
        //    , gameBoardRectTrans);

        //battleBoard.transform.localPosition     = Vector3.zero;
        //battleBoard.transform.localScale        = Vector3.one;

        //currentGameController                   = battleBoard.GetComponent<BattleGameControllerBase>();

        //battleBoard.SetActive(false);
        //StartCountdown();

        //currentGameController.StartGame();
    }

    private void ChangeGame()
    {
        if (battleBoard != null)
            Destroy(battleBoard);

        battleBoard = Instantiate(
            Resources.Load<GameObject>(Helpful.GetBattleGameboardLoadingPath(currentEnemyUnitController.UnitInfo.BattleGame))
            , gameBoardRectTrans);

        battleBoard.transform.localPosition     = Vector3.zero;
        battleBoard.transform.localScale        = Vector3.one;

        currentGameController                   = battleBoard.GetComponent<BattleGameControllerBase>();

        battleBoard.SetActive(false);
        StartCountdown();
    }

    private void StartCountdown()
    {
        Signal.Send("Battle", "StartCountdown");
    }

    private void CountdownEndedBattleBegin(Signal signal)
    {
        battleBoard.SetActive(true);
        pauseButton.SetActive(true);
        currentGameController.StartGame();
    }

    private void EnemyKO(Signal signal)
    {
        int nextEnemyIndex = enemyParty.FindIndex(x => x.CurrentHP > 0);

        if (nextEnemyIndex >= 0)
        {
            bool switchGame 
                = currentEnemyUnitController.UnitInfo.BattleGame != enemyParty[nextEnemyIndex].BattleGame;

            currentEnemyUnitController.ResetBattleUnitController();

            SetupEnemyBattleUnit(enemyParty[nextEnemyIndex]);

            if (switchGame)
                ChangeGame();
        }
        else
        {
            BattleComplete(true);
        }
    }

    private void PlayerKO(Signal signal)
    {
        if (PlayerPartyManager.instance.GetFirstLivingUnit() != null)
        {
            currentPlayerUnitController.ResetBattleUnitController();

            SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());
        }
        else
        {
            BattleComplete(false);
        }
    }

    private void SetEnemyAbilityButtonsInteractability(bool isInteractable)
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
        {
            enemyAbilityButtons[i].GetComponentInChildren<UIButton>().enabled = isInteractable;
        }
    }

    private void Pause(Signal signal)
    {
        currentGameController.Pause();

        battleBoard.SetActive(false);

        pauseScreen.SetActive(true);
        pauseButton.SetActive(false);
        SetEnemyAbilityButtonsInteractability(true);
    }

    private void Unpause(Signal signal)
    {
        pauseScreen.SetActive(false);
        pauseButton.SetActive(true);

        battleBoard.SetActive(true);

        currentGameController.Unpause();
    }

    private void BattleComplete(bool playerWon)
    {
        Debug.Log("Battle Complete - " + (playerWon ? "WIN" : "LOST"));

        if (playerWon)
        {
            if (GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel.Count > 0)
            {
                for (int i = 0; i < GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel.Count; i++)
                {
                    if (!GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i].unlocked)
                    {
                        GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i].unlocked = true;
                        GameManager.instance.SetWorldMapUnlockLevels(GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i]);
                    }
                }
            }
        }
        else
        {
            Debug.Log("battle lost");
        }

        currentPlayerUnitController.ResetBattleUnitController();
        currentEnemyUnitController.ResetBattleUnitController();

        Signal.Send("Battle", "ReturnToWorldMap");
    }

    #endregion
}
