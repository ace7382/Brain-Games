using Doozy.Runtime.Signals;
using Doozy.Runtime.UIManager.Components;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SequentialNumbersController : TimedMinigameController
{
    #region Inspector Variables

    [SerializeField] GameObject tilePrefab;
    [SerializeField] Transform  tileGridTransform;

    #endregion

    #region Private Variables

    private List<SequentialNumbersTile> tiles;
    private int                         currentSolutionIndex;

    #endregion

    #region Signal Variables

    private SignalReceiver  sequentialnumbers_tileclicked_receiver;
    private SignalStream    sequentialnumbers_tileclicked_stream;

    #endregion

    #region Unity Functions

    protected override void Awake()
    {
        base.Awake();

        sequentialnumbers_tileclicked_stream    = SignalStream.Get("SequentialNumbers", "TileClicked");

        sequentialnumbers_tileclicked_receiver  = new SignalReceiver().SetOnSignalCallback(TileClicked);
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        sequentialnumbers_tileclicked_stream.ConnectReceiver(sequentialnumbers_tileclicked_receiver);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        sequentialnumbers_tileclicked_stream.DisconnectReceiver(sequentialnumbers_tileclicked_receiver);
    }

    #endregion

    #region Override Functions

    protected override void Setup(Signal signal)
    {
        base.Setup(signal);

        SetupTiles();
    }

    protected override void EndGameEarly(Signal signal)
    {
        foreach (UIButton b in tileGridTransform.GetComponentsInChildren<UIButton>())
            b.interactable = false;

        base.EndGameEarly(signal);
    }

    public override void EndGame()
    {
        foreach (UIButton b in tileGridTransform.GetComponentsInChildren<UIButton>())
            b.interactable = false;

        base.EndGame();
    }

    #endregion

    #region Private Functions

    private void TileClicked(Signal signal)
    {
        //Signal data - SequentialNumbersTile, the tile clicked

        SequentialNumbersTile t = signal.GetValueUnsafe<SequentialNumbersTile>();

        if (t.SolutionIndex == currentSolutionIndex)
        {
            t.HideTile();
            currentSolutionIndex++;

            if (tiles.FindIndex(x => x.Showing) < 0)
            {
                Debug.Log("Board Completed");

                correctResponses++;
                AudioManager.instance.Play("Go");
                SetupTiles();
            }
        }
        else
        {
            incorrectResponses++;
            AudioManager.instance.Play("No");
            SetupTiles();
        }
    }

    private void SetupTiles()
    {
        //TODO: Calculate number of tiles that should be used
        //      Then resize Grid
        //      Then spawn/destroy tiles based on the number currently
        //      or don't use a grid and just make the objects spawn in random places w/o overlapping
        //      RN just using 15 tiles for the whole thing

        currentSolutionIndex            = 0;

        int HARDCODEDTILELIMIT          = 15;

        int tilesShowing                = GetNumOfTilesShowing();

        List<decimal> numbersLeft       = GetNumbersToDisplay(tilesShowing, Random.Range(0,2));
        List<decimal> solutionOrder     = new List<decimal>(numbersLeft);

        if (tiles == null)
        {
            tiles               = new List<SequentialNumbersTile>();
            List<int> tileNums  = Enumerable.Range(0, HARDCODEDTILELIMIT).ToList();

            for (int i = 0; i < HARDCODEDTILELIMIT - tilesShowing; i++)
                tileNums.RemoveAt(Random.Range(0, tileNums.Count));

            for (int i = 0; i < HARDCODEDTILELIMIT; i++)
            {
                GameObject go = Instantiate(tilePrefab, tileGridTransform);
                go.transform.localScale = Vector3.one;

                SequentialNumbersTile control = go.GetComponent<SequentialNumbersTile>();

                tiles.Add(control);

                if (!tileNums.Contains(i))
                {
                    control.HideTile();
                    continue;
                }

                control.ShowTile();

                int indexLeft = Random.Range(0, numbersLeft.Count);

                control.Setup(numbersLeft[indexLeft], solutionOrder.IndexOf(numbersLeft[indexLeft]));

                numbersLeft.RemoveAt(indexLeft);
            }
        }
        else
        {
            List<int> tileNums = Enumerable.Range(0, tiles.Count).ToList();

            for (int i = 0; i < tiles.Count - tilesShowing; i++)
                tileNums.RemoveAt(Random.Range(0, tileNums.Count));

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tileNums.Contains(i))
                {
                    tiles[i].ShowTile();

                    int indexLeft = Random.Range(0, numbersLeft.Count);

                    tiles[i].Setup(numbersLeft[indexLeft], solutionOrder.IndexOf(numbersLeft[indexLeft]));

                    numbersLeft.RemoveAt(indexLeft);
                }
                else
                {
                    tiles[i].HideTile();
                }
            }
        }
    }

    private int GetNumOfTilesShowing()
    {
        return Random.Range(3, 6);
    }

    private List<decimal> GetNumbersToDisplay(int numOfNumbers, int numberType)
    {
        List<decimal> ret = new List<decimal>();

        if(numberType == 0) //Integers
        {
            int startingNum = Random.Range(0, 50);

            ret.Add(startingNum);

            for (int i = 1; i < numOfNumbers; i++)
            {
                startingNum += Random.Range(1, 6);

                ret.Add(startingNum);
            }
        }
        else if (numberType == 1) //decimals
        {
            decimal startingNum = System.Decimal.Round((decimal)Random.Range(0f, 50f), 2);

            ret.Add(startingNum);

            for (int i = 1; i < numOfNumbers; i++)
            {
                startingNum += System.Decimal.Round((decimal)Random.Range(.1f, 6f), 2);

                ret.Add(startingNum);
            }
        }

        return ret;
    }

    #endregion
}
