using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Doozy.Runtime.Reactor;
using UnityEngine.Events;

public class CountdownClockController : MonoBehaviour
{
    private bool                notifiedOutOfTime;
    private float               clockMaxSeconds;
    private float               secondsRemaining;
    private float               midPointSeconds;

    private IEnumerator         countingDown;
    private IEnumerator         pulsing;

    public Image                clockFill;
    public Progressor           clockProgressor;
    public TextMeshProUGUI      timeDisplay;
    public Color                clockMidColor;
    public UnityEvent           onOutOfTime;

    public float                SecondsRemaining { get { return secondsRemaining; } }

    // pulse parameters
    public float                approachSpeed = 0.0015f;
    public float                growthBound = 1.1f;
    public float                shrinkBound = 0.9f;
    private float               currentRatio = 1;

    public void SetupTimer(float startingSeconds, float midSeconds)
    {
        notifiedOutOfTime   = false;
        clockMaxSeconds     = startingSeconds;
        secondsRemaining    = startingSeconds;
        midPointSeconds     = midSeconds;
        countingDown        = null;
        pulsing             = null;

        StopAllCoroutines();

        transform.localScale = Vector3.one;

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

    public IEnumerator Tick()
    {
        WaitForSeconds w = new WaitForSeconds(1f);

        while (secondsRemaining > 0f)
        {
            Debug.Log(secondsRemaining);
            SubtractTime(1);

            yield return w;
        }

        if (pulsing != null)
        {
            StopCoroutine(pulsing);
            pulsing = null;
        }

        if (onOutOfTime != null && !notifiedOutOfTime)
        {
            onOutOfTime.Invoke();
        }
        
        notifiedOutOfTime = true;
    }

    public void AddTime(float timeToAdd)
    {
        secondsRemaining += timeToAdd;

        UpdateTimerDisplay();
    }

    public void SubtractTime(float timeToSubtract)
    {
        AddTime(timeToSubtract * -1);

        UpdateTimerDisplay();
    }

    public void SetTime(float secondsToSetTo)
    {
        secondsRemaining = secondsToSetTo;

        UpdateTimerDisplay();
    }

    public void Pause()
    {
        if (countingDown != null)
        {
            StopCoroutine(countingDown);
        }

        countingDown = null;
    }

    public void Unpause()
    {
        countingDown = Tick();

        StartCoroutine(countingDown);
    }

    private void UpdateTimerDisplay()
    {
        //TODO: Add 1 to the display/remaining seconds. Currently the last second shows 0:00 on the clock.
        //      It's just a visual issue

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

        if (secondsRemaining <= 15f)
        {
            AudioManager.instance.Play("Countdown Tick");
        }

        if (secondsRemaining <= 5f && pulsing == null)
        {
            pulsing = Pulse();
            StartCoroutine(pulsing);
        }

        if (pulsing != null && secondsRemaining > 5f)
        {
            StopCoroutine(pulsing);
            pulsing = null;
        }

        System.TimeSpan ts = System.TimeSpan.FromSeconds(secondsRemaining);
        timeDisplay.text = ts.Minutes + ":" + ts.Seconds.ToString("00");
    }



    private IEnumerator Pulse()
    {
        // Run this indefinitely
        while (true)
        {
            // Get bigger for a few seconds
            while (currentRatio != growthBound)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards(currentRatio, growthBound, approachSpeed);

                // Update scale
                transform.localScale = Vector3.one * currentRatio;

                yield return new WaitForEndOfFrame();
            }

            // Shrink for a few seconds
            while (currentRatio != shrinkBound)
            {
                // Determine the new ratio to use
                currentRatio = Mathf.MoveTowards(currentRatio, shrinkBound, approachSpeed);

                // Update scale
                transform.localScale = Vector3.one * currentRatio;

                yield return new WaitForEndOfFrame();
            }
        }
    }
}
