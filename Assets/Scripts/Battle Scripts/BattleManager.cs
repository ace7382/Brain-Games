using BizzyBeeGames;
using Doozy.Runtime.Reactor;
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

    public static BattleManager instance = null; //Does NOT persist between scenes currently though

    #endregion

    #region Private Structs

    private struct UnitStat
    {
        public Helpful.StatTypes    statType;
        public Unit                 unit;
    }

    private struct PreBattleStats
    {
        public Unit                 unit;
        public Helpful.StatTypes    statType;
        public int                  statLevelWithMods;
        public int                  statLevelWithoutMods;
        public int                  exp;
        public int                  nextExp;

        public PreBattleStats(Unit u, Helpful.StatTypes s, int levWithMods, int levNoMods, int e, int nex)
        {
            unit                    = u;
            statType                = s;
            statLevelWithMods       = levWithMods;
            statLevelWithoutMods    = levNoMods;
            exp                     = e;
            nextExp                 = nex;
        }
    }

    #endregion

    #region Inspector Variables

    [Space]

    [Header("Prefabs")]
    [SerializeField] private GameObject                 abilityButtonPrefab;
    [SerializeField] private GameObject                 abilityButtonChargeMarkerPrefab;
    [SerializeField] private GameObject                 abilityButtonTimerBarPrefab;
    [SerializeField] private GameObject                 itemSlot;   

    [Space]

    [Header("Variables")]
    [SerializeField] private RectTransform              gameBoardRectTrans;
    [SerializeField] private RectTransform              gameBoardBGRectTrans;
    [SerializeField] private BattleUnitController       currentPlayerUnitController;
    [SerializeField] private BattleUnitController       currentEnemyUnitController;
    [SerializeField] private RectTransform              playerAbilityButtonPanel;
    [SerializeField] private RectTransform              enemyAbilityButtonPanel;
    [SerializeField] private Image                      playerSprite;
    [SerializeField] private Image                      enemySprite;

    [Space]

    [Header("Pre Battle Screen")]
    [SerializeField] private GameObject                 preBattleScreen;
    [SerializeField] private Image                      enemyPreBattleImage;
    [SerializeField] private TextMeshProUGUI            enemyCountText;
    [SerializeField] private TextMeshProUGUI            enemyNameText;
    [SerializeField] private TextMeshProUGUI            enemyBattleGameName;
    [SerializeField] private GameObject                 previousArrow;
    [SerializeField] private GameObject                 nextArrow;

    [Space]

    [Header("Post Battle Screen")]
    [SerializeField] private GameObject                 postBattleScreen;
    [SerializeField] private TextMeshProUGUI            postBattleScreenVictoryDefeatText;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenSectionLabels;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenExpBarLevels;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenExpBarAdds;
    [SerializeField] private Progressor[]               postBattleScreenExpBarProgressors;
    [SerializeField] private RectTransform              postBattleScreenEnemiesDefeatedContainer;
    [SerializeField] private RectTransform              postBattleScreenItemRewardContainer;
    [SerializeField] private TextMeshProUGUI            postBattleScreenExpTitle;
    //[SerializeField] private UIButton                   postBattleScreenContinueButton;
    //[SerializeField] private UIButton                   postBattleScreenRetryButton;
    [SerializeField] private CanvasGroup                postBattleScreenButtonPanel;

    [Space]
    [Header("Pause Screen")]
    [SerializeField] private GameObject                 pauseButton;
    [SerializeField] private GameObject                 pauseScreen;

    #endregion

    #region Private Variables

    private GameObject                                  battleBoard;
    private BattleGameControllerBase                    currentGameController;
    private List<AbilityButtonController>               playerAbilityButtons;
    private List<AbilityButtonController>               enemyAbilityButtons;
    private List<Unit>                                  enemyParty;
    private int                                         prePanelEnemyDisplayedIndex;
    private Dictionary<UnitStat, int>                   expEarnedDuringBattle;
    private List<PreBattleStats>                        preBattleStats;

    #endregion

    #region Signal Variables

    private SignalReceiver                              battle_playerkoed_receiver;
    private SignalStream                                battle_playerkoed_stream;
    private SignalReceiver                              battle_enemykoed_receiver;
    private SignalStream                                battle_enemykoed_stream;
    private SignalReceiver                              battle_pause_receiver;
    private SignalStream                                battle_pause_stream;
    private SignalReceiver                              battle_unpause_receiver;
    private SignalStream                                battle_unpause_stream;
    private SignalReceiver                              battle_countdownended_receiver;
    private SignalStream                                battle_countdownended_stream;
    private SignalReceiver                              battle_boardreset_receiver;
    private SignalStream                                battle_boardreset_stream;
    private SignalReceiver                              partymanagement_expadded_receiver;
    private SignalStream                                partymanagement_expadded_stream;

    #endregion

    #region Public Properties

    public GameObject                                   AbilityButtonPrefab     { get { return abilityButtonPrefab; } }
    public GameObject                                   ChargeMarkerPrefab      { get { return abilityButtonChargeMarkerPrefab; } }
    public GameObject                                   TimerBarPrefab          { get { return abilityButtonTimerBarPrefab; } }
    public bool                                         IsPaused                { get { return pauseScreen.activeInHierarchy; } }
    public string                                       CurrentGameName         { get { return currentGameController.GetBattleGameName(); } }
    public BattleUnitController                         CurrentEnemy            { get { return currentEnemyUnitController; } }
    public BattleUnitController                         CurrentPlayerUnit       { get { return currentPlayerUnitController; } }
    public List<AbilityButtonController>                PlayerAbilityButtons    { get { return playerAbilityButtons; } }
    public List<AbilityButtonController>                EnemyAbilityButtons     { get { return enemyAbilityButtons; } }

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        Canvas c                                    = GameObject.Find("Battle Canvas").GetComponent<Canvas>();
        c.worldCamera                               = Camera.main;
        c.sortingOrder                              = UniversalInspectorVariables.instance.gameScreenOrderInLayer;

        battle_playerkoed_stream                    = SignalStream.Get("Battle", "PlayerKOed");
        battle_enemykoed_stream                     = SignalStream.Get("Battle", "EnemyKOed");
        battle_pause_stream                         = SignalStream.Get("Battle", "Pause");
        battle_unpause_stream                       = SignalStream.Get("Battle", "Unpause");
        battle_countdownended_stream                = SignalStream.Get("Battle", "CountdownEnded");
        battle_boardreset_stream                    = SignalStream.Get("Battle", "BoardReset");
        partymanagement_expadded_stream             = SignalStream.Get("PartyManagement", "AwardExperience");  

        battle_playerkoed_receiver                  = new SignalReceiver().SetOnSignalCallback(PlayerKO);
        battle_enemykoed_receiver                   = new SignalReceiver().SetOnSignalCallback(EnemyKO);
        battle_pause_receiver                       = new SignalReceiver().SetOnSignalCallback(Pause);
        battle_unpause_receiver                     = new SignalReceiver().SetOnSignalCallback(Unpause);
        battle_countdownended_receiver              = new SignalReceiver().SetOnSignalCallback(CountdownEndedBattleBegin);
        battle_boardreset_receiver                  = new SignalReceiver().SetOnSignalCallback(BoardReset);
        partymanagement_expadded_receiver           = new SignalReceiver().SetOnSignalCallback(AddEXPEarnedInBattle);

        enemyParty                                  = new List<Unit>(GameManager.instance.CurrentLevel.enemyUnits);
        prePanelEnemyDisplayedIndex                 = 0;

        playerSprite.color                          = Color.clear;
        enemySprite.color                           = Color.clear;

        expEarnedDuringBattle                       = new Dictionary<UnitStat, int>();
        preBattleStats                              = new List<PreBattleStats>();

        //TODO: Set the snapshot when party for the level is selected vs every single party member
        //      Might be easier to just apply points earned in battle at end though
        for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
        {
            for (int stat = 0; stat < (int)Helpful.StatTypes.COUNT; stat++)
            {
                Unit u              = PlayerPartyManager.instance.partyBattleUnits[i];
                Helpful.StatTypes s = (Helpful.StatTypes)stat;

                preBattleStats.Add(
                    new PreBattleStats(
                        u
                        , s
                        , u.GetStatWithMods(s)
                        , u.GetStat(s)
                        , u.GetExpForStat(s)
                        , u.GetEXPNextLevelValue(s)
                    )
                );
            }
        }

        SetupPreBattleScreen();
    }

    private void OnEnable()
    {
        battle_playerkoed_stream.ConnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.ConnectReceiver(battle_enemykoed_receiver);
        battle_pause_stream.ConnectReceiver(battle_pause_receiver);
        battle_unpause_stream.ConnectReceiver(battle_unpause_receiver);
        battle_countdownended_stream.ConnectReceiver(battle_countdownended_receiver);
        battle_boardreset_stream.ConnectReceiver(battle_boardreset_receiver);
        partymanagement_expadded_stream.ConnectReceiver(partymanagement_expadded_receiver);
    }

    private void OnDisable()
    {
        battle_playerkoed_stream.DisconnectReceiver(battle_playerkoed_receiver);
        battle_enemykoed_stream.DisconnectReceiver(battle_enemykoed_receiver);
        battle_pause_stream.DisconnectReceiver(battle_pause_receiver);
        battle_unpause_stream.DisconnectReceiver(battle_unpause_receiver);
        battle_countdownended_stream.DisconnectReceiver(battle_countdownended_receiver);
        battle_boardreset_stream.DisconnectReceiver(battle_boardreset_receiver);
        partymanagement_expadded_stream.DisconnectReceiver(partymanagement_expadded_receiver);
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    #endregion

    #region Public Functions

    //Called by the Begin Battle Button's OnClick Behavior
    public void BeginBattle()
    {
        preBattleScreen.SetActive(false);

        SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());

        SetupEnemyBattleUnit(enemyParty[0]);

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
    }

    public void ReturnToWorldMap()
    {
        currentPlayerUnitController.ResetBattleUnitController();
        currentEnemyUnitController.ResetBattleUnitController();

        Signal.Send("Battle", "ReturnToWorldMap");
    }

    #endregion

    #region Private Functions

    private void SetupPreBattleScreen()
    {
        InitializeEnemyUnits();

        preBattleScreen.SetActive(true);

        pauseButton.SetActive(false);
        pauseScreen.SetActive(false);

        enemyPreBattleImage.sprite  = enemyParty[prePanelEnemyDisplayedIndex].InBattleSprite;
        enemyCountText.text         = string.Format("# {0} / {1}", (prePanelEnemyDisplayedIndex + 1).ToString(), enemyParty.Count.ToString());
        enemyNameText.text          = enemyParty[prePanelEnemyDisplayedIndex].Name;
        enemyBattleGameName.text    = enemyParty[prePanelEnemyDisplayedIndex].BattleGameName;

        Signal.Send("Battle", "DisplayMaxHPUpdate", enemyParty[prePanelEnemyDisplayedIndex].GetStatWithMods(Helpful.StatTypes.MaxHP));
        Signal.Send("Battle", "DisplayCurrentHPUpdate", enemyParty[prePanelEnemyDisplayedIndex].GetStatWithMods(Helpful.StatTypes.MaxHP)); //The HP is always full on the pre-battle screen

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
            go.name                         = string.Format("{0} - {1} button", currentPlayerUnitController.UnitInfo.Name
                                                , currentPlayerUnitController.UnitInfo.Abilities[i].abilityName);
            go.transform.localPosition      = Vector3.zero;
            go.transform.localScale         = Vector3.one;

            AbilityButtonController control = go.GetComponent<AbilityButtonController>();
            control.SetupButton(currentPlayerUnitController.UnitInfo.Abilities[i]);

            playerAbilityButtons.Add(control);
        }

        playerSprite.sprite = currentPlayerUnitController.UnitInfo.InBattleSprite;
        playerSprite.color  = Color.white;

        Signal.Send("Battle", "PlayerMaxHPUpdate", currentPlayerUnitController.UnitInfo.GetStatWithMods(Helpful.StatTypes.MaxHP));
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
            go.name                         = string.Format("{0} - {1} button", currentEnemyUnitController.UnitInfo.Name
                                                , currentEnemyUnitController.UnitInfo.Abilities[i].abilityName);
            go.transform.localPosition      = Vector3.zero;
            go.transform.localScale         = Vector3.one;

            AbilityButtonController control = go.GetComponent<AbilityButtonController>();
            control.SetupButton(currentEnemyUnitController.UnitInfo.Abilities[i]);

            enemyAbilityButtons.Add(control);
        }

        SetEnemyAbilityButtonsInteractability(false);

        enemySprite.sprite  = currentEnemyUnitController.UnitInfo.InBattleSprite;
        enemySprite.color   = Color.white;

        Signal.Send("Battle", "EnemyMaxHPUpdate", currentEnemyUnitController.UnitInfo.GetStatWithMods(Helpful.StatTypes.MaxHP));
        Signal.Send("Battle", "EnemyCurrentHPUpdate", currentEnemyUnitController.UnitInfo.CurrentHP);
    }

    private void SetupGame()
    {
        pauseScreen.SetActive(false);

        GameObject.Find("Player HP Bar").GetComponent<CanvasGroup>().alpha = 1;
        GameObject.Find("Enemy HP Bar").GetComponent<CanvasGroup>().alpha = 1;

        ChangeGame();
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
        pauseButton.SetActive(false);

        DisablePlayerAbilityButtons();
        DisableEnemyAbilityButtons();

        PausePlayerTimerAbilities();
        PauseEnemyTimerAbilities(); 

        Signal.Send("Battle", "StartCountdown");
    }

    private void CountdownEndedBattleBegin(Signal signal)
    {
        battleBoard.SetActive(true);
        pauseButton.SetActive(true);

        EnablePlayerAbilityButtons();
        EnableEnemyAbilityButtons();

        StartPlayerTimerAbilities();
        StartEnemyTimerAbilities();

        currentGameController.StartGame();
    }

    private void DisablePlayerAbilityButtons()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
        {
            playerAbilityButtons[i].DisableButton();
        }
    }

    private void DisableEnemyAbilityButtons()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
        {
            enemyAbilityButtons[i].DisableButton();
        }
    }

    private void EnablePlayerAbilityButtons()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
        {
            playerAbilityButtons[i].EnableButton();
        }
    }

    private void EnableEnemyAbilityButtons()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
        {
            enemyAbilityButtons[i].EnableButton();
        }
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
            else
            {
                StartEnemyTimerAbilities();
            }
        }
        else
        {
            //BattleComplete(true);
            ShowEndOfBattleScreen(true);
        }
    }

    private void PlayerKO(Signal signal)
    {
        if (PlayerPartyManager.instance.GetFirstLivingUnit() != null)
        {
            currentPlayerUnitController.ResetBattleUnitController();

            SetupPlayerBattleUnit(PlayerPartyManager.instance.GetFirstLivingUnit());

            StartPlayerTimerAbilities();
        }
        else
        {
            //BattleComplete(false);
            ShowEndOfBattleScreen(false);
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

        pauseScreen.GetComponentInChildren<AbilityDisplayController>().ResetDisplay();

        PauseEnemyTimerAbilities();
        PausePlayerTimerAbilities();

        SetEnemyAbilityButtonsInteractability(true);
    }

    private void Unpause(Signal signal)
    {
        pauseScreen.SetActive(false);
        pauseButton.SetActive(true);

        battleBoard.SetActive(true);

        StartEnemyTimerAbilities();
        StartPlayerTimerAbilities();

        SetEnemyAbilityButtonsInteractability(false);

        currentGameController.Unpause();
    }

    private void ShowEndOfBattleScreen(bool won)
    {
        //TODO: Maybe move this elsewhere?
        if (won)
        {
            if (GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel.Count > 0)
            {
                for (int i = 0; i < GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel.Count; i++)
                {
                    if (!GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i].unlocked)
                    {
                        GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i].unlocked = true;
                        //GameManager.instance.SetWorldMapUnlockLevels(GameManager.instance.CurrentLevel.levelsUnlockedByThisLevel[i]);
                    }
                }
            }
        }

        GameObject.FindObjectOfType<BattlePopupController>().enabled = false;

        float screenOpeningTime             = .6f;

        //Stop the battle game
        currentGameController.Pause();

        //Disable and hide the pause button
        pauseButton.SetActive(false);

        //Stop abilities
        DisableEnemyAbilityButtons();
        DisablePlayerAbilityButtons();
        PauseEnemyTimerAbilities();
        PausePlayerTimerAbilities();

        //Hide Abilities & HP bars -
        //TODO: might want to store references to these from the inspector vs the find and get component calls here
        UIAnimation u           = UIAnimation.Alpha(playerAbilityButtonPanel.GetComponentInParent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(enemyAbilityButtonPanel.GetComponentInParent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(GameObject.Find("Player HP Bar").GetComponent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(GameObject.Find("Enemy HP Bar").GetComponent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();

        //fadeout the game
        u                       = UIAnimation.Alpha(currentGameController.GameElementsCanvasGroup, 0f, screenOpeningTime);
        u.Play();

        //Stretch the battle board and bg out to 1800 and start loading the screen's content
        u                       = UIAnimation.Width(gameBoardRectTrans, 1800, screenOpeningTime);
        u.Play();
        u                       = UIAnimation.Width(gameBoardBGRectTrans, 1800, screenOpeningTime);
        u.OnAnimationFinished   = delegate { PostBattleScreenAnimation(won); };
        u.Play();
    }

    private void PostBattleScreenAnimation(bool won)
    {
        float animationTime                     = .5f;

        postBattleScreenVictoryDefeatText.text  = won ? "Victory" : "Defeat";
        postBattleScreenVictoryDefeatText.color = won ? Color.green : Color.red;

        postBattleScreen.SetActive(true);

        SetPostScreenEXPBars(currentPlayerUnitController.UnitInfo);

        //TODO: I should do this correctly with coroutines and fade in functions instead of these nested UIAnimation.OnEnds
        UIAnimation u;

        u = UIAnimation.Alpha(postBattleScreenVictoryDefeatText.GetComponent<CanvasGroup>(), 1f, animationTime);

        u.OnAnimationFinished = delegate
        {
            for (int i = 0; i < postBattleScreenSectionLabels.Length; i++)
            {
                u = UIAnimation.Color(postBattleScreenSectionLabels[i], Color.black, animationTime);
                if (i == postBattleScreenSectionLabels.Length - 1)
                {
                    u.OnAnimationFinished = delegate
                    {
                        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
                        {
                            u = UIAnimation.Alpha(postBattleScreenExpBarProgressors[i].GetComponentInParent<CanvasGroup>(), 1f, animationTime);

                            if (i == postBattleScreenSectionLabels.Length - 1)
                                u.OnAnimationFinished = delegate
                                {
                                    StartCoroutine(ProcessPostBattleRewards());
                                };

                            u.Play();
                        }
                    };
                }
                u.Play();
            }
        };
        u.Play();
    }

    private void SetPostScreenEXPBars(Unit u)
    {
        //TODO: If i want the exp numbers on the bars i'll need to set them up here.
        //      Turning them off for now bc i'm sure the screen will change eventually

        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
        {
            Helpful.StatTypes cs                            = (Helpful.StatTypes)i;
            PreBattleStats prestat                          = preBattleStats.Find(x => x.unit == u && x.statType == cs);

            postBattleScreenExpBarProgressors[i].toValue    = prestat.nextExp;
            postBattleScreenExpBarProgressors[i].fromValue  = 0f;

            postBattleScreenExpBarProgressors[i].SetValueAt(0f);
            postBattleScreenExpBarProgressors[i].SetValueAt(prestat.exp);

            postBattleScreenExpBarLevels[i].text            = prestat.statLevelWithMods.ToString();

            postBattleScreenExpBarAdds[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator ProcessPostBattleRewards()
    {
        //TODO: Sync the EXP column's title change with the other fades/fills.
        //      Not important now bc the screen will almost definitely be redone

        float tickUpTime                        = 2.3f;
        float pauseBetweenEnemiesTime           = .7f;
        WaitForSeconds tickTimeWait             = new WaitForSeconds(tickUpTime);
        WaitForSeconds pauseBetweenEnemies      = new WaitForSeconds(pauseBetweenEnemiesTime);
        List<Unit> enemiesDefeated              = enemyParty.FindAll(x => x.CurrentHP <= 0);
        List<ItemSlotController> items          = new List<ItemSlotController>();

        UIAnimation.SwapText(postBattleScreenExpTitle, "Battle Performance", pauseBetweenEnemiesTime);

        UnitStat k                              = new UnitStat();
        k.unit                                  = currentPlayerUnitController.UnitInfo;
        bool needsToYield                       = false;
        for (int i = 0; i < postBattleScreenExpBarAdds.Length; i++)
        {
            k.statType                          = (Helpful.StatTypes)i;
            int expEarned                       = expEarnedDuringBattle.ContainsKey(k) ? expEarnedDuringBattle[k] : 0;
            postBattleScreenExpBarAdds[i].text  = "+0";

            if (expEarned > 0)
            {
                needsToYield                    = true;

                postBattleScreenExpBarAdds[i].gameObject.SetActive(true);

                StartCoroutine(Helpful.IncreaseDisplayNumberOverTime(postBattleScreenExpBarAdds[i], 0, expEarned, "+", "", tickUpTime));
                StartCoroutine(FillBar(postBattleScreenExpBarProgressors[i], postBattleScreenExpBarLevels[i],
                                        k.unit, k.statType, expEarned, tickUpTime));
            }
        }

        if (needsToYield)
            yield return tickTimeWait;

        UIAnimation.SwapText(postBattleScreenExpTitle, "Enemy Rewards", pauseBetweenEnemiesTime);
        yield return pauseBetweenEnemies;

        for (int e = 0; e < enemiesDefeated.Count; e++)
        {
            GameObject go   = new GameObject();
            Image goI       = go.AddComponent<Image>();
            goI.sprite      = enemiesDefeated[e].InBattleSprite;
            goI.color       = new Color32(255, 255, 255, 255);
            Transform t     = go.transform;
            t.SetParent(postBattleScreenEnemiesDefeatedContainer);
            t.localScale    = Vector3.one;
            t.localPosition = Vector3.zero;

            StartCoroutine(Helpful.FadeGraphicIn(goI, tickUpTime));

            if (enemiesDefeated[e].EXPAward.Count > 0)
            { 
                for (int i = 0; i < enemiesDefeated[e].EXPAward.Count; i++)
                {
                    int statIndex = enemiesDefeated[e].EXPAward[i].x;

                    if (!postBattleScreenExpBarAdds[statIndex].gameObject.activeInHierarchy)
                    {
                        postBattleScreenExpBarAdds[statIndex].text = "+0";
                        postBattleScreenExpBarAdds[statIndex].gameObject.SetActive(true);
                    }

                    int cv = int.Parse(postBattleScreenExpBarAdds[statIndex].text.Substring(1));
                    int ev = cv + enemiesDefeated[e].EXPAward[i].y;

                    StartCoroutine(Helpful.IncreaseDisplayNumberOverTime(postBattleScreenExpBarAdds[statIndex], cv, ev, "+", "", tickUpTime));
                    StartCoroutine(FillBar(postBattleScreenExpBarProgressors[statIndex], postBattleScreenExpBarLevels[statIndex]
                                            , k.unit, (Helpful.StatTypes)statIndex, enemiesDefeated[e].EXPAward[i].y, tickUpTime));

                    //TODO: This better. Will need to be reworked when allowing multiple units per battle to earn exp
                    object[] info   = new object[3];
                    info[0]         = enemiesDefeated[e].EXPAward[i].x; //Stat
                    info[1]         = enemiesDefeated[e].EXPAward[i].y; //Amount
                    info[2]         = k.unit;                           //Unit

                    Signal.Send("PartyManagement", "AwardExperience", info);
                }
            }

            if (enemiesDefeated[e].ItemRewards.Count > 0)
            {
                for (int i = 0; i < enemiesDefeated[e].ItemRewards.Count; i++)
                {
                    ItemReward reward                       = enemiesDefeated[e].ItemRewards[i];
                    float rand                              = Random.Range(0f, 100f);

                    if (rand <= reward.Chance) //Item Received
                    {
                        int index                           = items.FindIndex(x => x.Item == reward.Item);
                        
                        //TODO; maybe weight this toward fewer drops?
                        int count                           = Random.Range(reward.MinDropped, reward.MaxDropped + 1);

                        if (index > -1) //Add to existing 
                        {
                            items[index].AddToCount(count);
                        }
                        else
                        {
                            //TODO: Remove the button slot's OnClick behavior.
                            //      Probably want it to give a tooltip with the name or something instead
                            //      itemGO.GetComponent<UIButton>().GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();
                            //         ^^ this won't work to remove it, will need to remove the inspector given click function and set with this in code
                            //          here and in the inventory where these are also used
                            GameObject itemGO               = Instantiate(itemSlot, postBattleScreenItemRewardContainer);
                            itemGO.transform.localPosition  = Vector3.zero;
                            itemGO.transform.localScale     = Vector3.one;
                            ItemSlotController slot         = itemGO.GetComponent<ItemSlotController>();
                            slot.Setup(reward.Item, count);
                            items.Add(slot);
                        }

                        //TODO: Probably have this function return true/false. THere should probably be a limit on item count
                        //  in inventory, and this would allow this screen to show if you're at max/able to receive the item
                        PlayerPartyManager.instance.AddItemToInventory(reward.Item, count); 
                    }
                }
            }

            yield return tickTimeWait; //To allow for EXP bars to fill and enemies/items to fade in

            yield return pauseBetweenEnemies;
        }

        UIAnimation.SwapText(postBattleScreenExpTitle, "Total", pauseBetweenEnemiesTime);
        UIAnimation.Alpha(postBattleScreenButtonPanel, 1, pauseBetweenEnemiesTime).Play();
    }

    //TODO: Functionality is pretty similar to ItemTargetCardController_Unit.UpdateLevelBar().
    //      might want to consider making the bars a prefab or something and consolidating funcitonality to a "bar controller"
    private IEnumerator FillBar(Progressor progressor, TextMeshProUGUI label, Unit u, Helpful.StatTypes s, int expAdded, float animationTime)
    {
        if (progressor.currentValue + expAdded > progressor.toValue)
        {
            int fillAnimationsNeeded    = 1;
            int expRemaining            = expAdded - ((int)progressor.currentValue - (int)progressor.toValue);
            int currentLevel            = int.Parse(label.text);

            while (expRemaining > 0)
            {
                currentLevel++;
                fillAnimationsNeeded++;

                expRemaining -= Formulas.GetNextLevelEXP((Helpful.StatGrowthRates)u.GetGrowthRate(s), currentLevel);
            }

            progressor.reaction.settings.duration = animationTime / (float)fillAnimationsNeeded;
        }
        else
            progressor.reaction.settings.duration = animationTime;

        WaitForSeconds wait             = new WaitForSeconds(progressor.GetDuration());

        while (expAdded > 0)
        {
            float startValue        = progressor.currentValue;
            float playToValue       = progressor.currentValue + expAdded > progressor.toValue ?
                                        progressor.toValue : progressor.currentValue + expAdded;

            Debug.Log(string.Format("Filling {0}. Current Value {1}, Bar.toValue {2}, Setting bar to {3} over the next {4} seconds"
                    , s.GetShorthand(), progressor.currentValue.ToString(), progressor.toValue.ToString()
                    , playToValue.ToString(), progressor.GetDuration().ToString()));

            progressor.PlayToValue(playToValue);

            yield return wait;

            //Depending on the fraction of the bar and the duration, the current fill value might not be exactly where
            //it needs to be, so this should set the fill to the exact spot it needs to be at so that it's ready
            //for the next loop.
            //TODO: Using progressor.SetProgressAt(fraction) bc progressor.SetValueAt(playToValue) has a bug in this version
            //      of Doozy. If I update then I should probably switch to SetValueAt bc it's probably more accurate if I add
            //      animation curves to the fill
            if (playToValue == progressor.toValue)
                progressor.SetProgressAtOne();
            else
                progressor.SetProgressAt(playToValue / progressor.toValue);

            expAdded -= (int)playToValue - (int)startValue;

            Debug.Log(string.Format("Mid Fill: {0}. Current Value {1}, Bar.toValue {2}"
                , s.GetShorthand(), progressor.currentValue.ToString(), progressor.toValue.ToString()));

            if (progressor.currentValue >= progressor.toValue)
            {
                int newStatValue        = int.Parse(label.text) + 1;

                label.text              = newStatValue.ToString();

                progressor.toValue      = Formulas.GetNextLevelEXP((Helpful.StatGrowthRates)u.GetGrowthRate(s), newStatValue);
                progressor.fromValue    = 0f;

                progressor.SetValueAt(0f);
            }
        }
    }

    private void StartPlayerTimerAbilities()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
            playerAbilityButtons[i].StartTimer();

        Debug.Log("Player Abilities Unpaused");
    }

    private void StartEnemyTimerAbilities()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
            enemyAbilityButtons[i].StartTimer();

        Debug.Log("Enemy Abilities Unpaused");
    }

    private void PausePlayerTimerAbilities()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
            playerAbilityButtons[i].PauseTimer();

        Debug.Log("Player Abilities Paused");
    }

    private void PauseEnemyTimerAbilities()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
            enemyAbilityButtons[i].PauseTimer();

        Debug.Log("Enemy Abilities Paused");
    }

    private void BoardReset(Signal signal)
    {
        currentGameController.BoardReset();
    }

    #endregion

    private void AddEXPEarnedInBattle(Signal signal)
    {
        //Signal is object[]
        //info[0]   - Helpful.StatType  - The stat that earned EXP
        //info[1]   - int               - The amount of EXP earned for the action
        //info[2]   - Unit              - The unit that earns the EXP

        object[] info       = signal.GetValueUnsafe<object[]>();

        UnitStat k          = new UnitStat();
        k.unit              = (Unit)info[2];
        k.statType          = (Helpful.StatTypes)info[0];

        if (expEarnedDuringBattle.ContainsKey(k))
        {
            expEarnedDuringBattle[k] += (int)info[1];
        }
        else
        {
            expEarnedDuringBattle.Add(k, (int)info[1]);
        }
    }
}
