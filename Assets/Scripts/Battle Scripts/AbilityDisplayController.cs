using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Runtime.Signals;
using TMPro;

public class AbilityDisplayController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private TextMeshProUGUI    abilityNameDisplay;

    [Space]

    [SerializeField] private string             displayDefaultText;

    #endregion

    #region Private Variables

    private Ability                             currentlyDisplayedAbility;

    #endregion

    #region Signal Variables

    private SignalReceiver                      ability_details_receiver;
    private SignalStream                        ability_details_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        ability_details_stream = SignalStream.Get("Ability", "Details");

        ability_details_receiver = new SignalReceiver().SetOnSignalCallback(UpdateDisplay);
    }

    private void OnEnable()
    {
        ability_details_stream.ConnectReceiver(ability_details_receiver);
    }

    private void OnDisable()
    {
        ability_details_stream.DisconnectReceiver(ability_details_receiver);
    }

    #endregion

    #region Public Functions

    public void ResetDisplay()
    {
        currentlyDisplayedAbility = null;
        abilityNameDisplay.text = displayDefaultText;
    }

    #endregion

    #region Private Functions

    private void UpdateDisplay(Signal signal)
    {
        //Signal info   - object[1]
        //info[0]       - Ability       - The ability that needs to be displayed

        object[] info   = signal.GetValueUnsafe<object[]>();

        if (currentlyDisplayedAbility == (Ability)info[0])
        {
            ResetDisplay();
            return;
        }

        currentlyDisplayedAbility = (Ability)info[0];

        abilityNameDisplay.text = currentlyDisplayedAbility.abilityName;
    }



    #endregion
}
