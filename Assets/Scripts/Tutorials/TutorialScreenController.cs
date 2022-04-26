using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreenController : MonoBehaviour
{
    public TutorialInfoObject   info;
    public GameObject           leftPanel;
    public GameObject           rightPanel;
    public GameObject           previousButton;
    public GameObject           nextButton;

    private int                 currentPageIndex;
    private List<UIContainer>   currentPages;
    private bool                pagebuttonsClickable;

    private SignalReceiver      tutorialscreen_pageloaded_receiver;
    private SignalStream        tutorialscreen_pageloaded_stream;

    private void Awake()
    {
        tutorialscreen_pageloaded_stream = SignalStream.Get("TutorialScreen", "PageLoaded");

        tutorialscreen_pageloaded_receiver = new SignalReceiver().SetOnSignalCallback(AllowPageMovement);
    }

    private void OnEnable()
    {
        tutorialscreen_pageloaded_stream.ConnectReceiver(tutorialscreen_pageloaded_receiver);
    }

    private void OnDisable()
    {
        tutorialscreen_pageloaded_stream.DisconnectReceiver(tutorialscreen_pageloaded_receiver);
    }

    public void Setup()
    {
        pagebuttonsClickable    = false;

        currentPages            = new List<UIContainer>();
        currentPageIndex        = 0;

        Transform parentTrans   = info.pagesOnRight ? rightPanel.transform : leftPanel.transform;

        for (int i = 0; i < info.pages.Count; i++)
        {
            GameObject go       = Instantiate(info.pages[i].instructionPage);
            RectTransform rt    = (RectTransform)go.transform;

            rt.SetParent(parentTrans);

            rt.localScale       = Vector3.one;
            rt.localPosition    = Vector3.zero;
            rt.offsetMax        = Vector2.zero;
            rt.offsetMin        = Vector2.zero;

            currentPages.Add(go.GetComponent<UIContainer>());

            if (i == 0)
                currentPages[i].InstantShow();
            else
                currentPages[i].InstantHide();
        }

        RectTransform prevTran      = (RectTransform)previousButton.transform;
        RectTransform nextTran      = (RectTransform)nextButton.transform;

        prevTran.SetParent(parentTrans);
        prevTran.localScale         = Vector3.one;
        prevTran.anchoredPosition   = new Vector3(100, 100, 0);

        nextTran.SetParent(parentTrans);
        nextTran.localScale         = Vector3.one;
        nextTran.anchoredPosition   = new Vector3(-100, 100, 0);

        if (currentPages.Count <= 1)
        {
            previousButton.SetActive(false);
            nextButton.SetActive(false);
        }
        else
        {
            previousButton.SetActive(true);
            nextButton.SetActive(true);
        }
    }

    //Called by the Next Page Button's OnClick event
    public void NextPage()
    {
        if (!pagebuttonsClickable)
            return;

        pagebuttonsClickable = false;

        currentPages[currentPageIndex].Hide();

        currentPageIndex = currentPageIndex + 1 >= currentPages.Count ? 0 : currentPageIndex + 1;

        currentPages[currentPageIndex].Show();
    }

    //Called by the Previous Page Button's OnClick event
    public void PreviousPage()
    {
        if (!pagebuttonsClickable)
            return;

        pagebuttonsClickable = false;

        currentPages[currentPageIndex].Hide();

        currentPageIndex = currentPageIndex - 1 < 0 ? currentPages.Count - 1 : currentPageIndex - 1;

        currentPages[currentPageIndex].Show();
    }

    private void AllowPageMovement(Signal signal)
    {
        Debug.Log("page should be clickable now");

        pagebuttonsClickable = true;
    }
}
