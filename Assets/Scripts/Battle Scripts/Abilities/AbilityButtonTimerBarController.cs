using Doozy.Runtime.Reactor;
using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityButtonTimerBarController : MonoBehaviour
{
    //need reference to the ability to directly update it's charge percent i think
    //Timer max
    //reference to fill bar
    //Want to be able to pause, don't need to do now but build with pause in mind
    //add and subtract time

    //On charge reaching max, tell button controller to update ability
    //  button controller can then tell next timerbar to start filling

    #region Inspector Variables

    [SerializeField] private Progressor     progressor;

    #endregion

    #region Private Variables

    private Ability                         ability;
    private bool                            timerIsOn;
    private float                           currentSeconds;
    private float                           maxSeconds;

    #endregion

    #region Unity Functions

    private void Update()
    {
        if (timerIsOn)
        {
            currentSeconds += Time.deltaTime;

            UpdateBarFill();
        }
    }

    #endregion

    #region Public Functions

    public void SetupTimerBar(Ability a)
    {
        ability         = a;
        currentSeconds  = 0;
        maxSeconds      = a.secondsToChargeOneBar;
        
        timerIsOn       = false;

        UpdateBarFill();
    }

    public void Pause()
    {
        timerIsOn = false;
    }

    public void Unpause()
    {
        timerIsOn = true;
    }

    public void AddTime(float secondsToAdd)
    {
        currentSeconds += secondsToAdd;

        UpdateBarFill();
    }

    #endregion

    #region Private Functions

    private void UpdateBarFill()
    {
        float percentFill = Mathf.Clamp(currentSeconds / maxSeconds, 0f, 1f);

        progressor.SetProgressAt(percentFill);

        if (currentSeconds >= maxSeconds)
        {
            currentSeconds = maxSeconds;
            BarFilled();
        }
    }

    private void BarFilled()
    {
        object[] info   = new object[1];
        info[0]         = ability; //The ability to charge

        Signal.Send("Ability", "TimerBarFilled", info); 
        //The Ability needs to listen for this, unlike the num_of_charges abilities
        //      which send the message to the display
    }

    #endregion
}