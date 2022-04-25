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
            SubtractTime(1);

            yield return w;
        }

        secondsRemaining = -1;
        UpdateTimerDisplay();

        AudioManager.instance.Play("Out of Time", .5f);

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

        //TODO: See if i should pause pulsing when paused.
        //      probably should; i just don't feel like testing it rn lollll

        //if (pulsing != null)
        //{
        //    StopCoroutine(pulsing);
        //}

        //pulsing = null;
    }

    public void Unpause()
    {
        countingDown = Tick();

        StartCoroutine(countingDown);
    }

    public void Stop()
    {
        StopAllCoroutines();

        pulsing                 = null;
        countingDown            = null;

        secondsRemaining        = 0f;
        transform.localScale    = Vector3.one;
    }

    private void UpdateTimerDisplay()
    {
        //TODO: Add 1 to the display/remaining seconds. Currently the last second shows 0:00 on the clock.
        //      It's just a visual issue

        float fakeSecondsRemaining = secondsRemaining + 1;

        float percentFill = Mathf.Clamp((fakeSecondsRemaining / clockMaxSeconds), 0f, float.MaxValue);

        clockProgressor.SetProgressAt(percentFill);

        if (percentFill >= 1)
        {
            clockFill.color = Color.green;
        }
        else if (fakeSecondsRemaining <= midPointSeconds)
        {
            clockFill.color = Color.red;
        }
        else
        {
            clockFill.color = clockMidColor;
        }

        if (fakeSecondsRemaining <= 10f && fakeSecondsRemaining > 0)
        {
            if (fakeSecondsRemaining <= 3f)
                StartCoroutine(HellaTick());
            else if (fakeSecondsRemaining <= 5f)
                StartCoroutine(DoubleTick());
            else
                AudioManager.instance.Play("Countdown Tick");
        }

        if (fakeSecondsRemaining <= 5f && pulsing == null)
        {
            pulsing = Helpful.Pulse(transform, currentRatio, growthBound, shrinkBound, approachSpeed);
            StartCoroutine(pulsing);
        }

        if (pulsing != null && (fakeSecondsRemaining > 5f || fakeSecondsRemaining == 0))
        {
            StopCoroutine(pulsing);
            pulsing = null;
        }

        System.TimeSpan ts = System.TimeSpan.FromSeconds(fakeSecondsRemaining);
        timeDisplay.text = ts.Minutes + ":" + ts.Seconds.ToString("00");

        if (secondsRemaining < 0f)
            secondsRemaining = 0f;
    }

    private IEnumerator DoubleTick()
    {
        AudioManager.instance.Play("Countdown Tick", 2f);
        yield return new WaitForSeconds(.5f);
        AudioManager.instance.Play("Countdown Tick", 2f);
    }

    private IEnumerator HellaTick()
    {
        //Max pitch is three, and the last 3 seconds should get more and more urgent
        //5 through 3 secs left pitch is 2
        float pitch = 3f - ((SecondsRemaining + 1f) * .33f);

        AudioManager.instance.Play("Countdown Tick", pitch);
        yield return new WaitForSeconds(.25f);
        AudioManager.instance.Play("Countdown Tick", pitch);
        yield return new WaitForSeconds(.25f);
        AudioManager.instance.Play("Countdown Tick", pitch);
        yield return new WaitForSeconds(.25f);
        AudioManager.instance.Play("Countdown Tick", pitch);
    }
}
