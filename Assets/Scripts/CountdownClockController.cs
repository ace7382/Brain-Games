using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Signals;
using Doozy.Runtime.Reactor;
using Doozy.Runtime.UIManager.Components;
using UnityEngine.Events;

public class CountdownClockController : MonoBehaviour
{
    private bool                isCountingDown;
    private float               clockMaxSeconds;
    private float               secondsRemaining;
    private float               midPointSeconds;

    public Image                clockFill;
    public Progressor           clockProgressor;
    public TextMeshProUGUI      timeDisplay;
    public Color                clockMidColor;
    public UnityEvent           onOutOfTime;

    public float                SecondsRemaining { get { return secondsRemaining; } }

    private void Update()
    {
        if (isCountingDown)
        {
            secondsRemaining = Mathf.Clamp(secondsRemaining - Time.deltaTime, 0f, float.MaxValue);

            UpdateTimerDisplay();

            if (secondsRemaining <= 0.0f)
            {
                if (onOutOfTime != null)
                {
                    onOutOfTime.Invoke();
                }
            }
        }
    }

    public void SetupTimer(float startingSeconds, float midSeconds)
    {
        clockMaxSeconds     = startingSeconds;
        secondsRemaining    = startingSeconds;
        midPointSeconds     = midSeconds;

        UpdateTimerDisplay();
    }

    public void StartTimer()
    {
        if (clockMaxSeconds <= 0)
        {
            Debug.Log("Clock needs to call SetupTimer before it starts");
            return;
        }

        isCountingDown = true;
    }

    public void AddTime(float timeToAdd)
    {
        secondsRemaining += timeToAdd;

        if (isCountingDown)
            return;
        else
            UpdateTimerDisplay();
    }

    public void SubtractTime(float timeToSubtract)
    {
        AddTime(timeToSubtract * -1);

        if (isCountingDown)
            return;
        else
            UpdateTimerDisplay();
    }

    public void SetTime(float secondsToSetTo)
    {
        secondsRemaining = secondsToSetTo;

        if (isCountingDown)
            return;
        else
            UpdateTimerDisplay();
    }

    public void Pause()
    {
        isCountingDown = false;
    }

    public void Unpause()
    {
        isCountingDown = true;
    }

    private void UpdateTimerDisplay()
    {
        float percentFill = Mathf.Clamp((secondsRemaining / clockMaxSeconds), 0f, float.MaxValue);

        clockProgressor.SetProgressAt(percentFill);

        if (percentFill >= 1)
        {
            clockFill.color = Color.green;
        }
        else if (secondsRemaining <= midPointSeconds)
        {
            clockFill.color = Color.red;
        }
        else
        {
            clockFill.color = clockMidColor;
        }

        System.TimeSpan ts = System.TimeSpan.FromSeconds(secondsRemaining);
        timeDisplay.text = ts.Minutes + ":" + ts.Seconds.ToString("00");
    }
}
