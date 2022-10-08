using BizzyBeeGames;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
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

    private struct PlayerUnitEnemyEXPAward
    {
        public Unit                playerUnit;
        public Unit                enemyUnit;
        public Helpful.StatTypes   statType;
        public int                 expAmount;
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
    [SerializeField] private GameObject                 unitSelectCardPrefab;

    [Space]

    [Header("Variables")]
    [SerializeField] private RectTransform              gameBoardRectTrans;
    [SerializeField] private RectTransform              gameBoardBGRectTrans;
    [SerializeField] private BattleUnitController       currentPlayerUnitController;
    [SerializeField] private BattleUnitController       currentEnemyUnitController;
    [SerializeField] private RectTransform              playerAbilitiesParentContainer;
    [SerializeField] private RectTransform              enemyAbilitiesParentContainer;
    [SerializeField] private RectTransform              playerAbilityButtonPanel;
    [SerializeField] private RectTransform              enemyAbilityButtonPanel;
    [SerializeField] private RectTransform              playerContainer;
    [SerializeField] private RectTransform              enemyContainer;
    [SerializeField] private Image                      playerSprite;
    [SerializeField] private Image                      enemySprite;
    [SerializeField] private GameObject                 bottomButtonPanel;
    [SerializeField] private RectTransform              battlePartyButtonContainer;
    [SerializeField] private MentalHealthBarController  playerMHBar;
    [SerializeField] private MentalHealthBarController  enemyMHBar;

    [Space]

    [Header("Pre Battle Screen")]
    [SerializeField] private GameObject                 preBattleScreen;
    [SerializeField] private RectTransform              preBattleScreenAllUnitsContainer;
    [SerializeField] private RectTransform              preBattleScreenBattleUnitsContainer;
    [SerializeField] private TextMeshProUGUI            preBattleScreenBattlePartyCount;
    [SerializeField] private Image                      enemyPreBattleImage;
    [SerializeField] private UIButton                   preBattleScreenStartBattleButton;
    [SerializeField] private TextMeshProUGUI            enemyCountText;
    [SerializeField] private TextMeshProUGUI            enemyNameText;
    [SerializeField] private TextMeshProUGUI            enemyBattleGameName;
    [SerializeField] private GameObject                 previousArrow;
    [SerializeField] private GameObject                 nextArrow;
    [SerializeField] private MentalHealthBarController  preBattleEnemyMHBar;

    [Space]

    [Header("Post Battle Screen")]
    [SerializeField] private GameObject                 postBattleScreen;
    [SerializeField] private TextMeshProUGUI            postBattleScreenVictoryDefeatText;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenSectionLabels;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenExpBarLevels;
    [SerializeField] private TextMeshProUGUI[]          postBattleScreenExpBarAdds;
    [SerializeField] private Progressor[]               postBattleScreenExpBarProgressors;
    [SerializeField] private RectTransform              postBattleScreenEnemiesDefeatedContainer;
    [SerializeField] private RectTransform              postBattleScreenPlayerBattlePartyContainer;
    [SerializeField] private RectTransform              postBattleScreenItemRewardContainer;
    [SerializeField] private TextMeshProUGUI            postBattleScreenExpTitle;
    [SerializeField] private CanvasGroup                postBattleScreenButtonPanel;
    [SerializeField] private UIButton                   postBattleScreenInvisibleButton;

    [Space]

    [SerializeField] private GameObject                 pauseScreen;

    #endregion

    #region Private Variables

    private GameObject                                  battleBoard;
    private BattleGameControllerBase                    currentGameController;
    private List<AbilityButtonController>               playerAbilityButtons;
    private List<AbilityButtonController>               enemyAbilityButtons;
    private List<Unit>                                  playerBattleParty;
    private List<Unit>                                  enemyParty;
    private int                                         prePanelEnemyDisplayedIndex;
    private Dictionary<UnitStat, int>                   expEarnedDuringBattle;
    private List<PreBattleStats>                        preBattleStats;
    private bool                                        battleWon = false;
    private List<UIAnimation>                           currentAnimations;
    private List<Task>                                  currentTasks;
    private Dictionary<Item, int>                       battleItemRewards;
    private List<PlayerUnitEnemyEXPAward>               unitEXPRewardsFromDefeatedEnemies;
    private GameObject                                  preBattleEnemyModel;


    #endregion

    #region Signal Variables

    private SignalReceiver                              partymanagement_unitkoed_receiver;
    private SignalStream                                partymanagement_unitkoed_stream;
    private SignalReceiver                              battle_pause_receiver;
    private SignalStream                                battle_pause_stream;
    private SignalReceiver                              battle_countdownended_receiver;
    private SignalStream                                battle_countdownended_stream;
    private SignalReceiver                              battle_boardreset_receiver;
    private SignalStream                                battle_boardreset_stream;
    private SignalReceiver                              partymanagement_expadded_receiver;
    private SignalStream                                partymanagement_expadded_stream;
    private SignalReceiver                              battle_unitselectcardclicked_receiver;
    private SignalStream                                battle_unitselectcardclicked_stream;
    private SignalReceiver                              battle_unitattacked_receiver;
    private SignalStream                                battle_unitattacked_stream;

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

        partymanagement_unitkoed_stream             = SignalStream.Get("PartyManagement", "UnitKOed");
        battle_pause_stream                         = SignalStream.Get("Battle", "Pause");
        battle_countdownended_stream                = SignalStream.Get("Battle", "CountdownEnded");
        battle_boardreset_stream                    = SignalStream.Get("Battle", "BoardReset");
        partymanagement_expadded_stream             = SignalStream.Get("PartyManagement", "AwardExperience");
        battle_unitselectcardclicked_stream         = SignalStream.Get("Battle", "UnitSelectCardClicked");
        battle_unitattacked_stream                  = SignalStream.Get("Battle", "UnitAttacked");

        partymanagement_unitkoed_receiver           = new SignalReceiver().SetOnSignalCallback(UnitKO);
        battle_pause_receiver                       = new SignalReceiver().SetOnSignalCallback(PauseButtonClicked);
        battle_countdownended_receiver              = new SignalReceiver().SetOnSignalCallback(CountdownEndedBattleBegin);
        battle_boardreset_receiver                  = new SignalReceiver().SetOnSignalCallback(BoardReset);
        partymanagement_expadded_receiver           = new SignalReceiver().SetOnSignalCallback(AddEXPEarnedInBattle);
        battle_unitselectcardclicked_receiver       = new SignalReceiver().SetOnSignalCallback(UnitSelectCardClicked);
        battle_unitattacked_receiver                = new SignalReceiver().SetOnSignalCallback(UnitAttacked);

        enemyParty                                  = new List<Unit>(GameManager.instance.CurrentLevel.enemyUnits);
        prePanelEnemyDisplayedIndex                 = 0;

        playerSprite.color                          = Color.clear;
        enemySprite.color                           = Color.clear;

        expEarnedDuringBattle                       = new Dictionary<UnitStat, int>();
        preBattleStats                              = new List<PreBattleStats>();
        currentAnimations                           = new List<UIAnimation>();
        currentTasks                                = new List<Task>();
        battleItemRewards                           = new Dictionary<Item, int>();
        unitEXPRewardsFromDefeatedEnemies           = new List<PlayerUnitEnemyEXPAward>();

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
        partymanagement_unitkoed_stream.ConnectReceiver(partymanagement_unitkoed_receiver);
        battle_pause_stream.ConnectReceiver(battle_pause_receiver);
        battle_countdownended_stream.ConnectReceiver(battle_countdownended_receiver);
        battle_boardreset_stream.ConnectReceiver(battle_boardreset_receiver);
        partymanagement_expadded_stream.ConnectReceiver(partymanagement_expadded_receiver);
        battle_unitselectcardclicked_stream.ConnectReceiver(battle_unitselectcardclicked_receiver);
        battle_unitattacked_stream.ConnectReceiver(battle_unitattacked_receiver);
    }

    private void OnDisable()
    {
        partymanagement_unitkoed_stream.DisconnectReceiver(partymanagement_unitkoed_receiver);
        battle_pause_stream.DisconnectReceiver(battle_pause_receiver);
        battle_countdownended_stream.DisconnectReceiver(battle_countdownended_receiver);
        battle_boardreset_stream.DisconnectReceiver(battle_boardreset_receiver);
        partymanagement_expadded_stream.DisconnectReceiver(partymanagement_expadded_receiver);
        battle_unitselectcardclicked_stream.DisconnectReceiver(battle_unitselectcardclicked_receiver);
        battle_unitattacked_stream.DisconnectReceiver(battle_unitattacked_receiver);
    }

    private void OnDestroy()
    {
        OnDisable();
    }

    #endregion

    #region Public Functions

    //Called by the Start Battle Button's OnClick Behavior
    public void BeginBattle()
    {
        preBattleScreen.SetActive(false);

        playerBattleParty = new List<Unit>();

        for (int i = 0; i < preBattleScreenBattleUnitsContainer.childCount; i++)
        {
            playerBattleParty.Add(preBattleScreenBattleUnitsContainer.GetChild(i).GetComponent<UnitSelectCardController>().Unit);
        }

        SetupPlayerBattleUnit(playerBattleParty.GetFirstLivingUnit());
        SetupEnemyBattleUnit(enemyParty[0]);

        SetupGame();
    }

    //Called by the Previous Button and Next Button's OnClick Behaviors
    public void ChangePreBattleDisplay(int upDown)
    {
        prePanelEnemyDisplayedIndex = Mathf.Clamp(prePanelEnemyDisplayedIndex + upDown, 0, enemyParty.Count - 1);

        UpdatePreBattleEnemyDisplay();
    }

    public void InitializeEnemyUnits()
    {
        for (int i = 0; i < enemyParty.Count; i++)
        {
            enemyParty[i].Init();
        }
    }

    //Called by the OnClick behaviors of the PreBattle and PostBattle screens' exit buttons
    public void ReturnToWorldMap()
    {
        currentPlayerUnitController.ResetBattleUnitController();
        currentEnemyUnitController.ResetBattleUnitController();

        Signal.Send("Battle", "ReturnToWorldMap");
    }

    //Called by the Post Battle Screen's Invisible Button's OnClick Behavior
    public void SkipEndBattleScreenAnimation()
    {
        //Hide invisible button
        postBattleScreenInvisibleButton.gameObject.SetActive(false);

        //Stop all currently running animations
        //Iterate backwards bc we want to remove items from the list each loop
        for (int i = currentAnimations.Count - 1; i >= 0; i--)
        {
            currentAnimations[i].Stop();
            AnimationEnded(currentAnimations[i]);
        }
        for (int i = currentTasks.Count - 1; i >= 0; i--)
        {
            currentTasks[i].Stop();
        }

        //Set all of the screen titles to full alpha
        postBattleScreenVictoryDefeatText.GetComponent<CanvasGroup>().alpha = 1f;

        for (int i = 0; i < postBattleScreenSectionLabels.Length; i++)
        {
            postBattleScreenSectionLabels[i].color = Color.black;
        }

        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
        {
            postBattleScreenExpBarProgressors[i].GetComponentInParent<CanvasGroup>().alpha = 1f;
        }

        //Create Battle Party Icons
        AddBattlePartyIcons();

        //Set the EXP bar label to Total
        postBattleScreenExpTitle.text   = "Total";
        postBattleScreenExpTitle.color  = new Color(postBattleScreenExpTitle.color.r, postBattleScreenExpTitle.color.g
                                                    , postBattleScreenExpTitle.color.b, 1f);

        //Set all of the EXP levels, bars, and +xxx values
        int[] expEarned                 = new int[postBattleScreenExpBarProgressors.Length];
        UnitStat k                      = new UnitStat();
        k.unit                          = currentPlayerUnitController.UnitInfo;
        List<Unit> enemiesDefeated      = enemyParty.FindAll(x => x.CurrentHP <= 0);

        //Get the performance exp
        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
        {
            k.statType                  = (Helpful.StatTypes)i;
            expEarned[i]                = expEarnedDuringBattle.ContainsKey(k) ? expEarnedDuringBattle[k] : 0;
        }

        //Reset the bars back to their base so we aren't adding to anything that's already filled
        SetPostScreenEXPBars(currentPlayerUnitController.UnitInfo);

        //Add enemy icons, and add the enemy defeat reward xp to existing totals
        foreach (Transform child in postBattleScreenEnemiesDefeatedContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < enemiesDefeated.Count; i++)
        {
            AddEnemyIconToPostBattleScreen(enemiesDefeated[i]).color = Color.white;
        }

        for (int i = 0; i < unitEXPRewardsFromDefeatedEnemies.Count; i++)
        {
            if (unitEXPRewardsFromDefeatedEnemies[i].playerUnit == CurrentPlayerUnit.UnitInfo)
            {
                expEarned[(int)unitEXPRewardsFromDefeatedEnemies[i].statType] += unitEXPRewardsFromDefeatedEnemies[i].expAmount;
            }
        }

        //Set all of the displays
        for (int i = 0; i < expEarned.Length; i++)
        {
            if (expEarned[i] > 0)
            {
                //zero time, so this is completed instantly (really large amounts of exp will make this stutter/jump fyi
                Task fillBarsTask = new Task(FillBar(postBattleScreenExpBarProgressors[i], postBattleScreenExpBarLevels[i],
                                  k.unit, (Helpful.StatTypes)i, expEarned[i], 0f));

                postBattleScreenExpBarAdds[i].text = "+" + expEarned[i].ToString();
                postBattleScreenExpBarAdds[i].gameObject.SetActive(true);
            }
        }

        //Show all items gained
        foreach (Transform child in postBattleScreenItemRewardContainer)
            Destroy(child.gameObject);

        AddItemIcons();

        //Show buttons at bottom
        postBattleScreenButtonPanel.alpha = 1f;

        ApplyEndOfBattleRewards();
    }

    #endregion

    #region Private Functions

    private void SetupPreBattleScreen()
    {
        InitializeEnemyUnits();

        preBattleScreen.SetActive(true);

        bottomButtonPanel.SetActive(false);
        pauseScreen.SetActive(false);

        UpdatePreBattleEnemyDisplay();
        SetupBattlePartyChoices();
    }

    private void UpdatePreBattleEnemyDisplay()
    {
        if (preBattleEnemyModel != null)
            Destroy(preBattleEnemyModel);

        if (enemyParty[prePanelEnemyDisplayedIndex].UnitModel != null)
        {
            

            preBattleEnemyModel         = Instantiate(enemyParty[prePanelEnemyDisplayedIndex].UnitModel
                                                    , enemyPreBattleImage.transform);
            Transform t                 = preBattleEnemyModel.transform;
            t.localPosition             = Vector3.zero;
            t.localScale                = new Vector3(50, 50, 1);

            enemyPreBattleImage.sprite  = null;
        }
        else
        {
            preBattleEnemyModel         = null;
            enemyPreBattleImage.sprite  = enemyParty[prePanelEnemyDisplayedIndex].InBattleSprite;
        }

        enemyCountText.text             = string.Format("# {0} / {1}", (prePanelEnemyDisplayedIndex + 1).ToString(), enemyParty.Count.ToString());
        enemyNameText.text              = enemyParty[prePanelEnemyDisplayedIndex].Name;
        enemyBattleGameName.text        = enemyParty[prePanelEnemyDisplayedIndex].BattleGameName;

        preBattleEnemyMHBar.Init(enemyParty[prePanelEnemyDisplayedIndex]);

        nextArrow.SetActive(prePanelEnemyDisplayedIndex != enemyParty.Count - 1);
        previousArrow.SetActive(prePanelEnemyDisplayedIndex > 0);
    }

    private void SetupBattlePartyChoices()
    {
        //TODO: Set "unlimited" unit levels better
        int maxUnits    = GameManager.instance.CurrentLevel.numOfUnitsAllowed < 1 ? 100 : GameManager.instance.CurrentLevel.numOfUnitsAllowed;
        bool canStart   = true;

        for (int i = 0; i < PlayerPartyManager.instance.partyBattleUnits.Count; i++)
        {
            bool r = GameManager.instance.CurrentLevel.requiredUnits.Contains(PlayerPartyManager.instance.partyBattleUnits[i].Name);

            GameObject go                       = Instantiate(unitSelectCardPrefab
                                                    , r ? preBattleScreenBattleUnitsContainer : preBattleScreenAllUnitsContainer);

            go.transform.localPosition          = Vector2.zero;
            go.transform.localScale             = Vector3.one;

            UnitSelectCardController control    = go.GetComponent<UnitSelectCardController>();

            control.Setup(PlayerPartyManager.instance.partyBattleUnits[i]
                , GameManager.instance.CurrentLevel.forbiddenUnits.Contains(PlayerPartyManager.instance.partyBattleUnits[i].Name)
                , r
                , PlayerPartyManager.instance.partyBattleUnits[i].KOed);

            if (r && PlayerPartyManager.instance.partyBattleUnits[i].KOed)
                canStart = false;
        }

        preBattleScreenBattlePartyCount.text = string.Format("{0} / {1}"
            , preBattleScreenBattleUnitsContainer.childCount.ToString()
            , maxUnits.ToString());

        //If a required unit is KOed or if there are no required units, you need to select some to start the battle
        if (!canStart || preBattleScreenBattleUnitsContainer.childCount == 0)
            preBattleScreenStartBattleButton.interactable = false;
    }

    private void UnitSelectCardClicked(Signal signal)
    {
        //Signal is object[2]
        //info[1]   - UnitSelectCardController  - The card that was clicked's controller //TODO: Not sure i need this sent by the signal
        //info[2]   - RectTransform             - The cards transform

        object[] info   = signal.GetValueUnsafe<object[]>();

        RectTransform r = (RectTransform)info[1];
        int maxUnits    = GameManager.instance.CurrentLevel.numOfUnitsAllowed < 1 ? 100 : GameManager.instance.CurrentLevel.numOfUnitsAllowed;

        if (r.parent == preBattleScreenAllUnitsContainer)
        {
            if (preBattleScreenBattleUnitsContainer.childCount < maxUnits)
            {
                r.SetParent(preBattleScreenBattleUnitsContainer);
            }
        }
        else
        {
            r.SetParent(preBattleScreenAllUnitsContainer);
        }

        preBattleScreenBattlePartyCount.text = string.Format("{0} / {1}"
                                                , preBattleScreenBattleUnitsContainer.childCount.ToString()
                                                , maxUnits.ToString());

        bool canStart = preBattleScreenBattleUnitsContainer.childCount > 0;

        foreach (UnitSelectCardController control in preBattleScreenBattleUnitsContainer.GetComponentsInChildren<UnitSelectCardController>())
        {
            if (control.Required && control.KOed)
            {
                canStart = false;
                break;
            }
        }

        preBattleScreenStartBattleButton.interactable = canStart;
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

        playerMHBar.Init(CurrentPlayerUnit.UnitInfo);
    }

    private void ChangePlayerBattleUnit(Unit playerUnit)
    {
        if (playerUnit == CurrentPlayerUnit.UnitInfo)
            return;

        CurrentPlayerUnit.ResetBattleUnitController();

        SetupPlayerBattleUnit(playerUnit);

        if (!IsPaused)
            StartPlayerTimerAbilities();
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

        enemyMHBar.Init(currentEnemyUnitController.UnitInfo);

        if (enemyUnit.UnitModel != null)
        {
            enemySprite.sprite  = null;
            enemySprite.color   = Color.clear;

            CurrentEnemy.Model  = Instantiate(enemyUnit.UnitModel, enemyContainer);
            Transform t         = CurrentEnemy.Model.transform;
            t.localScale        = new Vector3(55, 55, 1);
            t.localPosition     = new Vector3(0, 10, 0);
            SortingGroup s      = CurrentEnemy.Model.GetComponent<SortingGroup>();
            s.sortingOrder      = 100;
        }
    }

    private void SetupGame()
    {
        pauseScreen.SetActive(false);

        for (int i = 0; i < playerBattleParty.Count; i++)
        {
            Unit temp                           = playerBattleParty[i];

            GameObject go                       = new GameObject();
            RectTransform goT                   = go.AddComponent<RectTransform>();
            go.AddComponent<Image>().sprite     = temp.InBattleSprite;
            UIButton goButt                     = go.AddComponent<UIButton>();

            goT.SetParent(battlePartyButtonContainer);
            goT.localScale                      = Vector3.one;
            goT.localPosition                   = Vector3.zero;

            goButt.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                .Event.AddListener(delegate { ChangePlayerBattleUnit(temp); });
        }

        playerMHBar.GetComponent<CanvasGroup>().alpha   = 1f;
        enemyMHBar.GetComponent<CanvasGroup>().alpha    = 1f;

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
        bottomButtonPanel.SetActive(false);

        DisablePlayerAbilityButtons();
        DisableEnemyAbilityButtons();

        PausePlayerTimerAbilities();
        PauseEnemyTimerAbilities();

        UIAnimation u   = UIAnimation.Width(gameBoardRectTrans, 950, 1.5f);
        u.Play();
        u               = UIAnimation.Width(gameBoardBGRectTrans, 950, 1.5f);
        u.Play();
        u               = UIAnimation.PositionX(playerContainer, -680, 1.5f);
        u.Play();
        u               = UIAnimation.PositionX(enemyContainer, 680, 1.5f);
        u.Play();
        u               = UIAnimation.PositionX(playerAbilitiesParentContainer, -730, 1.5f);
        u.Play();
        u               = UIAnimation.PositionX(enemyAbilitiesParentContainer, 730, 1.5f);
        u.Play();

        Signal.Send("Battle", "StartCountdown");
    }

    private void CountdownEndedBattleBegin(Signal signal)
    {
        battleBoard.SetActive(true);
        bottomButtonPanel.SetActive(true);

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

    private void EnemyKO()
    {
        // Testing 
        Animator a = enemyContainer.GetComponentInChildren<Animator>();
        
        if (a != null)
            a.SetInteger("State", 9);

        foreach (Transform child in enemyAbilityButtonPanel)
            Destroy(child.gameObject);

        enemyAbilityButtons.Clear();

        DisablePlayerAbilityButtons();

        if (a == null)
            OnEnemyKOAnimationFinish();
        //End Testing
    }

    public void OnEnemyKOAnimationFinish()
    {
        EnablePlayerAbilityButtons();

        //Move back up to enemy KO if I undo
        int nextEnemyIndex = enemyParty.FindIndex(x => x.CurrentHP > 0);

        if (nextEnemyIndex >= 0)
        {
            Destroy(CurrentEnemy.Model);

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
            battleWon = true;
            ShowEndOfBattleScreen();
        }
        //^^^^^^^^^^^^^^^
    }

    private void UnitAttacked(Signal signal)
    {
        //Signal info is object[]
        //info[0] BattleUnitController  - the unit controller attacking
        //info[1] int                   - the attack power

        object[] info                                   = signal.GetValueUnsafe<object[]>();
        BattleUnitController attackingUnitController    = (BattleUnitController)info[0];
        int damage                                      = (int)info[1];

        if (CurrentPlayerUnit == attackingUnitController)
        {
            enemyContainer.GetComponentInChildren<Animator>()?.SetInteger("State", 6);
            CurrentEnemy.TakeDamage(damage);
        }
        else if (CurrentEnemy == attackingUnitController)
        {
            enemyContainer.GetComponentInChildren<Animator>()?.SetBool("Attack", true);
            CurrentPlayerUnit.TakeDamage(damage);
        }
    }

    private void PlayerKO()
    {
        Unit next = playerBattleParty.GetFirstLivingUnit();

        if (next != null)
        {
            ChangePlayerBattleUnit(next);
        }
        else
        {
            battleWon = false;
            ShowEndOfBattleScreen();
        }
    }

    private void UnitKO(Signal signal)
    {
        //Signal info is object[1]
        //info[0] Unit  - the unit KOed

        object[] info   = signal.GetValueUnsafe<object[]>();
        Unit koed       = (Unit)info[0];

        if (koed == currentPlayerUnitController.UnitInfo)
            PlayerKO();
        else if (koed == currentEnemyUnitController.UnitInfo)
            EnemyKO();
        else
            Debug.LogError(koed.Name + " was KOed and isn't the player or enemy");
    }

    private void SetEnemyAbilityButtonsInteractability(bool isInteractable)
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
        {
            enemyAbilityButtons[i].GetComponentInChildren<UIButton>().enabled = isInteractable;
        }
    }

    private void PauseButtonClicked(Signal signal)
    {
        if (IsPaused) //Unpause
        {
            pauseScreen.SetActive(false);

            battleBoard.SetActive(true);

            StartEnemyTimerAbilities();
            StartPlayerTimerAbilities();

            SetEnemyAbilityButtonsInteractability(false);

            currentGameController.Unpause();
        }
        else //Pause
        {
            currentGameController.Pause();

            battleBoard.SetActive(false);

            pauseScreen.SetActive(true);

            pauseScreen.GetComponentInChildren<AbilityDisplayController>().ResetDisplay();

            PauseEnemyTimerAbilities();
            PausePlayerTimerAbilities();

            SetEnemyAbilityButtonsInteractability(true);
        }
    }

    private void ShowEndOfBattleScreen()
    {
        //TODO: Maybe move this elsewhere?
        if (battleWon)
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

        //Calculate items and EXP won from defeated enemies
        List<Unit> enemiesDefeated = enemyParty.FindAll(x => x.CurrentHP <= 0);

        for (int e = 0; e < enemiesDefeated.Count; e++)
        {
            //Items Earned
            for (int i = 0; i < enemiesDefeated[e].ItemRewards.Count; i++)
            {
                ItemReward reward   = enemiesDefeated[e].ItemRewards[i];
                float rand          = Random.Range(0f, 100f);

                if (rand <= reward.Chance) //Item Received
                {
                    //TODO; maybe weight this toward fewer drops?
                    int count = Random.Range(reward.MinDropped, reward.MaxDropped + 1);

                    if (battleItemRewards.ContainsKey(reward.Item)) //Add to existing 
                    {
                        battleItemRewards[reward.Item] += count;
                    }
                    else
                    {
                        battleItemRewards.Add(reward.Item, count);
                    }
                }
            }

            //EXP earned
            for (int i = 0; i < enemiesDefeated[e].EXPAward.Count; i++)
            {
                for (int p = 0; p < playerBattleParty.Count; p++)
                { 
                    PlayerUnitEnemyEXPAward award   = new PlayerUnitEnemyEXPAward();
                    award.playerUnit                = playerBattleParty[p];
                    award.enemyUnit                 = enemiesDefeated[e];
                    award.statType                  = (Helpful.StatTypes)enemiesDefeated[e].EXPAward[i].x;
                    award.expAmount                 = enemiesDefeated[e].EXPAward[i].y / playerBattleParty.Count;

                    unitEXPRewardsFromDefeatedEnemies.Add(award);
                }
            }
        }

        FindObjectOfType<BattlePopupController>().enabled   = false;

        float screenOpeningTime                             = .6f;

        //Stop the battle game
        currentGameController.Pause();

        //Disable and hide the bottom buttons
        bottomButtonPanel.SetActive(false);

        //Stop abilities
        DisableEnemyAbilityButtons();
        DisablePlayerAbilityButtons();
        PauseEnemyTimerAbilities();
        PausePlayerTimerAbilities();

        //Hide Abilities & HP bars
        UIAnimation u           = UIAnimation.Alpha(playerAbilityButtonPanel.GetComponentInParent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(enemyAbilityButtonPanel.GetComponentInParent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(playerMHBar.GetComponent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();
        u                       = UIAnimation.Alpha(enemyMHBar.GetComponent<CanvasGroup>(), 0f, screenOpeningTime / 2f);
        u.Play();

        //fadeout the game
        u                       = UIAnimation.Alpha(currentGameController.GameElementsCanvasGroup, 0f, screenOpeningTime);
        u.Play();

        //Stretch the battle board and bg out to 1800 and start loading the screen's content
        u                       = UIAnimation.Width(gameBoardRectTrans, 1800, screenOpeningTime);
        u.Play();
        u                       = UIAnimation.Width(gameBoardBGRectTrans, 1800, screenOpeningTime);

        u.OnAnimationFinished   = delegate { PostBattleScreenInitialAnimation(); };

        u.Play();

        //Fully remove the ability panels bc their transparent canvases block clicks
        playerAbilitiesParentContainer.gameObject.SetActive(false);
        enemyAbilitiesParentContainer.gameObject.SetActive(false);
    }

    private void PostBattleScreenInitialAnimation()
    {
        float animationTime                     = .5f;

        postBattleScreenVictoryDefeatText.text  = battleWon ? "Victory" : "Defeat";
        postBattleScreenVictoryDefeatText.color = battleWon ? Color.green : Color.red;

        postBattleScreen.SetActive(true);

        SetPostScreenEXPBars(currentPlayerUnitController.UnitInfo);

        postBattleScreenInvisibleButton.gameObject.SetActive(true);

        //TODO: I should do this correctly with coroutines and fade in functions instead of these nested UIAnimation.OnEnds
        UIAnimation victoryTextFadeIn = UIAnimation.Alpha(postBattleScreenVictoryDefeatText.GetComponent<CanvasGroup>(), 1f, animationTime);
        currentAnimations.Add(victoryTextFadeIn);

        victoryTextFadeIn.OnAnimationFinished = delegate
        {
            AnimationEnded(victoryTextFadeIn);

            //Fade each header label in
            for (int i = 0; i < postBattleScreenSectionLabels.Length; i++)
            {
                UIAnimation headerFadeIn = UIAnimation.Color(postBattleScreenSectionLabels[i], Color.black, animationTime);
                currentAnimations.Add(headerFadeIn);

                if (i == postBattleScreenSectionLabels.Length - 1)
                {
                    headerFadeIn.OnAnimationFinished = delegate
                    {
                        AnimationEnded(headerFadeIn);

                        //Fade in the EXP Bars
                        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
                        {
                            UIAnimation EXPBarFadeIn = UIAnimation.Alpha(postBattleScreenExpBarProgressors[i].GetComponentInParent<CanvasGroup>(), 1f, animationTime);
                            currentAnimations.Add(EXPBarFadeIn);

                            if (i == postBattleScreenSectionLabels.Length - 1)
                            {
                                EXPBarFadeIn.OnAnimationFinished = delegate
                                {
                                    AnimationEnded(EXPBarFadeIn);

                                    Task rewards = new Task(PostBattleScreenSecondaryAnimation());
                                    rewards.Finished += delegate { currentTasks.Remove(rewards); };
                                    currentTasks.Add(rewards);
                                };
                            }
                            else
                            {
                                EXPBarFadeIn.OnAnimationFinished = delegate { AnimationEnded(EXPBarFadeIn); };
                            }

                            EXPBarFadeIn.Play();
                        }
                    };
                }
                else
                {
                    headerFadeIn.OnAnimationFinished = delegate { AnimationEnded(headerFadeIn); };
                }

                headerFadeIn.Play();
            }

            //Add icons for each Battle Party Unit
            AddBattlePartyIcons();
        };
        victoryTextFadeIn.Play();
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

    private IEnumerator PostBattleScreenSecondaryAnimation()
    {
        float tickUpTime                        = 2.3f;
        float pauseBetweenEnemiesTime           = .7f;
        WaitForSeconds tickTimeWait             = new WaitForSeconds(tickUpTime);
        WaitForSeconds pauseBetweenEnemies      = new WaitForSeconds(pauseBetweenEnemiesTime);
        List<Unit> enemiesDefeated              = enemyParty.FindAll(x => x.CurrentHP <= 0);

        //***Switch EXP Header***
        UIAnimation text1out                    = UIAnimation.Color(postBattleScreenExpTitle,
                                                new Color(postBattleScreenExpTitle.color.r, postBattleScreenExpTitle.color.g
                                                    , postBattleScreenExpTitle.color.b, 0f)
                                                , pauseBetweenEnemiesTime / 2f);
        currentAnimations.Add(text1out);

        text1out.OnAnimationFinished            = delegate { 
                                                    AnimationEnded(text1out); 

                                                    postBattleScreenExpTitle.text   = "Battle Performance";
                                                    UIAnimation text1in             = UIAnimation.Color(postBattleScreenExpTitle,
                                                                                        new Color(postBattleScreenExpTitle.color.r
                                                                                            , postBattleScreenExpTitle.color.g
                                                                                            , postBattleScreenExpTitle.color.b, 1f)
                                                                                        , pauseBetweenEnemiesTime / 2f);
                                                    
                                                    currentAnimations.Add(text1in);
                                                    
                                                    text1in.OnAnimationFinished = delegate { AnimationEnded(text1in); };
            
                                                    text1in.Play();
                                                };
        text1out.Play();
        //**********

        //***Update EXP Bars for Battle Performance EXP***
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

                Task increaseNumTask            = new Task(Helpful.IncreaseDisplayNumberOverTime(postBattleScreenExpBarAdds[i]
                                                    , 0, expEarned, "+", "", tickUpTime));
                increaseNumTask.Finished        += delegate { currentTasks.Remove(increaseNumTask); };

                currentTasks.Add(increaseNumTask);

                Task fillBarsTask               = new Task(FillBar(postBattleScreenExpBarProgressors[i]
                                                    , postBattleScreenExpBarLevels[i],
                                                     k.unit, k.statType, expEarned, tickUpTime));
                fillBarsTask.Finished           += delegate { currentTasks.Remove(fillBarsTask); };

                currentTasks.Add(fillBarsTask);
            }
        }

        if (needsToYield)
            yield return tickTimeWait;
        //*********

        //***Switch EXP Header***
        UIAnimation text2out                    = UIAnimation.Color(postBattleScreenExpTitle,
                                                new Color(postBattleScreenExpTitle.color.r, postBattleScreenExpTitle.color.g
                                                , postBattleScreenExpTitle.color.b, 0f)
                                                , pauseBetweenEnemiesTime / 2f);
        currentAnimations.Add(text2out);

        text2out.OnAnimationFinished            = delegate {
                                                    AnimationEnded(text2out);

                                                    postBattleScreenExpTitle.text   = "Enemy Rewards";
                                                    UIAnimation text2in             = UIAnimation.Color(postBattleScreenExpTitle,
                                                                                    new Color(postBattleScreenExpTitle.color.r
                                                                                        , postBattleScreenExpTitle.color.g
                                                                                        , postBattleScreenExpTitle.color.b, 1f)
                                                                                    , pauseBetweenEnemiesTime / 2f);
            
                                                    currentAnimations.Add(text2in);

                                                    text2in.OnAnimationFinished     = delegate { AnimationEnded(text2in); };
            
                                                    text2in.Play();
                                                };
        
        text2out.Play();

        yield return pauseBetweenEnemies;
        //**********

        //***Fade in enemies and fill EXP bars for enemy rewards***
        for (int e = 0; e < enemiesDefeated.Count; e++)
        {
            Task enemyIn        = new Task(Helpful.FadeGraphicIn(AddEnemyIconToPostBattleScreen(enemiesDefeated[e]), tickUpTime));
            enemyIn.Finished    += delegate { currentTasks.Remove(enemyIn); };
            currentTasks.Add(enemyIn);

            List<PlayerUnitEnemyEXPAward> expFromThisEnemy  = unitEXPRewardsFromDefeatedEnemies.FindAll(x => x.enemyUnit == enemiesDefeated[e]);
            
            for (int i = 0; i < expFromThisEnemy.Count; i++)
            {
                int statIndex = (int)expFromThisEnemy[i].statType;

                if (!postBattleScreenExpBarAdds[statIndex].gameObject.activeInHierarchy)
                {
                    postBattleScreenExpBarAdds[statIndex].text = "+0";
                    postBattleScreenExpBarAdds[statIndex].gameObject.SetActive(true);
                }

                int currentValue                = int.Parse(postBattleScreenExpBarAdds[statIndex].text.Substring(1));
                int endValue                    = currentValue + expFromThisEnemy[i].expAmount;

                Task increaseDisplayNumber      = new Task(Helpful.IncreaseDisplayNumberOverTime(
                                                    postBattleScreenExpBarAdds[statIndex], currentValue, endValue
                                                    , "+", "", tickUpTime));
                increaseDisplayNumber.Finished  += delegate { currentTasks.Remove(increaseDisplayNumber); };

                currentTasks.Add(increaseDisplayNumber);

                Task fillBars                   = new Task(FillBar(postBattleScreenExpBarProgressors[statIndex]
                                                    , postBattleScreenExpBarLevels[statIndex], k.unit
                                                    , (Helpful.StatTypes)statIndex, expFromThisEnemy[i].expAmount, tickUpTime));
                fillBars.Finished               += delegate { currentTasks.Remove(fillBars); };

                currentTasks.Add(fillBars);
            }

            yield return tickTimeWait; //To allow for EXP bars to fill and enemies/items to fade in

            yield return pauseBetweenEnemies;
        }
        //**********

        //***Switch EXP Header***
        UIAnimation text3out                    = UIAnimation.Color(postBattleScreenExpTitle,
                                                new Color(postBattleScreenExpTitle.color.r, postBattleScreenExpTitle.color.g
                                                , postBattleScreenExpTitle.color.b, 0f)
                                                , pauseBetweenEnemiesTime / 2f);
        currentAnimations.Add(text3out);

        text3out.OnAnimationFinished            = delegate {
                                                    AnimationEnded(text3out);

                                                    postBattleScreenExpTitle.text   = "Total";
                                                    UIAnimation text3in             = UIAnimation.Color(postBattleScreenExpTitle,
                                                                                    new Color(postBattleScreenExpTitle.color.r
                                                                                        , postBattleScreenExpTitle.color.g
                                                                                        , postBattleScreenExpTitle.color.b, 1f)
                                                                                    , pauseBetweenEnemiesTime / 2f);
            
                                                    currentAnimations.Add(text3in);

                                                    text3in.OnAnimationFinished     = delegate { AnimationEnded(text3in); };
            
                                                    text3in.Play();
                                                };
        
        text3out.Play();
        //**********

        //***Add Items
        AddItemIcons();
        //**********

        //***Fade in button panel at bottom
        //TODO: If the "Back to world map" button is clicked before it's fully loaded (like at alpha = .8)
        //      battle rewards won't be given (i think) very small window to do this tho?
        UIAnimation buttonsIn                   = UIAnimation.Alpha(postBattleScreenButtonPanel, 1, pauseBetweenEnemiesTime);

        currentAnimations.Add(buttonsIn);

        buttonsIn.OnAnimationFinished           = delegate { 
                                                    AnimationEnded(buttonsIn);
                                                    postBattleScreenInvisibleButton.gameObject.SetActive(false);
                                                    ApplyEndOfBattleRewards();
                                                };
        
        buttonsIn.Play();
        //**********
    }

    //TODO: Functionality is pretty similar to ItemTargetCardController_Unit.UpdateLevelBar().
    //      might want to consider making the bars a prefab or something and consolidating funcitonality to a "bar controller"
    //
    //TODO: If you get exactly the exp needed for the next level, the bar should bump up the value and then be empty but
    //      it will stop at 100% fill and not bump the level up <- only was able to get this to happen once?
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

            //Debug.Log(string.Format("Mid Fill: {0}. Current Value {1}, Bar.toValue {2}"
            //    , s.GetShorthand(), progressor.currentValue.ToString(), progressor.toValue.ToString()));

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

    private void AddBattlePartyIcons()
    {
        List<Image> icons = postBattleScreenPlayerBattlePartyContainer.GetComponentsInChildren<Image>().ToList();

        //TODO: Maybe fade these in?
        for (int i = 0; i < playerBattleParty.Count; i++)
        {
            if (icons.FindIndex(x => x.sprite == playerBattleParty[i].InBattleSprite) > -1)
                continue;

            Unit unit           = playerBattleParty[i];
            GameObject go       = new GameObject();
            Image im            = go.AddComponent<Image>();
            im.sprite           = playerBattleParty[i].InBattleSprite;
            RectTransform rt    = im.rectTransform;
            rt.SetParent(postBattleScreenPlayerBattlePartyContainer);
            rt.localPosition    = Vector3.zero;
            rt.localScale       = Vector3.one;
            UIButton butt       = go.AddComponent<UIButton>();

            butt.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick)
                .Event.AddListener(delegate { ChangeEndScreenUnit(unit); });
        }
    }

    private void ChangeEndScreenUnit(Unit u)
    {
        playerSprite.sprite         = u.InBattleSprite;
        playerSprite.color          = Color.white;
        float fillDur               = .5f;

        //All EXP (from performance AND enemy rewards) has been calculated and stored, so we just need to reference it
        int[] expEarned             = new int[postBattleScreenExpBarProgressors.Length];
        UnitStat k                  = new UnitStat();
        k.unit                      = u;

        for (int i = 0; i < postBattleScreenExpBarProgressors.Length; i++)
        {
            k.statType              = (Helpful.StatTypes)i;
            expEarned[i]            = expEarnedDuringBattle.ContainsKey(k) ? expEarnedDuringBattle[k] : 0;
        }

        //All exp has already been awarded bc the invisible button will no longer be visible,
        //  so we can just go by the unit's stats instead of calculating anything based on earned exp
        for (int i = 0; i < postBattleScreenExpBarLevels.Length; i++)
        {
            Helpful.StatTypes stat                                      = (Helpful.StatTypes)i;

            postBattleScreenExpBarLevels[i].text                        = u.GetStatWithMods(stat).ToString();
            postBattleScreenExpBarAdds[i].text                          = "+" + expEarned[i].ToString();
            postBattleScreenExpBarAdds[i].gameObject.SetActive(expEarned[i] > 0);

            postBattleScreenExpBarProgressors[i].reaction.settings.duration = fillDur;
            postBattleScreenExpBarProgressors[i].PlayToProgress((float)u.GetExpForStat(stat) / (float)u.GetEXPNextLevelValue(stat));
        }
    }

    private void AddItemIcons()
    {
        //TODO: Maybe fade these in?
        foreach (KeyValuePair<Item, int> itemEarned in battleItemRewards)
        {
            //TODO: Remove the button slot's OnClick behavior.
            //      Probably want it to give a tooltip with the name or something instead
            //      itemGO.GetComponent<UIButton>().GetBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event.RemoveAllListeners();
            //         ^^ this won't work to remove it, will need to remove the inspector given click function and set with this in code
            //          here and in the inventory where these are also used
            //TODO: If I implement max number held in inventory, that will probably need to be checked here, and visually represented
            GameObject itemGO               = Instantiate(itemSlot, postBattleScreenItemRewardContainer);
            itemGO.transform.localPosition  = Vector3.zero;
            itemGO.transform.localScale     = Vector3.one;
            ItemSlotController slot         = itemGO.GetComponent<ItemSlotController>();
            slot.Setup(itemEarned.Key, itemEarned.Value);
        }
    }

    private Image AddEnemyIconToPostBattleScreen(Unit enemy)
    {
        GameObject go   = new GameObject();
        Image goI       = go.AddComponent<Image>();
        goI.sprite      = enemy.InBattleSprite;
        goI.color       = new Color32(255, 255, 255, 0);
        Transform t     = go.transform;
        t.SetParent(postBattleScreenEnemiesDefeatedContainer);
        t.localScale    = Vector3.one;
        t.localPosition = Vector3.zero;

        return goI;
    }

    private void ApplyEndOfBattleRewards()
    {
        //EXP
        for (int i = 0; i < unitEXPRewardsFromDefeatedEnemies.Count; i++)
        {
            object[] info   = new object[3];
            info[0]         = unitEXPRewardsFromDefeatedEnemies[i].statType;    //Stat
            info[1]         = unitEXPRewardsFromDefeatedEnemies[i].expAmount;   //Amount
            info[2]         = unitEXPRewardsFromDefeatedEnemies[i].playerUnit;  //Unit

            Signal.Send("PartyManagement", "AwardExperience", info);
        }

        //Items
        foreach (KeyValuePair<Item, int> reward in battleItemRewards)
        {
            PlayerPartyManager.instance.AddItemToInventory(reward.Key, reward.Value);
        }
    }

    private void StartPlayerTimerAbilities()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
            playerAbilityButtons[i].StartTimer();
    }

    private void StartEnemyTimerAbilities()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
            enemyAbilityButtons[i].StartTimer();
    }

    private void PausePlayerTimerAbilities()
    {
        for (int i = 0; i < playerAbilityButtons.Count; i++)
            playerAbilityButtons[i].PauseTimer();
    }

    private void PauseEnemyTimerAbilities()
    {
        for (int i = 0; i < enemyAbilityButtons.Count; i++)
            enemyAbilityButtons[i].PauseTimer();
    }

    private void BoardReset(Signal signal)
    {
        currentGameController.BoardReset();
    }

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

    private void AnimationEnded(UIAnimation anim)
    {
        currentAnimations.Remove(anim);
    }

    #endregion
}