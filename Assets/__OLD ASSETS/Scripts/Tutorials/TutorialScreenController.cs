using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreenController : MonoBehaviour
{
    [SerializeField] private GameObject             leftPanel;
    [SerializeField] private GameObject             rightPanel;
    [SerializeField] private GameObject             previousButton;
    [SerializeField] private GameObject             nextButton;

    public  TutorialInfoObject                      info;
    private int                                     currentPageIndex;
    private List<UIContainer>                       currentInfoPages;
    private List<UIContainer>                       currentDisplayPages;
    private bool                                    pagebuttonsClickable;

    private SignalReceiver                          tutorialscreen_pageloaded_receiver;
    private SignalStream                            tutorialscreen_pageloaded_stream;

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

    //Called by the Tutorial Screen's OnShow Callback;
    public void Setup()
    {
        //info                            = GameManager.instance.currentLevelOLD != null ? GameManager.instance.currentLevelOLD.tutorial
        //                                  : GameManager.instance.currentMinigame.tutorial;

        pagebuttonsClickable            = true;

        currentInfoPages                = new List<UIContainer>();
        currentDisplayPages             = new List<UIContainer>();
        currentPageIndex                = 0;

        Transform infoParentTrans       = info.pagesOnRight ? rightPanel.transform : leftPanel.transform;
        Transform displayParentTrans    = info.pagesOnRight ? leftPanel.transform : rightPanel.transform;

        for (int i = 0; i < info.pages.Count; i++)
        {
            GameObject go               = Instantiate(info.pages[i].instructionPage);
            RectTransform rt            = (RectTransform)go.transform;

            rt.SetParent(infoParentTrans);

            rt.localScale               = Vector3.one;
            rt.localPosition            = Vector3.zero;
            rt.offsetMax                = Vector2.zero;
            rt.offsetMin                = Vector2.zero;

            currentInfoPages.Add(go.GetComponent<UIContainer>());

            if (info.pages[i].displayPage != null)
            {
                GameObject displayGO    = Instantiate(info.pages[i].displayPage);
                RectTransform drt       = (RectTransform)displayGO.transform;

                drt.SetParent(displayParentTrans);

                drt.localScale          = Vector3.one;
                drt.localPosition       = Vector3.zero;
                drt.offsetMax           = Vector2.zero;
                drt.offsetMin           = Vector2.zero;

                currentDisplayPages.Add(displayGO.GetComponent<UIContainer>());
            }
            else
            {
                currentDisplayPages.Add(null);
            }

            if (i == 0)
            {
                currentInfoPages[i].InstantShow();
                currentDisplayPages[i].InstantShow(); //the starting page will always have a display page
            }
            else
            {
                currentInfoPages[i].InstantHide();

                if (currentDisplayPages[i] != null)
                    currentDisplayPages[i].InstantHide();
            }
        }

        RectTransform prevTran          = (RectTransform)previousButton.transform;
        RectTransform nextTran          = (RectTransform)nextButton.transform;

        prevTran.SetParent(infoParentTrans);
        prevTran.localScale             = Vector3.one;
        prevTran.anchoredPosition       = new Vector3(100, 100, 0);
        prevTran.SetSiblingIndex(50);

        nextTran.SetParent(infoParentTrans);
        nextTran.localScale             = Vector3.one;
        nextTran.anchoredPosition       = new Vector3(-100, 100, 0);
        nextTran.SetSiblingIndex(50);

        if (currentInfoPages.Count <= 1)
        {
            previousButton.SetActive(false);
            nextButton.SetActive(false);
        }
        else
        {
            //TODO: Decide if I want to keep the previous button.
            //      animation pages that span a couple od instruction pages
            //      will have a weird order when going backwards. Requires more design consideration.
            //previousButton.SetActive(true); 
            nextButton.SetActive(true);
        }
    }

    //Called by the Next Page Button's OnClick event
    public void NextPage()
    {
        if (!pagebuttonsClickable)
            return;

        pagebuttonsClickable = false;

        currentInfoPages[currentPageIndex].Hide();

        currentPageIndex = currentPageIndex + 1 >= currentInfoPages.Count ? 0 : currentPageIndex + 1;

        currentInfoPages[currentPageIndex].Show();

        if (currentDisplayPages[currentPageIndex] != null)
        {
            for (int i = 0; i < currentDisplayPages.Count; i++)
            {
                if (currentDisplayPages[i] != null
                    && i != currentPageIndex
                    && (currentDisplayPages[i].visibilityState == Doozy.Runtime.UIManager.VisibilityState.Visible
                    || currentDisplayPages[i].visibilityState == Doozy.Runtime.UIManager.VisibilityState.IsShowing))
                {
                    currentDisplayPages[i].Hide();
                }
            }

            currentDisplayPages[currentPageIndex].Show();
        }
    }

    //Called by the Previous Page Button's OnClick event
    public void PreviousPage()
    {
        if (!pagebuttonsClickable)
            return;

        pagebuttonsClickable = false;

        currentInfoPages[currentPageIndex].Hide();

        currentPageIndex = currentPageIndex - 1 < 0 ? currentInfoPages.Count - 1 : currentPageIndex - 1;

        currentInfoPages[currentPageIndex].Show();

        if (currentDisplayPages[currentPageIndex] != null)
        {
            for (int i = 0; i < currentDisplayPages.Count; i++)
            {
                if (currentDisplayPages[i] != null
                    && i != currentPageIndex
                    && (currentDisplayPages[i].visibilityState == Doozy.Runtime.UIManager.VisibilityState.Visible
                    || currentDisplayPages[i].visibilityState == Doozy.Runtime.UIManager.VisibilityState.IsShowing))
                {
                    currentDisplayPages[i].Hide();
                }
            }

            currentDisplayPages[currentPageIndex].Show();
        }
    }

    //TODO: Maybe move this to on return to main menu? If it shows between each level might want to keeep it loaded?
    //      Might not need to show between levels though? Depends on if I want to have the level progress info on it
    //Called by the Tutorial Screen's OnHidden callback
    public void OnHide()
    {
        //TODO: Pool these
        foreach (Transform child in leftPanel.transform)
        {
            if (child.gameObject != previousButton && child.gameObject != nextButton)
                Destroy(child.gameObject);
        }

        foreach (Transform child in rightPanel.transform)
        {
            if (child.gameObject != previousButton && child.gameObject != nextButton)
                Destroy(child.gameObject);
        }
    }

    public void LeaveTutorialScreen()
    {
        AudioManager.instance.Play("Button Click");

        //Signal.Send("GameManagement", "LeaveTutorialScreen",
        //    GameManager.instance.currentLevelOLD != null ? GameManager.instance.currentLevelOLD.timedLevel : GameManager.instance.currentMinigame.timed);
    }

    private void AllowPageMovement(Signal signal)
    {
        pagebuttonsClickable = true;
    }
}
