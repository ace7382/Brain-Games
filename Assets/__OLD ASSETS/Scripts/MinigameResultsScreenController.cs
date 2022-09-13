using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Containers;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinigameResultsScreenController : MonoBehaviour
{
    #region Inspector Variables

    [SerializeField] TextMeshProUGUI    title;
    [SerializeField] TextMeshProUGUI    correctCount;
    [SerializeField] TextMeshProUGUI    incorrectCount;
    [SerializeField] CanvasGroup        correctCanvasGroup;
    [SerializeField] CanvasGroup        incorrectCanvasGroup;
    [SerializeField] CanvasGroup        buttonsCanvasGroup;
    [SerializeField] GameObject         skipMenuAnimationButton;
    [SerializeField] TextMeshProUGUI    difficultyBarText;
    [SerializeField] Image              difficultyBarFillPrimary;
    [SerializeField] Image              difficultyBarFillSecondary;
    [SerializeField] UIView             uiView;

    #endregion

    #region Private Variables

    private IEnumerator                 menuLoading;
    private int                         diff;
    private bool                        difficultyChanged;

    #endregion

    #region Signal Variables

    private SignalReceiver              endlevelscreen_setup_receiver;
    private SignalStream                endlevelscreen_setup_stream;

    #endregion

    #region Unity Functions

    private void Awake()
    {
        Canvas c        = GetComponentInParent<Canvas>();
        c.worldCamera   = Camera.main;
        c.sortingOrder  = UniversalInspectorVariables.instance.popupScreenOrderInLayer;

        endlevelscreen_setup_stream     = SignalStream.Get("EndLevelScreen", "Setup");
        
        endlevelscreen_setup_receiver   = new SignalReceiver().SetOnSignalCallback(Setup);
    }

    private void OnEnable()
    {
        endlevelscreen_setup_stream.ConnectReceiver(endlevelscreen_setup_receiver);
    }

    private void OnDisable()
    {
        endlevelscreen_setup_stream.DisconnectReceiver(endlevelscreen_setup_receiver);
    }

    #endregion

    #region Public Functions

    public void Setup(Signal signal)
    {
        SetTitleText();
        SetDifficultyBarText();
        StopAllCoroutines();

        ////TODO: This formula is overkill. Once the changes are applied while playing the game this complexity won't be needed
        //float fill = (float)(GameManager.instance.currentMinigame.currentDifficultyLevel > GameManager.instance.currentMinigame.maxDifficulty ?
        //            GameManager.instance.currentMinigame.maxDifficulty : GameManager.instance.currentMinigame.currentDifficultyLevel)
        //            / (float)(GameManager.instance.currentMinigame.maxDifficulty);

        //difficultyBarFillPrimary.fillAmount     = fill;
        //difficultyBarFillSecondary.fillAmount   = fill;

        //correctCount.text           = GameManager.instance.currentMinigameResults.numCorrect.ToString();
        //incorrectCount.text         = GameManager.instance.currentMinigameResults.numIncorrect.ToString();

        //diff                        = GameManager.instance.currentMinigameResults.numCorrect 
        //                              - GameManager.instance.currentMinigameResults.numIncorrect;

        correctCanvasGroup.alpha    = 0;
        incorrectCanvasGroup.alpha  = 0;
        buttonsCanvasGroup.alpha    = 0;

        menuLoading                 = null;
        difficultyChanged           = false;

        skipMenuAnimationButton.SetActive(true);

        Signal.Send("EndLevelScreen", "ShowScreen");
    }

    //Called by the Replay Button's OnClick behavior
    public void ReplayMinigame()
    {
        AudioManager.instance.Play("Button Click");

        Signal.Send("GameManagement", "ReplayCurrentMinigame");
    }

    //Called by the EndLevel View's OnShow Callback
    public void StartMenuAnimation()
    {
        menuLoading = MenuAnimation();

        StartCoroutine(menuLoading);
    }

    //Called by the invisible button's OnClick Behavior
    public void SkipMenuAnimation()
    {
        skipMenuAnimationButton.SetActive(false);

        if (menuLoading != null)
        {
            StopCoroutine(menuLoading);
            menuLoading = null;
        }

        correctCanvasGroup.alpha    = 1;
        incorrectCanvasGroup.alpha  = 1;
        buttonsCanvasGroup.alpha    = 1;

        if (!difficultyChanged)
            AwardDifficultyChanges();

        ////TODO: reduce this formula complexity based on other TODO's in here
        //float fill = (float)(GameManager.instance.currentMinigame.currentDifficultyLevel > GameManager.instance.currentMinigame.maxDifficulty ?
        //    GameManager.instance.currentMinigame.maxDifficulty : GameManager.instance.currentMinigame.currentDifficultyLevel)
        //    / (float)(GameManager.instance.currentMinigame.maxDifficulty);

        //difficultyBarFillPrimary.fillAmount     = fill;
        //difficultyBarFillSecondary.fillAmount   = fill;

        SetDifficultyBarText();
    }

    #endregion

    #region Private Functions

    private void SetTitleText()
    {
        ////TODO: Add variety based on how well the player did/the overall difficulty etc
        //if (GameManager.instance.currentMinigameResults.numCorrect > GameManager.instance.currentMinigameResults.numIncorrect)
        //    title.text = "~Way to Go~";
        //else
        //    title.text = "~Nice Try~";
    }

    private void SetDifficultyBarText()
    {
        //if (GameManager.instance.currentMinigame.currentDifficultyLevel >= GameManager.instance.currentMinigame.maxDifficulty)
        //{
        //    difficultyBarText.text      = "MAX!!";
        //    difficultyBarText.fontSize  = 100;
        //    difficultyBarText.margin    = new Vector4(0, 0, 0, -21); //This value is hardcoded based on the current font size.
        //}
        //else
        //{
        //    difficultyBarText.text      = GameManager.instance.currentMinigame.currentDifficultyLevel.ToString();
        //    difficultyBarText.fontSize  = 36;
        //    difficultyBarText.margin    = Vector4.zero;
        //}
    }

    private IEnumerator MenuAnimation()
    {
        WaitForSeconds w = new WaitForSeconds(.15f);

        yield return w;

        yield return Helpful.FadeCanvasIn(correctCanvasGroup, .75f);

        yield return w;

        yield return Helpful.FadeCanvasIn(incorrectCanvasGroup, .75f);

        AwardDifficultyChanges();

        //TODO: use progressors on these with set times. It's clunky bc idfk how to use ease formulas
        //      glad i spent hours doing it this way bc i didnt want to bother with the SUPER USEFUL TOOL I PAID $100 FOR
        //      fuck i'm an idiot sometimes
        yield return FirstBarFill();

        SetDifficultyBarText();

        if (diff != 0)
            DiffPopup();

        yield return SecondBarFill();
        //^^^^^^TODO: /end ^^^^

        yield return Helpful.FadeCanvasIn(buttonsCanvasGroup, .75f);

        skipMenuAnimationButton.SetActive(false);
    }

    //TODO: Make the changes active while playing the game.
    private void AwardDifficultyChanges()
    {
        //GameManager.instance.currentMinigame.currentDifficultyLevel = Mathf.Clamp(GameManager.instance.currentMinigame.currentDifficultyLevel + diff
        //                                                            , 0, GameManager.instance.currentMinigame.maxDifficulty);

        difficultyChanged = true;
    }

    private void DiffPopup()
    {
        //REMOVING THIS TO CHANGE THE FONT ASSET. This was still in use in the old code
        //Helpful.TextPopup(diff >= 0 ? string.Format("+{0}", diff.ToString()) : string.Format("{0}", diff.ToString())
        //    , difficultyBarText.transform, Vector2.zero, diff >= 0 ? Color.green : Color.red
        //    , UniversalInspectorVariables.instance.KGHappySolid, (int)(difficultyBarText.fontSize - 5));
    }

    private IEnumerator FirstBarFill()
    {
        //float fill = (float)(GameManager.instance.currentMinigame.currentDifficultyLevel)
        //    / (float)(GameManager.instance.currentMinigame.maxDifficulty);

        //if (diff > 0)
        //{
        //    float startMarker   = difficultyBarFillSecondary.fillAmount;
        //    float journeyLength = fill - startMarker; //Bar is moving up to show gain
        //    float startTime     = Time.time;

        //    while (difficultyBarFillSecondary.fillAmount != fill)
        //    {
        //        float distCovered           = (Time.time - startTime) * .15f;
        //        float fractionOfJourney     = distCovered / journeyLength;

        //        difficultyBarFillSecondary.fillAmount = Mathf.Lerp(startMarker, fill, fractionOfJourney);

        //        yield return null;
        //    }

        //    difficultyBarFillSecondary.fillAmount = fill;
        //}
        //else
        //{
        //    difficultyBarFillSecondary.color = Color.red;

        //    float startMarker   = difficultyBarFillPrimary.fillAmount;
        //    float journeyLength = startMarker - fill; //Bar is moving back to show loss
        //    float startTime     = Time.time;

        //    while (difficultyBarFillPrimary.fillAmount != fill)
        //    {
        //        float distCovered           = (Time.time - startTime) * .15f;
        //        float fractionOfJourney     = distCovered / journeyLength;

        //        difficultyBarFillPrimary.fillAmount = Mathf.Lerp(startMarker, fill, fractionOfJourney);

        //        yield return null;
        //    }

        //    difficultyBarFillPrimary.fillAmount = fill;
        //}

        yield return null; //added when removing the above code
    }

    private IEnumerator SecondBarFill()
    {
        if (diff > 0)
        {
            float fill = difficultyBarFillSecondary.fillAmount;

            float startMarker   = difficultyBarFillPrimary.fillAmount;
            float journeyLength = fill - startMarker; //Bar is moving up to show gain
            float startTime     = Time.time;

            while (difficultyBarFillPrimary.fillAmount != fill)
            {
                float distCovered       = (Time.time - startTime) * .15f;
                float fractionOfJourney = distCovered / journeyLength;

                difficultyBarFillPrimary.fillAmount = Mathf.Lerp(startMarker, fill, fractionOfJourney);

                yield return null;
            }

            difficultyBarFillPrimary.fillAmount = fill;
        }
        else
        {
            float fill          = difficultyBarFillPrimary.fillAmount;

            float startMarker   = difficultyBarFillSecondary.fillAmount;
            float journeyLength = startMarker - fill; //Bar is moving back to show loss
            float startTime     = Time.time;

            Debug.Log(string.Format("current fill: {0} || target {1}", difficultyBarFillSecondary.fillAmount, fill));

            while (difficultyBarFillSecondary.fillAmount != fill)
            {
                Debug.Log("while loop");

                float distCovered       = (Time.time - startTime) * .15f;
                float fractionOfJourney = distCovered / journeyLength;

                difficultyBarFillSecondary.fillAmount = Mathf.Lerp(startMarker, fill, fractionOfJourney);

                yield return null;
            }

            difficultyBarFillSecondary.fillAmount = fill;
        }
    }

    #endregion
}
