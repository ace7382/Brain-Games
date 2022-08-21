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

    [SerializeField] private Image                      characterPortrait;
    [SerializeField] private UIButton                   button;

    [SerializeField] private TextMeshProUGUI            levelLabel;
    [SerializeField] private Progressor                 levelExpBarProgressor;
    [SerializeField] private TextMeshProProgressTarget  levelExpBarTextProgressorTarget;

    [SerializeField] private Progressor                 mHealthBarProgressor;
    [SerializeField] private TextMeshProProgressTarget  mHealthBarTextProgressorTarget;
    [SerializeField] private Image                      mHealthSecondaryFillBar;

    [SerializeField] private TextMeshProUGUI            stat1Label;
    [SerializeField] private TextMeshProUGUI            stat1Current;
    [SerializeField] private GameObject                 stat1Arrows;
    [SerializeField] private TextMeshProUGUI            stat1Change;
    [SerializeField] private TextMeshProUGUI            stat2Label;
    [SerializeField] private TextMeshProUGUI            stat2Current;
    [SerializeField] private GameObject                 stat2Arrows;
    [SerializeField] private TextMeshProUGUI            stat2Change;
    [SerializeField] private TextMeshProUGUI            stat3Label;
    [SerializeField] private TextMeshProUGUI            stat3Current;
    [SerializeField] private GameObject                 stat3Arrows;
    [SerializeField] private TextMeshProUGUI            stat3Change;
    [SerializeField] private TextMeshProUGUI            stat4Label;
    [SerializeField] private TextMeshProUGUI            stat4Current;
    [SerializeField] private GameObject                 stat4Arrows;
    [SerializeField] private TextMeshProUGUI            stat4Change;
    [SerializeField] private TextMeshProUGUI            stat5Label;
    [SerializeField] private TextMeshProUGUI            stat5Current;
    [SerializeField] private GameObject                 stat5Arrows;
    [SerializeField] private TextMeshProUGUI            stat5Change;
    [SerializeField] private TextMeshProUGUI            stat6Label;
    [SerializeField] private TextMeshProUGUI            stat6Current;
    [SerializeField] private GameObject                 stat6Arrows;
    [SerializeField] private TextMeshProUGUI            stat6Change;

    #endregion

    #region Private Variables

    private Unit                                        unit;

    #endregion

    #region Public Properties

    public Unit Unit { get { return unit; } }

    #endregion

    #region Public Functions

    public void Setup(Unit u, Item i)
    {
        unit                                    = u;
        characterPortrait.sprite                = unit.InBattleSprite;

        levelLabel.text                         = "LVL " + unit.GetStat(Helpful.StatTypes.Level).ToString();

        levelExpBarProgressor.toValue           = unit.GetEXPNextLevelValue(Helpful.StatTypes.Level);
        levelExpBarProgressor.fromValue         = 0;
        levelExpBarTextProgressorTarget.suffix  = " / " + unit.GetEXPNextLevelValue(Helpful.StatTypes.Level).ToString();

        levelExpBarProgressor.SetValueAt(0); //This is needed for it to set the fill correctly on the next line
        levelExpBarProgressor.SetValueAt(unit.GetExpForStat(Helpful.StatTypes.Level));
        
        mHealthBarProgressor.toValue            = unit.GetStat(Helpful.StatTypes.MaxHP);
        mHealthBarProgressor.fromValue          = 0;
        mHealthBarTextProgressorTarget.suffix   = " / " + unit.GetStat(Helpful.StatTypes.MaxHP).ToString();

        mHealthBarProgressor.SetValueAt(0);
        mHealthBarProgressor.SetValueAt(unit.CurrentHP);

        mHealthSecondaryFillBar.fillAmount      = 0f;

        //TODO: Maybe alphabatize these? currently just in the arbitrary order set up in the enum
        stat1Label.text             = Helpful.StatTypes.Memory.GetShorthand();
        stat1Current.text           = unit.GetStat(Helpful.StatTypes.Memory).ToString();

        stat2Label.text             = Helpful.StatTypes.Observation.GetShorthand();
        stat2Current.text           = unit.GetStat(Helpful.StatTypes.Observation).ToString();

        stat3Label.text             = Helpful.StatTypes.Math.GetShorthand();
        stat3Current.text           = unit.GetStat(Helpful.StatTypes.Math).ToString();

        stat4Label.text             = Helpful.StatTypes.Language.GetShorthand();
        stat4Current.text           = unit.GetStat(Helpful.StatTypes.Language).ToString();

        stat5Label.text             = Helpful.StatTypes.Speed.GetShorthand();
        stat5Current.text           = unit.GetStat(Helpful.StatTypes.Speed).ToString();

        stat6Label.text             = Helpful.StatTypes.ProblemSolving.GetShorthand();
        stat6Current.text           = unit.GetStat(Helpful.StatTypes.ProblemSolving).ToString();

        ShowStatChangesForConsumable(i);

        button.RemoveBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick);

        button.AddBehaviour(Doozy.Runtime.UIManager.UIBehaviour.Name.PointerClick).Event
            .AddListener(delegate { OnClick(i); });
    }

    public IEnumerator UpdateCardOnItemUse()
    {
        //Update display
        mHealthBarProgressor.PlayToValue(unit.CurrentHP);

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

        //

        //TODO: This is real janky. Should save all the coroutines in an array/list.
        //      remove them on completion, and have this be while (list.count > 0) yield return null;
        yield return new WaitForSeconds(mHealthBarProgressor.GetDuration()); 
    }

    #endregion

    #region Private Functions

    private void OnClick(Item i)
    {
        if (i is Item_Consumable)
            ((Item_Consumable)i).UseItemFromInventory(unit);
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

    private void ShowStatChangesForConsumable(Item i)
    {
        //TODO: Do This better lol. Once the stat order is more concretely defined in the enum this can probably be a loop?

        Item_Consumable c = (Item_Consumable)i;

        int ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Memory);
        int amo = -1;

        stat1Arrows.SetActive(ind > -1);
        stat1Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.Memory, unit.GetStat(Helpful.StatTypes.Memory));

            int changedStatValue    = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Memory) + amo, 0, int.MaxValue);

            stat1Change.text        = changedStatValue.ToString();

            stat1Change.color       = changedStatValue > unit.GetStat(Helpful.StatTypes.Memory) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Observation);
        amo = -1;

        stat2Arrows.SetActive(ind > -1);
        stat2Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.Observation, unit.GetStat(Helpful.StatTypes.Observation));

            int changedStatValue = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Observation) + amo, 0, int.MaxValue);

            stat2Change.text = changedStatValue.ToString();

            stat2Change.color = changedStatValue > unit.GetStat(Helpful.StatTypes.Observation) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Math);
        amo = -1;

        stat3Arrows.SetActive(ind > -1);
        stat3Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.Math, unit.GetStat(Helpful.StatTypes.Math));

            int changedStatValue = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Math) + amo, 0, int.MaxValue);

            stat3Change.text = changedStatValue.ToString();

            stat3Change.color = changedStatValue > unit.GetStat(Helpful.StatTypes.Math) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Language);
        amo = -1;

        stat4Arrows.SetActive(ind > -1);
        stat4Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.Language, unit.GetStat(Helpful.StatTypes.Language));

            int changedStatValue = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Language) + amo, 0, int.MaxValue);

            stat4Change.text = changedStatValue.ToString();

            stat4Change.color = changedStatValue > unit.GetStat(Helpful.StatTypes.Language) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.Speed);
        amo = -1;

        stat5Arrows.SetActive(ind > -1);
        stat5Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.Speed, unit.GetStat(Helpful.StatTypes.Speed));

            int changedStatValue = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.Speed) + amo, 0, int.MaxValue);

            stat5Change.text = changedStatValue.ToString();

            stat5Change.color = changedStatValue > unit.GetStat(Helpful.StatTypes.Speed) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.ProblemSolving);
        amo = -1;

        stat6Arrows.SetActive(ind > -1);
        stat6Change.gameObject.SetActive(ind > -1);

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.ProblemSolving, unit.GetStat(Helpful.StatTypes.ProblemSolving));

            int changedStatValue = Mathf.Clamp(unit.GetStat(Helpful.StatTypes.ProblemSolving) + amo, 0, int.MaxValue);

            stat6Change.text = changedStatValue.ToString();

            stat6Change.color = changedStatValue > unit.GetStat(Helpful.StatTypes.ProblemSolving) ? Color.blue : Color.red;
        }

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.MaxHP);
        amo = -1;

        if (ind > -1)
        {
            //TODO: MaxHP changes from consumables. Need to consider a non-zero floor for hp
        }

        //Healing current m. Health

        ind = c.OnUseStatChanges.FindIndex(x => x.statToChange == Helpful.StatTypes.COUNT); //TODO: just more instances of Count == hp
        amo = -1;

        if (ind > -1)
        {
            amo = c.GetStatChangeAmount(Helpful.StatTypes.COUNT, unit.MaxHP); //Either a hard value or percent of M.M. Health

            int changedStatValue = Mathf.Clamp(unit.CurrentHP + amo, 0, unit.MaxHP);

            mHealthSecondaryFillBar.fillAmount = (float)changedStatValue / (float)unit.MaxHP;
        }
    }

    #endregion
}
