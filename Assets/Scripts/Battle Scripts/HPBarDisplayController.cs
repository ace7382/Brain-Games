using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HPBarDisplayController : MonoBehaviour
{
    #region Enums

    private enum HPBarOwner
    {
        PLAYER,
        ENEMY,
        DISPLAY,
    }

    #endregion

    #region Inspector Variables

    [SerializeField] private HPBarOwner         owner;

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
        if (owner == HPBarOwner.PLAYER)
        {
            currentStr  = SignalStream.Get("Battle", "PlayerCurrentHPUpdate");
            maxStr      = SignalStream.Get("Battle", "PlayerMaxHPUpdate");
        }
        else if (owner == HPBarOwner.ENEMY)
        {
            currentStr  = SignalStream.Get("Battle", "EnemyCurrentHPUpdate");
            maxStr      = SignalStream.Get("Battle", "EnemyMaxHPUpdate");
        }
        else
        {
            currentStr  = SignalStream.Get("Battle", "DisplayCurrentHPUpdate");
            maxStr      = SignalStream.Get("Battle", "DisplayMaxHPUpdate");
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
        currentStr.DisconnectReceiver(currentRec);
        maxStr.DisconnectReceiver(maxRec);
    }

    private void OnDestroy()
    {
        currentStr.DisconnectReceiver(currentRec);
        maxStr.DisconnectReceiver(maxRec);
    }

    #endregion

    #region Private Functions

    private void UpdateCurrentHP(Signal signal)
    {
        //Signal info is int, current HP

        int newVal = signal.GetValueUnsafe<int>();

        current = newVal;

        UpdateDisplay();
    }

    private void UpdateMaxHP(Signal signal)
    {
        //Signal info is int, current HP

        int newVal = signal.GetValueUnsafe<int>();

        max = newVal;

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
