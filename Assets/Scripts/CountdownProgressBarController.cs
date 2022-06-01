using Doozy.Runtime.Reactor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CountdownProgressBarController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] Progressor     progressor;
    [SerializeField] string         onOutOfTimeAudioClip;

    [Space]
    [Space]

    [SerializeField] UnityEvent     onOutOfTime;

    #endregion

    #region Private Variables

    private float                   clockMaxSeconds;
    private float                   secondsRemaining;
    private bool                    clockOn;

    #endregion

    #region Public Properties

    public float                    SecondsRemaining { get { return secondsRemaining; } }

    #endregion

    #region Unity Functions

    private void Update()
    {
        if (clockOn)
        {
            secondsRemaining -= Time.deltaTime;

            UpdateTimerDisplay();
        }
    }

    #endregion

    #region Public Functions

    public void SetupTimer(float startingSeconds)
    {
        clockMaxSeconds     = startingSeconds;
        secondsRemaining    = startingSeconds;

        clockOn             = false;

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        if (clockMaxSeconds <= 0)
        {
            Debug.Log("Clock needs to call SetupTimer before it starts");
            return;
        }

        Unpause();
    }

    public void Pause()
    {
        clockOn = false;
    }

    public void Unpause()
    {
        clockOn = true;
    }

    public void SetTime(float t)
    {
        secondsRemaining = t;

        UpdateTimerDisplay();
    }

    public void DrainClock()
    {
        RanOutOfTime();

        secondsRemaining = 0;

        progressor.PlayToProgress(0);
    }

    #endregion

    #region Private Functions

    private void UpdateTimerDisplay()
    {
        float percentFill = Mathf.Clamp(secondsRemaining / clockMaxSeconds, 0f, float.MaxValue);

        progressor.SetProgressAt(percentFill);

        if (secondsRemaining <= 0f)
        {
            secondsRemaining = 0f;
            RanOutOfTime();
        }
    }

    private void RanOutOfTime()
    {
        Pause();

        AudioManager.instance.Play(onOutOfTimeAudioClip);

        if (onOutOfTime != null)
            onOutOfTime.Invoke();
    }

    #endregion
}
