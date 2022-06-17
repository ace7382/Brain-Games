using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Doozy.Runtime.Signals;
using UnityEngine.UI;
using Doozy.Runtime.UIManager.Components;

public class AbilityButtonController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI            abilityNameText;
    [SerializeField] private RectTransform              chargeMarkerPanelTrans;
    [SerializeField] private RectTransform              timerbarPanelTrans;

    #endregion

    #region Private Variables

    private int                                         currentChargeMarkerIndex;
    private Ability                                     ability;
    private List<AbilityButtonChargeMarkerController>   chargeMarkers;
    private Color                                       chargeMarkerChargedColor;
    private Color                                       chargeMarkerUnchargedColor;
    private List<AbilityButtonTimerBarController>       timerBars;

    #endregion

    #region Signal Variables

    private SignalReceiver                              battle_abilitycharge_receiver;
    private SignalStream                                battle_abilitycharge_stream;
    private SignalReceiver                              battle_resetabilitycharges_receiver;
    private SignalStream                                battle_resetabilitycharges_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        battle_abilitycharge_stream         = SignalStream.Get("Battle", "AbilityCharge");
        battle_resetabilitycharges_stream   = SignalStream.Get("Battle", "ResetAbilityCharges");

        battle_abilitycharge_receiver       = new SignalReceiver().SetOnSignalCallback(UpdateChargeButtons);
        battle_resetabilitycharges_receiver = new SignalReceiver().SetOnSignalCallback(ClearCharges);
    }

    private void OnEnable()
    {
        battle_abilitycharge_stream.ConnectReceiver(battle_abilitycharge_receiver);
        battle_resetabilitycharges_stream.ConnectReceiver(battle_resetabilitycharges_receiver);
    }

    private void OnDisable()
    {
        battle_abilitycharge_stream.DisconnectReceiver(battle_abilitycharge_receiver);
        battle_resetabilitycharges_stream.DisconnectReceiver(battle_resetabilitycharges_receiver);
    }

    #endregion

    #region Public Functions

    public void SetupButton(Ability a)
    {
        ability                     = a;
        currentChargeMarkerIndex    = 0;
        abilityNameText.text        = ability.abilityName;

        if (ability.chargeType == Ability.AbilityChargeType.NUM_OF_CHARGES)
        {
            Destroy(timerbarPanelTrans.gameObject);

            chargeMarkers               = new List<AbilityButtonChargeMarkerController>();

            chargeMarkerUnchargedColor  = Color.black;
            chargeMarkerChargedColor    = Color.green;

            for (int i = 0; i < ability.numOfCharges; i++)
            {
                GameObject go                                   = Instantiate(BattleManager.instance.ChargeMarkerPrefab, chargeMarkerPanelTrans);
                go.transform.localPosition                      = Vector3.zero;
                go.transform.localScale                         = Vector3.one;

                AbilityButtonChargeMarkerController control     = go.GetComponent<AbilityButtonChargeMarkerController>();
                control.SetFillColor(chargeMarkerUnchargedColor);

                chargeMarkers.Add(control);
            }
        }
        else if (ability.chargeType == Ability.AbilityChargeType.TIMED)
        {
            Destroy(chargeMarkerPanelTrans.gameObject);

            timerBars                           = new List<AbilityButtonTimerBarController>();

            for (int i = 0; i < ability.numOfCharges; i++)
            {
                GameObject go                               = Instantiate(BattleManager.instance.TimerBarPrefab, timerbarPanelTrans);
                go.transform.localPosition                  = Vector3.zero;
                go.transform.localScale                     = Vector3.one;

                AbilityButtonTimerBarController control     = go.GetComponent<AbilityButtonTimerBarController>();
                control.SetupTimerBar(ability);

                timerBars.Add(control);
            }
        }

        Canvas.ForceUpdateCanvases();

        LayoutElement l     = gameObject.AddComponent<LayoutElement>();
        l.preferredHeight   = transform.Find("Button Shadow").GetComponent<RectTransform>().rect.height;

        Canvas.ForceUpdateCanvases();

        ContentSizeFitter c = gameObject.AddComponent<ContentSizeFitter>();
        c.verticalFit       = ContentSizeFitter.FitMode.PreferredSize;

        Canvas.ForceUpdateCanvases();
    }

    //Called by the Ability Button Controller's OnClick Behavior
    public void Activate()
    {
        if (!BattleManager.instance.IsPaused)
            ability.Activate();
        else
            ability.SendDetailsSignal();
    }

    public void DisableButton()
    {
        GetComponentInChildren<UIButton>().interactable = false;
    }

    public void EnableButton()
    {
        GetComponentInChildren<UIButton>().interactable = true;
    }

    public void StartTimer()
    {
        if (ability.chargeType != Ability.AbilityChargeType.TIMED || currentChargeMarkerIndex >= ability.numOfCharges)
            return;

        timerBars[currentChargeMarkerIndex].Unpause();
    }

    public void PauseTimer()
    {
        if (ability.chargeType != Ability.AbilityChargeType.TIMED || currentChargeMarkerIndex >= ability.numOfCharges)
            return;

        timerBars[currentChargeMarkerIndex].Pause();
    }

    #endregion

    #region Private Functions

    private void UpdateChargeButtons(Signal signal)
    {
        //Signal is object[2]
        //info[1]   - Ability   - The ability to charge
        //info[2]   - int       - the number of charges

        object[] info = signal.GetValueUnsafe<object[]>();

        if (ability.chargeType == Ability.AbilityChargeType.NUM_OF_CHARGES)
        {
            if ((Ability)info[0] == ability)
            {
                int chargesToSet = (int)info[1];

                while (chargesToSet > 0)
                {
                    SetNextCharge();

                    chargesToSet--;
                }
            }
        }
        else if (ability.chargeType == Ability.AbilityChargeType.TIMED)
        {
            if ((Ability)info[0] == ability)
            {
                Debug.Log("Updating Timed Charges for " + name);

                if (currentChargeMarkerIndex >= ability.numOfCharges)
                    return;

                timerBars[currentChargeMarkerIndex].Pause();

                currentChargeMarkerIndex++;

                if (currentChargeMarkerIndex < ability.numOfCharges)
                {
                    timerBars[currentChargeMarkerIndex].Unpause();
                }
                else
                {
                    //currentChargeMarkerIndex = ability.numOfCharges - 1;
                    Debug.Log(name + "'s charge index is outside of charge count. CurrentChargeMarkerIndex: " + currentChargeMarkerIndex);
                }
            }
        }
    }

    private void SetNextCharge()
    {
        if (currentChargeMarkerIndex >= ability.numOfCharges)
            return;

        chargeMarkers[currentChargeMarkerIndex].SetFillColor(chargeMarkerChargedColor);

        currentChargeMarkerIndex++;
    }

    private void ClearCharges(Signal signal)
    {
        //Signal info   - object[1]
        //info[0]       - Ability       - The ability that needs to be cleared

        object[] info   = signal.GetValueUnsafe<object[]>(); 

        if (ability != (Ability)info[0])
            return;

        if (ability.chargeType == Ability.AbilityChargeType.NUM_OF_CHARGES)
        {
            for (int i = 0; i < chargeMarkers.Count; i++)
            {
                chargeMarkers[i].SetFillColor(chargeMarkerUnchargedColor);
            }

            currentChargeMarkerIndex = 0;
        }
        else if (ability.chargeType == Ability.AbilityChargeType.TIMED)
        {
            for (int i = 0; i < timerBars.Count; i++)
                timerBars[i].SetupTimerBar(ability);

            currentChargeMarkerIndex = 0;

            timerBars[currentChargeMarkerIndex].Unpause();
        }
    }

    #endregion
}
