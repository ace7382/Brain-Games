using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleGame_ShadowShapes : BattleGameControllerBase
{
    #region Inspector Variables

    [SerializeField] private RectTransform      pieceViewPanel;
    [SerializeField] private RectTransform      solutionPanel;
    [SerializeField] private GameObject         previousArrow;
    [SerializeField] private GameObject         nextArrow;

    #endregion

    #region Private Variables

    private int                                 currentPuzzleIndex;
    private int                                 currentPiecesChildIndex;

    #endregion

    #region Private Properties

    private int CurrentPiecesChildIndex
    {
        get { return currentPiecesChildIndex; }
        set
        {
            int temp = value;

            if (temp >= pieceViewPanel.childCount)
                temp = pieceViewPanel.childCount - 1;

            if (temp < 0)
                temp = 0;

            Debug.Log("Setting current child index: " + temp);

            pieceViewPanel.GetChild(currentPiecesChildIndex >= pieceViewPanel.childCount ? pieceViewPanel.childCount - 1 : currentPiecesChildIndex).gameObject.SetActive(false);
            pieceViewPanel.GetChild(temp).gameObject.SetActive(true);

            previousArrow.SetActive(temp != 0);
            nextArrow.SetActive(temp != pieceViewPanel.childCount - 1);

            currentPiecesChildIndex = temp;
        }
    }

    #endregion

    #region Public Functions

    public override void StartGame()
    {
        currentPuzzleIndex          = -1;

        NextPuzzle();
    }

    public override void BoardReset()
    {
        NextPuzzle();
    }

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.ShadowShapes);
    }

    //Called by the Next Button's OnClick behavior
    public void NextPiece()
    {
        CurrentPiecesChildIndex += 1;
    }

    //Called by the Previous Button's OnClick behavior
    public void PreviousPiece()
    {
        CurrentPiecesChildIndex -= 1;
    }

    //Called by the View Panel Button's OnClickBehavior
    public void CheckPiece()
    {
        Transform currentPieceTrans = pieceViewPanel.GetChild(CurrentPiecesChildIndex);
        ShadowShapePiece currentSSP = currentPieceTrans.GetComponent<ShadowShapePiece>();

        Debug.Log(currentSSP.name + ": " + (currentSSP.isSolution ? "in solution" : "not in solution"));

        if (currentSSP.isSolution)
        {
            GameObject destinationGO = solutionPanel.GetComponentsInChildren<ShadowShapePiece>().ToList().Find(x => !x.isSolution && x.id == currentSSP.id).gameObject;

            currentPieceTrans.SetParent(solutionPanel);

            Vector3 destination = destinationGO.transform.localPosition;

            currentPieceTrans.localPosition = destination;

            destinationGO.transform.SetParent(null);

            Destroy(destinationGO);

            Debug.Log(solutionPanel.GetComponentsInChildren<ShadowShapePiece>().ToList().FindIndex(x => x.isSolution == false));

            if (solutionPanel.GetComponentsInChildren<ShadowShapePiece>().ToList().FindIndex(x => !x.isSolution) < 0)
            {
                AudioManager.instance.Play("Go");

                Signal.Send("Battle", "CorrectResponse");

                NextPuzzle();
            }
            else
            {
                CurrentPiecesChildIndex = CurrentPiecesChildIndex;
            }
        }
        else
        {
            Signal.Send("Battle", "IncorrectResponse");
        }
    }

    #endregion

    #region Private Functions

    private void NextPuzzle()
    {
        currentPuzzleIndex++;

        if (currentPuzzleIndex >= BattleManager.instance.CurrentEnemy.UnitInfo.ShadowShapePuzzles.Count)
            currentPuzzleIndex = 0;

        foreach (Transform child in pieceViewPanel)
            Destroy(child.gameObject);

        foreach (Transform child in solutionPanel)
            Destroy(child.gameObject);

        ShadowShapePuzzle currentShadowShapePuzzle  = BattleManager.instance.CurrentEnemy.UnitInfo.ShadowShapePuzzles[currentPuzzleIndex];

        List<GameObject> pieces = new List<GameObject>();

        for (int i = 0; i < currentShadowShapePuzzle.solutionPieces.Count; i++)
        {
            GameObject go                   = Instantiate(currentShadowShapePuzzle.solutionPieces[i].piece
                                            , solutionPanel);
            RectTransform goTrans           = (RectTransform)go.transform;

            goTrans.anchoredPosition        = currentShadowShapePuzzle.solutionPieces[i].position;
            goTrans.localRotation           = Quaternion.Euler(currentShadowShapePuzzle.solutionPieces[i].rotation);
            goTrans.localScale              = currentShadowShapePuzzle.solutionPieces[i].scale;

            go.GetComponent<Image>().color  = Color.black;

            ShadowShapePiece ssp            = go.GetComponent<ShadowShapePiece>();
            ssp.isSolution                  = false;
            ssp.id                          = i;


            GameObject go2                  = Instantiate(currentShadowShapePuzzle.solutionPieces[i].piece
                                            , pieceViewPanel);
            RectTransform goTrans2          = (RectTransform)go2.transform;

            goTrans2.anchoredPosition       = Vector3.zero;
            goTrans2.localRotation          = Quaternion.Euler(currentShadowShapePuzzle.solutionPieces[i].rotation);
            goTrans2.localScale             = currentShadowShapePuzzle.solutionPieces[i].scale;

            go2.GetComponent<Image>().color = Color.red;
            ShadowShapePiece ssp2           = go2.GetComponent<ShadowShapePiece>();
            ssp2.isSolution                 = true;
            ssp2.id                         = ssp.id;

            pieces.Add(go2);
        }

        for (int i = 0; i < currentShadowShapePuzzle.extraPieces.Count; i++)
        {
            GameObject go                   = Instantiate(currentShadowShapePuzzle.extraPieces[i].piece
                                            , pieceViewPanel);
            RectTransform goTrans           = (RectTransform)go.transform;

            goTrans.anchoredPosition        = Vector3.zero;
            goTrans.localRotation           = Quaternion.Euler(currentShadowShapePuzzle.extraPieces[i].rotation);
            goTrans.localScale              = currentShadowShapePuzzle.extraPieces[i].scale;
            
            go.GetComponent<Image>().color  = Color.red;

            ShadowShapePiece ssp            = go.GetComponent<ShadowShapePiece>();
            ssp.isSolution                  = false;
            ssp.id                          = -1;

            pieces.Add(go);
        }

        //Shuffle pieceViewPanel children and then make the first one visible
        int[] finalChildIndexes             = new int[pieces.Count];

        for (int i = 0; i < finalChildIndexes.Length; i++)
            finalChildIndexes[i]            = i;

        int n                               = pieces.Count;;
        System.Random rng                   = new System.Random();

        while (n > 1)
        {
            n--;
            int k                           = rng.Next(n + 1);
            int value                       = finalChildIndexes[k];
            finalChildIndexes[k]            = finalChildIndexes[n];
            finalChildIndexes[n]            = value;
        }

        for (int i = 0; i < pieces.Count; i++)
        {
            pieces[i].transform.SetSiblingIndex(finalChildIndexes[i]);

            pieces[i].SetActive(finalChildIndexes[i] == 0);
        }

        CurrentPiecesChildIndex             = 0;
        //
    }

    #endregion
}
