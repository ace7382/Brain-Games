using Doozy.Runtime.Signals;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BattleGame_SequentialNumbers : BattleGameControllerBase
{
    #region Inspector Variables

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform  tileGridTransform;

    #endregion

    #region Private Variables

    private List<SequentialNumbersTile> tiles;
    private int                         currentSolutionIndex;

    #endregion

    #region Signal Variables

    private SignalReceiver              sequentialnumbers_tileclicked_receiver;
    private SignalStream                sequentialnumbers_tileclicked_stream;

    #endregion

    #region Unity Functions

    protected override void Awake()
    {
        sequentialnumbers_tileclicked_stream    = SignalStream.Get("SequentialNumbers", "TileClicked");

        sequentialnumbers_tileclicked_receiver  = new SignalReceiver().SetOnSignalCallback(TileClicked);
    }

    protected override void OnEnable()
    {
        sequentialnumbers_tileclicked_stream.ConnectReceiver(sequentialnumbers_tileclicked_receiver);
    }

    protected override void OnDisable()
    {
        sequentialnumbers_tileclicked_stream.DisconnectReceiver(sequentialnumbers_tileclicked_receiver);
    }

    private void OnDestroy()
    {
        sequentialnumbers_tileclicked_stream.DisconnectReceiver(sequentialnumbers_tileclicked_receiver);
    }

    #endregion

    #region Public Functions

    public override string GetBattleGameName()
    {
        return Helpful.GetStringFromBattleGameType(Helpful.BattleGameTypes.SequentialNumbers);
    }

    public override void StartGame()
    {
        NextSet();
    }

    public override void BoardReset()
    {
        NextSet();
    }

    #endregion

    #region Private Functions

    private void NextSet()
    {
        currentSolutionIndex = 0;
        int HARDCODEDTILELIMIT          = 16; //TODO - have this fluctuate with difficulty etc
        int tilesShowing                = GetNumberOfTilesToShow();

        List<decimal> numbersLeft       = GetNumbersToDisplay(tilesShowing, Random.Range(0, 2));
        List<decimal> solutionOrder     = new List<decimal>(numbersLeft);

        if (tiles == null)
        {
            tiles               = new List<SequentialNumbersTile>();
            List<int> tileNums  = Enumerable.Range(0, HARDCODEDTILELIMIT).ToList();

            for (int i = 0; i < HARDCODEDTILELIMIT - tilesShowing; i++)
                tileNums.RemoveAt(Random.Range(0, tileNums.Count));

            for (int i = 0; i < HARDCODEDTILELIMIT; i++)
            {
                GameObject go           = Instantiate(tilePrefab, tileGridTransform);
                go.name                 = "Tile #:" + i.ToString();
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

    private int GetNumberOfTilesToShow()
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

    private void TileClicked(Signal signal)
    {
        //Signal data - SequentialNumbersTile, the tile clicked

        SequentialNumbersTile t = signal.GetValueUnsafe<SequentialNumbersTile>();

        //Debug.Log("Current Solution Index: " + currentSolutionIndex);
        //Debug.Log("Tile's solution index: " + t.SolutionIndex);

        object[] info   = new object[2];
        info[1]         = (Vector2)t.transform.position;

        if (t.SolutionIndex == currentSolutionIndex)
        {
            t.HideTile();

            currentSolutionIndex++;

            if (tiles.FindIndex(x => x.Showing) < 0)
            {
                info[0] = AbilityCharger.AbilityChargeActions.CORRECT_RESPONSE;

                Signal.Send("Battle", "AbilityChargeGenerated", info);

                AudioManager.instance.Play("Go");

                NextSet();
            }
        }
        else
        {
            info[0] = AbilityCharger.AbilityChargeActions.INCORRECT_RESPONSE;

            Signal.Send("Battle", "AbilityChargeGenerated", info);

            AudioManager.instance.Play("No");

            NextSet();
        }
    }

    #endregion
}
