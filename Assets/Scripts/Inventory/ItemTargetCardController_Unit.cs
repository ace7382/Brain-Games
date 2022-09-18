using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;
using Doozy.Runtime.Reactor.Targets.ProgressTargets;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemTargetCardController_Unit : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private Image                          characterPortrait;
    [SerializeField] private UIButton                       button;

    [Header("EXP and MHealth Bars")]
    [SerializeField] private TextMeshProUGUI                levelLabel;
    [SerializeField] private Progressor                     levelExpBarProgressor;
    [SerializeField] private TextMeshProProgressTarget      levelExpBarTextProgressorTarget;
    [SerializeField] private Image                          levelExpSecondaryFillBar;
    [SerializeField] private TextMeshProUGUI                levelExpChangeText;

    [SerializeField] private Progressor                     mHealthBarProgressor;
    [SerializeField] private TextMeshProProgressTarget      mHealthBarTextProgressorTarget;
    [SerializeField] private Image                          mHealthSecondaryFillBar;
    [SerializeField] private TextMeshProUGUI                mHealthChangeText;

    [Space]

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI                stat1Label;
    [SerializeField] private TextMeshProUGUI                stat1Current;
    [SerializeField] private GameObject                     stat1Arrows;
    [SerializeField] private TextMeshProUGUI                stat1Change;
    [SerializeField] private TextMeshProUGUI                stat2Label;
    [SerializeField] private TextMeshProUGUI                stat2Current;
    [SerializeField] private GameObject                     stat2Arrows;
    [SerializeField] private TextMeshProUGUI                stat2Change;
    [SerializeField] private TextMeshProUGUI                stat3Label;
    [SerializeField] private TextMeshProUGUI                stat3Current;
    [SerializeField] private GameObject                     stat3Arrows;
    [SerializeField] private TextMeshProUGUI                stat3Change;
    [SerializeField] private TextMeshProUGUI                stat4Label;
    [SerializeField] private TextMeshProUGUI                stat4Current;
    [SerializeField] private GameObject                     stat4Arrows;
    [SerializeField] private TextMeshProUGUI                stat4Change;
    [SerializeField] private TextMeshProUGUI                stat5Label;
    [SerializeField] private TextMeshProUGUI                stat5Current;
    [SerializeField] private GameObject                     stat5Arrows;
    [SerializeField] private TextMeshProUGUI                stat5Change;
    [SerializeField] private TextMeshProUGUI                stat6Label;
    [SerializeField] private TextMeshProUGUI                stat6Current;
    [SerializeField] private GameObject                     stat6Arrows;
    [SerializeField] private TextMeshProUGUI                stat6Change;

    [Space]

    [Header("Equipment")]
    [SerializeField] private GameObject                     equipmentSlotsPanel;
    [SerializeField] private List<EquipmentSlotController>  equipmentSlots;
    [SerializeField] private EquipmentSlotController        fromEquipmentSlot;
    [SerializeField] private EquipmentSlotController        toEquipmentSlot;
    [SerializeField] private GameObject                     lilArrow;
    [SerializeField] private GameObject                     equipmentAdditionalEffectsScrollView;
    [SerializeField] private RectTransform                  equipmentAdditionalEffectsContainer;
    [SerializeField] private UIButton                       equipButton;

    #endregion

    #region Private Variables

    private Unit                                            unit;
    private int                                             unitPreItemUseLevel;
    private int                                             currentlySelectedEquipmentSlot;

    #endregion

    #region Public Properties

    public Unit Unit { get { return unit; } }

    #endregion

    #region Public Functions

    public void Setup(Unit u, Item i)
    {
        unit                                    = u;
        unitPreItemUseLevel                     = u.GetStat(Helpful.StatTypes.Level);
        characterPortrait.sprite                = unit.InBattleSprite;

        levelLabel.text                         = "LVL " + unit.GetStat(Helpful.StatTypes.Level).ToString();

        levelExpBarProgressor.toValue           = unit.GetEXPNextLevelValue(Helpful.StatTypes.Level);
        levelExpBarProgressor.fromValue         = 0f;
        levelExpBarTextProgressorTarget.suffix  = " / " + unit.GetEXPNextLevelValue(Helpful.StatTypes.Level).ToString();

        levelExpBarProgressor.SetValueAt(0f); //This is needed for it to set the fill correctly on the next line
        levelExpBarProgressor.SetValueAt(unit.GetExpForStat(Helpful.StatTypes.Level));

        levelExpSecondaryFillBar.fillAmount     = 0f;
        
        mHealthBarProgressor.toValue            = unit.GetStatWithMods(Helpful.StatTypes.MaxHP);
        mHealthBarProgressor.fromValue          = 0;
        mHealthBarTextProgressorTarget.suffix   = " / " + unit.GetStatWithMods(Helpful.StatTypes.MaxHP).ToString();

        mHealthBarProgressor.SetValueAt(0);
        mHealthBarProgressor.SetValueAt(unit.CurrentHP);

        mHealthSecondaryFillBar.fillAmount      = 0f;

        //TODO: Maybe alphabatize these? currently just in the arbitrary order set up in the enum
        //TODO: Add Luck lolllllllll
        stat1Label.text                         = Helpful.StatTypes.Memory.GetShorthand();
        stat1Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.Memory).ToString();

        stat2Label.text                         = Helpful.StatTypes.Observation.GetShorthand();
        stat2Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.Observation).ToString();

        stat3Label.text                         = Helpful.StatTypes.Math.GetShorthand();
        stat3Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.Math).ToString();

        stat4Label.text                         = Helpful.StatTypes.Language.GetShorthand();
        stat4Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.Language).ToString();

        stat5Label.text                         = Helpful.StatTypes.Speed.GetShorthand();
        stat5Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.Speed).ToString();

        stat6Label.text                         = Helpful.StatTypes.ProblemSolving.GetShorthand();
        stat6Current.text                       = unit.GetStatWithMods(Helpful.StatTypes.ProblemSolving).ToString();

        //TODO: Probably put these all under 1 parent object and change that's visibility etc
        equipmentSlotsPanel.SetActive(i is Item_Equipment);
        equipButton.gameObject.SetActive(i is Item_Equipment);
        equipmentAdditionalEffectsScrollView.SetActive(i is Item_Equipment);
        toEquipmentSlot.gameObject.SetActive(i is Item_Equipment);
        fromEquipmentSlot.gameObject.SetActive(i is Item_Equipment);
        lilArrow.gameObject.SetActive(i is Item_Equipment);

        button.RemoveBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick);
        equipButton.RemoveBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick);

        if (i is Item_Consumable)
        {
            ShowStatChangesForConsumable(i);

            //The whole card is a button if it's a consumable
            button.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event
                .AddListener(delegate { OnClick(i); });
        }
        else if (i is Item_Equipment)
        {
            toEquipmentSlot.Setup((Item_Equipment)i);

            //Show the appropriate number of Equipment Slots
            for (int cs = 0; cs < equipmentSlots.Count; cs++)
            {
                equipmentSlots[cs].UIButton.RemoveBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick);

                if (cs < unit.Equipment.Count)
                {
                    equipmentSlots[cs].gameObject.SetActive(true);
                    equipmentSlots[cs].Setup(unit.Equipment[cs]);

                    int temp = cs;

                    equipmentSlots[cs].UIButton.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event
                        .AddListener(delegate { ChangeEquipmentSelection(temp); });
                }
                else
                    equipmentSlots[cs].gameObject.SetActive(false);
            }

            //Select the first slot to update the display
            ChangeEquipmentSelection(0);

            //The Equip Button "activates" the item if it's equipment
            //The whole card is a button if it's a consumable
            equipButton.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event
                .AddListener(delegate { EquipItem(i); });
        }
    }

    public IEnumerator UpdateCardOnItemUse()
    {
        if (unit.GetStatWithMods(Helpful.StatTypes.MaxHP) != mHealthBarProgressor.toValue)
        {
            //TODO: currently if a unit has taken damage and the max m health is falling below their current m health
            //      ie. - 10 M MHealth: 97/100 >> 90/90. The bar's fill is jumping up to 100% and then the numbers are falling and
            //      it's jarring. should probably make the bar incrementally move up at the same rate the numbers are moving down
            //      probably can solve by making the MMH number it's own progressor
            StartCoroutine(UpdateProgressorBarSuffix(mHealthBarTextProgressorTarget, (int)mHealthBarProgressor.toValue, unit.GetStatWithMods(Helpful.StatTypes.MaxHP)));
            mHealthBarProgressor.toValue = unit.GetStatWithMods(Helpful.StatTypes.MaxHP);
        }

        mHealthBarProgressor.PlayToValue(unit.CurrentHP);

        StartCoroutine(UpdateLevelBar());

        if (stat1Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat1Current, int.Parse(stat1Current.text), int.Parse(stat1Change.text)));
        if (stat2Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat2Current, int.Parse(stat2Current.text), int.Parse(stat2Change.text)));
        if (stat3Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat3Current, int.Parse(stat3Current.text), int.Parse(stat3Change.text)));
        if (stat4Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat4Current, int.Parse(stat4Current.text), int.Parse(stat4Change.text)));
        if (stat5Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat5Current, int.Parse(stat5Current.text), int.Parse(stat5Change.text)));
        if (stat6Arrows.activeInHierarchy)
            StartCoroutine(UpdateStatValue(stat6Current, int.Parse(stat6Current.text), int.Parse(stat6Change.text)));

        //TODO: This is real janky. Should save all the coroutines in an array/list.
        //      remove them on completion, and have this be while (list.count > 0) yield return null;
        //      (like i did in old path puzzle's tile spin on level complete)
        //      butttttttt they're all the same length of time so this works for now 
        yield return new WaitForSeconds(mHealthBarProgressor.GetDuration()); 
    }

    //TODO: Evaluate the need to make the max m health value on the health bar its own progressor.
    //      currently when losing Max M Health, if the unit's current m health is also being clamped to the newly lowerd value
    //      then the current MH and the MaxMH displays fall at different rates whether it's using this function
    //      or the coroutine currently in use.
    ////Called by the M.Health Bar Progressor's On Change Value Callback
    //public void OnMHealthBarUpdate()
    //{
    //    if (mHealthBarProgressor.currentValue >= mHealthBarProgressor.toValue)
    //    {
    //        mHealthBarTextProgressorTarget.suffix = " / " + ((int)mHealthBarProgressor.currentValue).ToString();
    //    }
    //}

    #endregion

    #region Private Functions

    private void OnClick(Item i)
    {
        if (i is Item_Consumable)
            ((Item_Consumable)i).UseItemFromInventory(unit);
    }

    private void ChangeEquipmentSelection(int slotNum)
    {
        //Change the selection num
        currentlySelectedEquipmentSlot = slotNum;

        //Update the From image
        fromEquipmentSlot.Setup(unit.Equipment[currentlySelectedEquipmentSlot]);

        //Update the stat changes
        ShowStatChangesForEquipment();

        //Update the additional effects
    }

    private void EquipItem(Item i)
    {
        unit.EquipItem(currentlySelectedEquipmentSlot, (Item_Equipment)i);
    }

    private IEnumerator UpdateLevelBar()
    {
        int difference = unit.GetStat(Helpful.StatTypes.Level) - unitPreItemUseLevel; //items can only give level exp i have decided

        levelExpBarProgressor.reaction.settings.duration = mHealthBarProgressor.GetDuration() / (float)difference;

        WaitForSeconds w = new WaitForSeconds(levelExpBarProgressor.GetDuration());

        while (unitPreItemUseLevel < unit.GetStat(Helpful.StatTypes.Level))
        {
            levelExpBarProgressor.PlayToValue(levelExpBarProgressor.toValue);

            unitPreItemUseLevel++;

            yield return w;

            levelLabel.text                         = "LVL " + unitPreItemUseLevel.ToString();

            levelExpBarProgressor.toValue           = Formulas.GetNextLevelEXP(
                                                    (Helpful.StatGrowthRates)unit.GetGrowthRate(Helpful.StatTypes.Level)
                                                    , unitPreItemUseLevel);
            levelExpBarProgressor.fromValue         = 0f;
            levelExpBarTextProgressorTarget.suffix  = " / " + levelExpBarProgressor.toValue.ToString();

            levelExpBarProgressor.SetValueAt(0f);
        }

        levelExpBarProgressor.PlayToValue(unit.GetExpForStat(Helpful.StatTypes.Level));
    }

    private IEnumerator UpdateStatValue(TextMeshProUGUI t, int startingValue, int endingValue)
    {
        float totalTimeAllowed  = mHealthBarProgressor.GetDuration();

        bool goingUp            = startingValue < endingValue;

        int numOfTicks          = Mathf.Abs(startingValue - endingValue);
        float tickTime          = totalTimeAllowed / numOfTicks;
        float time              = 0f;

        while (startingValue != endingValue)
        {
            time += Time.deltaTime;

            if (time >= tickTime)
            {
                int ticksToDo   = (int)(time / tickTime);

                startingValue   = goingUp ? Mathf.Clamp(startingValue + ticksToDo, startingValue, endingValue) 
                                    : Mathf.Clamp(startingValue - ticksToDo, endingValue, startingValue);

                time            -= ticksToDo * tickTime;
            }

            t.text = startingValue.ToString();

            yield return null;
        }
    }

    private IEnumerator UpdateProgressorBarSuffix(TextMeshProProgressTarget t, int startingValue, int endingValue)
    {
        float totalTimeAllowed  = mHealthBarProgressor.GetDuration();

        bool goingUp            = startingValue < endingValue;

        int numOfTicks          = Mathf.Abs(startingValue - endingValue);
        float tickTime          = totalTimeAllowed / numOfTicks;
        float time              = 0f;

        while (startingValue != endingValue)
        {
            time += Time.deltaTime;

            if (time >= tickTime)
            {
                int ticksToDo   = (int)(time / tickTime);

                startingValue   = goingUp ? Mathf.Clamp(startingValue + ticksToDo, startingValue, endingValue) 
                                    : Mathf.Clamp(startingValue - ticksToDo, endingValue, startingValue);

                time            -= ticksToDo * tickTime;
            }

            t.suffix = " / " + startingValue.ToString();

            yield return null;
        }
    }

    private void ShowStatChangesForConsumable(Item i)
    {
        //TODO: Do This better lol. Once the stat order is more concretely defined in the enum this can probably be a loop?

        Item_Consumable c = (Item_Consumable)i;

        //===Memory===
        int ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Memory);
        int amo = -1;

        stat1Arrows.SetActive(ind > -1);
        stat1Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.Memory, unit.GetStat(Helpful.StatTypes.Memory)); //Change amount is pre mods, display isnt though

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Memory) + amo, 0, int.MaxValue);

            stat1Change.text        = changedStatValue.ToString();

            stat1Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.Memory) ? Color.blue : Color.red;
        }
        //=============

        //===Observation===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Observation);
        amo = -1;

        stat2Arrows.SetActive(ind > -1);
        stat2Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.Observation, unit.GetStat(Helpful.StatTypes.Observation));

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Observation) + amo, 0, int.MaxValue);

            stat2Change.text        = changedStatValue.ToString();

            stat2Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.Observation) ? Color.blue : Color.red;
        }
        //=============

        //===Math===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Math);
        amo = -1;

        stat3Arrows.SetActive(ind > -1);
        stat3Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.Math, unit.GetStat(Helpful.StatTypes.Math));

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Math) + amo, 0, int.MaxValue);

            stat3Change.text        = changedStatValue.ToString();

            stat3Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.Math) ? Color.blue : Color.red;
        }
        //=============

        //===Language===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Language);
        amo = -1;

        stat4Arrows.SetActive(ind > -1);
        stat4Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.Language, unit.GetStat(Helpful.StatTypes.Language));

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Language) + amo, 0, int.MaxValue);

            stat4Change.text        = changedStatValue.ToString();

            stat4Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.Language) ? Color.blue : Color.red;
        }
        //=============

        //===Speed===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Speed);
        amo = -1;

        stat5Arrows.SetActive(ind > -1);
        stat5Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.Speed, unit.GetStat(Helpful.StatTypes.Speed));

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Speed) + amo, 0, int.MaxValue);

            stat5Change.text        = changedStatValue.ToString();

            stat5Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.Speed) ? Color.blue : Color.red;
        }
        //=============

        //===Problem Solving===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.ProblemSolving);
        amo = -1;

        stat6Arrows.SetActive(ind > -1);
        stat6Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.ProblemSolving, unit.GetStat(Helpful.StatTypes.ProblemSolving));

            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.ProblemSolving) + amo, 0, int.MaxValue);

            stat6Change.text        = changedStatValue.ToString();

            stat6Change.color       = changedStatValue > unit.GetStatWithMods(Helpful.StatTypes.ProblemSolving) ? Color.blue : Color.red;
        }
        //=============

        //===Max M Health==
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.MaxHP);
        amo = -1;

        mHealthChangeText.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            //TODO: MaxHP changes from consumables. Need to consider a non-zero floor for max hp
            amo                     = c.GetStatChangeAmount(Helpful.StatTypes.MaxHP, unit.GetStat(Helpful.StatTypes.MaxHP));

            //TODO: make a floor for max M.Health and apply here instead of 5.
            //      might want to consider just not allowing items to lower max m. health too
            int changedStatValue    = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.MaxHP) + amo, 5, int.MaxValue);
            changedStatValue        = changedStatValue - unit.GetStatWithMods(Helpful.StatTypes.MaxHP); //difference
            mHealthChangeText.text  = "M " + (changedStatValue >= 0 ? "+" : "") + changedStatValue.ToString();
            mHealthChangeText.color = changedStatValue > 0 ? Color.blue : Color.red;

            //TODO: Update the health bar fill/secondary fills
        }
        //=============


        //===Healing Current M. Health===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.COUNT); //TODO: just more instances of Count == hp
        amo = -1;

        if (ind > -1)
        {
            amo                                 = c.GetStatChangeAmount(Helpful.StatTypes.COUNT, unit.GetStatWithMods(Helpful.StatTypes.MaxHP)); //Healing percents are always based on max hp

            int changedStatValue                = Mathf.Clamp(unit.CurrentHP + amo, 0, unit.GetStatWithMods(Helpful.StatTypes.MaxHP));

            mHealthSecondaryFillBar.fillAmount  = (float)changedStatValue / (float)unit.GetStatWithMods(Helpful.StatTypes.MaxHP);

            //We want to show how much we're healing too if there isn't already a max m. health change showing
            if (!mHealthChangeText.gameObject.activeInHierarchy)
            {
                mHealthChangeText.gameObject.SetActive(true);

                int diff                        = changedStatValue - unit.CurrentHP;

                mHealthChangeText.text          = (diff >= 0 ? "+" : "") + diff.ToString();
                mHealthChangeText.color         = diff >= 0 ? Color.blue : Color.red;
            }
        }
        //=============

        //===Level===
        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Level);
        amo = -1;

        levelExpChangeText.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo                                     = c.GetStatChangeAmount(Helpful.StatTypes.Level, unit.GetStat(Helpful.StatTypes.Level));

            int changedStatValue                    = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Level) + amo, 0, int.MaxValue);
            changedStatValue                        = changedStatValue - unit.GetStat(Helpful.StatTypes.Level);

            //Raising a level will just give the current level's EXP, so the secondary bar always is full
            levelExpSecondaryFillBar.fillAmount     = 1f;

            levelExpChangeText.text                 = (changedStatValue >= 0 ? "+" : "") + changedStatValue.ToString();

            levelExpChangeText.color                = Color.blue; //Can't lower level with items, so its always adding
        }
        //=============
    }

    private void ShowStatChangesForEquipment()
    {
        //TODO: Do This better lol. Once the stat order is more concretely defined in the enum this can probably be a loop?
        //      also might be able to combine with function above.

        //===Memory===
        int amoFrom     = fromEquipmentSlot.Item != null ? 
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Memory, unit.GetStat(Helpful.StatTypes.Memory)) 
                            : 0;
        int amoTo       = toEquipmentSlot.Item != null ? 
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Memory, unit.GetStat(Helpful.StatTypes.Memory))
                            : 0;

        int difference  = amoTo - amoFrom;
        
        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Memory) + difference, 0, int.MaxValue);

            stat1Change.text = newStatValue.ToString();

            stat1Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.Memory) ? Color.blue : Color.red;
        }

        stat1Arrows.SetActive(difference != 0);
        stat1Change.gameObject.SetActive(difference != 0);
        //=============

        ////===Observation===
        amoFrom         = fromEquipmentSlot.Item != null ?
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Observation, unit.GetStat(Helpful.StatTypes.Observation))
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ?
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Observation, unit.GetStat(Helpful.StatTypes.Observation))
                            : 0;

        difference      = amoTo - amoFrom;

        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Observation) + difference, 0, int.MaxValue);

            stat2Change.text = newStatValue.ToString();

            stat2Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.Observation) ? Color.blue : Color.red;
        }

        stat2Arrows.SetActive(difference != 0);
        stat2Change.gameObject.SetActive(difference != 0);
        ////=============

        ////===Math===
        amoFrom         = fromEquipmentSlot.Item != null ? 
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Math, unit.GetStat(Helpful.StatTypes.Math)) 
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ? 
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Math, unit.GetStat(Helpful.StatTypes.Math))
                            : 0;

        difference      = amoTo - amoFrom;
        
        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Math) + difference, 0, int.MaxValue);

            stat3Change.text = newStatValue.ToString();

            stat3Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.Math) ? Color.blue : Color.red;
        }

        stat3Arrows.SetActive(difference != 0);
        stat3Change.gameObject.SetActive(difference != 0);
        ////=============

        ////===Language===
        amoFrom         = fromEquipmentSlot.Item != null ? 
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Language, unit.GetStat(Helpful.StatTypes.Language)) 
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ? 
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Language, unit.GetStat(Helpful.StatTypes.Language))
                            : 0;

        difference      = amoTo - amoFrom;
        
        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Language) + difference, 0, int.MaxValue);

            stat4Change.text = newStatValue.ToString();

            stat4Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.Language) ? Color.blue : Color.red;
        }

        stat4Arrows.SetActive(difference != 0);
        stat4Change.gameObject.SetActive(difference != 0);
        ////=============

        ////===Speed===
        amoFrom         = fromEquipmentSlot.Item != null ? 
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Speed, unit.GetStat(Helpful.StatTypes.Speed)) 
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ? 
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.Speed, unit.GetStat(Helpful.StatTypes.Speed))
                            : 0;

        difference      = amoTo - amoFrom;
        
        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.Speed) + difference, 0, int.MaxValue);

            stat5Change.text = newStatValue.ToString();

            stat5Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.Speed) ? Color.blue : Color.red;
        }

        stat5Arrows.SetActive(difference != 0);
        stat5Change.gameObject.SetActive(difference != 0);
        ////=============

        ////===Problem Solving===
        amoFrom         = fromEquipmentSlot.Item != null ? 
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.ProblemSolving, unit.GetStat(Helpful.StatTypes.ProblemSolving)) 
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ? 
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.ProblemSolving, unit.GetStat(Helpful.StatTypes.ProblemSolving))
                            : 0;

        difference      = amoTo - amoFrom;
        
        if (difference != 0)
        {
            int newStatValue = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.ProblemSolving) + difference, 0, int.MaxValue);

            stat6Change.text = newStatValue.ToString();

            stat6Change.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.ProblemSolving) ? Color.blue : Color.red;
        }

        stat6Arrows.SetActive(difference != 0);
        stat6Change.gameObject.SetActive(difference != 0);
        ////=============

        ////===Max M Health==
        amoFrom         = fromEquipmentSlot.Item != null ?
                            fromEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.MaxHP, unit.GetStat(Helpful.StatTypes.MaxHP))
                            : 0;
        amoTo           = toEquipmentSlot.Item != null ?
                            toEquipmentSlot.Item.GetStatChangeAmount(Helpful.StatTypes.MaxHP, unit.GetStat(Helpful.StatTypes.MaxHP))
                            : 0;

        difference = amoTo - amoFrom;

        if (difference != 0)
        {
            //TODO: More Max HP floor hardcoding
            int newStatValue        = Mathf.Clamp(unit.GetStatWithMods(Helpful.StatTypes.MaxHP) + difference, 5, int.MaxValue);

            //TODO: Make this formatting match the format above in consumables "M +xxx" or M -xxx"
            mHealthChangeText.text  = newStatValue.ToString();

            mHealthChangeText.color = newStatValue > unit.GetStatWithMods(Helpful.StatTypes.MaxHP) ? Color.blue : Color.red;
        }

        mHealthChangeText.gameObject.SetActive(difference != 0);

        //ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.MaxHP);
        //amo = -1;

        //mHealthChangeText.gameObject.SetActive(ind > -1);

        //if (ind > -1)
        //{
        //    //TODO: MaxHP changes from consumables. Need to consider a non-zero floor for max hp
        //    amo = c.GetStatChangeAmount(Helpful.StatTypes.MaxHP, unit.MaxHP);

        //    //TODO: make a floor for max M.Health and apply here instead of 5.
        //    //      might want to consider just not allowing items to lower max m. health too
        //    int changedStatValue = Mathf.Clamp(unit.MaxHP + amo, 5, int.MaxValue);
        //    changedStatValue = changedStatValue - unit.MaxHP; //difference
        //    mHealthChangeText.text = "M " + (changedStatValue >= 0 ? "+" : "") + changedStatValue.ToString();

        //    //TODO: Update the health bar fill/secondary fills
        //}
        ////=============


        ////===Healing Current M. Health===
        ////===Level===
        ///No equipment will raise current M Health or Level
    }

    #endregion
}
