using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarDisplayController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] private bool               isPlayerHPBar;

    [Space]

    [SerializeField] private TextMeshProUGUI    displayText;
    [SerializeField] private Image              imageMask;

    #endregion

    #region Private Variables

    private int current;
    private int max;

    #endregion

    #region Signal Variables

    private SignalReceiver  currentRec;
    private SignalReceiver  maxRec;
    private SignalStream    currentStr;
    private SignalStream    maxStr;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        if (isPlayerHPBar)
        {
            currentStr  = SignalStream.Get("Battle", "PlayerCurrentHPUpdate");
            maxStr      = SignalStream.Get("Battle", "PlayerMaxHPUpdate");
        }
        else
        {
            currentStr  = SignalStream.Get("Battle", "EnemyCurrentHPUpdate");
            maxStr      = SignalStream.Get("Battle", "EnemyMaxHPUpdate");
        }

        currentRec      = new SignalReceiver().SetOnSignalCallback(UpdateCurrentHP);
        maxRec          = new SignalReceiver().SetOnSignalCallback(UpdateMaxHP);
    }

    private void OnEnable()
    {
        currentStr.ConnectReceiver(currentRec);
        maxStr.ConnectReceiver(maxRec);
    }

    private void OnDisable()
    {
        currentStr.DisconnectAllReceivers();
        maxStr.DisconnectAllReceivers();
    }

    #endregion

    #region Private Functions

    private void UpdateCurrentHP(Signal signal)
    {
        //Signal info is int, current HP

        int newVal = signal.GetValueUnsafe<int>();

        current = newVal;

        Debug.Log(name + " Changed CURRENT value to: " + current);

        UpdateDisplay();
    }

    private void UpdateMaxHP(Signal signal)
    {
        //Signal info is int, current HP

        int newVal = signal.GetValueUnsafe<int>();

        max = newVal;

        Debug.Log(name + " Changed MAX value to: " + max);

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        displayText.text = current.ToString() + " / " + max.ToString();

        float fillPercent = (float)((float)current / (float)max);

        imageMask.fillAmount = fillPercent;
    }

    #endregion
}
